using UnityEngine;
using System.Collections;
using System;
using MissionOfMercenary;
using UnityEditor.Rendering.LookDev;
using UnityEditor.SettingsManagement;

namespace MIssionOfMercenary
{
    
    public class AssultRifle : MonoBehaviour, IFirearm
    {
        public enum SingleOrAuto
        {
            single,
            auto
        }

        public SingleOrAuto AttackType { get; set; } = SingleOrAuto.auto;

        ShellEjector _shell;
        TryGetAimHit _aimHit;
        Coroutine _autoFireCoroutine;
        Coroutine _reloadRoutine;
        Coroutine _muzzleFlashRoutine;
        GameObject _muzzleFlash;

        int _currentAmmo;
        float _reloadDelay = 2.0f;
        bool _isReloading = false;
        
        public bool CanReload => !_isReloading && _currentAmmo<_firearmDefinition.MagazineCapacity;
    
    

        public bool IsShot { get; private set; } = false;

        public AimType AimType { get { return _firearmDefinition.GunAimType; }}

        public WeaponType WeaponType { get { return _firearmDefinition.GunWeaponType; } }

        public int Damage { get { return _firearmDefinition.Damage; } }

        public float AttackRange { get { return _firearmDefinition.Range; } }

        public int CurrentAmmo { get { return _currentAmmo; } set { _currentAmmo = value; } } 

        [Header("Firearm Definition")]
        [SerializeField] FirearmDefinition _firearmDefinition;

        [Header("WeaponEffect Definition")]
        [SerializeField] WeaponEffectDefinition _weaponEffectDefinition;

        [SerializeField] WeaponRecoil _weaponRecoil;
        [SerializeField] PlayerBulletTrailPooling _bulletTrailPooling;
        [SerializeField] BulletMarkPooling _bulletMarkPooling;
        [SerializeField] float _trailRendererSpeed;

        [Header("Input")]
        [SerializeField] InputReader _inputReader;

        [Header("Muzzle")]
        [SerializeField] Transform _muzzle;
        [SerializeField] float _flashDestroyedTime = 2.0f;
        [SerializeField] float _bulletMarkDestroyedTime = 2.0f;

        private void Awake()
        {
            _shell = GetComponent<ShellEjector>();
            _aimHit = GetComponentInParent<TryGetAimHit>();
            if (_weaponRecoil == null)
            {
                _weaponRecoil = GetComponent<WeaponRecoil>(); 
            }
            InitializeMuzzleFlash();
        }
        private void Start()
        {
            _currentAmmo = _firearmDefinition.MagazineCapacity;
        }

        void OnEnable()
        {
            //_inputReader.OnshotEvent += HandleShot;
            //_inputReader.OnShotCancled += HandleShotCancled;
            //_inputReader.OnReloadEvent += TryReload;
        }

        void OnDisable()
        {
            HideMuzzleFlash();

            if (_reloadRoutine != null)
            {
                _reloadRoutine = null;
                TriggeredReleased();
            }
            
            //_inputReader.OnshotEvent -= HandleShot;
            //_inputReader.OnShotCancled -= HandleShotCancled;
        }

        public void Attack(float value)
        {
            IsShot = false;
            Vector3 targetPoint;

            if (_muzzle == null) { return; }

            Ray ray = _aimHit.RayHit;

            if (Physics.Raycast(ray, out RaycastHit hit, AttackRange))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.origin + ray.direction * AttackRange;
            }

            _bulletTrailPooling.PlayTrail(_muzzle.position, _muzzle.forward, AttackRange, _trailRendererSpeed);

            PlayMuzzleFlash();

            Vector3 muzzleDir = (targetPoint - _muzzle.position).normalized;
            if(!Physics.Raycast(_muzzle.position, muzzleDir, out RaycastHit muzzleHit, AttackRange)) { return; }
            EnemyHit enemyHit = muzzleHit.collider.GetComponentInParent<EnemyHit>();

            if (enemyHit == null)
            {
                _bulletMarkPooling.GetBulletMark(muzzleHit.point + muzzleHit.normal * 0.01f, Quaternion.LookRotation(muzzleHit.normal));
            }

            IsShot = true;

            if (enemyHit != null)
            {
                enemyHit.RecieveHit(muzzleHit, Damage);
            }
        }

        void InitializeMuzzleFlash()
        {
            if (_muzzle == null || _weaponEffectDefinition == null ||
                _weaponEffectDefinition.MuzzleFlash == null)
            {
                return;
            }

            // 총구의 자식으로 한 번 생성하여 무기의 이동과 회전을 따라가게 한다.
            _muzzleFlash = Instantiate(_weaponEffectDefinition.MuzzleFlash, _muzzle);
            _muzzleFlash.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            _muzzleFlash.SetActive(false);
        }

        void PlayMuzzleFlash()
        {
            if (_muzzleFlash == null) { return; }

            // 연사 시 이전 타이머가 새 플래시를 먼저 끄지 않도록 갱신한다.
            HideMuzzleFlash();
            _muzzleFlash.SetActive(true);
            _muzzleFlashRoutine = StartCoroutine(HideMuzzleFlashAfterDelay());
        }

        IEnumerator HideMuzzleFlashAfterDelay()
        {
            yield return new WaitForSeconds(_flashDestroyedTime);
            if (_muzzleFlash != null) { _muzzleFlash.SetActive(false); }
            _muzzleFlashRoutine = null;
        }

        void HideMuzzleFlash()
        {
            if (_muzzleFlashRoutine != null)
            {
                StopCoroutine(_muzzleFlashRoutine);
                _muzzleFlashRoutine = null;
            }

            if (_muzzleFlash != null) { _muzzleFlash.SetActive(false); }
        }
        IEnumerator AutoFireRoutine()
        {
            while (_currentAmmo > 0 && !_isReloading)
            {
                Attack(1f);
                _currentAmmo--;
                _weaponRecoil?.WeaponRecoilApply();
                _shell?.Ejector();

                yield return new WaitForSeconds(
                    1f / _firearmDefinition.FireInterval);
            }

            _autoFireCoroutine = null;
        }


        public void TriggeredPressed()
        {
            if (_isReloading) return;

            if (_currentAmmo <= 0)
            {
                _currentAmmo = 0;
                TriggeredReleased();
                return;
            }

            if (AttackType == SingleOrAuto.auto)
            {
                if (_autoFireCoroutine != null) return;

                _autoFireCoroutine = StartCoroutine(AutoFireRoutine());
            }
            else
            {
                Attack(1f);
                _currentAmmo--;
                _weaponRecoil?.WeaponRecoilApply();
                _shell?.Ejector();
            }
        }

        public void TriggeredReleased()
        {
            if (_autoFireCoroutine != null)
            {
                StopCoroutine(_autoFireCoroutine);
                _autoFireCoroutine = null;
            }
        }

        public void TryReload()
        {
            if (!CanReload) { return; }

            _isReloading = true;
            Debug.Log("Reloading");
            TriggeredReleased();//재장전중엔 무기공격 캔슬

            _reloadRoutine = StartCoroutine(ReloadDelayRoutine());
        }

        IEnumerator ReloadDelayRoutine()
        {
            yield return new WaitForSeconds(_reloadDelay);
            _currentAmmo = _firearmDefinition.MagazineCapacity;    
            _isReloading = false;
            _reloadRoutine = null;
        }
    }
}

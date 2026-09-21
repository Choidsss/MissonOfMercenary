using UnityEngine;
using System.Collections;
using System;
using MissionOfMercenary;
using UnityEditor.Rendering.LookDev;
using UnityEditor.SettingsManagement;

namespace MIssionOfMercenary
{
    
    public class AssultRifle : MonoBehaviour, IWeapons
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
        GameObject _bulletMarkObj;

        //string _gunName;
        //float _fireInterval;
        //float _range;
        //int _damage;

        int _currentAmmo;
        float _reloadDelay = 2.0f;
        
        public bool CanReload { get; private set; } = false;

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
            _inputReader.OnshotEvent += HandleShot;
            _inputReader.OnShotCancled += HandleShotCancled;
            _inputReader.OnReloadEvent += HandledReload;
        }

        void OnDisable()
        {
            HideMuzzleFlash();
            _inputReader.OnshotEvent -= HandleShot;
            _inputReader.OnShotCancled -= HandleShotCancled;
        }

        public void Attack(float isShot)
        {
            IsShot = false;
            if(CurrentAmmo != _firearmDefinition.MagazineCapacity) { CanReload = true; }
            else { CanReload = false; }

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

        void HandleShot(float shot)
        {
            if(CurrentAmmo <= 0) { CurrentAmmo = 0; StopCoroutine(AutoFireRoutine()); }


            if (AttackType == SingleOrAuto.auto && CurrentAmmo != 0)
            {
                _autoFireCoroutine = StartCoroutine(AutoFireRoutine());
            }
            else
            {
                if (CurrentAmmo <= 0) { return; }

                Attack(shot);
                _currentAmmo--;
                _weaponRecoil?.WeaponRecoilApply();
                _shell?.Ejector(); 
            }
        }

        void HandledReload(float value)
        {
            if (!CanReload) { return; }
            if (_reloadRoutine != null) StopCoroutine(_reloadRoutine); 
            _reloadRoutine = StartCoroutine(ReloadDelayRoutine());
            Debug.Log("Reloading");
        }

        void HandleShotCancled()
        {
            if (_autoFireCoroutine != null)
            {
                StopCoroutine(_autoFireCoroutine);
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

        void OnDestroy()
        {
            if (_muzzleFlash != null) { Destroy(_muzzleFlash); }
        }

        IEnumerator BulletMarkEffectDestoryRoutine(GameObject mark)
        {
            if (mark == null) { yield return null; }

            yield return new WaitForSeconds(_bulletMarkDestroyedTime);
            Destroy(mark);
        }

        IEnumerator AutoFireRoutine()
        {
            while (_currentAmmo > 0)
            {
                Attack(1f);
                _currentAmmo--;

                _weaponRecoil?.WeaponRecoilApply();
                yield return new WaitForSeconds(1f/_firearmDefinition.FireInterval);
            }
        }

        IEnumerator ReloadDelayRoutine()
        {
            CanReload = false;
            yield return new WaitForSeconds(_reloadDelay);
            _currentAmmo = _firearmDefinition.MagazineCapacity;    
        }

        IEnumerator SpawnBulletTrail(Vector3 targetPoint, Vector3 direction)
        {
            float movedDistance = 0f;
            GameObject trail = Instantiate(_weaponEffectDefinition.Trail, _muzzle.position, Quaternion.LookRotation(direction));
            float totalDistance = Vector3.Distance(_muzzle.position, targetPoint);

            while (movedDistance < totalDistance)
            {
                float step = _trailRendererSpeed * Time.deltaTime;
                trail.transform.position += direction * step;
                movedDistance += step;
                yield return null;
            }

            Destroy(trail);
        }
    }
}

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
        readonly HitscanResolver _hitscanResolver = new HitscanResolver();
        
        float shotValue = 1.0f;

        public enum SingleOrAuto
        {
            single,
            auto
        }

        public SingleOrAuto AttackType { get; set; } = SingleOrAuto.auto;

        TryGetAimHit _getAimRay;
        ShellEjector _shell;
        Coroutine _autoFireCoroutine;
        Coroutine _reloadRoutine;

        int _currentAmmo;
        float _reloadDelay = 2.0f;
        bool _isReloading = false;
        
        public bool CanReload => !_isReloading && _currentAmmo<_firearmDefinition.MagazineCapacity;
    
        public bool IsShot { get; private set; } = false;

        public AimType AimType { get { return _firearmDefinition.GunAimType; }}

        public WeaponType WeaponType { get { return _firearmDefinition.GunWeaponType; } }

        public int Damage { get { return _firearmDefinition.Damage; } }

        public float AttackRange { get { return _firearmDefinition.Range; } }

        public int CurrentAmmo { get { return _currentAmmo; } } 

        [Header("Firearm Definition")]
        [SerializeField] FirearmDefinition _firearmDefinition;

        [Header("WeaponEffect Definition")]
        [SerializeField] WeaponEffectDefinition _weaponEffectDefinition;

        [Header("MuzzleFlashEffect")]
        [SerializeField] MuzzleFlashEffect _weaponMuzzleFlashEffect;

        [SerializeField] WeaponRecoil _weaponRecoil;
        [SerializeField] PlayerBulletTrailPooling _bulletTrailPooling;
        [SerializeField] BulletMarkPooling _bulletMarkPooling;
        [SerializeField] float _trailRendererSpeed;

        [Header("Muzzle")]
        [SerializeField] Transform _muzzle;
        [SerializeField] float _bulletMarkDestroyedTime = 2.0f;

        float _nextFireTime;

        private void Awake()
        {
            _shell = GetComponent<ShellEjector>();
            _getAimRay = GetComponentInParent<TryGetAimHit>();

            if (_weaponRecoil == null)
            {
                _weaponRecoil = GetComponent<WeaponRecoil>(); 
            }
        }
        private void Start()
        {
            _currentAmmo = _firearmDefinition.MagazineCapacity;
            
        }

        void OnDisable()
        {
            TriggeredReleased();

            if (_reloadRoutine != null)
            {
                StopCoroutine(_reloadRoutine);
                _reloadRoutine = null;
            }

            _isReloading = false;
            IsShot = false;
        }

        private void ExecuteShot()
        {
            IsShot = false;

            ShotResult shot = _hitscanResolver.Resolver(_getAimRay.GetAimRay(), _muzzle.position, AttackRange);

            _weaponMuzzleFlashEffect?.PlayMuzzleFlash();

            if(shot.Distance >= 0.01f)
            {
                _bulletTrailPooling.PlayTrail(shot.Origin, shot.Direction, shot.Distance, _trailRendererSpeed);
            }

            if (_muzzle == null) { return; }

            if (!shot.IsHit) { return; }

            RaycastHit hit = shot.Hit;
            EnemyHit enemyHit = hit.collider.GetComponentInParent<EnemyHit>();
            
            if (enemyHit != null)
            {
                enemyHit.RecieveHit(hit, Damage);
            }
            else
            {
                _bulletMarkPooling.GetBulletMark(hit.point + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal));
            }

            IsShot = true;
        }

        IEnumerator AutoFireRoutine()
        {
            while (_currentAmmo > 0 && !_isReloading)
            {
                TryFire();

                yield return null;
            }

            _autoFireCoroutine = null;
        }

        bool TryFire()
        {
            if (_isReloading || _currentAmmo <= 0) { return false; }
            if (_muzzle == null || _getAimRay == null) { return false; }
            if (!isActiveAndEnabled) { return false; }
            if (Time.time < _nextFireTime) { return false; } //시간이 지나야만 다음 입력을 받음

            _nextFireTime = Time.time + _firearmDefinition.FireInterval;

            ExecuteShot();

            _currentAmmo--;
            _weaponRecoil?.WeaponRecoilApply();
            _shell?.Ejector();

            return true;
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
                TryFire();
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

        private void OnDrawGizmos()
        {
            //Vector3 targetPoint;
            Gizmos.color = Color.black;

            //Ray ray = _getAimRay.GetAimRay();

            //if (Physics.Raycast(ray, out RaycastHit hit, AttackRange))
            //{
            //    targetPoint = hit.point;
            //}
            //else
            //{
            //    targetPoint = ray.origin + ray.direction * AttackRange;
            //}

            //Vector3 muzzleDir = (targetPoint - _muzzle.position).normalized;
            Gizmos.DrawLine(_muzzle.transform.position, _muzzle.transform.position + _muzzle.forward * 100f);
        }
    }
}

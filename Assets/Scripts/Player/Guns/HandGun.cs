using MissionOfMercenary;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MIssionOfMercenary
{
    public class HandGun : MonoBehaviour, IFirearm
    {
        readonly HitscanResolver _hitscanResolver = new HitscanResolver();

        [Header("Definitions")]
        [SerializeField] FirearmDefinition _fireDef;
        [SerializeField] WeaponEffectDefinition _weaponDef;

        [SerializeField] BulletMarkPooling _bulletMarkPooling;
        TryGetAimHit _aimHit;

        [Header("Recoil")]
        [SerializeField] WeaponRecoil _weaponRecoil;

        [Header("MuzzleFlashEffect")]
        [SerializeField] MuzzleFlashEffect _weaponMuzzleFlashEffect;

        [Header("Needed Assets")]
        [SerializeField] InputReader _inputReader;
        [SerializeField] Transform _muzzle;
        [SerializeField] GameObject _bullet;
        [SerializeField] GameObject _bulletTrail;
        [SerializeField] PlayerBulletTrailPooling _bulletTrailPooling;
        [SerializeField] GameObject _shellEjector;
        [SerializeField] GameObject _bulletMark;

        [Header("Attack Fields")]
        [SerializeField] float _reloadDelay = 2.0f;
        [SerializeField] float _trailSpeeds = 1.0f;

        public AimType AimType { get { return _fireDef.GunAimType; } }

        public WeaponType WeaponType { get { return _fireDef.GunWeaponType; } }

        public int Damage { get { return _fireDef.Damage; }}

        public float AttackRange { get { return _fireDef.Range; }}

        public int CurrentAmmo { get { return _hgCurrentAmmo; }  }
        public bool IsShot { get; private set;} = false;

        Coroutine _reloadRoutine;

        int _hgCurrentAmmo = 1;
        bool _isReloading = false;
        float _nextFireTime;

        private void OnDisable()
        {
            if(_reloadRoutine != null)
            {
                StopCoroutine(_reloadRoutine);
                _reloadRoutine = null;
            }

            _isReloading = false;
        }

        void Start()
        {
            _aimHit = GetComponentInParent<TryGetAimHit>();
            _hgCurrentAmmo = _fireDef.MagazineCapacity;
        }

        private void ExecuteShot()
        {
            IsShot = false;

            ShotResult shot = _hitscanResolver.Resolver(_aimHit.GetAimRay(), _muzzle.position, AttackRange);
            _weaponMuzzleFlashEffect?.PlayMuzzleFlash();

            if (shot.Distance >= 0.01f)
            {
                _bulletTrailPooling.PlayTrail(shot.Origin, shot.Direction, shot.Distance, _trailSpeeds);
            }

            if (!shot.IsHit) { return; }

            RaycastHit hit = shot.Hit;
            IHitReceiver hitReceive = hit.collider.GetComponentInParent<IHitReceiver>();

            if (hitReceive != null) { hitReceive.ReceiveHit(hit, Damage); }
            else { _bulletMarkPooling.GetBulletMark(hit.point + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal)); }
        }

        bool TryFire()
        {
            if (_muzzle == null || _fireDef == null) { return false; }
            if (_isReloading || _hgCurrentAmmo <= 0) { return false; }
            if (!isActiveAndEnabled) { return false; }
            if (Time.time < _nextFireTime) { return false; }

            _nextFireTime = Time.time + _fireDef.FireInterval;

            ExecuteShot();
            _hgCurrentAmmo--;
            IsShot = true;
            _weaponRecoil?.WeaponRecoilApply();

            return true;
        }

        public void TriggeredPressed()
        {
            if (_isReloading) { return; }

            if (_hgCurrentAmmo <= 0)
            {
                _hgCurrentAmmo = 0;
                TriggeredReleased();
                return;
            }

            TryFire();
        }

        public void TriggeredReleased()
        {
            return;
        }

        public void TryReload()
        {
            if (_hgCurrentAmmo >= _fireDef.MagazineCapacity || _isReloading) { return; }
            _isReloading = true;

            _reloadRoutine = StartCoroutine(ReloadDelayRoutine());
        }
        IEnumerator ReloadDelayRoutine()
        {
            yield return new WaitForSeconds(_reloadDelay);
            _hgCurrentAmmo = _fireDef.MagazineCapacity;

            _isReloading = false;
            _reloadRoutine = null;
        }
    }
}

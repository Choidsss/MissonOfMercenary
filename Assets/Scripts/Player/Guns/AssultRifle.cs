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
        [SerializeField] IKController _ikController;
        [SerializeField] AimController _aimController;
        [SerializeField] GameObject _bullet;
        [SerializeField] PlayerBulletTrailPooling _bulletTrailPooling;
        [SerializeField] BulletMarkPooling _bulletMarkPooling;
        [SerializeField] float _trailRendererSpeed;

        ShellEjector _shell;
        TryGetAimHit _aimHit;

        public enum SingleOrAuto
        {
            single,
            auto
        }

        public AimType aimType { get; } = AimType.None;

        public WeaponType weaponType { get; } = WeaponType.AR;

        public SingleOrAuto AttackType { get; set; } = SingleOrAuto.auto;

        [SerializeField] InputReader _inputReader;

        [Header("Muzzle")]
        [SerializeField] Transform _muzzle;
        [SerializeField] GameObject _muzzleFlash;
        [SerializeField] GameObject _bulletMarkObj;
        [SerializeField] float _flashDestroyedTime = 2.0f;
        [SerializeField] float _bulletMarkDestroyedTime = 2.0f;
        //[SerializeField] GameObject _bulletMarkObj;
        

        [Header("Weapon Options")]
        [SerializeField] public int Damage { get; } = 5;

        [SerializeField] public float AttackRange { get; } = 100;
        
        [SerializeField] float _autoSpeed = 1.0f;

        [SerializeField] int _maxAmmo = 30;

        public int Ammo { get; private set; } = 30;

        public bool canReload { get; private set; } = false;

        public bool IsShot { get; private set; } = false;

        //[SerializeField] LayerMask _layer;

        Coroutine _autoFireCoroutine;
        Coroutine _reloadRoutine;

        float _reloadDelay = 2.0f;

        private void Start()
        {
            _shell = GetComponent<ShellEjector>();
            _aimHit = GetComponentInParent<TryGetAimHit>();
        }

        void OnEnable()
        {
            _inputReader.OnshotEvent += HandleShot;
            _inputReader.OnShotCancled += HandleShotCancled;
            _inputReader.OnReloadEvent += HandledReload;
        }

        void OnDisable()
        {
            _inputReader.OnshotEvent -= HandleShot;
            _inputReader.OnShotCancled -= HandleShotCancled;
        }

        public void Attack(float isShot)
        {
            IsShot = false;
            if(Ammo != _maxAmmo) { canReload = true; }
            else { canReload = false; }

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
            // Instantiate comparison:
            // StartCoroutine(SpawnBulletTrail(targetPoint, _muzzle.forward));

            //  머즐플래시는 항상 생성
            GameObject flash = Instantiate(_muzzleFlash, _muzzle.position, _muzzle.rotation);
            StartCoroutine(FlashEffectDestoryRoutine(flash));

            Vector3 muzzleDir = (targetPoint - _muzzle.position).normalized;
            if(!Physics.Raycast(_muzzle.position, muzzleDir, out RaycastHit muzzleHit, AttackRange)) { return; }
            EnemyHit enemyHit = muzzleHit.collider.GetComponentInParent<EnemyHit>();

            if (enemyHit == null)
            {
                _bulletMarkPooling.GetBulletMark(muzzleHit.point + muzzleHit.normal * 0.01f, Quaternion.LookRotation(muzzleHit.normal));
                // Instantiate comparison:
                // GameObject bulletMark = Instantiate(_bulletMarkObj, muzzleHit.point + muzzleHit.normal * 0.01f, Quaternion.LookRotation(muzzleHit.normal));
                // StartCoroutine(BulletMarkEffectDestoryRoutine(bulletMark));
            }

            IsShot = true;

            if (enemyHit != null)
            {
                enemyHit.RecieveHit(muzzleHit, Damage);
            }

            //if (_muzzle == null) { return; }


            //RaycastHit hit;
            //bool isHit = Physics.Raycast(_muzzle.position, _muzzle.forward, out hit, AttackRange);


            //if(!isHit) { return; }


            //EnemyHit enemyHit = hit.collider.GetComponent<EnemyHit>();

            //if (isHit && enemyHit != null)
            //{
            //    GameObject flash = Instantiate(_muzzleFlash, _muzzle.position, _muzzle.rotation);
            //    enemyHit.TakeDameged(Damage);
            //    StartCoroutine(FlashEffectDestoryRoutine(flash));
            //}
        }

        void HandleShot(float shot)
        {
            if(Ammo <= 0) { Ammo = 0; StopCoroutine(AutoFireRoutine()); }


            if (AttackType == SingleOrAuto.auto && Ammo != 0)
            {
                _autoFireCoroutine = StartCoroutine(AutoFireRoutine());
            }
            else
            {
                if (Ammo <= 0) { return; }

                Attack(shot);
                Ammo--;

                if (_aimController.IsAiming)
                {
                    _aimController.ApplyRecoilDuringAiming();
                }
                else
                {
                    _ikController.ApplyRecoil();
                }
                _shell.Ejector();
            }
        }

        void HandledReload(float value)
        {
            if (!canReload) { return; }
            if (_reloadRoutine != null) StopCoroutine(_reloadRoutine); // 재장전 코루틴 멈춤
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

        IEnumerator FlashEffectDestoryRoutine(GameObject flash)
        {
            if(flash == null) { yield return null; }

            yield return new WaitForSeconds(_flashDestroyedTime);
            Destroy(flash);
        }

        IEnumerator BulletMarkEffectDestoryRoutine(GameObject mark)
        {
            if (mark == null) { yield return null; }

            yield return new WaitForSeconds(_bulletMarkDestroyedTime);
            Destroy(mark);
        }

        IEnumerator AutoFireRoutine()
        {
            while (Ammo > 0)
            {
                Attack(1f);
                Ammo--;

                if (_aimController.IsAiming)
                {
                    _aimController.ApplyRecoilDuringAiming();
                }
                else
                {
                    _ikController.ApplyRecoil();
                }

                _shell.Ejector();
                yield return new WaitForSeconds(1f/_autoSpeed);
            }
        }

        IEnumerator ReloadDelayRoutine()
        {
            canReload = false;
            yield return new WaitForSeconds(_reloadDelay);
            Ammo = _maxAmmo;    
        }

        IEnumerator SpawnBulletTrail(Vector3 targetPoint, Vector3 direction)
        {
            float movedDistance = 0f;
            GameObject trail = Instantiate(_bullet, _muzzle.position, Quaternion.LookRotation(direction));
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

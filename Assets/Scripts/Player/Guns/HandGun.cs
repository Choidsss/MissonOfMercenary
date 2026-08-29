using MissionOfMercenary;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MIssionOfMercenary
{
    public class HandGun : MonoBehaviour, IWeapons
    {
        [SerializeField] BulletMarkPooling _bulletMarkPooling;
        TryGetAimHit _aimHit;

        [Header("Recoil")]
        [SerializeField] WeaponRecoil _weaponRecoil;

        //[Header("IK")]
        //[SerializeField] Transform _rightGripPoint;
        //[SerializeField] Transform _rightHandIKTarget;

        [Header("Needed Assets")]
        [SerializeField] InputReader _inputReader;
        [SerializeField] GameObject _muzzle;
        [SerializeField] GameObject _bullet;
        [SerializeField] GameObject _bulletTrail;
        [SerializeField] PlayerBulletTrailPooling _bulletTrailPooling;
        [SerializeField] GameObject _muzzleFlash;
        [SerializeField] GameObject _shellEjector;
        [SerializeField] GameObject _bulletMark;

        [Header("Attack Fields")]
        [SerializeField] float _muzzleFlashDestroyDelay = 1.5f;
        [SerializeField] float _bulletMarkDestroyDelay = 1.5f;
        [SerializeField] float _reloadDelay = 2.0f;
        [SerializeField] float _attackRange = 1.0f;
        [SerializeField] int _hgDamage = 1;
        [SerializeField] int _hgMaxAmmo = 12;
        [SerializeField] float _trailSpeeds = 1.0f;

        public AimType aimType { get; } = AimType.None;

        public WeaponType weaponType => WeaponType.HG;

        public int Damage { get { return _hgDamage; } private set { _hgDamage = value; } }

        public float AttackRange { get { return _attackRange; } private set { _attackRange = value; } }

        public int Ammo { get { return _hgCurrentAmmo; } private set { _hgCurrentAmmo = value; } }
        public bool IsShot { get; private set;} = false;

        int _hgCurrentAmmo = 1;
        bool _isReloading = false;

        private void OnEnable()
        {
            _inputReader.OnshotEvent += Attack;
            _inputReader.OnReloadEvent += HandledReload;
        }

        private void OnDisable()
        {
            _inputReader.OnshotEvent -= Attack;
            _inputReader.OnReloadEvent -= HandledReload;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _aimHit = GetComponentInParent<TryGetAimHit>();
            _hgCurrentAmmo = _hgMaxAmmo;
        }

        public void Attack(float isShot)
        {
            if (_hgCurrentAmmo <= 0) { Debug.Log("재장전이 필요합니다!"); IsShot = false; return; }
            IsShot = false;
            Vector3 targetPoint;

            Ray ray = _aimHit.RayHit;

            if (Physics.Raycast(ray, out RaycastHit hit, AttackRange))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.origin + ray.direction * AttackRange;
            }

            //총 발사시 화염 생성
            GameObject flash = Instantiate(_muzzleFlash, _muzzle.transform.position, _muzzle.transform.rotation);
            Vector3 direction = (targetPoint - _muzzle.transform.position).normalized;

            StartCoroutine(MuzzleFlashDestroyRoutine(flash));
            _bulletTrailPooling.PlayTrail(_muzzle.transform.position, _muzzle.transform.forward, AttackRange, _trailSpeeds);
            // Instantiate comparison:
            // StartCoroutine(SpawnBulletTrail(targetPoint, _muzzle.transform.forward));

            _hgCurrentAmmo--;
            IsShot = true; //트레일이 생성될 때 IsShot = true

            _weaponRecoil?.WeaponRecoilApply();

            if (!Physics.Raycast(ray, out RaycastHit hitInfo, AttackRange)) { return; }
            EnemyHit enemyHit = hitInfo.collider.GetComponentInParent<EnemyHit>();

            if (enemyHit == null)
            {
                _bulletMarkPooling.GetBulletMark(hitInfo.point + hitInfo.normal * 0.01f, Quaternion.LookRotation(hitInfo.normal));
                // Instantiate comparison:
                // GameObject bulletMark = Instantiate(_bulletMark, hitInfo.point + hitInfo.normal * 0.01f, Quaternion.LookRotation(hitInfo.normal));
                // StartCoroutine(BulletMarkDestroyRoutine(bulletMark));
            }

            if (enemyHit != null) { enemyHit.RecieveHit(hitInfo, Damage); }
            else { Debug.Log("맞은 적이 없어 컴포넌트를 가져올수 없습니다!"); }
        }

        void HandledReload(float shot)
        {
            if (_hgCurrentAmmo == _hgMaxAmmo && !_isReloading) { return; }

            StartCoroutine(ReloadDelayRoutine());
            _isReloading = false;
        }

        IEnumerator ReloadDelayRoutine()
        {
            yield return new WaitForSeconds(_reloadDelay);

            _isReloading = true;
            _hgCurrentAmmo = _hgMaxAmmo;
        }

        IEnumerator BulletMarkDestroyRoutine(GameObject bulletMark)
        {
            if (bulletMark == null) { yield return null; }
            yield return new WaitForSeconds(_bulletMarkDestroyDelay);
            Destroy(bulletMark);
        }

        IEnumerator MuzzleFlashDestroyRoutine(GameObject flash)
        {
            if (flash == null) { Debug.Log("파괴할 MuzzleFlash가 없습니다"); } //yield return null;
            yield return new WaitForSeconds(_muzzleFlashDestroyDelay);

            Destroy(flash);
        }

        IEnumerator SpawnBulletTrail(Vector3 targetPoint, Vector3 muzzleDirection)
        {
            float moveDistance = 0;
            GameObject go = Instantiate(_bulletTrail, _muzzle.transform.position, _muzzle.transform.rotation);

            float totalDistance = Vector3.Distance(go.transform.position, targetPoint);

            while (moveDistance < totalDistance)
            {
                if(go == null) { Debug.Log("Trail이 생성되지 않았습니다."); break; }

                float step = _trailSpeeds * Time.deltaTime;
                go.transform.position += step * muzzleDirection;

                moveDistance += step;
                yield return null;
            }

            Destroy(go);
        }
    }
}

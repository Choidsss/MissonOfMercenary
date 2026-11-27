using System;
using System.Collections;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class SentryEnemyAttack : MonoBehaviour
    {
        public event Action _firedAction;

        [SerializeField] LayerMask _obstacleMask;

        [Header("Get Script")]
        [SerializeField] EnemyWeaponRecoil _weaponRecoil;

        [Header("Attack Variable")]
        [SerializeField] int _damage;
        [SerializeField] float _attackRange;
        [SerializeField] int _maxAmmo = 30;

        [Header("Object Pool")]
        [SerializeField] EnemyBulletPooling _bulletPool;

        [Header("Muzzle")]
        [SerializeField] Transform _muzzle;
        [SerializeField] GameObject _muzzleFlash;
        [SerializeField] float _muzzleFlashDuration = 0.05f;
        [SerializeField] Vector3 _targetOffset = Vector3.up;

        [Header("Attack Timing")]
        [SerializeField] float _delayAttack = 0.1f;
        [SerializeField] float _delayReload = 2f;
        [SerializeField] float _ammoSpeed = 20f;

        [Header("Accuracy")]
        [SerializeField] float _nearDistance;
        [SerializeField] float _farDistance;
        [SerializeField] float _nearSpreadRadius;
        [SerializeField] float _farSpreadRadius;
 

        Coroutine _muzzleFlashRoutine;
        int _currentAmmo;
        float _nextAttackTime;
        bool _isReloading;

        public AimType AimType { get { return AimType.None; } }  
        public WeaponType WeaponType { get { return WeaponType.AR; } }
        public int Damage { get { return _damage; } }
        public float AttackRange { get { return _attackRange; } }
        public int Ammo { get { return _currentAmmo; } }
        public bool IsReloading {  get { return _isReloading; } }

        //Needed for Terrorist Animation
        public float FireInterval => Mathf.Max(_delayAttack, 0.01f);
       

        void Start()
        {
            SetMuzzleFlash(false);
            _currentAmmo = _maxAmmo; 
        }

        public void Attack(Vector3 targetPosition)
        {
            if (_isReloading || Time.time < _nextAttackTime) { return; }

            if (_currentAmmo <= 0)
            {
                StartCoroutine(ReloadRoutine()); 
                return;
            }
            if (_bulletPool == null || _muzzle == null) { return; }

            //Vector3 direction = (targetPosition + _targetOffset - _muzzle.position).normalized; 


            //명중률 구현
            Vector3 targetPoint = targetPosition + _targetOffset;
            Vector3 toTarget = targetPoint - _muzzle.position;
            float distance = toTarget.magnitude;

            //거리에 구간을 나누어 니어 보다 아래라면 0, far보다 넘어가면 1로 보고, 사이에 값을 나누는 메서드
            float t = Mathf.InverseLerp(_nearDistance, _farDistance, distance);
            float radius = Mathf.Lerp(_nearSpreadRadius, _farSpreadRadius, t);

            //총구가 바라보는 방향에서 수직인 평면 위쪽으로 조준점을 흩뜨림.
            Vector3 forward = toTarget.normalized;
            Vector3 referenceUp = Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.99f ? Vector3.right : Vector3.up;

            //두 벡터의 외적값을 구해서 새로운 벡터를 반환함
            Vector3 right = Vector3.Cross(referenceUp, forward).normalized;
            Vector3 up = Vector3.Cross(forward, right).normalized;

            //랜덤하게 원 안에서 좌표를 정함
            Vector2 spread = UnityEngine.Random.insideUnitCircle * radius;

            Vector3 aimPoint = targetPoint + right * spread.x + up * spread.y;
            Vector3 direction = (aimPoint - _muzzle.position).normalized;



            if(Physics.Linecast(_muzzle.position, targetPoint, _obstacleMask, QueryTriggerInteraction.Ignore))
            {
                Debug.Log("Player가 벽에 가려져있습니다. 사격하지 않습니다");
                return;
            }

            GameObject bullet = _bulletPool.GetBullet(_muzzle.position, Quaternion.LookRotation(direction));

            if (bullet == null) {  return; }

            Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();
            if (bulletRigidbody == null)
            {
                _bulletPool.ReturnBullet(bullet);
                return;
            }

            Debug.Log("사격");

            bulletRigidbody.useGravity = false; 
            bulletRigidbody.linearVelocity = direction * _ammoSpeed;

            _currentAmmo--;

            _weaponRecoil?.RecoilFromEnemyWeapon();

            _nextAttackTime = Time.time + Mathf.Max(_delayAttack, 0.01f); 
            PlayMuzzleFlash();

            //Needed for Animation
            _firedAction?.Invoke();
        }

        void PlayMuzzleFlash()
        {
            if (_muzzleFlash == null) { return; }

            if (_muzzleFlashRoutine != null) { StopCoroutine(_muzzleFlashRoutine); }
            _muzzleFlashRoutine = StartCoroutine(MuzzleFlashRoutine());
        }

        IEnumerator MuzzleFlashRoutine()
        {
            SetMuzzleFlash(true);
            yield return new WaitForSeconds(_muzzleFlashDuration);
            SetMuzzleFlash(false);
            _muzzleFlashRoutine = null;
        }

        void SetMuzzleFlash(bool isActive)
        {
            if (_muzzleFlash != null) { _muzzleFlash.SetActive(isActive); } 
        }

        IEnumerator ReloadRoutine()
        {
            //Debug.Log("Called ReloadRoutine Func"); 
            _isReloading = true; 
            yield return new WaitForSeconds(_delayReload);
            _currentAmmo = _maxAmmo;
            _isReloading = false;
        }
    }
}

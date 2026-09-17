using System.Collections;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class SentryEnemyAttack : MonoBehaviour
    {
        [Header("Attack Variable")]
        [SerializeField] int _damage;
        [SerializeField] float _attackRange;
        [SerializeField] int _ammo;
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

        float _nextAttackTime;
        bool _isReloading;
        Coroutine _muzzleFlashRoutine;

        public AimType AimType { get { return AimType.None; } }  
        public WeaponType WeaponType { get { return WeaponType.AR; } }
        public int Damage { get { return _damage; } }
        public float AttackRange { get { return _attackRange; } }
        public int Ammo { get { return _ammo; } }

        void Start()
        {
            SetMuzzleFlash(false); 
            _ammo = _maxAmmo; 
        }

        public void Attack(Vector3 targetPosition)
        {
            if (_isReloading || Time.time < _nextAttackTime) { return; }

            if (_ammo <= 0)
            {
                StartCoroutine(ReloadRoutine()); 
                return;
            }
            if (_bulletPool == null || _muzzle == null) { return; }

            Vector3 direction = (targetPosition + _targetOffset - _muzzle.position).normalized; 
            GameObject bullet = _bulletPool.GetBullet(_muzzle.position, Quaternion.LookRotation(direction));

            if (bullet == null) { Debug.Log("탄환 풀 비었음 — 반환 확인 필요"); return; }

            Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();
            if (bulletRigidbody == null)
            {
                _bulletPool.ReturnBullet(bullet);
            }

            bulletRigidbody.useGravity = false; 
            bulletRigidbody.linearVelocity = direction * _ammoSpeed;

            _ammo--;
            _nextAttackTime = Time.time + Mathf.Max(_delayAttack, 0.01f); 
            PlayMuzzleFlash();
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
            Debug.Log("Called ReloadRoutine Func"); 
            _isReloading = true; 
            yield return new WaitForSeconds(_delayReload); 
            _ammo = _maxAmmo;
            _isReloading = false;
        }
    }
}

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

        public AimType aimType => AimType.None;
        public WeaponType weaponType => WeaponType.AR;
        public int Damage => _damage;
        public float AttackRange => _attackRange;
        public int Ammo => _ammo;

        void Start()
        {
            _ammo = _maxAmmo; // 게임 시작 시 탄창을 가득 채웁니다. By Codex
            SetMuzzleFlash(false); // 시작할 때 머즐 플래시가 남아 있지 않게 합니다. By Codex
        }

        public void Attack(Vector3 targetPosition)
        {
            if (_isReloading || Time.time < _nextAttackTime) { return; } // 재장전과 연사 간격 중에는 중복 발사를 막습니다. By Codex

            if (_ammo <= 0)
            {
                StartCoroutine(ReloadRoutine()); // 탄약이 없을 때 재장전을 한 번만 시작합니다. By Codex
                return;
            }

            if (_bulletPool == null || _muzzle == null) { return; } // 풀 또는 머즐이 없으면 안전하게 발사를 중단합니다. By Codex

            Vector3 direction = (targetPosition + _targetOffset - _muzzle.position).normalized; // 현재 플레이어 위치를 향하는 발사 방향을 계산합니다. By Codex
            GameObject bullet = _bulletPool.GetBullet(_muzzle.position, Quaternion.LookRotation(direction)); // 새로 생성하지 않고 풀에서 탄환을 가져옵니다. By Codex

            if (bullet == null) { return; } // 풀에 사용 가능한 탄환이 없으면 이번 발사를 건너뜁니다. By Codex

            Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>(); // 풀에서 가져온 탄환의 Rigidbody를 찾습니다. By Codex
            if (bulletRigidbody == null)
            {
                _bulletPool.ReturnBullet(bullet); // 사용할 수 없는 탄환은 즉시 풀에 반환합니다. By Codex
                return;
            }

            bulletRigidbody.useGravity = false; // 탄환이 중력으로 떨어지지 않고 조준 방향으로 날아가게 합니다. By Codex
            bulletRigidbody.linearVelocity = direction * _ammoSpeed; // 설정한 탄속으로 탄환을 발사합니다. By Codex

            _ammo--; // 실제 발사에 성공했을 때만 탄약을 차감합니다. By Codex
            _nextAttackTime = Time.time + Mathf.Max(_delayAttack, 0.01f); // 다음 연사 가능 시간을 저장합니다. By Codex
            PlayMuzzleFlash(); // 발사 성공 시 머즐 플래시를 잠시 표시합니다. By Codex
        }

        void PlayMuzzleFlash()
        {
            if (_muzzleFlash == null) { return; } // 머즐 플래시가 연결되지 않아도 공격은 계속 가능하게 합니다. By Codex

            if (_muzzleFlashRoutine != null) { StopCoroutine(_muzzleFlashRoutine); } // 이전 플래시 종료 대기를 중복 실행하지 않게 합니다. By Codex
            _muzzleFlashRoutine = StartCoroutine(MuzzleFlashRoutine()); // 플래시 표시 시간을 관리하는 코루틴을 실행합니다. By Codex
        }

        IEnumerator MuzzleFlashRoutine()
        {
            SetMuzzleFlash(true); // 발사 순간 플래시를 활성화합니다. By Codex
            yield return new WaitForSeconds(_muzzleFlashDuration); // 설정한 짧은 시간 동안 플래시를 유지합니다. By Codex
            SetMuzzleFlash(false); // 표시 시간이 끝나면 플래시를 다시 숨깁니다. By Codex
            _muzzleFlashRoutine = null; // 코루틴 실행이 끝났음을 기록합니다. By Codex
        }

        void SetMuzzleFlash(bool isActive)
        {
            if (_muzzleFlash != null) { _muzzleFlash.SetActive(isActive); } // 필드에 연결한 플래시 오브젝트를 재사용합니다. By Codex
        }

        IEnumerator ReloadRoutine()
        {
            _isReloading = true; // BT가 매 프레임 Attack을 호출해도 재장전은 한 번만 진행합니다. By Codex
            yield return new WaitForSeconds(_delayReload); // 설정한 재장전 시간을 기다립니다. By Codex
            _ammo = _maxAmmo; // 대기가 끝나면 탄창을 가득 채웁니다. By Codex
            _isReloading = false; // 재장전 완료 후 다시 발사할 수 있게 합니다. By Codex
        }
    }
}

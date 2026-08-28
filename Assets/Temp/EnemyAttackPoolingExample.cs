using UnityEngine;

namespace MIssionOfMercenary.PoolingExample
{
    /// <summary>
    /// 기존 EnemyAttack.OnFire()를 풀링 방식으로 바꾼 참고 예제입니다.
    /// 현재 EnemyAttack과 동시에 붙이지 말고 코드 비교용으로 사용하세요.
    /// </summary>
    public class EnemyAttackPoolingExample : MonoBehaviour
    {
        [SerializeField] EnemyBulletPoolExample _bulletPool;
        [SerializeField] Transform _muzzle;
        [SerializeField] Transform _target;
        [SerializeField] float _bulletSpeed = 30f;

        public void OnFire()
        {
            if (_bulletPool == null || _muzzle == null || _target == null)
            {
                return;
            }

            Vector3 targetPosition = _target.position + Vector3.up;
            Vector3 direction = (targetPosition - _muzzle.position).normalized;

            PooledEnemyBulletExample bullet = _bulletPool.Get(_muzzle.position, _muzzle.rotation);
            if (bullet != null)
            {
                bullet.Launch(direction * _bulletSpeed);
            }
        }
    }
}

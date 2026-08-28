using System.Collections.Generic;
using UnityEngine;

namespace MIssionOfMercenary.PoolingExample
{
    /// <summary>
    /// Enemy 탄환을 미리 만들어 두고 반복해서 빌려주는 간단한 오브젝트 풀입니다.
    /// 씬의 빈 GameObject에 붙인 뒤 Inspector에서 Bullet Prefab을 지정하세요.
    /// </summary>
    public class EnemyBulletPoolExample : MonoBehaviour
    {
        [SerializeField] PooledEnemyBulletExample _bulletPrefab;
        [SerializeField, Min(1)] int _initialSize = 20;
        [SerializeField] bool _canExpand = true;

        readonly Queue<PooledEnemyBulletExample> _availableBullets = new Queue<PooledEnemyBulletExample>();

        void Awake()
        {
            if (_bulletPrefab == null)
            {
                Debug.LogError("EnemyBulletPoolExample: Bullet Prefab이 지정되지 않았습니다.", this);
                enabled = false;
                return;
            }

            for (int i = 0; i < _initialSize; i++)
            {
                CreateBullet();
            }
        }

        PooledEnemyBulletExample CreateBullet()
        {
            PooledEnemyBulletExample bullet = Instantiate(_bulletPrefab, transform);
            bullet.Initialize(this);
            bullet.gameObject.SetActive(false);
            _availableBullets.Enqueue(bullet);
            return bullet;
        }

        public PooledEnemyBulletExample Get(Vector3 position, Quaternion rotation)
        {
            if (_availableBullets.Count == 0)
            {
                if (!_canExpand)
                {
                    return null;
                }

                CreateBullet();
            }

            PooledEnemyBulletExample bullet = _availableBullets.Dequeue();
            bullet.transform.SetPositionAndRotation(position, rotation);
            bullet.gameObject.SetActive(true);
            return bullet;
        }

        public void Release(PooledEnemyBulletExample bullet)
        {
            if (bullet == null || !bullet.gameObject.activeSelf)
            {
                return;
            }

            bullet.gameObject.SetActive(false);
            bullet.transform.SetParent(transform);
            _availableBullets.Enqueue(bullet);
        }
    }
}

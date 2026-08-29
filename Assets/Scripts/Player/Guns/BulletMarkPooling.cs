using System.Collections.Generic;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class BulletMarkPooling : MonoBehaviour
    {
        readonly Queue<GameObject> _prefabBulletMark = new Queue<GameObject>();

        [SerializeField] GameObject _bulletMark;
        [SerializeField] int _totalCount;

        void Awake()
        {
            CreateBulletMark();
        }

        void CreateBulletMark()
        {
            for (int i = 0; i < _totalCount; i++)
            {
                GameObject bulletMark = Instantiate(_bulletMark, transform);
                BulletMarkPool bulletMarkPool = bulletMark.GetComponent<BulletMarkPool>();

                if (bulletMarkPool == null)
                {
                    bulletMarkPool = bulletMark.AddComponent<BulletMarkPool>();
                }

                bulletMarkPool.SetPool(this);
                bulletMark.SetActive(false);
                _prefabBulletMark.Enqueue(bulletMark);
            }
        }

        public void GetBulletMark(Vector3 position, Quaternion rotation)
        {
            if (_prefabBulletMark.Count == 0)
            {
                return;
            }

            GameObject mark = _prefabBulletMark.Dequeue();

            mark.transform.SetParent(null); //이렇게 해야 안따라옴
            mark.transform.SetPositionAndRotation(position, rotation);
            mark.SetActive(true);
            mark.GetComponent<BulletMarkPool>().BeginLifeTime();
        }

        public void ReturnBulletMark(GameObject bulletMark)
        {
            // 이미 큐에 반환된 오브젝트가 중복으로 들어가는 것을 막는다.
            if (!bulletMark.activeSelf)
            {
                return;
            }

            bulletMark.SetActive(false);
            bulletMark.transform.SetParent(transform);
            _prefabBulletMark.Enqueue(bulletMark);
        }
    }
}

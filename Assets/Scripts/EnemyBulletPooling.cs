using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyBulletPooling : MonoBehaviour
    {
        Queue<GameObject> _prefabPool = new Queue<GameObject>();

        [SerializeField] GameObject _bulletEnemy;
        [SerializeField] int _poolingCount = 20;

        private void Awake()
        {
            CreateBullets();
        }


        void CreateBullets()
        {
            for (int i = 0;i < _poolingCount;i++)
            {
                GameObject bullet = Instantiate(_bulletEnemy, transform);
                bullet.SetActive(false);
                _prefabPool.Enqueue(bullet);
            }
        }

        public GameObject GetBullet(Vector3 position, Quaternion rotation)
        {
            if (_prefabPool.Count == 0) { return null; }

            GameObject bullet = _prefabPool.Dequeue();

            bullet.transform.SetLocalPositionAndRotation(position, rotation); //뺀 총알을 머즐위치로 이동,월드 좌표니까 월드로 이동해야 더 적합
            bullet.SetActive(true);

            return bullet;
        }

        public void ReturnBullet(GameObject bullet)
        {
            Rigidbody rb = bullet.GetComponent<Rigidbody>();

            if(rb != null) 
            { 
                rb.linearVelocity = Vector3.zero;// 사용이 끝난 총알을 처리하는것. 가속도를 0으로 처리
                rb.angularVelocity = Vector3.zero;
            }

            bullet.SetActive(false);
            bullet.transform.SetParent(transform);//이 스크립트가 풀링오브젝트를 관리하는 스크립트이므로 쓴 총알은 다시 이 스크립트의 오브젝트로 정리
            _prefabPool.Enqueue(bullet);
        }
    }
}

using System.Collections;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyBullet : MonoBehaviour
    {
        [SerializeField] EnemyBulletPooling _enemyBulletPooling;
        [SerializeField] float _delay = 5.0f;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            StartCoroutine(DestroyBulletRoutine());
        }

        public void SetPool(EnemyBulletPooling pool)
        {
            _enemyBulletPooling = pool;
        }

        IEnumerator DestroyBulletRoutine()
        {
            yield return new WaitForSeconds(_delay);
            Destroy(this.gameObject);

            Debug.Log("Enemy Bullet Destroyed");
        }

        private void OnCollisionEnter(Collision collision)
        {
            //데미지 주는것도 여기서 할까?????

            Destroy(this.gameObject);

            if(collision.gameObject.layer == 20)
            {
                Debug.Log("******************Player Hit******************");
            }
        }
    }
}

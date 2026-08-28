using System.Collections;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyBullet : MonoBehaviour
    {
        EnemyBulletPooling _enemyBulletPooling;
        Coroutine _returnRoutine;

        [SerializeField] float _lifeTime = 3f;

        private void OnEnable()
        {
            _returnRoutine = StartCoroutine(ReturnAfterTime());
        }

        public void SetPool(EnemyBulletPooling pool)
        {
            _enemyBulletPooling = pool;
        }

        private IEnumerator ReturnAfterTime()
        {
            yield return new WaitForSeconds(_lifeTime);
            _enemyBulletPooling.ReturnBullet(gameObject);
        }


        private void OnCollisionEnter(Collision collision)
        {
            if(collision.gameObject.layer == 20)
            {
                Debug.Log("******************Player Hit******************");
            }

            //탄환에 붙어있으므로 이오브젝트 자체를 다시 큐에 넣음
            _enemyBulletPooling.ReturnBullet(this.gameObject);
        }
    }
}

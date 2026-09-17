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
            Debug.Log("탄환 활성화 — 반환 타이머 시작");

            _returnRoutine = StartCoroutine(ReturnAfterTime());
        }

        public void SetPool(EnemyBulletPooling pool)
        {
            _enemyBulletPooling = pool;
        }

        private IEnumerator ReturnAfterTime()
        {
            yield return new WaitForSeconds(_lifeTime);

            Debug.Log("탄환 수명 종료 — 풀 반환");
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

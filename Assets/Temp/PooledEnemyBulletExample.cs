using System.Collections;
using UnityEngine;

namespace MIssionOfMercenary.PoolingExample
{
    /// <summary>
    /// Destroy 대신 자신을 EnemyBulletPool에 반환하는 탄환입니다.
    /// 기존 EnemyBullet 대신 탄환 프리팹에 붙여 사용하세요.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PooledEnemyBulletExample : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] float _lifeTime = 5f;

        EnemyBulletPoolExample _pool;
        Rigidbody _rigidbody;
        Coroutine _lifeTimeRoutine;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void Initialize(EnemyBulletPoolExample pool)
        {
            _pool = pool;
        }

        public void Launch(Vector3 velocity)
        {
            _rigidbody.useGravity = false;
            _rigidbody.linearVelocity = velocity;
            _rigidbody.angularVelocity = Vector3.zero;

            if (_lifeTimeRoutine != null)
            {
                StopCoroutine(_lifeTimeRoutine);
            }

            _lifeTimeRoutine = StartCoroutine(ReturnAfterDelay());
        }

        IEnumerator ReturnAfterDelay()
        {
            yield return new WaitForSeconds(_lifeTime);
            Despawn();
        }

        public void Despawn()
        {
            if (_pool == null)
            {
                gameObject.SetActive(false);
                return;
            }

            _pool.Release(this);
        }

        void OnCollisionEnter(Collision collision)
        {
            // 실제 데미지 처리는 기존 PlayerHealth 등 프로젝트의 피격 코드에서 담당합니다.
            Despawn();
        }

        void OnDisable()
        {
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }

            if (_lifeTimeRoutine != null)
            {
                StopCoroutine(_lifeTimeRoutine);
                _lifeTimeRoutine = null;
            }
        }
    }
}

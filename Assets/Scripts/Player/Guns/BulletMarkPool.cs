using System.Collections;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class BulletMarkPool : MonoBehaviour
    {
        BulletMarkPooling _pool;

        [SerializeField] float _lifeTime = 3f;
        Coroutine _coroutine;

        public void SetPool(BulletMarkPooling pool)
        {
            _pool = pool;
        }

        public void BeginLifeTime()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
            }

            _coroutine = StartCoroutine(ReturnAfterTime());
        }

        IEnumerator ReturnAfterTime()
        {
            yield return new WaitForSeconds(_lifeTime);
            _coroutine = null;
            _pool.ReturnBulletMark(gameObject);
        }

        void OnDisable()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }
    }
}

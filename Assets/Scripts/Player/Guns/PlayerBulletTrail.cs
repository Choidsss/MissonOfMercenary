using UnityEngine;

namespace MIssionOfMercenary
{
    public class PlayerBulletTrail : MonoBehaviour
    {
        PlayerBulletTrailPooling _pool;
        Vector3 _direction;
        float _speed;
        float _remainingDistance;
        bool _isMoving;

        public void SetPool(PlayerBulletTrailPooling pool)
        {
            _pool = pool;
        }

        public void Launch(Vector3 direction, float distance, float speed)
        {
            _direction = direction.normalized;
            _speed = speed;
            _remainingDistance = distance;
            _isMoving = true;

            foreach (TrailRenderer trailRenderer in GetComponentsInChildren<TrailRenderer>(true))
            {
                trailRenderer.Clear();
            }
        }

        void Update()
        {
            if (!_isMoving)
            {
                return;
            }

            float moveDistance = Mathf.Min(_speed * Time.deltaTime, _remainingDistance);
            transform.position += _direction * moveDistance;
            _remainingDistance -= moveDistance;

            if (_remainingDistance <= 0f)
            {
                _pool.ReturnTrail(gameObject);
            }
        }

        public void StopTrail()
        {
            _isMoving = false;

            foreach (TrailRenderer trailRenderer in GetComponentsInChildren<TrailRenderer>(true))
            {
                trailRenderer.Clear();
            }
        }
    }
}

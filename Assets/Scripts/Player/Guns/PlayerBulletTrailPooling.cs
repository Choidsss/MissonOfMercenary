using System.Collections.Generic;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class PlayerBulletTrailPooling : MonoBehaviour
    {
        readonly Queue<GameObject> _trailPool = new Queue<GameObject>();

        [SerializeField] GameObject _trailPrefab;
        [SerializeField] int _poolingCount = 30;

        void Awake()
        {
            CreateTrail();
        }

        void CreateTrail()
        {
            for (int i = 0; i < _poolingCount; i++)
            {
                GameObject trailObject = Instantiate(_trailPrefab, transform);
                PlayerBulletTrail trail = trailObject.GetComponent<PlayerBulletTrail>();

                if (trail == null)
                {
                    trail = trailObject.AddComponent<PlayerBulletTrail>();
                }

                trail.SetPool(this);
                trailObject.SetActive(false);
                _trailPool.Enqueue(trailObject);
            }
        }


        public void PlayTrail(Vector3 position, Vector3 direction, float distance, float speed)
        {
            if (_trailPool.Count == 0)
            {
                return;
            }

            GameObject trailObject = _trailPool.Dequeue();
            trailObject.transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction));
            trailObject.SetActive(true);
            trailObject.GetComponent<PlayerBulletTrail>().Launch(direction, distance, speed);
        }

        public void ReturnTrail(GameObject trailObject)
        {
            PlayerBulletTrail trail = trailObject.GetComponent<PlayerBulletTrail>();
            trail.StopTrail();
            trailObject.SetActive(false);
            trailObject.transform.SetParent(transform);
            _trailPool.Enqueue(trailObject);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class PlayerBulletTrailPooling : MonoBehaviour
    {
        readonly Queue<GameObject> _trailPool = new Queue<GameObject>();
        readonly HashSet<GameObject> _activeTrail = new HashSet<GameObject>();

        [SerializeField] GameObject _trailPrefab;
        [SerializeField] int _poolingCount = 30;

        void Awake()
        {
            CreateTrail();
        }

        private void OnDestroy()
        {
            foreach (GameObject trailObject in _activeTrail)
            {
                if (trailObject != null)
                {
                    Destroy(trailObject);
                }
            }

            _activeTrail.Clear();
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
            if (_trailPool.Count == 0) { return; }
            if(direction.sqrMagnitude < 0.000001f || distance <= 0f || speed <= 0f) { return; }

            GameObject trailObject = _trailPool.Dequeue();

            trailObject.transform.SetParent(null, true);//자식오브젝트가 아닌 좌표를 월드기준으로 놓아 독립
            trailObject.transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction));//부모의 자식으로 생성하기에, 이동의 영향을 받는다

            _activeTrail.Add(trailObject);

            trailObject.SetActive(true);
            trailObject.GetComponent<PlayerBulletTrail>().Launch(direction, distance, speed);
        }

        public void ReturnTrail(GameObject trailObject)
        {
            if (trailObject == null || !_activeTrail.Remove(trailObject)){ return; }

            PlayerBulletTrail trail = trailObject.GetComponent<PlayerBulletTrail>();

            trail.StopTrail();
            trailObject.SetActive(false);
            trailObject.transform.SetParent(transform,true);
            _trailPool.Enqueue(trailObject);
        }
    }
}

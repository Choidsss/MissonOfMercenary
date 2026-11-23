using UnityEngine;

namespace MIssionOfMercenary
{
    [RequireComponent(typeof(BoxCollider))]
    public class CombatStartManager : MonoBehaviour
    {
        [SerializeField] AStarEnemySpawner _spawner;

        // 이 트리거를 밟으면 활성화할 스폰 지점들
        [SerializeField] Transform[] _spawnPositions;

        // 플레이어의 최상위 Transform을 연결
        [SerializeField] Transform _playerRoot;

        public bool HasStarted { get; private set; }

        void Reset()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log("충돌 성공");

            if (HasStarted || _spawner == null || _playerRoot == null)
            {
                Debug.Log("Null 확인1");
                return;
            }
                

            // 플레이어 본체 또는 자식 Collider인지 확인
            Transform entered = collision.transform;
            if (entered != _playerRoot && !entered.IsChildOf(_playerRoot))
            {
                Debug.Log("Null 확인2");
                return;
            }
                

            if (_spawnPositions == null)
            {
                Debug.Log("Null 확인3");
                return;
            }
                

            // Updated: clear previous trigger spawn points before enabling this group.
            _spawner.DisableAllSpawnPositions();

            bool activated = false;

            foreach (Transform position in _spawnPositions)
            {
                if (_spawner.SetSpawnPositionEnabled(position, true))
                    activated = true;
            }

            if (!activated)
                return;

            // 플레이어의 다른 Collider가 들어와도 다시 실행하지 않음
            HasStarted = true;

            Debug.Log("EnemySpawnOne() 실행");
            _spawner.EnemySpawnOne();
        }
    }
}

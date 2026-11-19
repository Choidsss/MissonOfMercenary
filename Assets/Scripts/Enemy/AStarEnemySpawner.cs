using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    // *** 추가: 지정한 위치에 적을 생성하고 공용 A* 격자로 목표까지 이동시킨다.
    public class AStarEnemySpawner : MonoBehaviour
    {
        [SerializeField] AStarGrid _grid;
        [SerializeField] NavMeshAgent _enemyPrefab;
        [SerializeField] Transform _target;
        [SerializeField] Transform _spawnPoint;
        [SerializeField, Min(1)] int _totalCount = 1;
        [SerializeField, Min(0.1f)] float _spawnInterval = 2f;
        [SerializeField, Min(0.01f)] float _spawnSampleDistance = 1f;
        [SerializeField] bool _spawnOnStart = true;

        int _spawnedCount;

        IEnumerator Start()
        {
            if (!_spawnOnStart) yield break;
            while (_spawnedCount < _totalCount)
            {
                // 실패 시 매 프레임 생성/로그를 반복하지 않고 중단한다.
                if (SpawnOne() == null) yield break;
                yield return new WaitForSeconds(Mathf.Max(0.1f, _spawnInterval));
            }
        }

        [ContextMenu("Spawn One")]
        void SpawnFromInspector()
        {
            if (Application.isPlaying) SpawnOne();
        }

        public AStarEnemyMover SpawnOne()
        {
            if (!Application.isPlaying || _spawnedCount >= _totalCount) return null;
            if (_grid == null || _enemyPrefab == null || _target == null)
            {
                Debug.LogWarning("Grid, Enemy Prefab, Target을 연결해 주세요.", this);
                return null;
            }

            Transform point = _spawnPoint != null ? _spawnPoint : transform;
            NavMeshQueryFilter filter = new NavMeshQueryFilter
            {
                agentTypeID = _enemyPrefab.agentTypeID,
                areaMask = _enemyPrefab.areaMask
            };
            if (!NavMesh.SamplePosition(point.position, out NavMeshHit hit,
                Mathf.Max(0.01f, _spawnSampleDistance), filter))
            {
                Debug.LogWarning("스폰 위치 주변에 적에게 맞는 NavMesh가 없습니다.", this);
                return null;
            }

            NavMeshAgent enemy = Instantiate(_enemyPrefab, hit.position, point.rotation);
            AStarEnemyMover mover = enemy.GetComponent<AStarEnemyMover>();
            if (mover == null) mover = enemy.gameObject.AddComponent<AStarEnemyMover>();

            if (!mover.MoveTo(_grid, _target.position))
            {
                Debug.LogWarning("스폰된 적의 경로를 찾지 못했습니다. 스폰/목표 셀을 확인해 주세요.", this);
                Destroy(enemy.gameObject);
                return null;
            }

            _spawnedCount++;
            return mover;
        }
    }
}

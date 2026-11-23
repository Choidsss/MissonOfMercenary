using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    //적 종류 찾기
    public enum EnemyType
    {
        Terrorrist1,
        Terrorrist2,
        Terrorrist3
    }

    //스폰된 적이 가지고 있는 것들
    public class SpawnedEnemy
    {
        EnemyType _enemyType; // 종류
        AStarEnemyMover _mover; // 움직이는 스크립트
        int _coveredIndex; // 엄폐 위치에 대한 인덱스 정보

        public EnemyType TypeOfEnemy { get { return _enemyType; } }
        public AStarEnemyMover Mover { get { return _mover; } }
        public int CoveredIndex { get { return _coveredIndex; } }

        //생성자
        public SpawnedEnemy(EnemyType type, AStarEnemyMover mover, int coveredIndex)
        {
            _enemyType = type;
            _mover = mover;
            _coveredIndex = coveredIndex;
        }
    }

    public class AStarEnemySpawner : MonoBehaviour
    {
        private List<SpawnedEnemy> _spawnedEnemies = new List<SpawnedEnemy>(); // 스폰된 적 관리
        private Dictionary<Transform, bool> _spawnPositionDic = new Dictionary<Transform, bool>();

        [Header("Enemy Move To Cover Position")]
        [SerializeField]
        private List<Transform> EnemyCoverPosition = new List<Transform>();

        [SerializeField] NavMeshAgent[] _enemyPrefabs;
        [SerializeField] AStarGrid _grid;
        [SerializeField, Min(1)] int _totalCount = 1;
        [SerializeField, Min(0.1f)] float _spawnInterval = 2f;
        [SerializeField, Min(0.01f)] float _spawnSampleDistance = 1f;
        [SerializeField] bool _spawnOnStart = true;

        [Header("Spawn Position List")]
        [SerializeField] Transform[] _spawnPositionList;

        bool[] _didCovered;

        int _spawnedCount;

        private void Awake()
        {
            _didCovered = new bool[EnemyCoverPosition.Count];
            AddSpawnPositionToDictionary();
        }

        void AddSpawnPositionToDictionary()
        {
            // 수정: 비어 있거나 중복된 스폰 지점으로 초기화가 실패하지 않도록 처리.
            if (_spawnPositionList == null) return;
            for(int i = 0;i < _spawnPositionList.Length;i++)
            {
                Transform point = _spawnPositionList[i];
                if (point != null) _spawnPositionDic[point] = false;
            }
        }

        //비어있는 위치 찾아서 숫자로 반환
        int FindEmptySpawnIndex()
        {
            for (int i = 0;i < EnemyCoverPosition.Count; i++)
            {
                if (!_didCovered[i] && EnemyCoverPosition[i] != null)
                {
                    return i;
                }
            }

            return -1;
        }

        //적이 스폰될 때마다 비어있는 위치의 리스트를 받아와서 0번부터 순차적으로 할당
        public void EnemySpawnOne()
        {
            //빈자리 찾은거 가져오고
            int reserve = FindEmptySpawnIndex();

            if(reserve < 0)
            {
                Debug.Log("빈 자리를 찾을 수 없습니다");
                return;
            }

            if (!TryGetSpawnPosition(out Vector3 spawnPosition))
            {
                Debug.Log("!TryGetSpawnPosition Exception");
                return;
            }

            // 수정: 프리팹 배열이 없거나 비어 있으면 랜덤 선택 전에 중단.
            if (_enemyPrefabs == null || _enemyPrefabs.Length == 0) return;
            int index = Random.Range(0, _enemyPrefabs.Length);

            if(_grid == null || _enemyPrefabs[index] == null)
            {
                Debug.Log("AStatGrid 혹은 NavMeshAgent가 없습니다");
                return;
            }

            // 수정: 선택한 적의 NavMesh에서 생성 가능한 좌표를 확인한 뒤 엄폐 자리 예약.
            NavMeshQueryFilter filter = new NavMeshQueryFilter
            {
                agentTypeID = _enemyPrefabs[index].agentTypeID,
                areaMask = _enemyPrefabs[index].areaMask
            };
            if (!NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit,
                Mathf.Max(0.01f, _spawnSampleDistance), filter)) return;
            spawnPosition = hit.position;
            _didCovered[reserve] = true;

            Debug.Log("생성 전 로그");
            //적의 NavMeshAgent를 복사해서 적을 생성
            NavMeshAgent enemy = Instantiate(_enemyPrefabs[index], spawnPosition, Quaternion.identity);
            Debug.Log("생성 후 로그");


            AStarEnemyMover mover = enemy.GetComponent<AStarEnemyMover>();



            if(mover == null || !mover.MoveTo(_grid, EnemyCoverPosition[reserve].position))
            {
                Debug.Log("무버 false");
                _didCovered[reserve] = false;
                Destroy(enemy.gameObject);
                return;
            }

            EnemyType type = (EnemyType)index;

            SpawnedEnemy spawnedEnemy = new SpawnedEnemy(type, mover, reserve);
            // 수정: 위에서 성공한 이동 요청을 중복 호출하지 않도록 제거.
            
            _spawnedEnemies.Add(spawnedEnemy);
        }

        bool TryGetSpawnPosition(out Vector3 position)
        {
            position = Vector3.zero;

            if (_spawnPositionList == null)
            {
                return false;
            }

            foreach (Transform point in _spawnPositionList)
            {
                if (point == null)
                {
                    continue;
                }

                if (_spawnPositionDic.TryGetValue(point, out bool enable) && enable)
                {
                    position = point.position;
                    // 수정: 터레인 높이에 맞춰 요청한 스폰 기준 Y 좌표를 유지.
                    position.y = 38f;
                    return true;
                }
            }

            return false;
        }

        // 수정: 새 트리거의 지점을 활성화하기 전에 이전 지점의 허용 상태 초기화.
        public void DisableAllSpawnPositions()
        {
            var positions = new List<Transform>(_spawnPositionDic.Keys);
            foreach (Transform position in positions)
                _spawnPositionDic[position] = false;
        }

        //Dic의 bool 값 true시키는 역할
        public bool SetSpawnPositionEnabled(Transform position, bool enabled)
        {
            if (position == null || !_spawnPositionDic.ContainsKey(position))
            {
                Debug.LogWarning("스포너에 등록되지 않은 스폰 지점입니다.", this);
                return false;
            }

            _spawnPositionDic[position] = enabled;
            return true;
        }
    }
}

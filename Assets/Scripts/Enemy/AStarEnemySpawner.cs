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

        [Header("For Find Player")]
        [SerializeField]
        private Transform _player;

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

        int _currentSpawnCount;
        bool _isSpawning;

        private void Awake()
        {
            _didCovered = new bool[EnemyCoverPosition != null ? EnemyCoverPosition.Count : 0];
            AddSpawnPositionToDictionary();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            _isSpawning = false;
        }

        public void StartSpawning()
        {
            if(_isSpawning || _currentSpawnCount >= _totalCount)
            {
                Debug.Log("StartSpawning() 에러");
                return;
            }

            StartCoroutine(SpawnOneRoutine());
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
            if (EnemyCoverPosition == null || _didCovered == null) return -1;
            for (int i = 0;i < EnemyCoverPosition.Count; i++)
            {
                if (i >= _didCovered.Length) return -1;
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
            // 수정: 연결 누락과 초기화 상태를 구분해서 스폰 실패 원인을 출력.
            if (_grid == null)
            {
                Debug.LogWarning("[Spawner] 스폰 중단: AStarGrid가 연결되지 않았습니다.", this);
                return;
            }
            if (EnemyCoverPosition == null || EnemyCoverPosition.Count == 0)
            {
                Debug.LogWarning("[Spawner] 스폰 중단: 엄폐 위치가 등록되지 않았습니다.", this);
                return;
            }
            if (_didCovered == null || _didCovered.Length != EnemyCoverPosition.Count)
            {
                Debug.LogWarning("[Spawner] 스폰 중단: 엄폐 예약 배열이 초기화되지 않았거나 실행 중 엄폐 목록 크기가 변경됐습니다.", this);
                return;
            }
            //빈자리 찾은거 가져오고
            int reserve = FindEmptySpawnIndex();

            if(reserve < 0)
            {
                Debug.LogWarning("[Spawner] 스폰 중단: 비어 있는 유효한 엄폐 위치가 없습니다.", this);
                return;
            }

            if (!TryGetSpawnPosition(out Vector3 spawnPosition))
            {
                return;
            }

            // 수정: 프리팹 배열이 없거나 비어 있으면 랜덤 선택 전에 중단.
            if (_enemyPrefabs == null || _enemyPrefabs.Length == 0)
            {
                Debug.LogWarning("[Spawner] 스폰 중단: 적 프리팹 배열이 비어 있습니다.", this);
                return;
            }
            int index = Random.Range(0, _enemyPrefabs.Length);

            if(_enemyPrefabs[index] == null)
            {
                Debug.LogWarning($"[Spawner] 스폰 중단: 프리팹 배열의 {index}번 항목이 비어 있습니다.", this);
                return;
            }

            if (_currentSpawnCount >= _totalCount)
            {
                Debug.LogWarning("이미 설정했던 만큼 적을 생성하였습니다");
                return;
            }

            // 수정: 선택한 적의 NavMesh에서 생성 가능한 좌표를 확인한 뒤 엄폐 자리 예약.
            NavMeshQueryFilter filter = new NavMeshQueryFilter
            {
                agentTypeID = _enemyPrefabs[index].agentTypeID,
                areaMask = _enemyPrefabs[index].areaMask
            };
            if (!NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit,
                Mathf.Max(0.01f, _spawnSampleDistance), filter))
            {
                Debug.LogWarning($"[Spawner] 스폰 중단: {spawnPosition} 주변에 '{_enemyPrefabs[index].name}'의 Agent 타입/영역에 맞는 NavMesh가 없습니다.", this);
                return;
            }
            spawnPosition = hit.position;
            _didCovered[reserve] = true;

            //적의 NavMeshAgent를 복사해서 적을 생성
            NavMeshAgent enemy = Instantiate(_enemyPrefabs[index], spawnPosition, Quaternion.identity);
            EnemyCombatController combat = enemy.GetComponent<EnemyCombatController>();

            if (combat != null)
            {
                combat.Initialize(_player);
            }

            AStarEnemyMover mover = enemy.GetComponent<AStarEnemyMover>();



            // 수정: Mover 누락과 이동 요청 실패를 구분하고 두 경우 모두 예약과 생성된 적 정리.
            if (mover == null)
            {
                Debug.LogWarning($"[Spawner] 생성 취소: '{enemy.name}'에 AStarEnemyMover가 없습니다. 엄폐 {reserve}번 예약을 해제합니다.", this);
                _didCovered[reserve] = false;
                Destroy(enemy.gameObject);
                return;
            }
            if(!mover.MoveTo(_grid, EnemyCoverPosition[reserve].position))
            {
                Debug.LogWarning($"[Spawner] 생성 취소: '{enemy.name}'의 엄폐 {reserve}번 이동 요청에 실패했습니다. Mover 로그를 확인하세요. 예약을 해제합니다.", this);
                _didCovered[reserve] = false;
                Destroy(enemy.gameObject);
                return;
            }

            EnemyType type = (EnemyType)index;

            SpawnedEnemy spawnedEnemy = new SpawnedEnemy(type, mover, reserve);
            // 수정: 위에서 성공한 이동 요청을 중복 호출하지 않도록 제거.
            
            _spawnedEnemies.Add(spawnedEnemy);

            _currentSpawnCount++;
        }

        bool TryGetSpawnPosition(out Vector3 position)
        {
            position = Vector3.zero;

            // 수정: 위치 미등록과 활성화된 위치 없음을 별도 원인으로 출력.
            if (_spawnPositionList == null || _spawnPositionList.Length == 0)
            {
                Debug.LogWarning("[Spawner] 스폰 중단: 스폰 지점 배열이 비어 있습니다.", this);
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

            Debug.LogWarning("[Spawner] 스폰 중단: 등록된 유효한 스폰 지점 중 활성화(true)된 곳이 없습니다.", this);
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

        void ReleaseDeadEnemies()
        {
            for (int i = _spawnedEnemies.Count - 1;i >= 0;i--)
            {
                SpawnedEnemy enemy = _spawnedEnemies[i];

                bool shouldRelease = enemy.Mover == null;

                if (!shouldRelease)
                {
                    EnemyHealth health = enemy.Mover.GetComponent<EnemyHealth>();
                    shouldRelease = health != null && health.IsDeath;
                }

                if (!shouldRelease)
                {
                    continue;
                }

                _didCovered[enemy.CoveredIndex] = false;
                _spawnedEnemies.RemoveAt(i);
                
            }
        }

        IEnumerator SpawnOneRoutine()
        {
            Debug.Log("SpawnOneRoutine() 들어옴");
            _isSpawning = true;

            while (_currentSpawnCount < _totalCount)
            {
                ReleaseDeadEnemies();

                if(FindEmptySpawnIndex() >= 0)
                {
                    int previousCount = _currentSpawnCount;

                    EnemySpawnOne();

                    if (previousCount == _currentSpawnCount)
                    {
                        Debug.LogWarning(
                    "[Spawner] 생성에 실패했습니다. 원인을 수정한 뒤 다시 시작하세요.",
                    this);
                        break;
                    }
                }

                //if (_currentSpawnCount < _totalCount)
                //{
                //    yield return new WaitForSeconds(_spawnInterval);
                //}
                yield return new WaitForSeconds(Mathf.Max(0.1f, _spawnInterval));

                _isSpawning = false;
            }

        }
    }
}

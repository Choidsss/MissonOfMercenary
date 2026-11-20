using System.Collections;
using System.Collections.Generic;
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

        [Header("Enemy Move To Cover Position")]
        [SerializeField]
        private List<Transform> EnemyCoverPosition = new List<Transform>();

        [SerializeField] AStarGrid _grid;
        [SerializeField] NavMeshAgent _enemyPrefab;
        [SerializeField] Transform _target;
        [SerializeField] Transform _spawnPoint;
        [SerializeField, Min(1)] int _totalCount = 1;
        [SerializeField, Min(0.1f)] float _spawnInterval = 2f;
        [SerializeField, Min(0.01f)] float _spawnSampleDistance = 1f;
        [SerializeField] bool _spawnOnStart = true;

        //EnemyCoverPosition만큼의 크기를 같는 bool 배열 => 빈자리 찾는 용도
        //_didCover를 확인하고 빈자리가 있다면 바로 그 자리에 적의 목적지로 설정(크기가 같으니 인덱스도 같음)
        //적 스폰 수 조절(최대 엄폐 위치의 수를 넘어가지 않도록)


        bool[] _didCovered;

        int _spawnedCount;

        private void Awake()
        {
            _didCovered = new bool[EnemyCoverPosition.Count];
        }

        //비어있는 위치를  찾아서 숫자로 반환
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
        void EnemySpawnOne()
        {
            //빈자리 찾은거 가져오고
            int reserve = FindEmptySpawnIndex();

            if(reserve < 0)
            {
                Debug.Log("빈 자리를 찾을 수 없습니다");
                return;
            }

            //빈자리 예약
            _didCovered[reserve] = true;

            Vector3 spawnPosition = GetSpawnPosition();

            //적의 NavMeshAgent를 복사해서 적을 생성
            NavMeshAgent enemy = Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);

            AStarEnemyMover mover = enemy.GetComponent<AStarEnemyMover>();

            if(mover == null || !mover.MoveTo(_grid, EnemyCoverPosition[reserve].position))
            {
                _didCovered[reserve] = false;
                Destroy(enemy.gameObject);
                return;
            }

            if(_grid == null || _enemyPrefab == null)
            {
                Debug.Log("AStatGrid 혹은 NavMeshAgent가 없습니다");
                return;
            }

            SpawnedEnemy spawnedEnemy = new SpawnedEnemy(EnemyType.Terrorrist1, mover, reserve);
            mover.MoveTo(_grid, EnemyCoverPosition[reserve].position);
            
            _spawnedEnemies.Add(spawnedEnemy);
        }

        Vector3 GetSpawnPosition()
        {
            //ToDo : 
            //스포너위치에서 랜덤한 범위에서 적을 랜덤하게 생성하도록
            //스폰 지점을 여러개를 만든다고 하면, 어떻게 만들래? 확장성까지 확보한다는 가정하에
            //그리고 언제 어떤 스포너를 부를거야.
            //그거까지 정하고 랜덤하게 위치 뽑아서 생성해야지
            

            //임시
            return Vector3.zero;
        }
    }
}

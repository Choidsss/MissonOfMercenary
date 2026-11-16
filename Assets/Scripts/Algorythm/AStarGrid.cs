using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class AStarGrid : MonoBehaviour
    {
        const float GRID_TICKNESS = 1f;
        const float WHITE_SPACE_OFFSET = 0.8f;

        [Header("Set Algorythm")]
        [Header("NavMesh Insert")]
        [SerializeField] NavMeshAgent _nav;

        //[Header("Start Position")]

        [Header("End Position")]
        [SerializeField] Transform _end;

        [Header("Variable Option")]
        [SerializeField] int _gridWidth; //전체 그리드 중 가로 방향의 칸의 개수
        [SerializeField] float _maxDistance;
        [SerializeField] float _cellSize; //셀 한칸의 크기
        [SerializeField] LayerMask _obstacleLayer;
        [SerializeField] Vector3 _positionOffset;

        List<Vector3> _moveToCoordinate = new List<Vector3>();
        Transform _start;


        bool[,] _moveAble;
        bool _isMoving;
        int _currentIndex = 0;

        private void Awake()
        {
            BuildGrid();
        }

        void Start()
        {
            if (_nav == null || !_nav.isActiveAndEnabled || !_nav.isOnNavMesh)
            {
                Debug.Log("활성화 된 NavMesh위에 있어야 합니다");
                return;
            }

            if (_end == null)
            {
                Debug.Log("목표 Transform을 연결해 주세요");
                return;
            }

            _start = _nav.transform;
            _moveToCoordinate = FindPath(_start.position, _end.position);

            if (_moveToCoordinate == null || _moveToCoordinate.Count == 0)
            {
                return;
            }

            _currentIndex = _moveToCoordinate.Count > 1 ? 1 : 0;

            _nav.isStopped = false;
            EnemyMoveToDestination();
        }

        private void Update()
        {
            if (!_isMoving)
            {
                return;
            }

            if (! _nav.isActiveAndEnabled || !_nav.isOnNavMesh)
            {
                _isMoving = false;
                return;
            }

            //경로 계산이 끝날때 까지 기다림
            if (_nav.pathPending)
            {
                return;
            }

            if (_nav.pathStatus != NavMeshPathStatus.PathComplete)
            {
                _isMoving = false;
                _nav.isStopped = true;
                _nav.ResetPath();

                Debug.Log("현재 목표 지점까지 도달할 수 없습니다");
                return;
            }

            float arrivalDistance = Mathf.Max(_nav.stoppingDistance, 0.1f);

            if(_nav.remainingDistance > arrivalDistance)
            {
                return;
            }

            _currentIndex++;
            EnemyMoveToDestination();
        }

        //월드 위치를 칸 번호로 반환하는 함수
        Vector2Int WorldToCell(Vector3 worldPosition)
        {
            float startX = transform.position.x + _positionOffset.x - _gridWidth * _cellSize * 0.5f;
            float startZ = transform.position.z + _positionOffset.z - _gridWidth * _cellSize * 0.5f;

            int x = Mathf.FloorToInt((worldPosition.x - startX) / _cellSize);
            int z = Mathf.FloorToInt((worldPosition.z - startZ) / _cellSize);

            return new Vector2Int(x, z);
        }

        //각 이웃칸에서 목적지까지 몇 걸음 남았을지 추정하는 함수
        int GetHeuristic(Vector2Int from, Vector2Int goal)
        {
            int heuristicX = Mathf.Abs(from.x - goal.x);
            int heuristicY = Mathf.Abs(from.y - goal.y) ;

            return heuristicX + heuristicY;
        }

        //셀 한칸의 중앙의 월드 좌표를 가져오는 함수
        Vector3 GetCellCenter(int x, int z)
        {
            float width = _gridWidth * _cellSize;//전체 그리드 가로의 길이(한칸의 길이 * 셀의 개수)
            float startX = transform.position.x + _positionOffset.x - width * 0.5f;//왼쪽 끝의 시작지점
            float startZ = transform.position.z + _positionOffset.z - width * 0.5f;//왼쪽 끝의 시작지점
            float firstCellX = startX + _cellSize * 0.5f;//첫번째 칸의 중앙 지점 구하는거1
            float firstCellZ = startZ + _cellSize * 0.5f;//첫번째 칸의 중앙 지점 구하는거2

            //셀의 중앙 좌표값
            return new Vector3(firstCellX + x * _cellSize, transform.position.y + _positionOffset.y + GRID_TICKNESS, firstCellZ + z * _cellSize);
        }

        //이동할 수 있는 셀인지 확인하는 함수
        bool IsMoveAble(int x, int z)
        {
            if (x > _gridWidth - 1 || z > _gridWidth - 1 || x < 0 || z < 0)
            {
                return false;
            }

            return _moveAble[x, z];
        }

        void BuildGrid()
        {
            _moveAble = new bool[_gridWidth, _gridWidth];

            for (int x = 0; x < _gridWidth; x++)
            {
                for (int z = 0; z < _gridWidth; z++)
                {
                    Vector3 cellSize = new Vector3(_cellSize, GRID_TICKNESS, _cellSize);
                    Vector3 pos = GetCellCenter(x, z);

                    //그리드 위에 장애물 확인 후 배열에 표기
                    //그리고 크기 넘기는건 반토막내서
                    bool blocked = Physics.CheckBox(pos, cellSize * 0.5f, Quaternion.identity, _obstacleLayer);

                    //Block이 true면 막힌것
                    _moveAble[x, z] = !blocked;
                }


            }
        }

        //이걸 반복문으로 하나에 묶어야 함.
        List<Vector2Int> SearchGrid(Vector2Int start, Vector2Int goal)
        {
            if (!IsMoveAble(start.x, start.y) || !IsMoveAble(goal.x, goal.y))
            {
                Debug.Log("목적지로 이동할 수 없습니다");
                return null;
            }

            //앞으로 조사할 후보의 칸들
            List<Vector2Int> open = new List<Vector2Int>();
            //출발점에서 각 칸까지 발견한 가장 짧은 이동 횟수
            Dictionary<Vector2Int, int> moveCount = new Dictionary<Vector2Int, int>();
            //각 칸에 도착하기 직전에 있었던 칸
            Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

            //출발칸을 먼저 조사하겠다고 말하는 것
            open.Add(start);
            moveCount.Add(start, 0);

            while (open.Count > 0)
            {
                int bestIndex = 0;
                int bestF = int.MaxValue;

                for (int i = 0; i < open.Count; i++)
                {
                    Vector2Int pos = open[i];
                    int f = moveCount[pos] + GetHeuristic(pos, goal);

                    if (f < bestF)
                    {
                        bestF = f;
                        bestIndex = i;
                    }
                }

                Vector2Int current = open[bestIndex];
                open.RemoveAt(bestIndex);

                if (current == goal)
                {
                    List<Vector2Int> path = new List<Vector2Int>();
                    Vector2Int step = goal;

                    path.Add(step);

                    while (step != start)
                    {
                        step = cameFrom[step];
                        path.Add(step);
                    }

                    path.Reverse();

                    Debug.Log($"경로 발견 {moveCount[goal]} 걸음");
                    return path;
                }

                foreach (Vector2Int neighbor in GetNeighborsInGrid(current.x, current.y))
                {
                    int newG = moveCount[current] + 1;

                    //이웃들의 비용들을 물어보고 비교해야함
                    if (moveCount.TryGetValue(neighbor, out int oldG))
                    {
                        if (oldG <= newG)
                        {
                            continue;
                        }
                    }

                    moveCount[neighbor] = newG;
                    cameFrom[neighbor] = current;

                    if (!open.Contains(neighbor))
                    {
                        open.Add(neighbor);
                    }
                }
            }

            Debug.Log("목적지까지 도달할 수 있는 경로가 존재하지 않습니다");
            return null;
        }

        //현재 위치에서 갈수 있는 칸에 대해 자료형에 담아두는 함수
        //상하좌우 중 갈수 있는 방향 확인 후 리스트에 담아서 리턴
        List<Vector2Int> GetNeighborsInGrid(int x, int z)
        {
            List<Vector2Int> neighborsList = new List<Vector2Int>();

            //방향만 집중
            //왼쪽을 갈 수 있는가?
            if (IsMoveAble(x - 1, z))
            {
                neighborsList.Add(new Vector2Int(x - 1, z)); //갈 수 있는 위치를 저장
            }

            //오른쪽을 갈 수 있는가?
            if (IsMoveAble(x + 1, z))
            {
                neighborsList.Add(new Vector2Int(x + 1, z)); //갈 수 있는 위치를 저장
            }

            //위쪽을 갈 수 있는가?
            if (IsMoveAble(x, z + 1))
            {
                neighborsList.Add(new Vector2Int(x, z + 1)); //갈 수 있는 위치를 저장
            }
            
            //아래쪽을 갈 수 있는가?
            if (IsMoveAble(x, z - 1))
            {
                neighborsList.Add(new Vector2Int(x, z - 1)); //갈 수 있는 위치를 저장
            }

            return neighborsList;
        }

        List<Vector3> FindPath(Vector3 startPosition, Vector3 goalPosition)
        {
            List<Vector3> worldPath = new List<Vector3>();

            Vector2Int start = WorldToCell(startPosition);
            Vector2Int goal = WorldToCell(goalPosition);

            List<Vector2Int> cellPath = SearchGrid(start, goal);

            if (cellPath == null)
            {
                return null;
            }

            foreach(Vector2Int cell in cellPath)
            {
                Vector3 position = GetCellCenter(cell.x, cell.y);
                worldPath.Add(position);
            }

            return worldPath;
        }

        private void EnemyMoveToDestination()
        {
            if (_currentIndex >= _moveToCoordinate.Count)
            {
                _isMoving = false;
                _nav.isStopped = true;

                Debug.Log("목적지에 도착하였습니다");
                return;
            }

            Vector3 target = _moveToCoordinate[_currentIndex];

            bool found = NavMesh.SamplePosition(target, out NavMeshHit hit, _maxDistance, _nav.areaMask);

            if (!found)
            {
                _isMoving = false;
                _nav.isStopped = true;
                _nav.ResetPath();

                Debug.LogWarning("목표 지점 주변에 NavMesh가 없습니다");
                return;
            }

            _isMoving = _nav.SetDestination(hit.position);

            if (!_isMoving)
            {
                _nav.isStopped = true;
                _nav.ResetPath();

                Debug.Log("이동 요쳥에 실패하였습니다");
            }
            //_moveToCoordinate = FindPath(_start, _end);

            //_nav.SetDestination(_moveToCoordinate[_currentIndex]);

            //if (_nav.remainingDistance <= 0.1f)
            //{
            //    _currentIndex++;
            //    return;
            //}

            //_nav.SetDestination()
        }

        private void OnDrawGizmos()
        {
            for(int x = 0; x < _gridWidth; x++)
            {
                for(int z = 0;z < _gridWidth; z++)
                {
                    Vector3 pos = GetCellCenter(x, z);
                    Vector3 cellSize = new Vector3(_cellSize, GRID_TICKNESS, _cellSize);

                    if(_moveAble != null && !_moveAble[x, z])
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube(pos, cellSize * WHITE_SPACE_OFFSET);
                    }
                    else
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawWireCube(pos, cellSize);
                    }
                }
            }

        }
    }
}

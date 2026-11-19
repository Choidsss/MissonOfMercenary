using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class AStarGrid : MonoBehaviour
    {
        const float GRID_TICKNESS = 1f;
        const float WHITE_SPACE_OFFSET = 0.4f;
        const float OBSTACLES_MULTIPLY = 0.5f;

        [Header("Set Algorythm")]
        [Header("NavMesh Insert")]
        [SerializeField] NavMeshAgent _nav;

        [Header("End Position")]
        [SerializeField] Transform _end;

        [Header("Variable Option")]
        [SerializeField] int _gridWidth; //전체 그리드 중 가로 방향의 칸의 개수
        [SerializeField] float _maxDistance;
        [SerializeField] float _cellSize; //셀 한칸의 크기
        [SerializeField] float _moveSpeed; //셀을 넘어가는 속도
        // *** 추가: 초당 회전 각도. 720이면 90도 회전에 약 0.125초가 걸린다.
        [SerializeField, Min(0f)] float _turnSpeed = 720f;
        [SerializeField] LayerMask _obstacleLayer;
        [SerializeField] Vector3 _positionOffset;

        [Header("Determine Obstacles In Algorythm")]
        [SerializeField] Vector3 _obstacleCheckSize = new Vector3(1f, 1f, 1f);
        [SerializeField] Vector3 _obstacleCheckOffset = Vector3.zero;

        // *** 추가: 끄면 셀마다 이동하는 기존 경로와 비교할 수 있다.
        [SerializeField] bool _simplifyPath = true;

        List<Vector3> _moveToCoordinate = new List<Vector3>();
        Transform _start;


        bool[,] _moveAble;
        bool _isMoving;
        int _currentIndex = 0;

        // *** 추가: 스폰된 적도 현재 격자의 이동 설정을 공유한다.
        public float MoveSpeed => Mathf.Max(0f, _moveSpeed);
        public float TurnSpeed => Mathf.Max(0f, _turnSpeed);
        public float SampleDistance => Mathf.Max(0f, _maxDistance);

        private void Awake()
        {
            BuildGrid();
        }

        void Start()
        {
            // *** 추가: 적/목표를 비워 두면 스포너용 공용 격자로만 사용한다.
            if (_nav == null && _end == null)
            {
                return;
            }

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

            // *** 추가: 실행 중 Inspector에서 조절한 이동/회전 속도를 바로 반영한다.
            ApplyMovementSettings();

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

        // *** 변경: 대각선 비용을 포함한 남은 거리를 추정하도록 반환형을 float로 변경.
        float GetHeuristic(Vector2Int from, Vector2Int goal)
        {
            int heuristicX = Mathf.Abs(from.x - goal.x);
            int heuristicY = Mathf.Abs(from.y - goal.y) ;

            // *** 추가: 대각선은 √2, 남은 직선은 1의 비용으로 추정한다.
            int diagonal = Mathf.Min(heuristicX, heuristicY);
            return diagonal * Mathf.Sqrt(2f) + Mathf.Abs(heuristicX - heuristicY);
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

        //현재 칸에서 이웃 칸 까지 검사 박스를 밀어서 지나갈수 있는지 판단
        bool CanMoveBetweenCells(Vector2Int from, Vector2Int to)
        {
            //도착 칸에 서 있을 공각이 있는지 확인
            if(!IsMoveAble(to.x, to.y))
            {
                // *** 변경: 탐색 중 반복 출력되던 이동 불가 로그를 제거.
                return false;
            }

            Vector3 start = GetCellCenter(from.x, from.y) + _obstacleCheckOffset;
            Vector3 end = GetCellCenter(to.x, to.y) + _obstacleCheckOffset;

            //먼저 움직일 방향을 구함
            Vector3 movement = end - start;
            float distance = movement.magnitude;

            if(distance <= Mathf.Epsilon)
            {
                return true;
            }

            bool blocked = Physics.BoxCast(start, _obstacleCheckSize * OBSTACLES_MULTIPLY, movement / distance, out RaycastHit hit, Quaternion.identity, distance, _obstacleLayer, QueryTriggerInteraction.Ignore);
            return !blocked;
        }

        void BuildGrid()
        {
            _moveAble = new bool[_gridWidth, _gridWidth];

            for (int x = 0; x < _gridWidth; x++)
            {
                for (int z = 0; z < _gridWidth; z++)
                {
                    Vector3 cellSize = new Vector3(_cellSize, GRID_TICKNESS, _cellSize);
                    Vector3 checkCenter = GetCellCenter(x, z) + _obstacleCheckOffset;
                    //Vector3 pos = GetCellCenter(x, z);

                    //그리드 위에 장애물 확인 후 배열에 표기
                    //그리고 크기 넘기는건 반토막내서
                    bool blocked = Physics.CheckBox(checkCenter, _obstacleCheckSize * OBSTACLES_MULTIPLY, Quaternion.identity, _obstacleLayer, QueryTriggerInteraction.Ignore);

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
            // *** 변경: 대각선의 소수 비용을 저장하도록 이동 비용 자료형을 float로 변경.
            Dictionary<Vector2Int, float> moveCount = new Dictionary<Vector2Int, float>();
            //각 칸에 도착하기 직전에 있었던 칸
            Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

            //출발칸을 먼저 조사하겠다고 말하는 것
            open.Add(start);
            moveCount.Add(start, 0);

            while (open.Count > 0)
            {
                int bestIndex = 0;
                // *** 변경: 최소 비용 비교도 float로 수행한다.
                float bestF = float.PositiveInfinity;

                for (int i = 0; i < open.Count; i++)
                {
                    Vector2Int pos = open[i];
                    // *** 변경: 누적 비용과 추정 비용의 합을 float로 계산한다.
                    float f = moveCount[pos] + GetHeuristic(pos, goal);

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

                    // *** 변경: 걸음 수 대신 셀 크기를 반영한 격자 이동 거리를 출력한다.
                    Debug.Log($"경로 발견: 격자 이동 거리 {moveCount[goal] * _cellSize:F2}");
                    return path;
                }

                foreach (Vector2Int neighbor in GetNeighborsInGrid(current.x, current.y))
                {
                    // *** 추가: 직선 이동 비용은 1, 대각선 이동 비용은 √2로 구분한다.
                    bool diagonal = neighbor.x != current.x && neighbor.y != current.y;
                    float newG = moveCount[current] + (diagonal ? Mathf.Sqrt(2f) : 1f);

                    // *** 변경: 기존 이동 비용도 float로 읽어 새 비용과 비교한다.
                    if (moveCount.TryGetValue(neighbor, out float oldG))
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

        // *** 변경 시작: 기존 4방향 이웃 탐색을 아래의 8방향 탐색으로 교체.
        List<Vector2Int> GetNeighborsInGrid(int x, int z)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>(8);
            Vector2Int current = new Vector2Int(x, z);

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dz == 0)
                    {
                        continue;
                    }

                    Vector2Int next = new Vector2Int(x + dx, z + dz);

                    // *** 추가: 막힌 옆 칸의 모서리를 대각선으로 잘라 가지 않는다.
                    if (dx != 0 && dz != 0 &&
                        (!IsMoveAble(x + dx, z) || !IsMoveAble(x, z + dz)))
                    {
                        continue;
                    }

                    // *** 변경: 8방향 후보 모두 기존 박스 검사로 도착 셀과 이동 구간을 확인한다.
                    if (CanMoveBetweenCells(current, next))
                    {
                        neighbors.Add(next);
                    }
                }
            }

            return neighbors;
        }

        // *** 변경 끝: 8방향 이웃 탐색.

        // *** 추가 시작: 적 크기의 박스로 직진 가능한 중간 경유점을 생략한다.
        List<Vector2Int> SimplifyPath(List<Vector2Int> path)
        {
            if (path.Count <= 2)
            {
                return path;
            }

            List<Vector2Int> simplified = new List<Vector2Int> { path[0] };
            int anchorIndex = 0;

            while (anchorIndex < path.Count - 1)
            {
                // 바로 다음 칸은 A*에서 이미 통과 가능하다고 확인한 구간이다.
                int nextIndex = anchorIndex + 1;

                // 목적지 쪽부터 검사해 직진 가능한 가장 먼 경유점을 선택한다.
                // 먼 지점이 막혀 있어도 가까운 지점은 열려 있을 수 있다.
                for (int candidate = path.Count - 1; candidate > anchorIndex + 1; candidate--)
                {
                    if (CanMoveBetweenCells(path[anchorIndex], path[candidate]))
                    {
                        nextIndex = candidate;
                        break;
                    }
                }

                simplified.Add(path[nextIndex]);
                anchorIndex = nextIndex;
            }

            // 출발 셀과 마지막 목적 셀은 항상 유지한다.
            return simplified;
        }
        // *** 추가 끝: 경로 단순화.

        // *** 변경: 스폰된 적이 공용 격자에 경로를 요청할 수 있게 공개한다.
        public List<Vector3> FindPath(Vector3 startPosition, Vector3 goalPosition)
        {
            // *** 추가: 잘못된 크기 또는 초기화 전 호출을 방어한다.
            if (_gridWidth <= 0 || _cellSize <= 0f)
            {
                Debug.LogWarning("Grid Width와 Cell Size는 0보다 커야 합니다.", this);
                return null;
            }
            if (_moveAble == null)
            {
                BuildGrid();
            }

            List<Vector3> worldPath = new List<Vector3>();

            Vector2Int start = WorldToCell(startPosition);
            Vector2Int goal = WorldToCell(goalPosition);

            List<Vector2Int> cellPath = SearchGrid(start, goal);

            if (cellPath == null)
            {
                return null;
            }

            // *** 추가: A* 탐색이 완료된 뒤, 월드 좌표로 바꾸기 전에 경유점을 줄인다.
            if (_simplifyPath)
            {
                int originalCount = cellPath.Count;
                cellPath = SimplifyPath(cellPath);
                Debug.Log($"경로 단순화: {originalCount}개 → {cellPath.Count}개 경유점");
            }

            foreach(Vector2Int cell in cellPath)
            {
                Vector3 position = GetCellCenter(cell.x, cell.y);
                worldPath.Add(position);
            }

            return worldPath;
        }

        // *** 추가: 이동 중 회전은 NavMeshAgent가 담당하며 속도를 별도로 조절한다.
        void ApplyMovementSettings()
        {
            _nav.speed = Mathf.Max(0f, _moveSpeed);
            _nav.angularSpeed = Mathf.Max(0f, _turnSpeed);
            _nav.updateRotation = true;
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

            // *** 변경: 첫 이동 요청 전에도 이동 속도와 회전 속도를 적용한다.
            ApplyMovementSettings();
            _isMoving = _nav.SetDestination(hit.position);

            if (!_isMoving)
            {
                _nav.isStopped = true;
                _nav.ResetPath();

                Debug.Log("이동 요쳥에 실패하였습니다");
            }
        }

        private void OnDrawGizmos()
        {
            // Display the original A* route before NavMesh projection.
            if (_moveToCoordinate != null && _moveToCoordinate.Count > 1)
            {
                Gizmos.color = Color.black;
                
                for (int i = 1; i < _moveToCoordinate.Count; i++)
                {
                    Gizmos.DrawLine(_moveToCoordinate[i - 1], _moveToCoordinate[i]);
                }
            }

            for(int x = 0; x < _gridWidth; x++)
            {
                for(int z = 0;z < _gridWidth; z++)
                {
                    Vector3 pos = GetCellCenter(x, z);
                    Vector3 cellSize = new Vector3(_cellSize, GRID_TICKNESS, _cellSize);
                    Vector3 checkCenter = pos + _obstacleCheckOffset;

                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireCube(checkCenter, _obstacleCheckSize);

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

using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class AStarGridTest : MonoBehaviour
    {
        [Header("To Play")]
        [SerializeField] Transform _character;

        [Header("Target Point")]
        [SerializeField] Transform _targetPoint;

        [Header("Obstacle Layer")]
        [SerializeField] LayerMask _obstacleLayer;

        bool[,] _walkable = new bool[10, 10];
        List<Vector2Int> _path;

        

        void Awake()
        {
            BuildGrid();
        }

        private void Start()
        {
            Vector2Int cell = WorldToCell(_character.position);
            Vector2Int goal = WorldToCell(_targetPoint.position);

            Vector2Int bestCell = cell;

            List<Vector2Int> open = new List<Vector2Int>() { cell };
            Dictionary<Vector2Int, int> gCost = new Dictionary<Vector2Int, int>();
            gCost[cell] = 0;

            Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

            //Vector2Int current = open[0];
            //open.RemoveAt(0);

            Debug.Log($"캐릭터의 현재 칸 : {cell}");

            if (!IsWalkable(cell.x, cell.y))
            {
                Debug.Log($"캐릭터가 현재 격자 밖이거나 벽이 있는 칸에 있습니다");
                return;
            }

            if (!IsWalkable(goal.x, goal.y))
            {
                Debug.Log("목적지가 벽이거나 격자 밖입니다.");
                return;
            }


            //foreach (Vector2Int neighbor in GetNeighbors(cell.x, cell.y))
            //{
            //    gCost[neighbor] = gCost[current] + 1;
            //    open.Add(neighbor);
            //}

            ////남은 후보군 출력
            //foreach (Vector2Int candidate in open)
            //{
            //    int g = gCost[candidate];
            //    int h = GetHeuristic(candidate, goal);
            //    Debug.Log($"남은 후보 {candidate}: g={g}, h={h}, f={g + h}");

            //}

            //남아있는 후보중 가장 f가 작은 것을 고르기
            //if(open.Count == 0)
            //{
            //    return;
            //}

            //int bestIndex = 0;
            //int bestF = int.MaxValue;

            //for(int i = 0;i < open.Count; i++)
            //{
            //    Vector2Int candidate = open[i];
            //    int f = gCost[candidate] + GetHeuristic(candidate, goal);

            //    if(f < bestF)
            //    {
            //        bestF = f;
            //        bestIndex = i;
            //    }
            //}

            //current = open[bestIndex];
            //open.RemoveAt(bestIndex);
            //Debug.Log($"다음에 조사할 칸: {current}, f={bestF}, 남은 후보 수={open.Count}");

            //if (current == goal)
            //{
            //    Debug.Log("목표도착");
            //    return;
            //}

            //foreach (Vector2Int neighbor in GetNeighbors(current.x, current.y))
            //{
            //    int newG = gCost[current] + 1;

            //    // 이미 같거나 더 짧은 길을 알고 있다면 건너뜀
            //    if (gCost.TryGetValue(neighbor, out int oldG) && oldG <= newG)
            //    {
            //        continue;
            //    }

            //    gCost[neighbor] = newG;

            //    if (!open.Contains(neighbor))
            //    {
            //        open.Add(neighbor);
            //    }

            //    Debug.Log($"후보 갱신 : {neighbor} , g = {newG}");
            //}

            if (!IsWalkable(cell.x, cell.y))
            {
                Debug.Log("목적지가 벽에 막혀있거나, 격자 밖입니다.");
                return;
            }

            while (open.Count > 0)
            {
                // 1. 남은 후보 전체에서 f가 가장 작은 칸을 고른다.
                int bestIndex = 0;
                int bestF = int.MaxValue;

                for (int i = 0;i < open.Count;i++)
                {
                    Vector2Int candidate = open[i];
                    int f = gCost[candidate] + GetHeuristic(candidate, goal);

                    if (f < bestF)
                    {
                        bestF = f;
                        bestIndex = i;
                    }
                }

                Vector2Int current = open[bestIndex];
                open.RemoveAt(bestIndex);

                // 2. 목적지를 골랐다면 탐색 성공.
                if (current == goal)
                {
                    List<Vector2Int> path = new List<Vector2Int>();
                    Vector2Int step = goal;
                    path.Add(step);

                    while (step != cell)
                    {
                        step = cameFrom[step];
                        path.Add(step);
                    }

                    path.Reverse();
                    _path = path;

                    Debug.Log($"총 {gCost[goal]}걸음: {string.Join(" → ", path)}");
                    return;
                }

                // 3. 이웃을 처음 발견했거나 더 짧게 갈 수 있으면 갱신한다.
                foreach (Vector2Int neighbor in GetNeighbors(current.x, current.y))
                {
                    int newG = gCost[current] + 1;

                    if (gCost.TryGetValue(neighbor, out int oldG) && oldG <= newG)
                    {
                        continue;
                    }

                    gCost[neighbor] = newG;
                    cameFrom[neighbor] = current;

                    if (!open.Contains(neighbor))
                    {
                        open.Add(neighbor);
                    }
                }
            }
            // 목적지를 고르기 전에 조사할 후보가 다 떨어졌다.
            Debug.Log("목적지까지 갈 수 있는 경로가 없어요.");

        }

        Vector3 GetCellCenter(int x, int z, Vector3 center)
        {
            return center + new Vector3(-15f + x * 3f + 1.5f, 0.5f, -15f + z * 3f + 1.5f);
        }

        void BuildGrid()
        {
            Vector3 center = transform.position;

            for (int x = 0; x < 10; x++)
            {
                for (int z = 0; z < 10; z++)
                {
                    Vector3 cellCenter = GetCellCenter(x, z, center);
                    bool blocked = Physics.CheckBox(cellCenter, new Vector3(1.45f, 0.5f, 1.45f), Quaternion.identity, _obstacleLayer);

                    _walkable[x, z] = !blocked;
                }
            }
        }

        //빨간 표시된 bool의 2차원 배열에서 값을 확인 후, 갈수 있는지 없는지 확인
        public bool IsWalkable(int x, int z)
        {
            if (x < 0 || x >= _walkable.GetLength(0) || z < 0 || z >= _walkable.GetLength(1)) 
            {
                return false;
            }

            return _walkable[x, z];
        }

        //상하좌우 중 갈수 있는 방향 확인 후 리스트에 담아서 리턴
        public List<Vector2Int> GetNeighbors(int x, int z)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();

            if(IsWalkable(x - 1, z))
            {
                neighbors.Add(new Vector2Int(x - 1, z));
            }

            if (IsWalkable(x + 1, z))
            {
                neighbors.Add(new Vector2Int(x + 1, z));
            }

            if (IsWalkable(x, z - 1))
            {
                neighbors.Add(new Vector2Int(x, z - 1));
            }

            if (IsWalkable(x, z + 1))
            {
                neighbors.Add(new Vector2Int(x, z + 1));
            }

            return neighbors;
        }

        Vector2Int WorldToCell(Vector3 worldPosition)
        {
            Vector3 corner = transform.position - new Vector3(15f, 0f, 15);

            int x = Mathf.FloorToInt((worldPosition.x - corner.x) / 3f);
            int z = Mathf.FloorToInt((worldPosition.z - corner.z) / 3f);

            return new Vector2Int(x, z);
        }
        
        int GetHeuristic(Vector2Int from,  Vector2Int to)
        {
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
        }
        
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                BuildGrid();
            }

            Vector3 center = transform.position;
            Vector3 size = new Vector3(30f, 1f, 30f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, size);

            for (int i = 0; i < 10; i++)
            {
                float offset = -15f + i * 3f;

                Gizmos.DrawLine(center + new Vector3(-15f, 0.5f, offset), center + new Vector3(15f, 0.5f, offset));
                Gizmos.DrawLine(center + new Vector3(offset, 0.5f, -15f), center + new Vector3(offset, 0.5f, 15f));

            }

            //각 칸을 한 번씩 검사하고, 벽이 있는 칸만 빨갛게 그리기.
            Gizmos.color = Color.red;

            for (int x = 0; x < 10; x++)
            {
                for (int z = 0; z < 10; z++)
                {
                    if(!_walkable[x, z])
                    {
                        Vector3 cellCenter = GetCellCenter(x, z, center);
                        Gizmos.DrawCube(cellCenter, new Vector3(2.8f, 0.05f, 2.8f));
                    }
                }
            }

            Gizmos.color = Color.cyan;
            //Gizmos.DrawWireCube(GetCellCenter(5, 5, center), new Vector3(2.9f, 1f, 2.9f));

            if (_path == null)
            {
                return;
            }

            for (int i = 1;i < _path.Count;i++)
            {
                Vector2Int prev = _path[i - 1];
                Vector2Int next = _path[i];

                Vector3 from = GetCellCenter(prev.x, prev.y, center);
                Vector3 to = GetCellCenter(next.x, next.y, center);

                Gizmos.DrawLine(from, to);
            }
        }
    }
}

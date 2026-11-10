using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace MIssionOfMercenary
{
    public class AStarGrid : MonoBehaviour
    {
        const float GRID_TICKNESS = 1f;
        const float WHITE_SPACE_OFFSET = 0.8f;

        [Header("Variable Option")]
        [SerializeField] int _gridWidth; //전체 그리드 중 가로 방향의 칸의 개수
        [SerializeField] float _cellSize; //셀 한칸의 크기
        [SerializeField] LayerMask _obstacleLayer;

        bool[,] _moveAble;

        private void Awake()
        {
            BuildGrid();
        }

        void Start()
        {
            //Debug.Log(WorldToCell(GetCellCenter(2, 3)));
        }

        void Update()
        {
        
        }

        //월드 위치를 칸 번호로 반환하는 함수
        Vector2Int WorldToCell(Vector3 worldPosition)
        {
            float startX = transform.position.x - _gridWidth * _cellSize * 0.5f;
            float startZ = transform.position.z - _gridWidth * _cellSize * 0.5f;

            int x = Mathf.FloorToInt((worldPosition.x - startX) / _cellSize);
            int z = Mathf.FloorToInt((worldPosition.z - startZ) / _cellSize);

            return new Vector2Int(x, z);
        }

        //셀 한칸의 중앙의 월드 좌표를 가져오는 함수
        Vector3 GetCellCenter(int x, int z)
        {
            float width = _gridWidth * _cellSize;//전체 그리드 가로의 길이(한칸의 길이 * 셀의 개수)
            float startX = transform.position.x - width * 0.5f;//왼쪽 끝의 시작지점
            float startZ = transform.position.z - width * 0.5f;//왼쪽 끝의 시작지점
            float firstCellX = startX + _cellSize * 0.5f;//첫번째 칸의 중앙 지점 구하는거1
            float firstCellZ = startZ + _cellSize * 0.5f;//첫번째 칸의 중앙 지점 구하는거2

            //셀의 중앙 좌표값
            return new Vector3(firstCellX + x * _cellSize, GRID_TICKNESS, firstCellZ + z * _cellSize);
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

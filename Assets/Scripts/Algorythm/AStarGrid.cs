using UnityEngine;

namespace MIssionOfMercenary
{
    public class AStarGrid : MonoBehaviour
    {
        const float GRID_TICKNESS = 1f;

        [Header("Variable Option")]
        [SerializeField] int _gridWidth; //전체 그리드 중 가로 방향의 칸의 개수
        [SerializeField] float _cellSize; //셀 한칸의 크기


        void Start()
        {
        
        }

        void Update()
        {
        
        }

        //Vector2Int WorldToCell()
        //{

        //}

        //셀 한칸의 중앙의 월드 좌표를 가져옴
        Vector3 GetCellCenter(int x, int z, Vector3 center)
        {
            float width = _gridWidth * _cellSize;//전체 그리드 가로의 길이(한칸의 길이 * 셀의 개수)
            float startX = transform.position.x - width * 0.5f;//왼쪽 끝의 시작지점
            float startZ = transform.position.z - width * 0.5f;//왼쪽 끝의 시작지점
            float firstCellX = startX + _cellSize * 0.5f;//첫번째 칸의 중앙 지점 구하는거1
            float firstCellZ = startZ + _cellSize * 0.5f;//첫번째 칸의 중앙 지점 구하는거2

            //셀의 중앙 좌표값
            return new Vector3(firstCellX + x * _cellSize, GRID_TICKNESS, firstCellX + z * _cellSize);
        }

        private void OnDrawGizmos()
        {
            /*
             * 1. 가로·세로 칸 수만큼 이중 for문을 돌리고
2. GetCellCenter(x, z)로 각 칸의 중심을 구한 뒤
3. Gizmos.DrawWireCube()로 한 칸 크기의 상자를 그려보세요.
             */

            Gizmos.color = Color.yellow;

            //Gizmos.DrawWireCube()

        }
    }
}

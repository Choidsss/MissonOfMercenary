using System.Linq;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyFindArea : MonoBehaviour
    {
        //This Script is aboout Enemy's Finding player that decide the size of area
        //Enemy's Vision limit forward to degree.

        [Header("Enemy Field Of Vision Options")]
        [SerializeField] float _degree; //전방 시야 +-각도 제한(한쪽으로 70도, 140도)
        [SerializeField] float _distance; //전방 시야 거리 제한
        [SerializeField] float _backsideDegree; //후방 시야 +-각도 제한(암살기능 추가예정 45,45 도)
        [SerializeField] float _backsideDistance; //후방 시야 거리 제한
        [SerializeField] LayerMask _layer;

        Vector3 _playerPosition;
        
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            IsDetectPlayer();
        }

        //bool IsGuardObjectHit()
        //{

        //}

        //overlapsphere로 Player가 있으면 바로 true를 리턴중
        //벽 체크를 어떻게 하지?
        bool IsDetectPlayer()
        {
            Collider[] collider = Physics.OverlapSphere(transform.position, _distance, _layer, QueryTriggerInteraction.Ignore);

            foreach (Collider col in collider)
            {
                if(col.gameObject.layer == 20)
                {
                    _playerPosition = col.gameObject.transform.position;
                    return true;
                }
            }
            return false;
        }
        

        //전방으로 부채꼴 만들어서 각도 안에 Player가 있는지 체크
        bool IsPlayerInEnemyDegree()
        {
            Vector3 enemyLook = transform.forward; //적의 전방 벡터
            Vector3 direction =  _playerPosition - transform.position; //Enemy 에서 Player로 가는 벡터
            float fov = Vector3.Angle(enemyLook, direction); //둘 사이의 각도를 구함

            //각도가 _degree 안쪽인지 체크
            if (fov < _degree) { return true; }
            else return false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _distance);
        }
    }
}

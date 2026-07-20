using MissionOfMercenary;
using Unity.Cinemachine;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyChase : MonoBehaviour
    {
        //Player가 총을 쏘면 그 위치를 전달받아서, 이동하게 만드는 스크립트
        [SerializeField] float _amount;

        EnemyFindArea _findArea;
    
        LayerMask _playerLayer;
        float _radius;

        Vector3 _targetPos = Vector3.zero;

        public bool DoFind { get; private set; } = false;

        public Vector3 TargetPosition { get { return _targetPos; } set { _targetPos = value; DoFind = true; } } 

        void Start()
        {
            _findArea = GetComponent<EnemyFindArea>();
        }

        private void Update()
        {
            EnemyMoveToSoundPosition();
        }

        void EnemyMoveToSoundPosition()
        {
            if(_targetPos == Vector3.zero || DoFind == false) { return; }

            _findArea.LookAtPlayer();

            //임시
            transform.position = Vector3.Lerp(transform.position, TargetPosition, _amount * Time.deltaTime);

            if(Vector3.Distance(transform.position, TargetPosition) < 0.5f)
            {
                Vector3 reTarget = FindPlayer().transform.position;

                if(reTarget == null) 
                {
                    /*원래 위치도 돌아가도록*/
                    DoFind = false;
                    return; 
                }

                _targetPos = reTarget;
            }
        }

        GameObject FindPlayer()
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, _radius, _playerLayer);

            foreach(Collider col in cols)
            {
                if(col == null) { DoFind = false; return null; }

                return col.gameObject;
            }
            return null;
        }
    }
}

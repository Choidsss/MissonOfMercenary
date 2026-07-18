using MissionOfMercenary;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyChase : MonoBehaviour
    {
        [SerializeField] float _amount;
        [SerializeField] InputReader _inputReader;

        EnemyFindArea _findArea;

        Vector3 _targetPos;
        float _radius;
        LayerMask _playerLayer;


        public bool DoFind { get; private set; } = false;
        /*
         * _findArea에서 적이 Player를 찾았다면 Plaayer를 추적하도록 함
         * 1. _findArea 에서 IsDetectedPlayer가 true가 된다면(벽 체크는 이미 하는 중) Enemy가 상태를chase로 전환
         * 
         * Enemy가 Player를 바라보는게 첫번째(이미 벽은 없고, 범위 안에 있는데 발견은 했다는거니까)
         * 이후에는 Enemy가 Player를 향해 공격을 하도록
         * 
         * 소리가 난 곳으로 이동시키는데 갔더니 오버랩스피어안에 아무것도 없으면, DoFind => false. (제자리로 다시 기도록)
         *                                                       Player가 있다면 다시 그곳으로 향하도록 DoFind = true
         *                                                       
         *                                                       다시 향했는데, Player가 IsHidden이 true라면 DoFind = false
         *                                                                                          아니라면 DoFind = true
         * 
         */

        //private void OnEnable()
        //{
        //    _inputReader.OnshotEvent += ShotPositionStored;
        //}

        //private void OnDisable()
        //{
        //    _inputReader.OnshotEvent -= ShotPositionStored;
        //}

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _findArea = GetComponent<EnemyFindArea>();
        }

        // Update is called once per frame
        void Update()
        {
        
        }


        void EnemyMoveToSoundPosition()
        {
            if(_targetPos == null || DoFind == false) { return; }

            //임시
            transform.position = Vector3.Lerp(transform.position, _targetPos, _amount * Time.deltaTime);

            if(transform.position == _targetPos)
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

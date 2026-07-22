using MissionOfMercenary;
using Unity.Cinemachine;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyChase : MonoBehaviour
    {
        //Player가 총을 쏘면 그 위치를 전달받아서, 이동하게 만드는 스크립트
        [SerializeField] float _amount;
        [SerializeField] float _arriveDistance;
        [SerializeField] float _moveSpeed;

        EnemyFindArea _findArea;
    
        LayerMask _playerLayer;
        Vector3 _targetPos = Vector3.zero;
        Vector3 _enemyOriginPosition;
        float _radius;
        bool _findPlayer = false;

        public bool DoFind { get; private set; } = false;

        public Vector3 TargetPosition { get { return _targetPos; } set { _targetPos = value; DoFind = true; } } 

        void Start()
        {
            _enemyOriginPosition = transform.position;
            _findArea = GetComponent<EnemyFindArea>();
        }

        private void Update()
        {
            EnemyMoveToSoundPosition();
        }

        //현재 Player가 총을 쏠 때마다 위치를 주변 Enemy들한테 보내는 중
        //이걸 총을 쐈을때 딱 한번만 받아오고, 그 뒤부터는 그 위치로 이동시키도록 하고싶은데(뒤부터는 주변범위에 플레이어 체크해서 그 위치를 또 한번만 받아오고 거기로 이동하고), 없으면 DoFind = false, 제자리로
        void EnemyMoveToSoundPosition()
        {
            if(_targetPos == Vector3.zero) { return; }

            Vector3 reTarget;

            DoFind = true;
            _findArea.LookAtPlayer();

            //임시
            transform.position = Vector3.Lerp(transform.position, TargetPosition, _amount * Time.deltaTime);

            //타겟위치로 갔는데 일정거리 수준보다 아래일경우, 주변에 적을 한번더 찾아서 타겟위치 갱신
            if(Vector3.Distance(transform.position, TargetPosition) < _arriveDistance)
            {
                DoFind = false;

                if(_findPlayer) 
                {
                    DoFind = true;
                    reTarget = GetPlayerObject().transform.position;
                    _targetPos = reTarget;
                    return; 
                }

                _targetPos = Vector3.zero;
                DoFind = false;

                //만약 순찰중이었다고 한다면 다시 순찰중인 위치로 돌라가도록 해야함(NavMesh)
                //지금은 임시로 originPos로 돌아가도록
                MoveToOriginPosition();
            }
        }

        GameObject GetPlayerObject()
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, _radius, _playerLayer);

            foreach(Collider col in cols)
            {
                if(col == null)
                {
                    DoFind = false;
                    _findPlayer = false;
                    return null; 
                }

                _findPlayer = true;
                return col.gameObject;
            }

            _findPlayer = false;
            return null;
        }

        void MoveToOriginPosition()
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = _enemyOriginPosition;

            //Vector3 dir = endPos - startPos;

            //제자리로 가는 코드
            transform.position = Vector3.Lerp(startPos, endPos, _moveSpeed * Time.deltaTime);
        }
    }
}

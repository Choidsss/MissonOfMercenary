using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class ChaseSoundPositionNode : BTNode
    {
        readonly float _speed;
        readonly NavMeshAgent _nav;
        readonly Vector3 _targetPosition;
        readonly Vector3 _originPosition;

        ChaseSoundPositionNode(float speed, NavMeshAgent nav, Vector3 combackPosition,Vector3 targetPosition)
        {
            _speed = speed;
            _nav = nav;
            _originPosition = combackPosition;
            _targetPosition = targetPosition;
        }

        public override State Evaluate()
        {
            if(_nav == null) { Debug.Log("NavMesh Agent Component Does Not Exist"); return State.Failure; }
            if (_targetPosition == null) { Debug.Log("Target_Position Does Not Exist"); return State.Failure; }

            _nav.speed = _speed;
            _nav.SetDestination(_targetPosition);

            if(_nav.remainingDistance <= _nav.stoppingDistance)
            {
                //그 주변으로 한번더 오버랩 스피어로 확인해봐서 있으면 다시 플레이어쪽으로 타겟을 잡음
                //그 이후에도 안보인다면 다시 _originPosition으로 복귀
            }

            return State.Running;


        }
    }
}

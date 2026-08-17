using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class EnemyPatrolNode : BTNode
    {
        readonly Transform[] _wayPoint;
        readonly float _speed;
        readonly NavMeshAgent _nav;

        int _wayPointIndex = 0;
        bool _isForward = true;

        public EnemyPatrolNode(NavMeshAgent nav,Transform[] wayPoint,float patrolSpeed)
        {
            _nav = nav;
            _wayPoint = wayPoint;
            _speed = patrolSpeed;
        }

        public override State Evaluate()
        {
            if (_wayPoint == null || _wayPoint.Length == 0) { return State.Failure; }
            if (_nav == null || _wayPoint == null) { return State.Failure; }

            if (!_nav.enabled) { return State.Failure; }

            if (_wayPointIndex >= _wayPoint.Length || _wayPoint[_wayPointIndex] == null) { return State.Failure; }

            //그냥 가만히 있으면서 러닝중 반환
            if(_wayPoint.Length == 1) { _nav.SetDestination(_wayPoint[_wayPointIndex].transform.position); return State.Running; }

            _nav.isStopped = false;
            _nav.speed = _speed;
            _nav.SetDestination(_wayPoint[_wayPointIndex].transform.position);

            if (_nav.pathPending) { return State.Running; }

            if (_nav.remainingDistance <= Mathf.Max(_nav.stoppingDistance, 0.2f))
            {
                if (_wayPointIndex == _wayPoint.Length - 1)
                {
                    _isForward = false;
                }
                else if(_wayPointIndex == 0)
                {
                    _isForward = true;
                }

                if (_isForward)
                {
                    _wayPointIndex++;
                }
                else
                {
                    _wayPointIndex--;
                }
            }

            return State.Running;
        }
    }
}

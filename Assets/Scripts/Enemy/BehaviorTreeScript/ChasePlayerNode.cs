using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class ChasePlayerNode : BTNode
    {
        readonly NavMeshAgent _agent;
        readonly EnemyFindArea _findArea;

        public ChasePlayerNode(NavMeshAgent agent, EnemyFindArea findArea)
        {
            _agent = agent;
            _findArea = findArea;
        }

        public override State Evaluate()
        {
            if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh || _findArea == null)
            {
                return State.Failure;
            }

            if (_findArea.DetectedTarget == null)
            {
                return State.Failure;
            }

            _agent.SetDestination(_findArea.DetectedTarget.position);

            if (_agent.pathPending)
            {
                return State.Running;
            }

            return _agent.remainingDistance <= _agent.stoppingDistance ? State.Success : State.Running;
        }
    }
}

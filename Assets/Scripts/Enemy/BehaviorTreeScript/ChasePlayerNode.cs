using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class ChasePlayerNode : BTNode
    {
        readonly EnemyAttack _enemyAttack;
        readonly NavMeshAgent _agent;
        readonly EnemyFindArea _findArea;

        public ChasePlayerNode(NavMeshAgent agent, EnemyFindArea findArea, EnemyAttack enemyAttack)
        {
            _agent = agent;
            _findArea = findArea;
            _enemyAttack = enemyAttack;
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

            float dis = Vector3.Distance(_findArea.transform.position, _findArea.DetectedTarget.position);

            if (dis > _enemyAttack.AttackRange)
            {
                _agent.isStopped = false;
                _agent.updateRotation = true;
                _agent.SetDestination(_findArea.DetectedTarget.position);

                return State.Running;
            }

            //if (_agent.pathPending)
            //{
            //    return State.Running;
            //}

            return State.Failure;


            //return _agent.remainingDistance > _enemyAttack.AttackRange ? State.Running : State.Success;
        }
    }
}

using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class EnemyAttackNode : BTNode
    {
        readonly EnemyAttack _enemyAttack;
        readonly EnemyFindArea _findArea;
        readonly NavMeshAgent _nav;

        public EnemyAttackNode(EnemyAttack enemyAttack, EnemyFindArea findArea, NavMeshAgent nav)
        {
            _enemyAttack = enemyAttack;
            _findArea = findArea;
            _nav = nav;
        }

        public override State Evaluate()
        {
            if(_enemyAttack == null || !_enemyAttack.enabled || _findArea == null) { return State.Failure; }
            Transform target = _findArea.DetectedTarget;

            if(target == null || !_findArea.IsDetectedPlayer) { return State.Failure; }

            float dis = Vector3.Distance(_findArea.transform.position, _findArea.DetectedTarget.position);

            if(dis <= _enemyAttack.AttackRange)
            {
                _nav.isStopped = true;
                _nav.ResetPath();

                return State.Running;
            }

            return State.Failure;
        }
    }
}

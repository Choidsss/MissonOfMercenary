using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyAttackNode : BTNode
    {
        readonly EnemyAttack _enemyAttack;
        readonly EnemyFindArea _findArea;

        public EnemyAttackNode(EnemyAttack enemyAttack, EnemyFindArea findArea)
        {
            _enemyAttack = enemyAttack;
            _findArea = findArea;
        }

        public override State Evaluate()
        {
            if(_enemyAttack == null || !_enemyAttack.enabled || _findArea == null) { return State.Failure; }
            Transform target = _findArea.DetectedTarget;

            if(target == null || !_findArea.IsDetectedPlayer) { return State.Failure; }

            return State.Running;
        }

    }
}

using UnityEngine;

namespace MIssionOfMercenary
{
    public class IsPlayerInEnemyAttackRange : BTNode
    {
        EnemyAttack _enemyAttack;
        EnemyFindArea _findArea;

        public IsPlayerInEnemyAttackRange(EnemyAttack enemyAttackScript, EnemyFindArea findArea)
        {
            _enemyAttack = enemyAttackScript;
            _findArea = findArea;
        }

        public override State Evaluate()
        {
            if(_findArea != null && !_findArea.IsDetectedPlayer) { Debug.Log("Can't Find A PlayerPosition!"); return State.Failure; }

            float attackRange = _enemyAttack.AttackRange;
            float distanceToPlayer = Vector3.Distance(_enemyAttack.transform.position, _findArea.DetectedTarget);

            if(distanceToPlayer <= attackRange)
            {
                return State.Success;
            }
            else
            {
                return State.Failure;
            }
        }
    }
}

using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class IsPlayerInEnemyAttackRange : BTNode
    {
        EnemyAttack _enemyAttack;
        EnemyAnimation _enemyAnimation;
        EnemyFindArea _findArea;
        NavMeshAgent _nav;

        public IsPlayerInEnemyAttackRange(EnemyAttack enemyAttackScript, EnemyAnimation enemyAnimation,EnemyFindArea findArea, NavMeshAgent nav)
        {
            _enemyAttack = enemyAttackScript;
            _enemyAnimation = enemyAnimation;
            _findArea = findArea;
            _nav = nav;
        }

        public override State Evaluate()
        {
            if (_findArea != null && !_findArea.IsDetectedPlayer) { Debug.Log("Can't Find A PlayerPosition!"); return State.Failure; }

            float attackRange = _enemyAttack.AttackRange;
            float distanceToPlayer = Vector3.Distance(_enemyAttack.transform.position, _findArea.DetectedTarget);

            if(distanceToPlayer <= attackRange)
            {
                Debug.Log($"**************************Stop!!!!!!!**************************");
                _nav.isStopped = true;
                _enemyAnimation.PlayEnemyChaseAnimationReset();
                return State.Success;
            }
            else
            {
                return State.Failure;
            }
        }
    }
}

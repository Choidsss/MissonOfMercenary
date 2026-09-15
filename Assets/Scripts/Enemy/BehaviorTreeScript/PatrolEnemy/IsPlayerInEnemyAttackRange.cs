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
                _nav.isStopped = true;
                _nav.updateRotation = true;
                _enemyAnimation.StopEnemyChaseAnimationReset();

                return State.Success;
            }
            else
            {
                _enemyAnimation.StopEnemyShotAnim();
                return State.Failure;
            }
        }
    }
}

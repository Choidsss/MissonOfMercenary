using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class ChaseToPlayerNode : BTNode
    {
        EnemyAttack _enemyAttack;
        EnemyAnimation _enemyAnimation;
        EnemyFindArea _findArea;
        NavMeshAgent _nav;

        public ChaseToPlayerNode(EnemyAttack enemyAttack,EnemyAnimation enemyAnimation, EnemyFindArea findArea, NavMeshAgent nav)
        {
            _enemyAttack = enemyAttack;
            _enemyAnimation = enemyAnimation;
            _findArea = findArea;
            _nav = nav;
        }

        public override State Evaluate()
        {
            if(_enemyAnimation == null || _findArea == null || _nav == null) { Debug.Log("Can't Find A Component, Please Check The Object"); return State.Failure; }

            _enemyAnimation.PlayEnemyChaseAnimation();
            _nav.SetDestination(_findArea.DetectedTarget);

            if(_nav.remainingDistance <= _enemyAttack.AttackRange)
            {
                _nav.isStopped = true;
                _enemyAnimation.PlayEnemyChaseAnimationReset();
                return State.Success;
            }

            return State.Running;
        }
    }
}

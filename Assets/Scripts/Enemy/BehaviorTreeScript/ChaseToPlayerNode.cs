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

        /*
         * ToDo : EnemyRunAnimation Will be Fixed;
         */
        public override State Evaluate()
        {
            if (_enemyAnimation == null || _findArea == null || _nav == null) { Debug.Log("Can't Find A Component, Please Check The Object"); return State.Failure; }

            //DetectedTarget에 값이 한번 담기고 그게 이미 AttackRange안쪽이면 그 위치로 고정되는거 같은데. Player의 위치가 계속 갱신되는게 아니라
            //DetectedTarget의 위치를 계속 갱신되게 하는건 안되는건가???이렇게 되야 되는디????????
            _nav.isStopped = false;
            _nav.updateRotation = true;

            _enemyAnimation.StopEnemyShotAnim();
            _enemyAnimation.PlayEnemyChaseAnimation();
            _nav.SetDestination(_findArea.DetectedTarget);

            float remainDistance = Vector3.Distance(_findArea.DetectedTarget, _findArea.transform.position);

            //if (remainDistance <= _enemyAttack.AttackRange)
            //{
            //    return State.Success;
            //}

            return State.Running;
        }
    }
}

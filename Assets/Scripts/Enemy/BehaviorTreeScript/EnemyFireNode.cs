using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class EnemyFireNode : BTNode
    {
        EnemyBT _enemyBT;
        EnemyHealth _enemyHealth;
        EnemyAttack _enemyAttack;
        EnemyFindArea _findArea;
        EnemyAnimation _enemyAnimation;
        NavMeshAgent _nav;

        /*
         * ToDo : FireAnimation Will be Implement;
         */
        public EnemyFireNode(EnemyBT bt,EnemyHealth enemyHealth, EnemyAnimation enemyAnimation ,EnemyAttack enemyAttack, EnemyFindArea findArea, NavMeshAgent nav)
        {
            _enemyBT = bt;
            _enemyHealth = enemyHealth;
            _enemyAnimation = enemyAnimation;
            _enemyAttack = enemyAttack;
            _findArea = findArea;
            _nav = nav;
        }

        public override State Evaluate()
        {
            if(_enemyAttack == null || _findArea == null) { Debug.Log("Can't Find A Component"); return State.Failure; }

            _nav.updateRotation = false;

            _enemyBT.LookAtPlayer();
            _enemyAnimation.PlayEnemyShotAnim();

            if (_enemyHealth.IsDeath)
            {
                return State.Failure;
            }

            return State.Running;
        }
    }
}

using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyFireNode : BTNode
    {
        EnemyBT _enemyBT;
        EnemyHealth _enemyHealth;
        EnemyAttack _enemyAttack;
        EnemyFindArea _findArea;
        EnemyAnimation _enemyAnimation;

        /*
         * ToDo : FireAnimation Will be Implement;
         */
        public EnemyFireNode(EnemyBT bt,EnemyHealth enemyHealth, EnemyAnimation enemyAnimation ,EnemyAttack enemyAttack, EnemyFindArea findArea)
        {
            _enemyBT = bt;
            _enemyHealth = enemyHealth;
            _enemyAnimation = enemyAnimation;
            _enemyAttack = enemyAttack;
            _findArea = findArea;
        }

        public override State Evaluate()
        {
            if(_enemyAttack == null || _findArea == null) { Debug.Log("Can't Find A Component"); return State.Failure; }

            _enemyBT.LookAtPlayer();
            //Debug.Log("Fire Node Playe");
            // _enemyAttack.AttackStart();
            //_enemyAnimation.PlayEnemyShotAnim();

            if (_enemyHealth.IsDeath)
            {
                return State.Failure;
            }

            return State.Running;
        }
    }
}

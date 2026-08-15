using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyFireNode : BTNode
    {
        EnemyHealth _enemyHealth;
        EnemyAttack _enemyAttack;
        EnemyFindArea _findArea;
        EnemyAnimation _enemyAnimation;

        public EnemyFireNode(EnemyHealth enemyHealth, EnemyAnimation enemyAnimation ,EnemyAttack enemyAttack, EnemyFindArea findArea)
        {
            _enemyHealth = enemyHealth;
            _enemyAnimation = enemyAnimation;
            _enemyAttack = enemyAttack;
            _findArea = findArea;
        }

        public override State Evaluate()
        {
            if(_enemyAttack == null || _findArea == null) { Debug.Log("Can't Find A Component"); return State.Failure; }

            _enemyAttack.AttackStart();
            _enemyAnimation.PlayEnemyShotAnim();

            if (_enemyHealth.IsDeath)
            {
                return State.Failure;
            }

            return State.Running;
        }
    }
}

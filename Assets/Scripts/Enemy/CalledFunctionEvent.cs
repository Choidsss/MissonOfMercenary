using UnityEngine;

namespace MIssionOfMercenary
{
    public class CalledFunctionEvent : MonoBehaviour
    {
        EnemyAttack _enemyAttack;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _enemyAttack = GetComponentInParent<EnemyAttack>();
        }

        void CalledOnFire()
        {
            _enemyAttack.OnFire();
        }

    }
}

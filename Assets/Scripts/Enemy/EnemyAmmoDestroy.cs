using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyAmmoDestroy : MonoBehaviour
    {
        /*
         * 1. Enemy가 총을 Player를 발견
         * 2. 바로 공격을 하는데
         *  2-1. 원뿔로 일정범위의 넓이를 잡음
         *  2-2. 그 넓이 안에서 랜덤하게 캐스트를 쏨(거리 150미터)
         *  2-3. 플레이어가 맞으면 피까이게 하면 되고, 안맞으면 그냥 아무것도 안함
         *  2-4. 단, 생성된 총알은 반드시 지워야 함
         *  
         */
        EnemyAttack _enemyAttack;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _enemyAttack = GetComponentInParent<EnemyAttack>();
        }

        // Update is called once per frame
        void Update()
        {
           // CalculateAttackDistance();
        }

        //void CalculateAttackDistance()
        //{
        //    //Vector3 target = _enemyAttack.Target;

        //    float distance = Vector3.Distance(transform.position, target);

        //    if(distance <= 0.01f)
        //    {
        //        Destroy(this.gameObject);
        //    }
        //}
    }
}

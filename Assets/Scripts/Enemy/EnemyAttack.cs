using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyAttack : MonoBehaviour
    {
        public AimType aimType { get; } = AimType.None;

        public WeaponType weaponType { get; set; } = WeaponType.AR;

        public int Damage { get; } = 30;

        public float AttackRange { get; } = 150;

        /*
         * 평소에는 Idle상태로 왔다갔다 하도록 함(NavMesh는 나중에 할 예정, Animator 상태만 바뀌도록 구현)
         * 
         * 1. 무기의 머즐을 가져와서 Trace를 하나 만든다(RayCast로)
         * 2. Enemy의 부모 중심으로, 커다란 구 형태의 범위를 하나 만든다
         * 3. 그 구 형태 안으로 Player가 진입시, 그때 Player를 바라보면서 전투상태로 돌입하도록
         */



        void Start()
        {
            
        }

        void Update()
        {
        
        }
    }
}

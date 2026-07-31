using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyAttack : MonoBehaviour
    {
        public AimType aimType { get; } = AimType.None;

        public WeaponType weaponType { get; set; } = WeaponType.AR;

        public int Damage { get; } = 30;

        public float AttackRange { get; } = 150;

        void Start()
        {
            
        }

        void Update()
        {
        
        }
    }
}

using UnityEngine;

namespace MIssionOfMercenary
{
    public interface IWeapons
    {
        public WeaponType weaponType { get; }

        public int Damage { get; }

        public float AttackRange { get; }


        void Attack();
    }
}

using UnityEngine;

namespace MIssionOfMercenary
{
    public interface IWeapons
    {
        public AimType AimType { get; }

        public WeaponType WeaponType { get; }

        public int Damage { get; }

        public float AttackRange { get; }

        public int CurrentAmmo { get; }


        //void Attack(float isShot);
    }
}

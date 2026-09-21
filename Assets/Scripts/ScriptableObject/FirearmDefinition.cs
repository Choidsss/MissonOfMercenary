using UnityEngine;

namespace MIssionOfMercenary
{
    [CreateAssetMenu(menuName = "Weapons/Firearm Definition")]
    public class FirearmDefinition : ScriptableObject
    {
        [Header("Gun Type Definition")]
        [SerializeField] WeaponType _weaponType;
        [SerializeField] AimType _aimType;

        [Header("Gun Name")]
        [SerializeField] string _gunName;

        [Header("Attack Damage Definition")]
        [SerializeField] int _damage;

        [Header("Magazine Capacity")]
        [SerializeField] int _magazineCapacity;

        [Header("Attack Range Definition")]
        [SerializeField] float _range;

        [Header("FireInterval Definition")]
        [SerializeField] float _fireInterval;

        public string GunName => _gunName;
        public int Damage => _damage;
        public int MagazineCapacity => _magazineCapacity;
        public float Range => _range;
        public float FireInterval => _fireInterval;
        public WeaponType GunWeaponType => _weaponType;
        public AimType GunAimType => _aimType;
    }
}

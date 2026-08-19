using UnityEngine;

namespace MIssionOfMercenary
{
    public enum WeaponSlot
    {
        Primary = 0,
        Secondary = 1,
        Melee = 2
    }

    public class WeaponManager : MonoBehaviour
    {

        [Header("Weapon Slot")]
        [SerializeField] GameObject _primaryWeapon;
        [SerializeField] GameObject _secondaryWeapon;
        [SerializeField] GameObject _meleeWeapon;

        GameObject[] _weapons;
        WeaponSlot _currentSlot;

        public WeaponSlot CurrentSlot => _currentSlot;
        public GameObject CurrentWeapon => _weapons[(int)_currentSlot];

        int _currentindex;

        private void Awake()
        {
            _weapons = new GameObject[] { _primaryWeapon, _secondaryWeapon, _meleeWeapon };
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}

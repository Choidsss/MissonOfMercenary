using TMPro;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class WeaponUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _ammo;
        [SerializeField] TextMeshProUGUI _weaponType;

        IWeapons _currentWeapon;
        WeaponType _currentWeaponType;
        int _currentAmmo;

        bool _isKnife = false;

        // Update is called once per frame
        void Update()
        {
            ShowAmmoAndType();
        }

        public void GetCurrentWeaponType(IWeapons weapon)
        {
            _currentWeapon = weapon;

            if (_currentWeapon.WeaponType == WeaponType.Knife) { _isKnife = true; }
        }

        void ShowAmmoAndType()
        {
            if (_isKnife)
            {
                _ammo.text = "-- / --";
                _weaponType.text = "Melee";
                _isKnife = false;
                return;
            }
            _currentAmmo = _currentWeapon.CurrentAmmo;
            _currentWeaponType = _currentWeapon.WeaponType;

            _ammo.text = _currentAmmo.ToString();
            _weaponType.text = _currentWeaponType.ToString();
        }
    }
}

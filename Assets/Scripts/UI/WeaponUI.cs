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

            if (_currentWeapon.weaponType == WeaponType.Knife) { _isKnife = true; }
            else { _isKnife = false; }
        }

        void ShowAmmoAndType()
        {
            if (_isKnife)
            {
                _ammo.text = "-- / --";
                _weaponType.text = _currentWeaponType.ToString();
                return;
            }

            _currentAmmo = _currentWeapon.Ammo;
            _currentWeaponType = _currentWeapon.weaponType;

            _ammo.text = _currentAmmo.ToString();
            _weaponType.text = _currentWeaponType.ToString();
        }
    }
}

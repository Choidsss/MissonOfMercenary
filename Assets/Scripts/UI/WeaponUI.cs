using TMPro;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class WeaponUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _ammo;
        [SerializeField] TextMeshProUGUI _weaponType;

        IWeapons _currentWeapon;

        void Update()
        {
            ShowAmmoAndType();
        }

        public void GetCurrentWeaponType(IWeapons weapon)
        {
            _currentWeapon = weapon;
            ShowAmmoAndType();
        }

        void ShowAmmoAndType()
        {
            if (_currentWeapon == null)
            {
                _ammo.text = "--";
                _weaponType.text = "None";
                return;
            }
            _weaponType.text = _currentWeapon.WeaponType.ToString();
            if(_currentWeapon is IFirearm firearm)
            {
                _ammo.text = firearm.CurrentAmmo.ToString();
            }
            else
            {
                _ammo.text = "-- / --";
            }
        }
    }
}

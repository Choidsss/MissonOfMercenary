using TMPro;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class WeaponUI : MonoBehaviour
    {
        [SerializeField] AssultRifle _ar;
        [SerializeField] TextMeshProUGUI _ammo;
        [SerializeField] TextMeshProUGUI _weaponType;

        // Update is called once per frame
        void Update()
        {
            ShowAmmoAndType();
        }

        void ShowAmmoAndType()
        {
            _ammo.text = _ar.Ammo.ToString();
            _weaponType.text = _ar.weaponType.ToString();
        }
    }
}

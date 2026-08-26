using TMPro;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class PickupWeaponUI : MonoBehaviour
    {
        [SerializeField] GameObject _pickupPanel;
        [SerializeField] TextMeshProUGUI _text;
        [SerializeField] WeaponPickup _player;

        void Start()
        {
            _pickupPanel.SetActive(false);
        }

        void Update()
        {
            //ShowPickupText();
        }

        void ShowPickupText()
        {
            if (!_player.CanPickup) 
            {
                _pickupPanel.SetActive(false);
                return; 
            }

            _text.text = $"Press 'E', Change Weapon To {gameObject.name}√—±‚¿« ¿Ã∏ß";
            _pickupPanel.SetActive(true);
        }
    }
}

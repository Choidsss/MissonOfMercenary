using UnityEngine;

namespace MIssionOfMercenary
{
    public class DroppedWeapons : MonoBehaviour
    {
        [SerializeField] WeaponSlot _slot;
        [SerializeField] GameObject _enEquipedWeaponPrefab;
        [SerializeField] string _displayName;

        public WeaponSlot Slot => _slot;
        public GameObject EnEquipedWeaponPrefab => _enEquipedWeaponPrefab;
        public string DisplayName => _displayName;


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

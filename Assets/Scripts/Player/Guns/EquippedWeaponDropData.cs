using UnityEngine;

namespace MIssionOfMercenary
{
    public class EquippedWeaponDropData : MonoBehaviour
    {
        [SerializeField] GameObject _droppedWeaponPrefab;

        public GameObject DroppedWeaponPrefab => _droppedWeaponPrefab; // 교체할 때 생성할 바닥용 무기 프리팹입니다. By_Codex
    }
}

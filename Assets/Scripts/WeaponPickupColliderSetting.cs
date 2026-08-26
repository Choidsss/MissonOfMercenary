using UnityEngine;

namespace MIssionOfMercenary
{
    public class WeaponPickupColliderSetting : MonoBehaviour
    {
        [Header("Pickup Collider Settings")]

        [Tooltip("자동 생성될 자식 오브젝트 이름")]
        [SerializeField] private string _triggerObjectName = "PickupTrigger";

        [Tooltip("총기 모델보다 추가로 넓힐 여백")]
        [SerializeField] private float _padding = 0.1f;

        public string TriggerObjectName => _triggerObjectName;
        public float Padding => _padding;
    }
}

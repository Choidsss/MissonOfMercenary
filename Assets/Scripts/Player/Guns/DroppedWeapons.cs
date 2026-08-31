using UnityEngine;
using MissionOfMercenary;

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

        public void SetDroppedState()
        {
            SetWeaponBehavioursEnabled(false);
            SetPickupCollidersEnabled(true);

            Rigidbody rigidbody = GetComponent<Rigidbody>();
            if(rigidbody == null)
            {
                rigidbody = gameObject.AddComponent<Rigidbody>();
            }

            rigidbody.isKinematic = false;
            rigidbody.useGravity = true; // 장착 무기 복제본을 바닥에서 물리 동작하는 무기로 전환합니다. By_Codex
        }

        public void SetEquippedState()
        {
            SetWeaponBehavioursEnabled(true);
            SetPickupCollidersEnabled(false);

            Rigidbody rigidbody = GetComponent<Rigidbody>();
            if(rigidbody != null)
            {
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
            }
        }

        void SetWeaponBehavioursEnabled(bool enabled)
        {
            MonoBehaviour[] behaviours = GetComponentsInChildren<MonoBehaviour>(true);

            foreach(MonoBehaviour behaviour in behaviours)
            {
                if(behaviour is IWeapons)
                {
                    behaviour.enabled = enabled; // 바닥 무기가 입력 이벤트를 받지 않도록 총기 동작 스크립트만 제어합니다. By_Codex
                }
            }
        }

        void SetPickupCollidersEnabled(bool enabled)
        {
            Transform pickupTrigger = transform.Find("PickupTrigger");
            if(pickupTrigger == null) { return; }

            Collider[] colliders = pickupTrigger.GetComponentsInChildren<Collider>(true);
            foreach(Collider collider in colliders)
            {
                collider.enabled = enabled; // 장착 중에는 자신의 Pickup Trigger를 감지하지 않도록 합니다. By_Codex
            }
        }

    }
}

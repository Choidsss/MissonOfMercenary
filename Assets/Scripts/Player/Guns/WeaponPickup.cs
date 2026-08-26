using MissionOfMercenary;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class WeaponPickup : MonoBehaviour
    {
        [SerializeField] InputReader _inputReader;

        [Header("Variable Options")]
        [SerializeField] float _offsetX;
        [SerializeField] float _offsetZ;
        [SerializeField] float _radius;
        [SerializeField] float _maxDistance;
        [SerializeField] LayerMask _layer;

        bool _dropWeapon = false;
        bool _canPickup = false;

        public bool CanPickup { get {return _canPickup; } }

        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            
        }

        void Update()
        {
            //PickupAndWeaponChange();
        }

        void PickupAndWeaponChange(GameObject weapon)
        {
            if (FindDroppedWeaponOverlapSphere() && FindDroppedWeaponRaycast())
            {
                //들고있는 총기의 정보를 함수에 집어넣음
                //떨어져 있는 총기를 GameObject로 저장해둔 다음, 원본은 Destroy()
                //이미 들고 있던 총기는 그대로 바닥에 Drop
                //주운 총기는 이미 들고있던 총기의 Transform을 물려받아 사용함
                //But, GripPoint 의 IK위치는 먼저 작업을 해야함
            }
        }

        bool FindDroppedWeaponOverlapSphere()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, _radius, _layer, QueryTriggerInteraction.Collide);

            foreach (Collider col in colliders)
            {
                if (colliders.Length == 0)
                {
                    _dropWeapon = false;
                    _canPickup = false;
                    Debug.Log("주울 수 있는 총기가 존재하지 않습니다.");
                }

                _dropWeapon = true;
            }

            return _dropWeapon;
        }

        bool FindDroppedWeaponRaycast()
        {
            if (!_dropWeapon) { Debug.Log("주울 수 있는 총기가 존재하지 않습니다."); }

            bool isWeaponHit = Physics.Raycast(transform.position, transform.forward, _maxDistance, _layer, QueryTriggerInteraction.Collide);

            if (isWeaponHit)
            {
                _canPickup = true;
            }
            else
            {
                _canPickup = false;
            }

            return _canPickup;
        }

    }
}

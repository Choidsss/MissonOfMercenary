using MissionOfMercenary;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class WeaponPickup : MonoBehaviour
    {
        //[SerializeField] Transform _weaponMount;
        [SerializeField] WeaponManager _weaponManager;
        [SerializeField] InputReader _inputReader;

        DroppedWeapons _targetWeapon;

        [Header("Variable Options")]
        [SerializeField] float _offsetX;
        [SerializeField] float _offsetZ;
        [SerializeField] float _radius;
        [SerializeField] float _maxDistance;
        [SerializeField] LayerMask _layer;
        [SerializeField] Transform _pickupOrigin; 

        //bool _dropWeapon = false;
        bool _canPickup = false;

        public bool CanPickup { get {return _canPickup; } }

        public DroppedWeapons TargetWeapon => _targetWeapon;

        private void OnEnable()
        {
            _inputReader.OnPickedUpAction += TryPickUp;
        }

        private void OnDisable()
        {
            _inputReader.OnPickedUpAction -= TryPickUp;
        }

        void Update()
        {
            UpdateTarget();
        }
        void UpdateTarget()
        {
            _targetWeapon = null;
            _canPickup = false;

            Transform castOrigin = _pickupOrigin != null ? _pickupOrigin : transform; 
            Vector3 origin = castOrigin.position + castOrigin.right * _offsetX + castOrigin.forward * _offsetZ;

            bool hitWeapon = Physics.SphereCast(
                origin,
                _radius,
                castOrigin.forward,
                out RaycastHit hit,
                _maxDistance,
                _layer,
                QueryTriggerInteraction.Collide); 

            if(!hitWeapon) 
            {
                //Debug.Log("***************hitWeapon False***************"); 
                return;
            }

            _targetWeapon = hit.collider.gameObject.GetComponentInParent<DroppedWeapons>();
            _canPickup = _targetWeapon != null;
        }

        void TryPickUp()
        {
            if (_targetWeapon == null || _weaponManager == null)
            {
                return;
            }

            DroppedWeapons pickedWeapon = _targetWeapon;

            bool succeed = _weaponManager.TryReplacedWeapon(_targetWeapon.Slot, pickedWeapon.EnEquipedWeaponPrefab, pickedWeapon.transform.position, pickedWeapon.transform.rotation);

            //_weaponManager.ReplacedWeapon( pickedWeapon.Slot, pickedWeapon.EnEquipedWeaponPrefab,pickedWeapon.transform.position,pickedWeapon.transform.rotation); // 주운 무기가 있던 자리에 기존 무기를 내려놓습니다. By_Codex

            if (!succeed)
            {
                return;
            }

            _targetWeapon = null;
            _canPickup = false;
            Destroy(pickedWeapon.gameObject);
        }

        void OnDrawGizmosSelected()
        {
            Transform castOrigin = _pickupOrigin != null ? _pickupOrigin : transform;
            Vector3 origin = castOrigin.position + castOrigin.right * _offsetX + castOrigin.forward * _offsetZ;
            Vector3 end = origin + castOrigin.forward * _maxDistance;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(origin, _radius);
            Gizmos.DrawLine(origin, end);
            Gizmos.DrawWireSphere(end, _radius); 
        }

        //bool FindDroppedWeaponOverlapSphere()
        //{
        //    Collider[] colliders = 

        //    foreach (Collider col in colliders)
        //    {
        //        if (colliders.Length == 0)
        //        {
        //            _dropWeapon = false;
        //            _canPickup = false;
        //            Debug.Log("주울 수 있는 총기가 존재하지 않습니다.");
        //        }

        //        _dropWeapon = true;
        //    }

        //    return _dropWeapon;
        //}

        //bool FindDroppedWeaponRaycast()
        //{
        //    if (!_dropWeapon) { Debug.Log("주울 수 있는 총기가 존재하지 않습니다."); }

        //    bool isWeaponHit = Physics.Raycast(transform.position, transform.forward, _maxDistance, _layer, QueryTriggerInteraction.Collide);

        //    if (isWeaponHit)
        //    {
        //        _canPickup = true;
        //    }
        //    else
        //    {
        //        _canPickup = false;
        //    }

        //    return _canPickup;
        //}

    }
}

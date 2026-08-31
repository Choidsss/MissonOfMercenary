using MissionOfMercenary;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class WeaponPickup : MonoBehaviour
    {
        [SerializeField] WeaponManager _weaponManager;
        [SerializeField] InputReader _inputReader;

        DroppedWeapons _targetWeapon;

        [Header("Variable Options")]
        [SerializeField] float _offsetX;
        [SerializeField] float _offsetZ;
        [SerializeField] float _radius;
        [SerializeField] float _maxDistance;
        [SerializeField] LayerMask _layer;
        [SerializeField] Transform _pickupOrigin; // SphereCast의 시작점과 방향입니다. MainCamera 또는 PickupOrigin을 연결합니다. By_Codex

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
            //PickupAndWeaponChange();
        }

        //void PickupAndWeaponChange(GameObject weapon)
        //{
        //    if (FindDroppedWeaponOverlapSphere() && FindDroppedWeaponRaycast())
        //    {
        //        //들고있는 총기의 정보를 함수에 집어넣음
        //        //떨어져 있는 총기를 GameObject로 저장해둔 다음, 원본은 Destroy()
        //        //이미 들고 있던 총기는 그대로 바닥에 Drop
        //        //주운 총기는 이미 들고있던 총기의 Transform을 물려받아 사용함
        //        //But, GripPoint 의 IK위치는 먼저 작업을 해야함
        //    }
        //}

        void UpdateTarget()
        {
            _targetWeapon = null;
            _canPickup = false;

            /* 기존에는 Player 루트의 forward를 사용해서 카메라 방향과 Cast 방향이 달라질 수 있었습니다. By_Codex
            Vector3 origin = transform.position + transform.right * _offsetX + transform.forward * _offsetZ;

            bool hitWeapon = Physics.SphereCast(origin, _radius, transform.forward, out RaycastHit hit, _maxDistance, _layer, QueryTriggerInteraction.Collide);
            */

            Transform castOrigin = _pickupOrigin != null ? _pickupOrigin : transform; // 참조가 없으면 기존 Transform을 사용합니다. By_Codex
            Vector3 origin = castOrigin.position + castOrigin.right * _offsetX + castOrigin.forward * _offsetZ;

            bool hitWeapon = Physics.SphereCast(
                origin,
                _radius,
                castOrigin.forward,
                out RaycastHit hit,
                _maxDistance,
                _layer,
                QueryTriggerInteraction.Collide); // 지정한 Origin이 바라보는 방향으로 검사합니다. By_Codex

            if(!hitWeapon) { Debug.Log("***************hitWeapon False***************"); return; }

            Debug.Log("!!!!!!!!!!!!!!!!!!!!!hitWeapon True!!!!!!!!!!!!!!!!!!!!!");
            _targetWeapon = hit.collider.gameObject.GetComponentInParent<DroppedWeapons>();
            _canPickup = _targetWeapon != null;
        }

        void TryPickUp()
        {
            //Debug.Log("e입력 들어옴");

            if (_targetWeapon == null)
            {
                //Debug.Log("감지된 무기 없음");
                return;
            }

            Debug.Log("픽업 시도키입");

            DroppedWeapons pickedWeapon = _targetWeapon;
            _targetWeapon = null;

            /* 기존 코드는 기존 무기를 바닥에 생성할 위치를 전달하지 않았습니다. By_Codex
            _weaponManager.ReplacedWeapon(pickedWeapon.Slot, pickedWeapon.EnEquipedWeaponPrefab);
            */

            _weaponManager.ReplacedWeapon(
                pickedWeapon.Slot,
                pickedWeapon.EnEquipedWeaponPrefab,
                pickedWeapon.transform.position,
                pickedWeapon.transform.rotation); // 주운 무기가 있던 자리에 기존 무기를 내려놓습니다. By_Codex

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
            Gizmos.DrawWireSphere(end, _radius); // Scene 뷰에 SphereCast의 시작점과 끝점을 표시합니다. By_Codex
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

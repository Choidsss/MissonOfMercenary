using MissionOfMercenary;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class PlayerAimController : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] InputReader _inputReader;
        [SerializeField] Transform _aimingWeapon;
        [SerializeField] Transform _cameraAimPoint;
        [SerializeField] Transform _weaponAimPoint;

        [Header("Camera Zoom")]
        [SerializeField] Camera _mainCamera;
        [SerializeField] Camera _weaponCamera;
        [SerializeField, Range(10f, 100f)] float _aimFieldOfView = 40f;
        [SerializeField, Min(0.01f)] float _zoomSpeed = 8f;

        [Header("Aim Motion")]
        [SerializeField, Min(0.01f)] float _aimSpeed;

        Vector3 _normalLocalPosition;
        Quaternion _normalLocalRotation;
        Vector3 _weaponAimLocalPosition;
        Quaternion _weaponAimLocalRotation;
        float _normalMainFieldOfView;
        float _normalWeaponFieldOfView;

        bool _isAiming = false;
        bool _canAim;

        public bool IsAiming { get { return _isAiming; } private set { _isAiming = value; } }

        private void OnEnable()
        {
            _inputReader.OnZoomInToggleAction += ToggleAim;
        }

        private void OnDisable()
        {
            _inputReader.OnZoomInToggleAction -= ToggleAim;
        }

        void Awake()
        {
            AimingInitialize();
        }

        void LateUpdate()
        {
            //AimingWeapon();
            if (_isAiming)
            {
                MoveToAimPose();
            }
            else
            {
                ReturnToNormalPose();
            }

            UpdateFieldOfView();
        }

        void AimingInitialize()
        {
            //Debug.Log("AimingInitialize Called");
            if (_aimingWeapon == null || _cameraAimPoint == null || _weaponAimPoint == null || _mainCamera == null)
            {
                Debug.Log("Aim Reference가 없습니다");
                enabled = false;
                return;
            }

            // 조준 해제 시 각 카메라의 원래 FOV로 복귀하도록 저장한다.
            _normalMainFieldOfView = _mainCamera.fieldOfView;
            _normalWeaponFieldOfView = _weaponCamera != null ? _weaponCamera.fieldOfView: _normalMainFieldOfView;

            _normalLocalPosition = _aimingWeapon.localPosition;
            _normalLocalRotation = _aimingWeapon.localRotation;

            _weaponAimLocalPosition = _aimingWeapon.InverseTransformPoint(_weaponAimPoint.position);
            _weaponAimLocalRotation = Quaternion.Inverse(_aimingWeapon.rotation) * _weaponAimPoint.rotation;
        }

        void MoveToAimPose()
        {
            //Debug.Log("AimingWeapon Called");
            Quaternion targetRotation = _cameraAimPoint.rotation * Quaternion.Inverse(_weaponAimLocalRotation);
            Vector3 targetPosition = _cameraAimPoint.position - targetRotation * _weaponAimLocalPosition;

            float blend = 1f - Mathf.Exp(-_aimSpeed * Time.deltaTime);

            _aimingWeapon.SetPositionAndRotation(Vector3.Lerp(_aimingWeapon.position, targetPosition, blend), Quaternion.Slerp(_aimingWeapon.rotation, targetRotation, blend));

            //_aimingWeapon.transform.localPosition = Vector3.Lerp(_aimingWeapon.localPosition, targetPosition, blend);
            //_aimingWeapon.transform.localRotation = Quaternion.Slerp(_aimingWeapon.localRotation, targetRotation, blend);
        }

        void ReturnToNormalPose()
        {
            Transform weaponParent = _aimingWeapon.parent;
            if (weaponParent == null) { return; }

            Vector3 targetPosition = weaponParent.TransformPoint(_normalLocalPosition);
            Quaternion targetRotation = weaponParent.rotation * _normalLocalRotation;

            float blend = 1f - Mathf.Exp(-_aimSpeed * Time.deltaTime);

            _aimingWeapon.SetPositionAndRotation(Vector3.Lerp(_aimingWeapon.position, targetPosition, blend), Quaternion.Slerp(_aimingWeapon.rotation, targetRotation, blend));
        }

        void UpdateFieldOfView()
        {
            float blend = 1f - Mathf.Exp(-_zoomSpeed * Time.deltaTime);
            float mainTargetFov = _isAiming ? _aimFieldOfView : _normalMainFieldOfView;

            _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, mainTargetFov,blend);

            if (_weaponCamera != null)
            {
                float weaponTargetFov = _isAiming? _aimFieldOfView : _normalWeaponFieldOfView;

                _weaponCamera.fieldOfView = Mathf.Lerp(_weaponCamera.fieldOfView,weaponTargetFov, blend);
            }
        }

        void ToggleAim()
        {
            if (!_canAim)
            {
                return;
            }

            _isAiming = !_isAiming;
        }

        public void SetCurrentWeapon(Transform weapon, Transform weaponAimPoint, bool canAim)
        {
            _aimingWeapon = weapon;
            _weaponAimPoint = weaponAimPoint;
            _canAim = canAim && weapon != null && weaponAimPoint != null;

            if (_aimingWeapon != null)
            {
                _normalLocalPosition = _aimingWeapon.localPosition;
                _normalLocalRotation = _aimingWeapon.localRotation;
            }

            if (_canAim)
            {
                _weaponAimLocalPosition = _aimingWeapon.InverseTransformPoint(_weaponAimPoint.position);
                _weaponAimLocalRotation = Quaternion.Inverse(_aimingWeapon.rotation) * _weaponAimPoint.rotation;
            }
            else
            {
                _isAiming = false;
            }
        }
    }
}

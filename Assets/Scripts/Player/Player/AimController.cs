using MissionOfMercenary;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.LookDev;

namespace MIssionOfMercenary
{
    public class AimController : MonoBehaviour
    {
        SkinnedMeshRenderer _skinnedMeshRenderer;
        
        [SerializeField] InputReader _inputReader;
        [SerializeField] AssultRifle _ar;
        [SerializeField] IKController _ik;
        [SerializeField] GameObject _crossHair;
        [SerializeField] ArmMeshOffset _offset;

        [Header("Fov Options")]
        [SerializeField] Camera _mainCamera;
        [SerializeField] Camera _weaponCamera;
        [SerializeField] float _normalFov;
        [SerializeField] float _aimFov;
        [SerializeField] float _aimSpeed;

        [Header("Aim Option")]
        [SerializeField] float _tiltSpeed;
        [SerializeField] float _returnTiltSpeed;
        [SerializeField] Transform _aimPosition;
        [SerializeField] Transform _gunPosition;

        Vector3 _defaultPosition;
        Quaternion _defaultRotation;
        //Vector3 _interpolationX;
        public bool IsAiming { get; private set; } = false;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            //_interpolationX = new Vector3(_offset.OffsetX, 0, 0);
            _normalFov = _mainCamera.fieldOfView;

            _defaultPosition = _gunPosition.localPosition;
            _defaultRotation = _gunPosition.localRotation;
        }

        private void Update()
        {
            UpdateFov();
            Aiming();
        }

        private void OnEnable()
        {
            _inputReader.OnZoomInToggleAction += AimHandle;
        }

        private void OnDisable()
        {
            _inputReader.OnZoomInToggleAction -= AimHandle;
        }

        void UpdateFov()
        {
            float targetFov = IsAiming ? _aimFov : _normalFov;

            _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, targetFov, _aimSpeed * Time.deltaTime);
            _weaponCamera.fieldOfView = _mainCamera.fieldOfView; // µø±‚»≠
        }

        void AimHandle()
        {
            if (_ar.aimType == AimType.IronSight) return;
            IsAiming = !IsAiming;
            _offset.SetOffsetActive(!IsAiming);

            Aiming();
        }

        public void Aiming()
        {
            if (IsAiming)
            {
                _ik.enabled = false;
                _skinnedMeshRenderer.enabled = false;
                _crossHair.SetActive(false);
                _gunPosition.localPosition = Vector3.Lerp(_gunPosition.localPosition, _aimPosition.localPosition, _tiltSpeed * Time.deltaTime);
                _gunPosition.localRotation = Quaternion.Lerp(_gunPosition.localRotation, _aimPosition.localRotation, _tiltSpeed * Time.deltaTime);
            }
            else
            {
                _ik.enabled = true;
                _skinnedMeshRenderer.enabled = true;
                _crossHair.SetActive(true);
                _gunPosition.localPosition = Vector3.Lerp(_gunPosition.localPosition, _defaultPosition, _returnTiltSpeed * Time.deltaTime);
                _gunPosition.localRotation = Quaternion.Lerp(_gunPosition.localRotation, _defaultRotation, _returnTiltSpeed * Time.deltaTime);
            }
        }
    }
}

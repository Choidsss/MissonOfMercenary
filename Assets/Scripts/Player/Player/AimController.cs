using MissionOfMercenary;
using UnityEngine;
using UnityEngine.Rendering.LookDev;

namespace MIssionOfMercenary
{
    public class AimController : MonoBehaviour
    {
        [SerializeField] InputReader _inputReader;
        [SerializeField] AssultRifle _ar;
        [SerializeField] WeaponSway _sway;
        [SerializeField] GameObject _crossHair;

        [Header("Fov Options")]
        [SerializeField] Camera _mainCamera;
        [SerializeField] Camera _weaponCamera;
        [SerializeField] float _normalFov;
        [SerializeField] float _aimFov;
        [SerializeField] float _aimSpeed;

        [Header("Sensitivity")]
        [SerializeField] float _normalSensitivity;
        [SerializeField] float _aimSensitivity;

        [Header("Aim Option")]
        [SerializeField] float _tiltSpeed;
        [SerializeField] Transform _aimPosition;
        [SerializeField] Transform _gunPosition;

        Vector3 _defaultPosition;
        Quaternion _defaultRotation;
        public bool IsAiming { get; private set; } = false;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
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

            Aiming();
        }

        public void Aiming()
        {
            if (IsAiming)
            {
                _sway.enabled = false;
                _crossHair.SetActive(false);
                _gunPosition.localPosition = Vector3.Lerp(_gunPosition.localPosition, _aimPosition.localPosition, _tiltSpeed * Time.deltaTime);
                _gunPosition.localRotation = Quaternion.Lerp(_gunPosition.localRotation, _aimPosition.localRotation, _tiltSpeed * Time.deltaTime);
            }
            else
            {
                _sway.enabled = true;
                _crossHair.SetActive(false);
                _gunPosition.localPosition = Vector3.Lerp(_gunPosition.localPosition, _defaultPosition, _tiltSpeed * Time.deltaTime);
                _gunPosition.localRotation = Quaternion.Lerp(_gunPosition.localRotation, _defaultRotation, _tiltSpeed * Time.deltaTime);
            }
        }
    }
}

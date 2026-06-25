using MissionOfMercenary;
using UnityEngine;
using UnityEngine.Rendering.LookDev;

namespace MIssionOfMercenary
{
    public class AimController : MonoBehaviour
    {
        [SerializeField] InputReader _inputReader;
        [SerializeField] AssultRifle _ar;

        [Header("Fov Options")]
        [SerializeField] Camera _mainCamera;
        [SerializeField] Camera _weaponCamera;
        [SerializeField] float _normalFov;
        [SerializeField] float _aimFov;
        [SerializeField] float _aimSpeed;

        [Header("Sensitivity")]
        [SerializeField] float _normalSensitivity;
        [SerializeField] float _aimSensitivity;

        [SerializeField] GunTransformCorrection _transformCorrection;

        public bool IsAiming { get; private set; } = false;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _normalFov = _mainCamera.fieldOfView;
            //_mainCamera.depth = 0;
            //_weaponCamera.depth = 1;

            //_mainCamera.clearFlags = CameraClearFlags.Skybox;
            //_weaponCamera.clearFlags = CameraClearFlags.Depth;
        }

        private void Update()
        {
            UpdateFov();
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

            _transformCorrection.Aiming();
        }
    }
}

using JetBrains.Annotations;
using MissionOfMercenary;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.LookDev;

namespace MIssionOfMercenary
{
    public class AimController : MonoBehaviour
    {
        SkinnedMeshRenderer _skinnedMeshRenderer;
        HeadBob _headBob;
        
        [SerializeField] InputReader _inputReader;
        [SerializeField] AssultRifle _ar;
        [SerializeField] IKController _ik;
        [SerializeField] GameObject _crossHair;
        [SerializeField] GameObject _cam;
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

        [Header("Recoil Interpolate")]
        [SerializeField] float _upDown;
        [SerializeField] float _vibration;
        [SerializeField] float _snapSpeed;
        [SerializeField] float _recoverySpeed;

        Vector3 _cameraOriginPos;
        Vector3 _currentRecoilPos;
        Vector3 _targetRecoilPos;

        Vector3 _defaultPosition;
        Quaternion _defaultRotation;

        public bool IsAiming { get; private set; } = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _headBob = GetComponent<HeadBob>();
            _skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            _cameraOriginPos = _cam.transform.localPosition;

            _normalFov = _mainCamera.fieldOfView;

            _defaultPosition = _gunPosition.localPosition;
            _defaultRotation = _gunPosition.localRotation;
        }

        private void Update()
        {
            UpdateFov();
            Aiming();

            _targetRecoilPos = Vector3.Lerp(_targetRecoilPos, Vector3.zero, _recoverySpeed * Time.deltaTime);
            _cam.transform.localPosition = _cameraOriginPos + _targetRecoilPos;
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
                _headBob.enabled = false;
                _ik.enabled = false;
                _skinnedMeshRenderer.enabled = false;
                _crossHair.SetActive(false);
                _gunPosition.localPosition = Vector3.Lerp(_gunPosition.localPosition, _aimPosition.localPosition, _tiltSpeed * Time.deltaTime);
                _gunPosition.localRotation = Quaternion.Lerp(_gunPosition.localRotation, _aimPosition.localRotation, _tiltSpeed * Time.deltaTime);

            }
            else
            {
                _headBob.enabled = true;
                _ik.enabled = true;
                _skinnedMeshRenderer.enabled = true;
                _crossHair.SetActive(true);
                _gunPosition.localPosition = Vector3.Lerp(_gunPosition.localPosition, _defaultPosition, _returnTiltSpeed * Time.deltaTime);
                _gunPosition.localRotation = Quaternion.Lerp(_gunPosition.localRotation, _defaultRotation, _returnTiltSpeed * Time.deltaTime);
            }
        }

        public void ApplyRecoilDuringAiming()
        {
            _targetRecoilPos = new Vector3(-_upDown, Random.Range(-_vibration, _vibration), 0f);
        }
    }
}

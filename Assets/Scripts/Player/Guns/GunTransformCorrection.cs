using MissionOfMercenary;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class GunTransformCorrection : MonoBehaviour
    {
        [SerializeField] Transform _leftHand;
        [SerializeField] Transform _gun;
        [SerializeField] PlayerMove _playerMove;
        [SerializeField] GameObject _camera;

        [SerializeField] Transform _aimPosition;
        [SerializeField] AimController _aimController;

        [Header("Run Correction Options")]
        [SerializeField] float _runTiltAngle;
        [SerializeField] float _tiltSpeed;
        [SerializeField] Vector3 _positionOffset;

        Vector3 _defaultPosition;
        Quaternion _defaultRotation;

        private void Start()
        {
            _defaultPosition = _gun.position;
            _defaultRotation = _gun.rotation;
        }

        private void LateUpdate()
        {
            if (_aimController.IsAiming)
            {
                Aiming();
                return;
            }

            // 손 위치 + 오프셋 (오른쪽으로 옮기려면 X값을 양수로)
            Vector3 targetPos = _leftHand.position + transform.right * _positionOffset.x
                                               + transform.up * _positionOffset.y
                                               + transform.forward * _positionOffset.z;
            _gun.position = Vector3.Lerp(_gun.position, targetPos, _tiltSpeed * Time.deltaTime);

            //DoRun changed true, Gun Rotate
            if (_playerMove.DoRun)
            {
                Quaternion rot = _camera.transform.rotation * Quaternion.Euler(0f, _runTiltAngle, 0f);
                _gun.rotation = Quaternion.Lerp(_gun.rotation, rot, _tiltSpeed * Time.deltaTime);
            }
            else
            {
                Quaternion rot = _camera.transform.rotation * Quaternion.Euler(0f, 0f, 0f);
                _gun.rotation = Quaternion.Lerp(_gun.rotation, rot, _tiltSpeed * Time.deltaTime);

                //_gun.rotation = _originRotation;
                //_gun.rotation = Quaternion.Lerp(_gun.rotation, _originRotation, _tiltSpeed * Time.deltaTime);
            }

            // 뛸 때 Y축으로 기울이기
            //float targetTilt = _playerMove.DoRun ? _runTiltAngle : 0f;
            //_currentTilt = Mathf.Lerp(_currentTilt, targetTilt, _tiltSpeed * Time.deltaTime);
            //_gun.rotation = _leftHand.rotation * Quaternion.Euler(0f, -_currentTilt, 0f);

        }

        public void Aiming()
        {
            if (_aimController.IsAiming)
            {
                _gun.position = Vector3.Lerp(_gun.position, _aimPosition.position, _tiltSpeed * Time.deltaTime);
                _gun.rotation = Quaternion.Lerp(_gun.rotation, _aimPosition.rotation, _tiltSpeed * Time.deltaTime);
            }
            else
            {
                _gun.position = Vector3.Lerp(_gun.position, _defaultPosition, _tiltSpeed * Time.deltaTime);
                _gun.rotation = Quaternion.Lerp(_gun.rotation, _defaultRotation, _tiltSpeed * Time.deltaTime);
            }
        }
    }
}

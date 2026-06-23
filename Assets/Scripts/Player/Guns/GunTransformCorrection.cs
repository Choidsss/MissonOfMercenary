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

        [Header("Run Correction Options")]
        [SerializeField] float _runTiltAngle;
        [SerializeField] float _tiltSpeed;
        [SerializeField] Vector3 _positionOffset;

        //float _currentTilt = 0.0f;
        //Quaternion _originRotation;

        private void Start()
        {
            //_originRotation = _gun.rotation;
        }

        private void LateUpdate()
        {
            //_gun.position = _leftHand.position * _positionOffset;

            //if (_playerMove.DoRun)
            //{
            //    //총을 90도로 돌림
            //}

            // 손 위치 + 오프셋 (오른쪽으로 옮기려면 X값을 양수로)
            _gun.position = _leftHand.position + transform.right * _positionOffset.x
                                               + transform.up * _positionOffset.y
                                               + transform.forward * _positionOffset.z;
            //DoRun changed true, Gun Rotate
            if (_playerMove.DoRun)
            {
                _gun.rotation = _camera.transform.rotation * Quaternion.Euler(0f, _runTiltAngle, 0f);
            }
            else
            {
                _gun.rotation = _camera.transform.rotation * Quaternion.Euler(0f, 0f, 0f);

                //_gun.rotation = _originRotation;
                //_gun.rotation = Quaternion.Lerp(_gun.rotation, _originRotation, _tiltSpeed * Time.deltaTime);
            }

            // 뛸 때 Y축으로 기울이기
            //float targetTilt = _playerMove.DoRun ? _runTiltAngle : 0f;
            //_currentTilt = Mathf.Lerp(_currentTilt, targetTilt, _tiltSpeed * Time.deltaTime);
            //_gun.rotation = _leftHand.rotation * Quaternion.Euler(0f, -_currentTilt, 0f);
        }
    }
}

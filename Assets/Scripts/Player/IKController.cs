using MissionOfMercenary;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using static UnityEngine.Rendering.DebugUI;

namespace MIssionOfMercenary
{
    public class IKController : MonoBehaviour
    {
        AssultRifle _assultRifle;

        [SerializeField] InputReader _inputReader;

        [Header("References")]
        [SerializeField] PlayerMove _playerMove;
        [SerializeField] Transform _weaponBox;
        [SerializeField] Transform _weaponPivot;
        [SerializeField] Transform _leftGripPoint;
        [SerializeField] Transform _rightGripPoint;
        [SerializeField] Transform _rightHandIKTarget;

        [Header("Recoil")]
        [SerializeField] float _kickBack;
        [SerializeField] float _upDown;
        [SerializeField] float _vibration;
        [SerializeField] float _snapSpeed;
        [SerializeField] float _recoverySpeed;
        [SerializeField] float _maxRecoil;

        [Header("Sway")]
        [SerializeField] float _swayAmount;
        [SerializeField] float _swaySpeed;
        [SerializeField] float _swayClamp;

        Vector3 _swayOffset;
        Vector2 _swayInput;
        Vector3 _weaponBoxOrigin;

        Vector3 _currentRecoilPos;
        Vector3 _currentRecoilRot;
        Vector3 _targetRecoilPos;
        Vector3 _targetRecoilRot;
        Vector3 _arOriginPos;
        Quaternion _arOriginRot;

        private void OnEnable()
        {
            _inputReader.OnLookEvent += HandleSway;
        }

        private void OnDisable()
        {
            _inputReader.OnLookEvent -= HandleSway;
        }

        void Start()
        {
            _assultRifle = GetComponentInChildren<AssultRifle>();

            _weaponBoxOrigin = _weaponBox.localPosition;
            _arOriginPos = _weaponPivot.localPosition;
            _arOriginRot = _weaponPivot.localRotation;

            //MoveLeftHandTargetOutsideAnimator();
            //MoveRightHandTargetOutsideAnimator();
        }

        void Update()
        {
            UpdateSway();
            UpdateRecoil();

            //SyncLeftHandTargetToGrip();
            //SyncRightHandTargetToGrip();
        }

        Transform _ikTargetRoot;
        Transform _leftHandIKTarget;
        Vector3 _leftHandTargetLocalPosition;
        Quaternion _leftHandTargetLocalRotation;

        void MoveLeftHandTargetOutsideAnimator()
        {
            if (_leftGripPoint == null || _leftGripPoint.childCount == 0) return;

            _leftHandIKTarget = _leftGripPoint.GetChild(0);
            _leftHandTargetLocalPosition = _leftHandIKTarget.localPosition;
            _leftHandTargetLocalRotation = _leftHandIKTarget.localRotation;
            _leftHandIKTarget.SetParent(transform, true);

            // The IK target must be outside the Animator hierarchy so the rig reads its current scene transform. // By Codex
            RigBuilder rigBuilder = GetComponentInChildren<RigBuilder>();
            if (rigBuilder != null)
            {
                rigBuilder.Clear();
                rigBuilder.Build();
            }
        }

        Transform GetIKTargetRoot()
        {
            if (_ikTargetRoot != null) return _ikTargetRoot;

            _ikTargetRoot = new GameObject("IKTargetRoot").transform;
            _ikTargetRoot.SetParent(null);
            // Keep runtime-driven IK targets completely outside the Player Animator hierarchy. // By Codex
            return _ikTargetRoot;
        }
        void MoveRightHandTargetOutsideAnimator()
        {
            if (_rightHandIKTarget == null) return;

            _rightHandIKTarget.SetParent(GetIKTargetRoot(), true);

            // The IK target must be outside the Animator hierarchy so the rig reads its current scene transform. // By Codex
            RigBuilder rigBuilder = GetComponentInChildren<RigBuilder>();
            if (rigBuilder != null)
            {
                rigBuilder.Clear();
                rigBuilder.Build();
            }
        }

        private void LateUpdate()
        {

            //UpdateIK();
        }

        void HandleSway(Vector2 value)
        {
            _swayInput = value;
        }

        void UpdateSway()
        {
            Vector3 swayInput = new Vector3(-_swayInput.y, _swayInput.x, 0);
            swayInput.x = Mathf.Clamp(swayInput.x, -_swayClamp, _swayClamp);
            swayInput.y = Mathf.Clamp(swayInput.y, -_swayClamp, _swayClamp);

            Quaternion rot = _weaponBox.rotation * Quaternion.Euler(swayInput.x, swayInput.y, 0);

            _weaponBox.localPosition = Vector3.Lerp(_weaponBox.localPosition, _weaponBoxOrigin + _swayOffset, _swayAmount * Time.deltaTime);
            _weaponBox.rotation = Quaternion.Lerp(_weaponBox.rotation, rot, _swayAmount * Time.deltaTime);

            _swayInput = Vector2.zero;
        }


        // AssultRifle calls this only after a bullet is actually fired.
        // Do not also subscribe to OnshotEvent here: that would apply recoil twice per shot. // By Codex
        public void ApplyRecoil()
        {
            _targetRecoilPos += new Vector3(0, 0, -_kickBack);
            _targetRecoilRot += new Vector3(-_upDown, Random.Range(-_vibration, _vibration), 0);

            _targetRecoilPos.x = Mathf.Clamp(_targetRecoilPos.x, -_maxRecoil, 0f);
            _targetRecoilPos.z = Mathf.Clamp(_targetRecoilPos.z, -_maxRecoil, 0f);
        }

        /*
         * ToDo : Muzzle must be Follow the _ar.localPosition
         */
        void UpdateRecoil()
        {
            if (_assultRifle == null || _assultRifle.Ammo == 0)
            {
                return;
            }

            //매 프레임마다 0으로 복귀하도록 함
            _targetRecoilPos = Vector3.Lerp(_targetRecoilPos, Vector3.zero, _recoverySpeed * Time.deltaTime);
            _targetRecoilRot = Vector3.Lerp(_targetRecoilRot, Vector3.zero, _recoverySpeed * Time.deltaTime);

            //현재값은 목표치를 향하도록 함
            _currentRecoilPos = Vector3.Lerp(_currentRecoilPos, _targetRecoilPos, _snapSpeed * Time.deltaTime);
            _currentRecoilRot = Vector3.Lerp(_currentRecoilRot, _targetRecoilRot, _snapSpeed * Time.deltaTime);

            //현재 AR의 위치에 현재 반동값을 적용
            _weaponPivot.localPosition = _arOriginPos + _currentRecoilPos;
            //_ar.localPosition = Mathf.Clamp(_ar.localPosition, )
            _weaponPivot.localRotation = _arOriginRot * Quaternion.Euler(_currentRecoilRot);

        }
        void SyncLeftHandTargetToGrip()
        {
            if (_leftHandIKTarget == null || _leftGripPoint == null) return;

            // Preserve the hand-placement offset authored under LeftGripPoint. // By Codex
            _leftHandIKTarget.SetPositionAndRotation(
                _leftGripPoint.TransformPoint(_leftHandTargetLocalPosition),
                _leftGripPoint.rotation * _leftHandTargetLocalRotation);
        }

        void SyncRightHandTargetToGrip()
        {
            if (_rightGripPoint == null || _rightHandIKTarget == null) { return; }

            if(!_rightGripPoint.gameObject.activeInHierarchy) { return; }


            _rightHandIKTarget.SetPositionAndRotation(_rightGripPoint.position, _rightGripPoint.rotation);
            Debug.Log(
        $"IKController Grip: {_rightGripPoint.name} / {_rightGripPoint.position}\n" +
        $"IKController Target: {_rightHandIKTarget.name} / {_rightHandIKTarget.position}");
        }

        //void UpdateIK()
        //{
        //    _leftArm.position = _leftGripPoint.position;
        //    _leftArm.rotation = _leftGripPoint.rotation;
        //}
    }
}

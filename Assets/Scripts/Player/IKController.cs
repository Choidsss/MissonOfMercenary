using MissionOfMercenary;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace MIssionOfMercenary
{
    public class IKController : MonoBehaviour
    {
        [SerializeField] InputReader _inputReader;

        [Header("References")]
        [SerializeField] PlayerMove _playerMove;
        [SerializeField] Transform _weaponBox;
        [SerializeField] Transform _ar;
        [SerializeField] Transform _leftGripPoint;
        [SerializeField] Transform _leftArm;
        [SerializeField] Transform _leftForceArm;
        [SerializeField] Transform _leftHand;

        [Header("Recoil")]
        [SerializeField] float _kickBack;
        [SerializeField] float _upDown;
        [SerializeField] float _vibration;
        [SerializeField] float _snapSpeed;
        [SerializeField] float _recoverySpeed;

        [Header("Sway")]
        [SerializeField] float _swayAmount;
        [SerializeField] float _swaySpeed;
        [SerializeField] float _swayClamp;

        Vector3 _swayOffset;
        Vector3 _swaytarget;
        Vector3 _weaponBoxOrigin;

        Vector3 _currentRecoilPos;
        Vector3 _currentRecoilRot;
        Vector3 _targetRecoilPos;
        Vector3 _targetRecoilRot;
        Vector3 _arOriginPos;
        Quaternion _arOriginRot;

        private void OnEnable()
        {
            //_inputReader.OnLookEvent += HandleSway;
            //_inputReader.OnshotEvent += HandleRecoil;
        }

        private void OnDisable()
        {
            //_inputReader.OnLookEvent -= HandleSway;
            //_inputReader.OnshotEvent -= HandleRecoil;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _weaponBoxOrigin = _weaponBox.localPosition;
            _arOriginPos = _ar.localPosition;
            _arOriginRot = _ar.localRotation;
        }

        // Update is called once per frame
        void Update()
        {
            UpdateSway();
            //UpdateWeaponBox();
        }

        private void LateUpdate()
        {
            //UpdateRecoil();
            //UpdateWeapon();
            //UpdateIK();
        }

        void UpdateSway()
        {
            _weaponBox.localPosition = Vector3.Lerp(_weaponBox.localPosition, _targetRecoilPos + _swayOffset, _swayAmount * Time.deltaTime);
        }
    }
}

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

        Vector3 _armOriginPos;
        Vector3 _foreArmOriginPos;

        private void OnEnable()
        {
            _inputReader.OnLookEvent += HandleSway;
            _inputReader.OnshotEvent += HandleRecoil;
        }

        private void OnDisable()
        {
            _inputReader.OnLookEvent -= HandleSway;
            _inputReader.OnshotEvent -= HandleRecoil;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _assultRifle = GetComponentInChildren<AssultRifle>();

            _weaponBoxOrigin = _weaponBox.localPosition;
            _arOriginPos = _ar.localPosition;
            _arOriginRot = _ar.localRotation;

            _armOriginPos = _leftArm.localPosition;
            _foreArmOriginPos = _leftForceArm.localPosition;
        }

        // Update is called once per frame
        void Update()
        {
            UpdateSway();
            UpdateRecoil();
            //UpdateWeaponBox();
        }

        private void LateUpdate()
        {
            UpdateIK();
        }

        void HandleSway(Vector2 value)
        {
            //Vector2로 들어오는 입력갑 Vector3 으로 변환(y가 축이 되어야 X로 움직임, x가 축이되어야 Y가 움직임, 부호반대는 관성의 느낌을 주기 위해)
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

            _swayInput = Vector2.zero; //초기화? 왜??????
        }


        void HandleRecoil(float shot)
        {
            //뒤로 밀려서 -
            _targetRecoilPos += new Vector3(0, 0, -_kickBack);
            //vibration은 랜덤하게
            _targetRecoilRot += new Vector3(-_upDown, Random.Range(-_vibration , _vibration), 0);
        }

        //AssultRifle Script Call this Function
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
            if (_assultRifle.Ammo == 0) { return; }

            //매 프레임마다 0으로 복귀하도록 함
            _targetRecoilPos = Vector3.Lerp(_targetRecoilPos, Vector3.zero, _recoverySpeed * Time.deltaTime);
            _targetRecoilRot = Vector3.Lerp(_targetRecoilRot, Vector3.zero, _recoverySpeed * Time.deltaTime);

            //현재값은 목표치를 향하도록 함
            _currentRecoilPos = Vector3.Lerp(_currentRecoilPos, _targetRecoilPos, _snapSpeed * Time.deltaTime);
            _currentRecoilRot = Vector3.Lerp(_currentRecoilRot, _targetRecoilRot, _snapSpeed * Time.deltaTime);

            //현재 AR의 위치에 현재 반동값을 적용
            _ar.localPosition = _arOriginPos + _currentRecoilPos;
            //_ar.localPosition = Mathf.Clamp(_ar.localPosition, )
            _ar.localRotation = _arOriginRot * Quaternion.Euler(_currentRecoilRot);




            // 원래 위치 + 현재 반동값
            //_leftArm.localPosition = _armOriginPos + _currentRecoilPos;
            //_leftForceArm.localPosition = _foreArmOriginPos + _currentRecoilPos;

        }

        void UpdateIK()
        {
            _leftArm.position = _leftGripPoint.position;
            _leftArm.rotation = _leftGripPoint.rotation;
        }
    }
}

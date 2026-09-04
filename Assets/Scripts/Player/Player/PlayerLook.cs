using MissionOfMercenary;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class PlayerLook : MonoBehaviour
    {
        Rigidbody _rigidBody;

        [SerializeField] InputReader _inputReader;

        [Header("Camera Component")]
        [SerializeField] Transform _cameraTransform;

        [Header("Camera Options")]
        [SerializeField] float _cameraSpeed = 1.0f;
        [SerializeField] float _cameraPitchSpeed = 1.0f;

        [Header("Mouse Inverty")]
        [SerializeField] public bool IsInverty { get; set; } = false;

        Vector2 _lookAngle; //이번 프레임에 얼마나 움직였는가
        float _currentPitch; //상하 각도 누적
        float _yRotation;

        public Vector3 LookDirection { get; set; }

        private void Start()
        {
            _rigidBody = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            Look();
        }

        void FixedUpdate()
        {
            // 리지드바디 회전만 FixedUpdate에서
            Quaternion rotationY = Quaternion.Euler(0, _yRotation, 0);
            _rigidBody.MoveRotation(_rigidBody.rotation * rotationY);
        }

        private void OnEnable()
        {
            _inputReader.OnLookEvent += HandledLook;
        }

        private void OnDisable()
        {
            _inputReader.OnLookEvent -= HandledLook;
        }

        //회전 로직을 부르는 함수
        void Look()
        {
            DoLook();
            LookDirection = this.transform.forward;
            _lookAngle = Vector2.zero;
        }

        void DoLook()
        {
            _currentPitch += IsInverty ? _lookAngle.y : -_lookAngle.y;

            _currentPitch = Mathf.Clamp(_currentPitch, -80, 80); //상하 회전의 누적 움직임 값

            _yRotation = _lookAngle.x * _cameraSpeed;

            _cameraTransform.localEulerAngles = new Vector3(_currentPitch * _cameraPitchSpeed, 0, 0);
        }


        void HandledLook(Vector2 value)
        {
            _lookAngle = value;
        }
    }
}

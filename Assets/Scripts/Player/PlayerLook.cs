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

        [Header("Mouse Inverty")]
        [SerializeField] public bool IsInverty { get; set; } = false;

        Vector2 _lookAngle; //이번 프레임에 얼마나 움직였는가
        float _currentPitch; //상하 각도 누적

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

        private void OnEnable()
        {
            _inputReader.OnLookEvent += HandledLook;
        }

        private void OnDisable()
        {
            _inputReader.OnLookEvent -= HandledLook;
        }

        void Look()
        {
            DoLook();
            LookDirection = this.transform.forward;
        }

        void DoLook()
        {
            //currentPitch에 Value.Y 값 누적시킴(Inverty는 위아래의 마우스 상하반전)
            _currentPitch += IsInverty ? _lookAngle.y : -_lookAngle.y;
            //일정 각도 이상으로 돌리지 안도록 제한
            _currentPitch = Mathf.Clamp(_currentPitch, -90 , 90);

            //*******돌릴때마다 Vector를 생성시키는데 괜찮나?************

            //카메라 좌우회전에 X축 을 생성해줌(더해줌)
            Quaternion rotationY = Quaternion.Euler(0, _lookAngle.x, 0);
            _rigidBody.MoveRotation(_rigidBody.rotation * rotationY);

            //플레이어의 몸통을 카메라가 돌아간 것 만큼 돌림(Y축 각도 + 카메라의 좌우 각도)
            //=> 좌우는 그냥 돌리면 되지만 Y축은 누적된만큼 돌려야 함

            //this.transform.eulerAngles = new Vector3(0, this.transform.eulerAngles.y + _lookAngle.x, 0);
            _cameraTransform.eulerAngles = new Vector3(_currentPitch, 0, 0);
        }


        void HandledLook(Vector2 value)
        {
            _lookAngle = value;
        }
    }
}

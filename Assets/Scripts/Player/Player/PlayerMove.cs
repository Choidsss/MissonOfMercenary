using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

namespace MissionOfMercenary
{
    public class PlayerMove : MonoBehaviour
    {
        Rigidbody _rigidBody;

        [SerializeField] InputReader _inputReader;
        [SerializeField] GameObject _camera;

        [Header("Movement Options")]
        [SerializeField] float _speed = 1.0f;
        [SerializeField] int _runMultiply = 2;

        public bool DoRun { get; private set; } = false;
        public float walkSpeed { get; private set; } = 0.0f;

        Vector2 _move;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _rigidBody = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            Move();
        }

        private void OnEnable()
        {
            _inputReader.OnMoveEvent += HandledMove;
            _inputReader.OnRunToggleEvent += HandledRunToggle;
            //_inputReader.OnshotEvent += HandledShot;
        }

        private void OnDisable()
        {
            _inputReader.OnMoveEvent -= HandledMove;
            _inputReader.OnRunToggleEvent -= HandledRunToggle;
            //_inputReader.OnshotEvent -= HandledShot;
        }

        

        //DoMove를 부르는 함수
        void Move()
        {
            DoMove();
        }

        void DoMove()
        {
            float moveAnim = _move.magnitude;
            float accel = _speed * Time.fixedDeltaTime;

            walkSpeed = Mathf.MoveTowards(walkSpeed, moveAnim, accel);

            float currentSpeed = DoRun ? _speed * _runMultiply : _speed;
            
            if (DoRun && walkSpeed < 0.1) { DoRun = false; }
            
            Vector3 forward = _camera.transform.forward;
            Vector3 right = _camera.transform.right;

            // y축 영향 제거
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            Vector3 movement = (forward * _move.y + right * _move.x);
            _rigidBody.MovePosition(_rigidBody.position + movement * Time.fixedDeltaTime * currentSpeed);


            //Vector3 movement = new Vector3(_move.x, 0, _move.y);
            //_rigidBody.MovePosition(_rigidBody.position + movement * Time.fixedDeltaTime * _speed);
        }

        void HandledMove(Vector2 value)
        {
            _move = value;
        }

        void HandledRunToggle()
        {
            DoRun = !DoRun;
        }
    }
}


using UnityEngine;

namespace MissionOfMercenary
{
    public class PlayerMove : MonoBehaviour
    {
        Rigidbody _rigidBody;

        [SerializeField] InputReader _inputReader;
        [SerializeField] GameObject _camera;

        [Header("Movement Options")]
        [SerializeField] float _speed = 1.0f;

        Vector2 _move;
        bool _isShot = false;

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
            //_inputReader.OnshotEvent += HandledShot;
        }

        private void OnDisable()
        {
            _inputReader.OnMoveEvent -= HandledMove;
            //_inputReader.OnshotEvent -= HandledShot;
        }

        

        //DoMove를 부르는 함수
        void Move()
        {
            DoMove();
        }

        //character 의 transform을 움직이는 함수(***************카메라를 기준으로 움직이도록 수정********************)
        void DoMove()
        {
            //Vector3 movement = new Vector3(_move.x, 0, _move.y);
            //_rigidBody.MovePosition(_rigidBody.position + movement * Time.fixedDeltaTime * _speed);




            Vector3 forward = _camera.transform.forward;
            Vector3 right = _camera.transform.right;

            // y축 영향 제거
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            Vector3 movement = (forward * _move.y + right * _move.x);
            _rigidBody.MovePosition(_rigidBody.position + movement * Time.fixedDeltaTime * _speed);
        }

        void HandledMove(Vector2 value)
        {
            _move = value;
        }

        //void HandledShot(float value)
        //{
            
        //}

    }
}


using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace MissionOfMercenary
{
    //메뉴에 Scriptable Object를 만들 수 있도록 띄워줌
    [CreateAssetMenu(fileName = "InputReader", menuName = "Input/InputReader")]

    public class InputReader : ScriptableObject
    {
        /*
         * 이 스크립트는 InputActionAsset의 Event를 처리해주는 스크립트입니다.
         * 움직임 관련 함수들은 PlayerMove 로 이동
         */

        [SerializeField] InputActionAsset _inputActionAsset;

        InputAction _moveAction;
        InputAction _shotAction;
        InputAction _lookAction;

        public event Action<Vector2> OnMoveEvent;
        public event Action<float> OnshotEvent;
        public event Action<Vector2> OnLookEvent;

        private void OnEnable()
        {
            //캐싱
            _moveAction = _inputActionAsset.FindAction("Move");
            _shotAction = _inputActionAsset.FindAction("Shot");
            _lookAction = _inputActionAsset.FindAction("Look");
            _moveAction.Enable();
            _shotAction.Enable();
            _lookAction.Enable();

            // 이벤트 콜백 등록
            _moveAction.performed += MoveEventCallback;
            _moveAction.canceled += MoveEventCallback;
            _lookAction.performed += LookEventCallback;
            _lookAction.canceled += LookEventCallback;
            _shotAction.performed += ShotEventCallback;
        }

        private void OnDisable()
        {
            _moveAction.Disable();
            _shotAction.Disable();
            _lookAction.Disable();

            //이벤트 콜백 해제
            _moveAction.performed -= MoveEventCallback;
            _moveAction.canceled -= MoveEventCallback;
            _lookAction.performed -= LookEventCallback;
            _shotAction.performed -= ShotEventCallback;
        }

        //*************이벤트 처리 함수들*******************

        //Unity에서 Move함수를 Invoke 시키는 함수
        void MoveEventCallback(InputAction.CallbackContext context)
        {
            Vector2 value = context.ReadValue<Vector2>();
            OnMoveEvent?.Invoke(value);
        }

        //Unity에서 Move함수를 Invoke 시키는 함수
        void ShotEventCallback(InputAction.CallbackContext context)
        {
            float value = context.ReadValue<float>();
            OnshotEvent?.Invoke(value);
        }

        //Unity에서 Look함수를 Invoke 시키는 함수
        void LookEventCallback(InputAction.CallbackContext context)
        {
            Vector2 value = context.ReadValue<Vector2>();
            OnLookEvent?.Invoke(value);
        }
    }
}


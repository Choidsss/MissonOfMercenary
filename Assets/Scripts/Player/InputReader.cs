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
        [SerializeField] InputActionAsset _inputActionAsset;

        InputAction _moveAction;
        InputAction _shotAction;
        InputAction _lookAction;
        InputAction _switchFireAction;
        InputAction _runAction;
        InputAction _reloadAction;
        InputAction _zoomInAction;
        InputAction _assasinAction;
        InputAction _equipPrimaryAction;
        InputAction _equipSecondaryAction;
        InputAction _equipMeleeAction;
        InputAction _pickUpAction;

        public event Action<Vector2> OnMoveEvent;
        public event Action OnRunToggleEvent;
        public event Action<float> OnshotEvent;
        public event Action OnShotCancled;
        public event Action<Vector2> OnLookEvent;
        public event Action OnAttackTypeToggleEvent;
        public event Action<float> OnReloadEvent;
        public event Action OnZoomInToggleAction;
        public event Action OnAssasinAction;
        public event Action OnEquipPrimaryAction;
        public event Action OnEquipSecondaryAction;
        public event Action OnEquipMeleeAction;
        public event Action OnPickedUpAction;

        private void OnEnable()
        {
            //캐싱
            _moveAction = _inputActionAsset.FindAction("Move");
            _shotAction = _inputActionAsset.FindAction("Shot");
            _lookAction = _inputActionAsset.FindAction("Look");
            _switchFireAction = _inputActionAsset.FindAction("SwitchFire");
            _runAction = _inputActionAsset.FindAction("Run");
            _reloadAction = _inputActionAsset.FindAction("Reload");
            _zoomInAction = _inputActionAsset.FindAction("ZoomIn");
            _assasinAction = _inputActionAsset.FindAction("Assasin");
            _equipPrimaryAction = _inputActionAsset.FindAction("EquipPrimary");
            _equipSecondaryAction = _inputActionAsset.FindAction("EquipSecondary");
            _equipMeleeAction = _inputActionAsset.FindAction("EquipMelee");
            _pickUpAction = _inputActionAsset.FindAction("PickUp");

            _moveAction.Enable();
            _runAction.Enable();
            _shotAction.Enable();
            _lookAction.Enable();
            _switchFireAction.Enable();
            _reloadAction.Enable();
            _zoomInAction.Enable();
            _assasinAction.Enable();
            _equipPrimaryAction.Enable();
            _equipSecondaryAction.Enable();
            _equipMeleeAction.Enable();
            _pickUpAction.Enable();

            // 이벤트 콜백 등록
            _moveAction.performed += MoveEventCallback;
            _moveAction.canceled += MoveEventCallback;

            _runAction.performed += RunEventCallBack;

            _lookAction.performed += LookEventCallback;
            _lookAction.canceled += LookEventCallback;

            _shotAction.performed += ShotEventCallback;
            _shotAction.canceled += ShotCancledCallBack;

            _switchFireAction.performed += AttackTypeToggleEventCallBack;

            _reloadAction.performed += WeponReloadActionCallBack;

            _zoomInAction.performed += ZoomInActionCallBack;

            _assasinAction.performed += AssasinActionCallBack;

            _equipPrimaryAction.performed += EquipPrimaryActionCallBack;
            _equipSecondaryAction.performed += EquipSecondaryActionCallBack;
            _equipMeleeAction.performed += EquipMeleeActionCallBack;
            _pickUpAction.performed += PickUpActionCallBack;
        }

        private void OnDisable()
        {
            _moveAction.Disable();
            _shotAction.Disable();
            _lookAction.Disable();
            _runAction.Disable();
            _switchFireAction.Disable();


            _reloadAction.Disable();

            _zoomInAction.Disable();

            _assasinAction.Disable();

            _equipPrimaryAction.Disable();
            _equipSecondaryAction.Disable();
            _equipMeleeAction.Disable();

            _pickUpAction.Disable();

            //이벤트 콜백 해제
            _moveAction.performed -= MoveEventCallback;
            _moveAction.canceled -= MoveEventCallback;

            _runAction.performed -= RunEventCallBack;

            _lookAction.performed -= LookEventCallback;

            _shotAction.performed -= ShotEventCallback;
            _shotAction.canceled -= ShotCancledCallBack;

            _switchFireAction.performed -= AttackTypeToggleEventCallBack;

            _reloadAction.performed -= WeponReloadActionCallBack;

            _zoomInAction.performed -= ZoomInActionCallBack;

            _assasinAction.performed -= AssasinActionCallBack;

            _equipPrimaryAction.performed -= EquipPrimaryActionCallBack;
            _equipSecondaryAction.performed -= EquipSecondaryActionCallBack;
            _equipMeleeAction.performed -= EquipMeleeActionCallBack;

            _pickUpAction.performed -= PickUpActionCallBack;
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

        //연발일때 멈추게 하는 이벤트를 Invoke 시키는 함수
        void ShotCancledCallBack(InputAction.CallbackContext context)
        {
            OnShotCancled?.Invoke();
        }

        //Unity에서 Look함수를 Invoke 시키는 함수
        void LookEventCallback(InputAction.CallbackContext context)
        {
            Vector2 value = context.ReadValue<Vector2>();
            OnLookEvent?.Invoke(value);
        }

        //Unity에서 조정간 단/연발 교체 함수를 Invoke 시키는 함수
        void AttackTypeToggleEventCallBack(InputAction.CallbackContext context)
        {
            OnAttackTypeToggleEvent?.Invoke();
        }

        void RunEventCallBack(InputAction.CallbackContext context)
        {
            OnRunToggleEvent?.Invoke();
        }

        void WeponReloadActionCallBack(InputAction.CallbackContext context)
        {
            float value = context.ReadValue<float>();
            OnReloadEvent?.Invoke(value);
        }

        void ZoomInActionCallBack(InputAction.CallbackContext context)
        {
            OnZoomInToggleAction?.Invoke();
        }

        void AssasinActionCallBack(InputAction.CallbackContext context)
        {
            //bool value = context.ReadValue<bool>();
            OnAssasinAction?.Invoke();
        }

        void EquipPrimaryActionCallBack(InputAction.CallbackContext context)
        {
            OnEquipPrimaryAction?.Invoke();
        }

        void EquipSecondaryActionCallBack(InputAction.CallbackContext context)
        {
            OnEquipSecondaryAction?.Invoke();
        }

        void EquipMeleeActionCallBack(InputAction.CallbackContext context)
        {
            OnEquipMeleeAction?.Invoke();
        }

        void PickUpActionCallBack(InputAction.CallbackContext context)
        {
            OnPickedUpAction?.Invoke();
        }
    }
}


using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputReader : ScriptableObject
{
    /*
     * 이 스크립트는 InputActionAsset의 Event를 처리해주는 스크립트입니다.
     * 움직임 관련 함수들은 PlayerMove 로
     */

    /*
     * InputActionAsset 참조
     * [SerializeField] 로 에셋 들고 있기
     * 근데 에셋 전체를 들고 있는 것보다, 에셋 안에서 각 액션을 꺼내서 변수로 따로 캐싱해두는 게 나중에 쓰기 편해
     */
    [SerializeField] InputActionAsset _moveActionAsset;
    [SerializeField] InputActionAsset _shootActionAsset;

    InputAction _moveAction;
    InputAction _shootAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        _moveAction = _moveActionAsset.FindAction("Move");
        _shootAction = _shootActionAsset.FindAction("Shot");
        _moveAction.Enable();
        _shootAction.Enable();

        // 이벤트 콜백 등록
        //_moveAction.performed +=
    }

    private void OnDisable()
    {
        
    }
}

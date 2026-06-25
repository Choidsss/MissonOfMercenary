using MissionOfMercenary;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class WeaponSway : MonoBehaviour
    {
        [Header("Sway Options")]
        [SerializeField] InputReader _inputReader;
        [SerializeField] float _swayAmount;
        [SerializeField] float _swaySpeed;
        [SerializeField] float _swayClamp;
        
        Vector3 _swayOffset;
        Vector3 _swayTarget;
        Vector3 _originPos;

        private void OnEnable()
        {
            _inputReader.OnLookEvent += WeaponSwayHandle;
        }

        private void OnDisable()
        {
            _inputReader.OnLookEvent -= WeaponSwayHandle;
        }

        

        void Start()
        {
            _originPos = transform.localPosition; // 원래 위치 저장
        }

        private void LateUpdate()
        {
            _swayTarget = Vector3.Lerp(_swayTarget, Vector3.zero, _swaySpeed * Time.deltaTime);
            _swayOffset = Vector3.Lerp(_swayOffset, _swayTarget, _swaySpeed * Time.deltaTime);

            transform.localPosition = _originPos + _swayOffset; // 원래 위치 + 오프셋
        }


        void WeaponSwayHandle(Vector2 value)
        {
            float mouseX = -value.x;
            float mouseY = -value.y;

            _swayTarget = new Vector3(Mathf.Clamp(mouseX * _swayAmount, -_swayClamp, _swayClamp), Mathf.Clamp(mouseY * _swayAmount, -_swayClamp, _swayClamp), 0);
        }
    }
}

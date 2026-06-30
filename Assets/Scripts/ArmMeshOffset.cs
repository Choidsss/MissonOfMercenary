using UnityEngine;

namespace MIssionOfMercenary
{
    public class ArmMeshOffset : MonoBehaviour
    {
        [SerializeField] Vector3 _localOffset;
        [SerializeField] Transform _obj;
        [SerializeField] float _lerpSpeed = 5f; // 기본값을 0 아닌 값으로

        public float OffsetX { get { return _localOffset.x; } }
        Vector3 _originalPosition;
        Vector3 _currentOffset;
        Vector3 _targetOffset;

        void Start()
        {
            _originalPosition = _obj.localPosition;
            _targetOffset = _localOffset; // 시작할 때부터 목표값을 오프셋으로
            _currentOffset = _localOffset; // 현재값도 처음부터 오프셋으로 (Lerp 안 거치고 즉시 적용)
        }

        void LateUpdate()
        {
            _currentOffset = Vector3.Lerp(_currentOffset, _targetOffset, _lerpSpeed * Time.deltaTime);
            _obj.localPosition = _originalPosition + _currentOffset;
        }

        public void SetOffsetActive(bool active)
        {
            _targetOffset = active ? _localOffset : Vector3.zero;
        }
    }
}

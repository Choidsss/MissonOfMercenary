using UnityEngine;

namespace MIssionOfMercenary
{
    public class ArmMeshOffset : MonoBehaviour
    {
        [SerializeField] Vector3 _localOffset;
        [SerializeField] Transform _obj;
        [SerializeField] float _lerpSpeed;

        public float OffsetX { get { return _localOffset.x; }  }

        Vector3 _originalPosition;
        Vector3 _currentOffset;
        Vector3 _targetOffset;

        void Start()
        {
            _originalPosition = _obj.localPosition;
            _targetOffset = _localOffset;
        }

        void LateUpdate()
        {
            _currentOffset = Vector3.Lerp(_currentOffset, _targetOffset, _lerpSpeed * Time.deltaTime);
            // 원래 위치 + 오프셋으로 고정 (누적 안 됨)
            _obj.localPosition = _originalPosition + _currentOffset;
        }

        // AimController에서 호출
        public void SetOffsetActive(bool active)
        {
            _targetOffset = active ? _localOffset : Vector3.zero;
        }
    }
}

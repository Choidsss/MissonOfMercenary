using UnityEngine;

namespace MIssionOfMercenary
{
    public class ArmMeshOffset : MonoBehaviour
    {
        [SerializeField] Vector3 _localOffset;
        [SerializeField] Transform _obj;

        Vector3 _originalPosition;
        //bool _initialized = false;

        void Start()
        {
            _originalPosition = _obj.localPosition;
        }

        void LateUpdate()
        {
            // 원래 위치 + 오프셋으로 고정 (누적 안 됨)
            _obj.localPosition = _originalPosition + _localOffset;
        }
    }
}

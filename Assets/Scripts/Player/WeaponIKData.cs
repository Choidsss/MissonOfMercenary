using UnityEngine;

namespace MIssionOfMercenary
{
    public class WeaponIKData : MonoBehaviour
    {
        [Header("Hand Grip Points")]
        [SerializeField] Transform _leftGripPoint;
        [SerializeField] Transform _rightGripPoint;

        [SerializeField] bool _useLeftHandIK = false;

        public Transform LeftGripPoint => _leftGripPoint;
        public Transform RightGripPoint => _rightGripPoint;
        public bool UseLeftHandIK => _useLeftHandIK;

        void Awake()
        {
            RefreshGripPoints(); // 런타임 생성 프리팹의 비어 있거나 잘못된 참조를 복구합니다. By_Codex
        }

        void OnValidate()
        {
            RefreshGripPoints(); // 프리팹 편집 시에도 자동으로 연결합니다. By_Codex
        }

        public void RefreshGripPoints()
        {
            if(_leftGripPoint != null && !_leftGripPoint.IsChildOf(transform))
            {
                _leftGripPoint = null; // 무기 프리팹 외부의 인스턴스 Override 참조는 새 무기에 사용할 수 없습니다. By_Codex
            }

            if(_rightGripPoint != null && !_rightGripPoint.IsChildOf(transform))
            {
                _rightGripPoint = null; // 기존 무기나 Player를 가리키는 참조를 제거합니다. By_Codex
            }

            Transform[] children = GetComponentsInChildren<Transform>(true);

            foreach(Transform child in children)
            {
                if(_leftGripPoint == null && child.name == "LeftGripPoint")
                {
                    _leftGripPoint = child;
                }

                if(_rightGripPoint == null && child.name == "RightGripPoint")
                {
                    _rightGripPoint = child;
                }

                if(_leftGripPoint != null && _rightGripPoint != null)
                {
                    break;
                }
            }

            if(_rightGripPoint == null)
            {
                Debug.LogWarning($"{name}: RightGripPoint를 찾지 못했습니다.", this); // 프리팹 내부에 정확한 이름의 Transform이 필요합니다. By_Codex
            }
        }

    }
}

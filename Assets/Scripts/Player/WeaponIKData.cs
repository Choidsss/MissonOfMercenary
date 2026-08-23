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

    }
}

using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace MIssionOfMercenary
{
    [DefaultExecutionOrder(11000)]
    public class WeaponIKController : MonoBehaviour
    {
        [Header("Firearm IK Data")]
        [SerializeField] Transform _leftHandTarget;
        [SerializeField] Transform _rightHandTarget;

        [Header("Knife IK Data")]
        [SerializeField] WeaponIKData _currentData;

        [Header("LeftArm IK Data")]
        [SerializeField] TwoBoneIKConstraint _leftIKConstraint;

        [Header("One-handed Weapon Rest Pose")]
        [Tooltip("Place outside the moving weapon. Used when Use Left Hand IK is off.")]
        [SerializeField] Transform _leftHandRestTarget;

        private void Update()
        {
            SyncTargets();    
        }

        public void BlindWeapon(WeaponIKData data)
        {
            _currentData = data;

            if (_currentData != null)
                _currentData.RefreshGripPoints();

            SyncTargets();
        }

        void CopyPose(Transform target, Transform grip)
        {
            if(target == null || grip == null)return;

            target.SetPositionAndRotation(grip.position, grip.rotation);
        }

        void SyncTargets()
        {
            Transform leftSource = GetLeftHandSource();

            // Keep the arm posed for one-handed weapons instead of releasing it to Animator. By Codex
            if (_leftIKConstraint != null)
                _leftIKConstraint.weight = leftSource != null ? 1f : 0f;

            if (_currentData == null)
                return;

            CopyPose(_rightHandTarget, _currentData.RightGripPoint);

            CopyPose(_leftHandTarget, leftSource);
        }

        Transform GetLeftHandSource()
        {
            if (_currentData == null)
                return null;

            // Two-handed weapons follow their grip; a free hand follows the independent rest pose. By Codex
            return _currentData.UseLeftHandIK
                ? _currentData.LeftGripPoint
                : _leftHandRestTarget;
        }
    }
}

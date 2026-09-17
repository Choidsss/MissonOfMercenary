using UnityEngine;

namespace MIssionOfMercenary
{
    public class SentryEnemySaveGunTransform : MonoBehaviour
    {
        [SerializeField] Transform _aimingGunTransform;
        [SerializeField] Transform _gunTransform;

        bool _aimingPoseApplied;

        public Transform AimingAfterGunTranform { get { return _aimingGunTransform; } }

        public bool ApplyAimingGunTransform()
        {
            if (_aimingPoseApplied) return true;
            if (_gunTransform == null || _aimingGunTransform == null) return false;

            // Sample once: the marker may itself be a child of the weapon.
            _gunTransform.SetPositionAndRotation(
                _aimingGunTransform.position, _aimingGunTransform.rotation);
            _aimingPoseApplied = true;
            return true;
        }
    }
}

using System.Runtime.ConstrainedExecution;
using UnityEngine;

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


        public void BlindWeapon(WeaponIKData data)
        {
            if (data == null) { Debug.Log("무기의 IK_Data가 없습니다"); return; }
            
            _currentData = data;

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
            if (_currentData == null)
                return;

            CopyPose(_rightHandTarget, _currentData.RightGripPoint);

            if (_currentData.UseLeftHandIK)
            {
                CopyPose(_leftHandTarget, _currentData.LeftGripPoint);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace MIssionOfMercenary
{
    public class IKDebugTest : MonoBehaviour
    {
        [SerializeField] RigBuilder _rigBuilder;
        [SerializeField] TwoBoneIKConstraint _leftHandIK;

        // Update is called once per frame
        void Update()
        {
            Debug.Log($"RigBuilder enabled: {_rigBuilder.enabled}");
            Debug.Log($"LeftHandIK weight: {_leftHandIK.weight}");
            Debug.Log($"Target: {_leftHandIK.data.target}");
            Debug.Log($"Root: {_leftHandIK.data.root}");
            Debug.Log($"Mid: {_leftHandIK.data.mid}");
            Debug.Log($"Tip: {_leftHandIK.data.tip}");
        }
    }
}

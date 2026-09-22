using MissionOfMercenary;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class TryGetAimHit : MonoBehaviour
    {
        [Header("MainCamera")]
        [SerializeField] Camera _mainCamera;
        
        Vector3 _centerPoint = new Vector3(0.5f, 0.5f, 0.0f);
        public Ray GetAimRay()
        {
            return _mainCamera.ViewportPointToRay(_centerPoint);
        }
    }
}

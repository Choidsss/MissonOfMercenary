using MissionOfMercenary;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class TryGetAimHit : MonoBehaviour
    {
        [Header("MainCamera")]
        [SerializeField] Camera _mainCamera;
        
        Vector3 _centerPoint = new Vector3(0.5f, 0.5f, 0.0f);
        public Ray GetAimRay(float spreadAngle = 0f)
        {
            Vector3 direction = _mainCamera.transform.forward;

            if (spreadAngle > 0f)
            {
                Vector2 spread = Random.insideUnitCircle * Mathf.Tan(spreadAngle * Mathf.Deg2Rad);

                direction += _mainCamera.transform.right * spread.x;
                direction += _mainCamera.transform.up * spread.y;
                direction.Normalize();
            }

            //return _mainCamera.ViewportPointToRay(_centerPoint);
            return new Ray(_mainCamera.transform.position, direction);
        }
    }
}

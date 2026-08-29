using MissionOfMercenary;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class TryGetAimHit : MonoBehaviour
    {
        [SerializeField] InputReader _inputReader;

        Ray _ray;
        public Ray RayHit => _ray;

        private void OnEnable()
        {
            _inputReader.OnshotEvent += GetAimHit;
        }

        private void OnDisable()
        {
            _inputReader.OnshotEvent -= GetAimHit;
        }

        void GetAimHit(float shot)
        {
            _ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        }
    }
}

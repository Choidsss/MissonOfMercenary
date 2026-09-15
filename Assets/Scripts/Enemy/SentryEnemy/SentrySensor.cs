using UnityEngine;

namespace MIssionOfMercenary
{
    public class SentrySensor : MonoBehaviour
    {
        [Header("Player Position")]
        [SerializeField] Transform _player;

        [Header("Player Layer Check")]
        [SerializeField] LayerMask _layer;

        [Header("Overlap Size Intents")]
        [SerializeField] Vector3 _boxSize;

        bool _sentryDetectedPlayer = false;

        public bool SentryDetectedPlayer { get { return _sentryDetectedPlayer; }  }

        public Vector3 DetectedTarget => _sentryDetectedPlayer ? _player.position: Vector3.zero;

        void Update()
        {
            SentryEnemySphereArea();
        }

        void SentryEnemySphereArea()
        {
            Collider[] colliders = Physics.OverlapBox(transform.position, _boxSize, Quaternion.identity, _layer);
            if(colliders.Length == 0) { return; }

            HeadBob hb = colliders[0].GetComponentInParent<HeadBob>();

            if(hb == null) { Debug.Log("Player가 아니거나 없습니다"); return; }
            
            _sentryDetectedPlayer = true;
        }
    }
}

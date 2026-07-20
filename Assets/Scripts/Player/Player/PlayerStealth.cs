using MissionOfMercenary;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class PlayerStealth : MonoBehaviour
    {
        [SerializeField] AudioSource _gunshotSound;
        [SerializeField] InputReader _inputReader;

        [Header("Player Hide Ops")]
        [SerializeField] float _radius;
        [SerializeField] LayerMask _enemyLayer;

        bool _isHidden = false;
        Vector3 _shotPosition;

        public bool IsHidden { get { return _isHidden; } }

        public Vector3 ShotPosition => _shotPosition;

        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        }

    }
}

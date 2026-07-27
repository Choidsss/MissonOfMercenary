using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class EnemyAnimation : MonoBehaviour
    {
        [SerializeField] NavMeshAgent _nav;

        Animator _anim;

        float _speed;
        void Start()
        {
            //_nav = GetComponent<NavMeshAgent>();
            _anim = GetComponentInChildren<Animator>();
        }

        void Update()
        {
            EnemyMovementAnim();
        }

        void EnemyMovementAnim()
        {
            if(_nav == null || _anim == null)
            {
                return;
            }

            _speed = _nav.velocity.magnitude / _nav.speed;

            if(_nav.speed > 0.1f)
            {
                _anim.SetFloat("Speed", _speed);
            }
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class EnemyAnimation : MonoBehaviour
    {
        [SerializeField] NavMeshAgent _nav;
        [SerializeField] float _delay = 2.5f;

        Animator _anim;

        float _speed;
        void Start()
        {
            //_nav = GetComponent<NavMeshAgent>();
            _anim = GetComponentInChildren<Animator>();
        }

        void Update()
        {
            PlayEnemyMovementAnim();
        }

        void PlayEnemyMovementAnim()
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

        public void PlayEnemyShotAnim()
        {
            StartCoroutine("EnemyShotDelay");
            _anim.SetTrigger("Shot");
        }

        IEnumerator EnemyShotDelay()
        {
            yield return new WaitForSeconds(_delay);

        }
    }
}

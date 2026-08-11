using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class EnemyAnimation : MonoBehaviour
    {
        [SerializeField] NavMeshAgent _nav;
        [SerializeField] float _delay = 2.5f;

        EnemyFindArea _findArea;
        Animator _anim;

        float _speed;
        bool _aim = false;



        void Start()
        {
            _anim = GetComponentInChildren<Animator>();
            _findArea = GetComponent<EnemyFindArea>();
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
            StartCoroutine(EnemyShotDelay());
            _anim.SetTrigger("Shot");
        }

        public void PlayEnemyAimAnimation()
        {
            _aim = true;
            _anim.SetBool("Aim", _aim);
        }

        public void PlayEnemyAimCancelAnimation()
        {
            _aim = false;
            _anim.SetBool("Aim", _aim);
        }

        public void PlayEnemyChaseAnimation()
        {
            _anim.SetTrigger("Chase");
        }
        IEnumerator EnemyShotDelay()
        {
            yield return new WaitForSeconds(_delay);
        }

    }
}

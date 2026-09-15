using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class EnemyAnimation : MonoBehaviour
    {
        [SerializeField] NavMeshAgent _nav;
        //[SerializeField] float _delay = 2.5f;

        EnemyFindArea _findArea;
        Animator _anim;

        float _speed;
        bool _isChase = false;
        bool _isShot = false;
        bool _aim = false;

        public bool IsChase { get { return _isChase; }  }

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
            _isShot = true;
            _anim.SetBool("Shot", _isShot);
        }

        public void StopEnemyShotAnim()
        {
            _isShot = false;
            _anim.SetBool("Shot", _isShot);
        }

        public void PlayEnemyAimAnimation()
        {
            _aim = true;
            _anim.SetBool("Aim", _aim);
        }

        public void StopEnemyAimCancelAnimation()
        {
            _aim = false;
            _anim.SetBool("Aim", _aim);
        }

        public void PlayEnemyChaseAnimation()
        {
            _isChase = true;
            _anim.SetBool("Chase", _isChase);
        }

        public void StopEnemyChaseAnimationReset()
        {
            _isChase = false;
            _anim.SetBool("Chase", _isChase);
        }

        //IEnumerator EnemyShotDelay()
        //{
        //    yield return new WaitForSeconds(_delay);
        //}

    }
}

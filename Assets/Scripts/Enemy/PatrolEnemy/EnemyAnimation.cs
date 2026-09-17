using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class EnemyAnimation : MonoBehaviour
    {
        Animator _anim;

        [SerializeField] NavMeshAgent _nav;

        float _speed;
        bool _isChase = false;
        bool _isShot = false;
        bool _aim = false;
        bool _isFinish = false;

        public bool IsChase { get { return _isFinish; }  }
        public bool IsAnimationFinish { get { return _isFinish; }  }

        void Start()
        {
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

            ReadAnimationFinishOrNot();
        }

        public void StopEnemyAimCancelAnimation()
        {
            _aim = false;
            _anim.SetBool("Aim", _aim);
        }

        //Aim포즈까지 자세를 올렸을때 정지하도록 하는 함수
        public void StopAnimation()
        {
            _anim.speed = 0.0f;
        }

        public void PlayAnimation()
        {
            _anim.speed = 1.0f;
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

        void ReadAnimationFinishOrNot()
        {
            AnimatorStateInfo stateInfo = _anim.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName("Aiming") && !_anim.IsInTransition(0) && stateInfo.normalizedTime >= 1)
            {
                _isFinish = true;
            }
            else
            {
                _isFinish = false;
            }
        }

        //IEnumerator EnemyShotDelay()
        //{
        //    yield return new WaitForSeconds(_delay);
        //}

    }
}

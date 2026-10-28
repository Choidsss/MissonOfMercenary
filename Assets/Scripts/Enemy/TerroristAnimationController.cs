using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class TerroristAnimationController : MonoBehaviour
    {
        const float MOTION_DELAY = 0.05f;

        Animator _animator;
        NavMeshAgent _agent;
        SentryEnemyAttack _enemyAttack;

        float _firingUtil;

        void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _agent = GetComponent<NavMeshAgent>();
            _enemyAttack = GetComponent<SentryEnemyAttack>();
        }

        void Update()
        {
            TerroristRunningAnimation();
            UpdateCombatAnimation();
        }

        private void OnEnable()
        {
            _firingUtil = 0f;

            if (_enemyAttack != null) 
            {
                _enemyAttack._firedAction += HandleShotFired;
            }
        }

        private void OnDisable()
        {
            if (_enemyAttack != null)
            {
                _enemyAttack._firedAction -= HandleShotFired;
            }
        }

        void TerroristRunningAnimation()
        {
            if (_animator == null || !_animator.isActiveAndEnabled)
            {
                Debug.Log("애니메이션 실행 조건을 만족하지 않아 실행되지 않습니다");
                return;
            }

            bool isMoving = false;

            if (_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
            {
                isMoving = !_agent.isStopped && _agent.velocity.sqrMagnitude > 0.01f;
            }

            _animator.SetBool("IsMoving", isMoving);
        }

        //void TerroristShootingAnimation()
        //{

        //}

        void UpdateCombatAnimation()
        {
            if (_animator == null || !_animator.isActiveAndEnabled)
            {
                return;
            }

            bool attackActive = _enemyAttack != null && _enemyAttack.isActiveAndEnabled;
            bool isReloading = attackActive && _enemyAttack.IsReloading;

            if (isReloading || !attackActive)
            {
                _firingUtil = 0f;
            }

            bool isFiring = attackActive && !isReloading && Time.time < _firingUtil;

            _animator.SetBool("IsReloading", isReloading);
            _animator.SetBool("IsFiring", isFiring);
        }

        /// <summary>
        /// 한 발이 나간 프레임에만 IsFiring = true로 만들면, 다음 발까지 기다리는 동안 false가 되어 Idle과 Shoot를 반복할 수 있어요. 그래서 다음 발이 나올 때까지 모션을 짧게 유지하고, 새 발사가 발생하면 시간을 연장합니다.
        /// </summary>
        void HandleShotFired()
        {
            _firingUtil = Time.time + _enemyAttack.FireInterval + MOTION_DELAY;
        }
    }
}

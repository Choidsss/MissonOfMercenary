using NUnit.Framework;
using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyAttack : MonoBehaviour
    {
        EnemyAnimation _enemyAnim;
        EnemyFindArea _findArea;
        Animator _anim;
        Queue<GameObject> _deleteQue = new Queue<GameObject>();

        [SerializeField] float _spreadAngle;
        [SerializeField] float _bulletSpeed;
        [SerializeField] float _delay = 3.0f;
        [SerializeField] float _attackInterval = 3.0f;
        [SerializeField, Min(1)] int _shotsPerAttack = 1;
        [SerializeField, Min(0.01f)] float _burstShotInterval = 0.1f;
        [SerializeField] GameObject _ammo;
        [SerializeField] GameObject _muzzle;
        [SerializeField] GameObject _muzzleFire;


        Vector3 _target = Vector3.zero;
        bool _isAttackRoutineRunning;

        public AimType aimType { get; } = AimType.None;

        public WeaponType weaponType { get; set; } = WeaponType.SR;

        public int Damage { get; } = 30;

        public float AttackRange { get; } = 150;

        public Vector3 Target { get { return _target; } }


        void Start()
        {
            _findArea = GetComponent<EnemyFindArea>();
            _enemyAnim = GetComponent<EnemyAnimation>();
            _anim = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            EnemyAttackOnPlayer();
        }

        //돌린 머즐의 방향대로 그대로 탄약을 생성시켜 물리로 발사
        /*
         * ToDo : 총의 발사 주기 수정(현재 매 프레임마다 나가게 하고 있음)
         *        총을 쏘게 되면 그 자리에 정지한 상태로 쏴야함
         *        생성된 총알 삭제(플레이어한테 맞지 않은 총알들) => 완
         *        플레이어 한테 맞았는지 안맞았는지 판단 -> 완
         */
        void EnemyAttackOnPlayer()
        {
            if (_muzzle == null) { Debug.Log("Muzzle does not exist! Please Check the Component"); return; }
            if(_findArea == null) { Debug.Log("EnemyFindArea Script does not exist! Please Check the Component"); return; }
            if(_findArea.DetectedTarget == null || _findArea.DetectedTarget.position == Vector3.zero) { return; }

            // Update가 매 프레임 호출되어도 공격 코루틴은 하나만 실행되도록 막는다. By Codex
            if (_isAttackRoutineRunning) { return; }

            StartCoroutine(EnemyAttackRoutine());
        }

        void FireAmmo()
        {
            // 발사 직전에 표적과 필수 컴포넌트를 다시 검사해 대기 중 사라진 표적을 쏘지 않는다. By Codex
            if (_ammo == null || _muzzle == null || _findArea == null || _findArea.DetectedTarget == null) { return; }

            GameObject ammo = Instantiate(_ammo, _muzzle.transform.position, _muzzle.transform.rotation);
            Rigidbody ammoRb = ammo.GetComponent<Rigidbody>();

            if(ammoRb == null)
            {
                Debug.Log("RigidBody does not exist! Please Check the Component");
                Destroy(ammo);
                return;
            }

            _deleteQue.Enqueue(ammo);
            Vector3 shot = Target * _bulletSpeed;

            ammoRb.AddForce(shot, ForceMode.Impulse);
        }

        //공격하는 범위를 원뿔로 계산, 이후 방향을 구해서, 그 방향대로 레이캐스트
        //정리하면 머즐을 돌리는 함수
        void CalculateShotArea()
        {
            Vector2 spread = Random.insideUnitCircle * _spreadAngle;
            Quaternion rotation = Quaternion.Euler(-spread.y, spread.x, 0);
            Vector3 shotDirection = rotation * _muzzle.transform.forward;

            _target = shotDirection;
            Physics.Raycast(_muzzle.transform.position, shotDirection, out RaycastHit hit, AttackRange);

            if(hit.point != Target)
            {
                //StartCoroutine(RemoveAmmoDelayRoutine());아 
            }

            if(_deleteQue == null) { StopCoroutine(RemoveAmmoDelayRoutine()); }
        }

        IEnumerator RemoveAmmoDelayRoutine()
        {
            if(_deleteQue == null) { yield return null; }
            yield return new WaitForSeconds(_delay);

            //먼저 들어갔던 총알 삭제
            _deleteQue.Dequeue();
        }

        IEnumerator EnemyAttackRoutine()
        {
            // 한 공격 사이클이 끝날 때까지 Update에서 새 코루틴이 생성되지 않게 잠근다. By Codex
            _isAttackRoutineRunning = true;

            // 무기 고유의 공격 간격을 기다린 뒤 다음 애니메이션을 시작한다. By Codex
            yield return new WaitForSeconds(_attackInterval);

            // 대기하는 동안 표적이 사라졌다면 이번 공격을 안전하게 취소한다. By Codex
            if (_findArea == null || _findArea.DetectedTarget == null || _enemyAnim == null || _anim == null)
            {
                _isAttackRoutineRunning = false;
                yield break;
            }

            AnimatorStateInfo previousState = _anim.GetCurrentAnimatorStateInfo(0);
            _enemyAnim.PlayEnemyShotAnim();

            // 트리거가 처리되어 실제 사격 애니메이션 상태로 진입할 때까지 기다린다. By Codex
            yield return null;
            while (_anim.IsInTransition(0) || _anim.GetCurrentAnimatorStateInfo(0).fullPathHash == previousState.fullPathHash)
            {
                if (_findArea.DetectedTarget == null)
                {
                    _isAttackRoutineRunning = false;
                    yield break;
                }

                yield return null;
            }

            int attackStateHash = _anim.GetCurrentAnimatorStateInfo(0).fullPathHash;

            // 진입한 사격 애니메이션이 1회 재생을 마칠 때까지 발사를 보류한다. By Codex
            while (!_anim.IsInTransition(0))
            {
                AnimatorStateInfo stateInfo = _anim.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.fullPathHash != attackStateHash || stateInfo.normalizedTime >= 1.0f) { break; }

                yield return null;
            }

            // 기본값은 한 발이며, 무기 교체 시 횟수와 간격만 바꾸면 점사/연사가 가능하다. By Codex
            for (int shotIndex = 0; shotIndex < _shotsPerAttack; shotIndex++)
            {
                if (_findArea.DetectedTarget == null) { break; }

                CalculateShotArea();
                FireAmmo();

                if (shotIndex < _shotsPerAttack - 1)
                {
                    yield return new WaitForSeconds(_burstShotInterval);
                }
            }

            // 다음 공격 사이클을 시작할 수 있도록 잠금을 해제한다. By Codex
            _isAttackRoutineRunning = false;
        }
    }
}

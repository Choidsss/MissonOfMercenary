using NUnit.Framework;
using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class EnemyAttack : MonoBehaviour
    {
        EnemyAnimation _enemyAnim;
        EnemyFindArea _findArea;
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

        Vector3 _shotDirection;

        bool _isInRange = false;
        bool _isAttackRoutineRunning;

        public AimType aimType { get; } = AimType.None;

        public WeaponType weaponType { get; set; } = WeaponType.SR;

        public int Damage { get; } = 30;

        public float AttackRange { get; } = 30;


        void Start()
        {
            _findArea = GetComponent<EnemyFindArea>();
            _enemyAnim = GetComponent<EnemyAnimation>();
        }

        private void Update()
        {
            EnemyAttackOnPlayer();
        }

        void EnemyAttackOnPlayer()
        {
            if (_muzzle == null) { Debug.Log("Muzzle does not exist! Please Check the Component"); return; }
            if(_findArea == null) { Debug.Log("EnemyFindArea Script does not exist! Please Check the Component"); return; }
            if (_isAttackRoutineRunning) { return; }
            if(_findArea.DetectedTarget == null) { return; }


            float distance = Vector3.Distance(transform.position, _findArea.DetectedTarget.position);

            if (distance > AttackRange)
            {
                _enemyAnim.PlayEnemyChaseAnimation();
                _isInRange = false;
            }
            else
            {
                _enemyAnim.PlayEnemyAimAnimation();
                _isInRange = true;
            }

            StartCoroutine(EnemyAttackRoutine());
        }

        void FireAmmo()
        {
            if (_ammo == null || _muzzle == null) { return; }

            GameObject ammo = Instantiate(_ammo, _muzzle.transform.position, _muzzle.transform.rotation);
            Rigidbody ammoRb = ammo.GetComponent<Rigidbody>();

            if(ammoRb == null)
            {
                Debug.Log("RigidBody does not exist! Please Check the Component");
                Destroy(ammo);
                return;
            }

            _deleteQue.Enqueue(ammo);
            Vector3 shot = _shotDirection * _bulletSpeed;

            Debug.Log($"{_deleteQue.Count}");

            ammoRb.AddForce(shot, ForceMode.Impulse);

            StartCoroutine(RemoveAmmoDelayRoutine());
        }


        IEnumerator RemoveAmmoDelayRoutine()
        {
            if(_deleteQue == null) { yield return null; }
            yield return new WaitForSeconds(_delay);

            //먼저 들어갔던 총알 삭제
            _deleteQue.Dequeue();
        }

        //이 함수가 불린다는건 이미 타겟을 발견했다는 것
        //이안에서 공격, 총알발사, 추격까지 처리
        IEnumerator EnemyAttackRoutine()
        {
            if (!_isInRange)
            {
                _enemyAnim.PlayEnemyAimCancelAnimation();
                _enemyAnim.PlayEnemyChaseAnimation();
                yield return null;
            }
            else
            {
                yield return new WaitForSeconds(_delay);

                FireAmmo();
                _enemyAnim.PlayEnemyShotAnim();
                
            }
        }
    }
}

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
        Queue<GameObject> _deleteQue = new Queue<GameObject>();

        [SerializeField] float _attackRange;
        [SerializeField] float _spreadAngle;
        [SerializeField] float _bulletSpeed;
        [SerializeField] float _delay = 3.0f;
        [SerializeField] float _attackDelay = 3.0f;
        [SerializeField] float _attackInterval = 3.0f;
        [SerializeField] GameObject _ammo;
        [SerializeField] GameObject _muzzle;
        [SerializeField] GameObject _muzzleFire;

        Vector3 _shotDirection;


        public AimType aimType { get; } = AimType.None;

        public WeaponType weaponType { get; set; } = WeaponType.SR;

        public int Damage { get; } = 30;

        public float AttackRange { get { return _attackRange; } }

        public void AttackStart()
        {
            StartCoroutine(EnemyFireRoutine());
        }

        public void StopAttack()
        {
            StopCoroutine(EnemyFireRoutine());
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
        }


        IEnumerator RemoveAmmoDelayRoutine()
        {
            if(_deleteQue == null) { yield return null; }
            yield return new WaitForSeconds(_delay);

            //먼저 들어갔던 총알 삭제
            _deleteQue.Dequeue();
        }

        IEnumerator EnemyFireRoutine()
        {
            FireAmmo();

            yield return new WaitForSeconds(_attackDelay);

            StartCoroutine(RemoveAmmoDelayRoutine());

        }
    }
}

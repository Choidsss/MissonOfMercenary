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
        EnemyFindArea _findArea;

        [SerializeField] float _attackRange;
        [SerializeField] float _bulletSpeed;
        [SerializeField] GameObject _ammo;
        [SerializeField] GameObject _muzzle;
        [SerializeField] GameObject _muzzleFire;

        public AimType aimType { get; } = AimType.None;

        public WeaponType weaponType { get; set; } = WeaponType.SR;

        public int Damage { get; } = 30;

        public float AttackRange { get { return _attackRange; } }

        private void Start()
        {
            _findArea = GetComponent<EnemyFindArea>();
        }

        public void OnFire()
        {
            if (_ammo == null || _muzzle == null) { return; }

            Vector3 target = _findArea.DetectedTarget + Vector3.up;
            Vector3 direction = (target - _muzzle.transform.position).normalized;

            GameObject ammo = Instantiate(_ammo, _muzzle.transform.position, _muzzle.transform.rotation);
            Rigidbody ammoRb = ammo.GetComponent<Rigidbody>();

            if(ammoRb == null)
            {
                Debug.Log("RigidBody does not exist! Please Check the Component");
                Destroy(ammo);
                return;
            }

            ammoRb.useGravity = false;
            ammoRb.linearVelocity = direction * _bulletSpeed;
            Debug.Log("Enemy Fire!");
        }
    }
}

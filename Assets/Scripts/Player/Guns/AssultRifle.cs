using UnityEngine;
using System.Collections;
using System;

namespace MIssionOfMercenary
{
    public class AssultRifle : MonoBehaviour, IWeapons
    {
        /*
         * 인풋으로 쏘는 거 들어오는거 체크해야됨
         */

        public enum SingleOrAuto
        {
            single,
            auto
        }

        public WeaponType weaponType { get; } = WeaponType.AR;

        public SingleOrAuto AttackType { get; set; } = SingleOrAuto.single;

        [Header("Muzzle")]
        [SerializeField] Transform _muzzle;
        [SerializeField] GameObject _muzzleFlash;
        [SerializeField] float _flashDestroyedTime = 2.0f;

        [Header("Weapon Options")]
        [SerializeField] public int Damage { get; } = 5;

        [SerializeField] public float AttackRange { get; } = 100;
        
        [SerializeField] float _autoSpeed = 1.0f;

        //[SerializeField] LayerMask _layer;

        // Update is called once per frame
        void Update()
        {
            Attack();
        }

        public void Attack()
        {
            if (_muzzle == null) { return; }


            RaycastHit hit;
            bool isHit = Physics.Raycast(_muzzle.position, _muzzle.forward, out hit, AttackRange);

            EnemyHit enemyHit = hit.collider.GetComponent<EnemyHit>();

            if(!isHit) { return; }



            if (isHit && enemyHit != null)
            {
                GameObject flash = Instantiate(_muzzleFlash, _muzzle.position, _muzzle.rotation);
                enemyHit.TakeDameged(Damage);
                StartCoroutine(FlashEffectDestoryRoutine(flash));
            }
        }

        IEnumerator FlashEffectDestoryRoutine(GameObject flash)
        {
            if(flash == null) { yield return null; }

            yield return new WaitForSeconds(_flashDestroyedTime);
            Destroy(flash);
        }
    }
}

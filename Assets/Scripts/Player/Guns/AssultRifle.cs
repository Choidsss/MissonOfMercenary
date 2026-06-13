using UnityEngine;
using System.Collections;
using System;
using MissionOfMercenary;

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

        [SerializeField] InputReader _inputReader;

        [Header("Muzzle")]
        [SerializeField] Transform _muzzle;
        [SerializeField] GameObject _muzzleFlash;
        [SerializeField] GameObject _bulletMarkObj;
        [SerializeField] float _flashDestroyedTime = 2.0f;
        [SerializeField] float _bulletMarkDestroyedTime = 2.0f;

        [Header("Weapon Options")]
        [SerializeField] public int Damage { get; } = 5;

        [SerializeField] public float AttackRange { get; } = 100;
        
        [SerializeField] float _autoSpeed = 1.0f;

        //[SerializeField] LayerMask _layer;



        // Update is called once per frame
        void Update()
        {
            //Attack();
        }

        void OnEnable()
        {
            _inputReader.OnshotEvent += Attack;
        }

        void OnDisable()
        {
            _inputReader.OnshotEvent -= Attack;
        }


        public void Attack(float isShot)
        {
            if (_muzzle == null) { return; }

            RaycastHit hit;
            bool isHit = Physics.Raycast(_muzzle.position, _muzzle.forward, out hit, AttackRange);

            //  머즐플래시는 항상 생성
            GameObject flash = Instantiate(_muzzleFlash, _muzzle.position, _muzzle.rotation);
            StartCoroutine(FlashEffectDestoryRoutine(flash));

            //  그 다음에 hit 체크
            if (!isHit) { return; }

            //Debug.Log("Success");
            //if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Maps"))
            //{

            //}
            Instantiate(_bulletMarkObj, hit.point, Quaternion.LookRotation(hit.normal));
            StartCoroutine(BulletMarkEffectDestoryRoutine(_bulletMarkObj));


            EnemyHit enemyHit = hit.collider.GetComponent<EnemyHit>();
            if (enemyHit != null)
            {
                enemyHit.TakeDameged(Damage);
            }

            //if (_muzzle == null) { return; }


            //RaycastHit hit;
            //bool isHit = Physics.Raycast(_muzzle.position, _muzzle.forward, out hit, AttackRange);


            //if(!isHit) { return; }


            //EnemyHit enemyHit = hit.collider.GetComponent<EnemyHit>();

            //if (isHit && enemyHit != null)
            //{
            //    GameObject flash = Instantiate(_muzzleFlash, _muzzle.position, _muzzle.rotation);
            //    enemyHit.TakeDameged(Damage);
            //    StartCoroutine(FlashEffectDestoryRoutine(flash));
            //}
        }

        IEnumerator FlashEffectDestoryRoutine(GameObject flash)
        {
            if(flash == null) { yield return null; }

            yield return new WaitForSeconds(_flashDestroyedTime);
            Destroy(flash);
        }

        IEnumerator BulletMarkEffectDestoryRoutine(GameObject mark)
        {
            if (mark == null) { yield return null; }

            yield return new WaitForSeconds(_bulletMarkDestroyedTime);
            Destroy(mark);
        }
    }
}

using UnityEngine;
using System.Collections;
using System;
using MissionOfMercenary;
using UnityEditor.Rendering.LookDev;

namespace MIssionOfMercenary
{
    public class AssultRifle : MonoBehaviour, IWeapons
    {
        public enum SingleOrAuto
        {
            single,
            auto
        }

        public WeaponType weaponType { get; } = WeaponType.AR;

        public SingleOrAuto AttackType { get; set; } = SingleOrAuto.auto;

        [SerializeField] InputReader _inputReader;

        //[Header("CrossHairTranform")]
        //[SerializeField] Transform _crossHairTransform;
        //[SerializeField] GameObject _camera;

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

        Coroutine _autoFireCoroutine;

        void Update()
        {
            //MuzzleGizmos();
        }

        void OnEnable()
        {
            _inputReader.OnshotEvent += HandleShot;
            _inputReader.OnShotCancled += HandleShotCancled;
        }

        void OnDisable()
        {
            _inputReader.OnshotEvent -= HandleShot;
            _inputReader.OnShotCancled -= HandleShotCancled;
        }


        public void Attack(float isShot)
        {
            if (_muzzle == null) { return; }

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));


            //카메라에서 중앙으로 레이를 쏨
            bool isHit = Physics.Raycast(ray, out hit, AttackRange);

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

        void HandleShot(float shot)
        {
            if(AttackType == SingleOrAuto.auto)
            {
                _autoFireCoroutine = StartCoroutine(AutoFireRoutine());
            }
            else
            {
                Attack(shot);
            }
        }

        void HandleShotCancled()
        {
            if (_autoFireCoroutine != null)
            {
                StopCoroutine(_autoFireCoroutine);
            }
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

        IEnumerator AutoFireRoutine()
        {
            while (true)
            {
                Attack(1f);
                yield return new WaitForSeconds(1f/_autoSpeed);
            }
        }

        void MuzzleGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(_muzzle.position, Vector3.forward);
        }
    }
}

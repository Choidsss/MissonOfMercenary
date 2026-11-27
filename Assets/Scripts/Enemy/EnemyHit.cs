using UnityEngine;
using System.Collections;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class EnemyHit : MonoBehaviour, IHitReceiver
    {
        [SerializeField] Rigidbody _pelivisRb;
        EnemyBodyPart _part;
        EnemyHealth  _health;
        Rigidbody[] _rigidbodies;

        Animator _anim;
        EnemyBT _bt;
        NavMeshAgent _nav;
        EnemyFindArea _area;

        [Header("Set Bonus Damage Numbers")]
        [SerializeField] int _bonusDamageArms = 5;
        [SerializeField] int _bonusDamageLegs = 5;
        [SerializeField] int _bonusDamageHead = 20;

        //int _bonusDamageBody = 0;
        int _totalDamage;


        private void Start()
        {
            _part = GetComponent<EnemyBodyPart>();
            _health = GetComponent<EnemyHealth>();
            _rigidbodies = GetComponentsInChildren<Rigidbody>();

            _anim = GetComponentInChildren<Animator>();
            _nav = GetComponent<NavMeshAgent>();
            _bt = GetComponent<EnemyBT>();
            _area = GetComponent<EnemyFindArea>();

            SetRagdoll(false);
        }

        public void SetRagdoll(bool active)
        {
            //foreach(Rigidbody rb in _rigidbodies)
            //{
            //    if (_anim != null) { /*Debug.Log("Animator Component is Not Exist!");*/ _anim.enabled = false; }
            //    if (_bt != null) { /*Debug.Log("EnemyBT Component is Not Exist!");*/ _bt.enabled = false;}
            //    if (_nav != null) { /*Debug.Log("NavMeshAgent Component is Not Exist!");*/ _nav.enabled = false; }
            //    if (_area != null) { /*Debug.Log("EnemyFindArea Component is Not Exist!");*/ _area.enabled = false; }

            //    _anim.enabled = !active;
            //    _bt.enabled = !active;
            //    _nav.enabled = !active;
            //    _area.enabled = !active;

            //    rb.isKinematic = !active;

            //    if (active)
            //    {
            //        rb.WakeUp();
            //    }
            //}

            if (active)
            {
                if (_anim != null) _anim.enabled = false;
                if (_bt != null) _bt.enabled = false;
                if (_nav != null) _nav.enabled = false;
                if (_area != null) _area.enabled = false;

                var mover = GetComponent<AStarEnemyMover>();
                if (mover != null) mover.enabled = false;

                var combat = GetComponent<EnemyCombatController>();
                if (combat != null) combat.enabled = false;

                var attack = GetComponent<SentryEnemyAttack>();
                if (attack != null)
                {
                    attack.StopAllCoroutines();
                    attack.enabled = false;
                }
            }

            foreach (Rigidbody rb in _rigidbodies)
            {
                rb.isKinematic = !active;

                if (active)
                    rb.WakeUp();
            }
        }

        public void ReceiveHit(RaycastHit hit, int damage)
        {
            if (hit.collider.gameObject.layer != 10) { Debug.Log("Does Not Equal Layer Enemy"); }// 일단 조건 이렇게 해놓음

            if (hit.collider.gameObject.layer == 10)
            {
                BodyPart hitPart = _part.GiveHitPart(hit);

                Debug.Log($"맞은 부위 : {hitPart}");

                switch (hitPart)
                {
                    case BodyPart.Arms:
                        _totalDamage = damage + _bonusDamageArms;
                        break;
                    case BodyPart.Legs:
                        _totalDamage = damage + _bonusDamageLegs;
                        break;
                    case BodyPart.Head:
                        _totalDamage = damage + _bonusDamageHead;
                        break;
                    default:
                        _totalDamage = damage;
                        break;
                }
                Debug.Log($"준 데미지 : {_totalDamage}");
                _health.TakeDamege(_totalDamage);
                _pelivisRb.constraints = RigidbodyConstraints.FreezeRotationY;//Y만 체크(시선은 고정이되, 팔다리는 움직임)
            }
        }
    }
}

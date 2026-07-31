using UnityEngine;
using System.Collections;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class EnemyHit : MonoBehaviour
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
        [SerializeField] int _bonusDamageBody = 10;

        [Header("Enemy Death Delay Sec Option")]
        [SerializeField] float _deathDelay = 3;

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

        void SetRagdoll(bool active)
        {
            foreach(Rigidbody rb in _rigidbodies)
            {
                if (_anim == null) { Debug.Log("Animator Component is Not Exist!"); }
                if (_bt == null) { Debug.Log("EnemyBT Component is Not Exist!"); }
                if (_nav == null) { Debug.Log("NavMeshAgent Component is Not Exist!"); }
                if (_area == null) { Debug.Log("EnemyFindArea Component is Not Exist!"); }

                _anim.enabled = !active;
                _bt.enabled = !active;
                _nav.enabled = !active;
                _area.enabled = !active;

                rb.isKinematic = !active;

                if (active)
                {
                    rb.WakeUp();
                }
            }
        }

        public void RecieveHit(RaycastHit muzzleHit, int damage)
        {
            if(muzzleHit.collider.gameObject.layer != 10) { Debug.Log("Does Not Equal Layer Enemy"); }// 일단 조건 이렇게 해놓음

            if(muzzleHit.collider.gameObject.layer == 10)
            {
                BodyPart hitPart = _part.GiveHitPart(muzzleHit);

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
                        _totalDamage = damage + _bonusDamageBody;
                        break;
                }
                Debug.Log($"준 데미지 : {_totalDamage}");
                _health.TakeDamege(_totalDamage);

                if (_health.IsDeath)
                {
                    _health.EnemyOnDeath();//일단 비워놓음

                    SetRagdoll(true);
                    _pelivisRb.constraints = RigidbodyConstraints.FreezeRotationY;//Y만 체크(시선은 고정이되, 팔다리는 움직임)

                    StartCoroutine(EnemyDeathDelay());
                }
            }
        }

        IEnumerator EnemyDeathDelay()
        {
            yield return new WaitForSeconds(_deathDelay);
            Debug.Log("Dealyed");
            Destroy(this.gameObject);
        }
    }
}

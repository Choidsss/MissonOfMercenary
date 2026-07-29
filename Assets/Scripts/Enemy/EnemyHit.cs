using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyHit : MonoBehaviour
    {
        EnemyBodyPart _part;
        EnemyHealth  _health;

        Rigidbody[] _rigidbodies;

        int amount;

        public bool IsDeath { get; private set; } = false;

        private void Start()
        {
            _part = GetComponent<EnemyBodyPart>();
            _health = GetComponent<EnemyHealth>();
            _rigidbodies = GetComponentsInChildren<Rigidbody>();

            SetRagdoll(false);
        }

        private void Update()
        {
            EnemyOnDeath();
        }

        void SetRagdoll(bool active)
        {
            foreach(Rigidbody rb in _rigidbodies)
            {
                rb.isKinematic = !active;
            }
        }

        public void RecieveHit(RaycastHit muzzleHit, int damage)
        {
            if(muzzleHit.collider.gameObject.layer != 10) { Debug.Log("Does Not Equal Layer Enemy"); }//일단 이렇게 해놓음

            if(muzzleHit.collider.gameObject.layer == 10)
            {
                BodyPart hitPart = _part.GiveHitPart(muzzleHit);

                //**********************여기서 체력을 깎는 함수를 만들고 부르는게 낫나? 조금더 나은 구조가 있나??**********************
                ////**********************그리고 상수코딩 수정**********************
                ///**********************Ragdoll 로 만들어놓은 적, RigidBody 여러개 다 가져와서 Kinematic.enable = false로 만들고 날아가게 만들어야 함**********************
                switch (hitPart)
                {
                    case BodyPart.Arms:
                        amount = damage + 5;
                        break;
                    case BodyPart.Legs:
                        amount = damage + 10;
                        break;
                    case BodyPart.Head:
                        amount = damage + 20;
                        break;
                    default:
                        amount = damage + 5;
                        break;
                }
            }
        }

        void EnemyOnDeath()
        {
            if(_health.Health <= 0)
            {
                

                Destroy(this.gameObject);
            }
        }
    }
}

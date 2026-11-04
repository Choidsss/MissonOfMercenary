using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class EnemyCombatController : MonoBehaviour
    {
        [SerializeField] SentryEnemyAttack _attack;
        [SerializeField] EnemyCombatPosition _combatPosition;
        [SerializeField] NavMeshAgent _agent;
        [SerializeField] Transform _player;

        //초당 회전하는 각도
        [SerializeField] float _turnSpeed = 180f;
        // 플레이어 방향과 이 각도 이내로 맞춰졌을 때 사격
        [SerializeField] float _fireAngle = 10f;

        bool _isInitialized;


        public bool Initialize(Transform player, Transform firePoint)
        {
            _isInitialized = false;

            if (player == null || firePoint == null || _combatPosition == null || _agent == null || _attack == null)
            {
                Debug.LogError("플레이어, 사격 위치, 이동 컴포넌트를 확인하세요.", this);
                return false;
            }

            if (!isActiveAndEnabled || !_combatPosition.isActiveAndEnabled || !_attack.isActiveAndEnabled)
            {
                Debug.Log("전투에 필요한 컴포넌트가 비활성화 되어 있습니다");
                return false;
            }
            _player = player;
            _combatPosition.SetFirePosition(firePoint);

            if (!_combatPosition.MoveToFirePoint())
            {
                return false;
            }

            _isInitialized = true;
            return true;
        }

        //UPdate에서 함수 돌리는거 낭비잖아---나중에 고쳐야지.
        void Update()
        {
            if (!_isInitialized)
            {
                //Initialize안해서 공격 안불리는 중
                Debug.Log("초기화 안함");
                return;
            }

            TestFacePlayer();
        }

        void TestFacePlayer()
        {
            if (_combatPosition == null || _agent == null || _player == null)
            {
                Debug.Log("1");
                return;
            }

            if(!_combatPosition.HasArrived)
            {
                Debug.Log("2");
                return;
            }

            if (!_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
            {
                Debug.Log("3");
                return;
            }

            _agent.updateRotation = false;

            //Debug.Log("내려왔음");
            FacePlayer();
            TryAttackPlayer();
        }

        void FacePlayer()
        {
            Vector3 direction = _player.position - transform.position;
            direction.y = 0;

            if(direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation.normalized, _turnSpeed * Time.deltaTime);

        }

        void TryAttackPlayer()
        {
            if(_attack == null || !_attack.isActiveAndEnabled)
            {
                return;
            }

            Vector3 direction = _player.position - transform.position;

            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            float angle = Vector3.Angle(transform.forward, direction);

            if(angle > _fireAngle)
            {
                return;
            }

            Debug.Log("Attack");
            _attack.Attack(_player.position);
        }
    }
}

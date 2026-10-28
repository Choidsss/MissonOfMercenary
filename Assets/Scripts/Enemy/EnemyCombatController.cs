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

        public void Initialize(Transform player, Transform firePoint)
        {
            if (player == null || firePoint || _combatPosition == null)
            {
                Debug.LogError("플레이어, 사격 위치, 이동 컴포넌트를 확인하세요.", this);
                return;
            }

            _player = player;
            _combatPosition.SetFirePosition(firePoint);
        }

        //UPdate에서 함수 돌리는거 낭비잖아---나중에 고쳐야지.
        void Update()
        {
            TestFacePlayer();
        }

        void TestFacePlayer()
        {
            if (_combatPosition == null || _agent == null || _player == null)
            {
                return;
            }

            if(!_combatPosition.HasArrived)
            {
                return;
            }

            if (!_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
            {
                return;
            }

            _agent.updateRotation = false;

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

            _attack.Attack(_player.position);
        }
    }
}

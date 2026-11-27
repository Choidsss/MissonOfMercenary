using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class EnemyCombatController : MonoBehaviour
    {
        Transform _player;
        AStarEnemyMover _mover;
        NavMeshAgent _agent;

        [SerializeField] SentryEnemyAttack _attack;

        //초당 회전하는 각도
        [SerializeField] float _turnSpeed = 180f;
        // 플레이어 방향과 이 각도 이내로 맞춰졌을 때 사격
        [SerializeField] float _fireAngle = 10f;

        bool _isInitialized;


        private void Awake()
        {
            _mover = GetComponent<AStarEnemyMover>();
            _agent = GetComponent<NavMeshAgent>();
        }

        public bool Initialize(Transform player)
        {
            _player = player;

            _isInitialized = _player != null && _mover != null && _agent != null;

            return _isInitialized;
        }

        //UPdate에서 함수 돌리는거 낭비잖아---나중에 고쳐야지.
        void Update()
        {
            if (!_isInitialized || _player == null)
            {
                return;
            }

            if (!_mover.HasArrived)
            {
                return;
            }

            if (!_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
            {
                return;
            }

            _agent.updatePosition = false;

            FacePlayer(); //Player를 바라봄
            TryAttackPlayer(); //그 상태에서 공격요청
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
            if (_attack == null || !_attack.isActiveAndEnabled)
            {
                return;
            }

            Vector3 direction = _player.position - transform.position;

            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            float angle = Vector3.Angle(transform.forward, direction);

            if (angle > _fireAngle)
            {
                return;
            }

            Debug.Log("Spawn Enemy Attack to player");
            _attack.Attack(_player.position);
        }
    }
}

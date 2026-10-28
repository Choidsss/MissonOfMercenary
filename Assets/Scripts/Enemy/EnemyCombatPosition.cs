using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class EnemyCombatPosition : MonoBehaviour
    {
        [SerializeField] NavMeshAgent _agent;
        [SerializeField] Transform _firePoint;

        public bool HasArrived { get; private set; }

        bool _isMoveing = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            MoveToFirePoint();
        }

        private void Update()
        {
            TestEnemyMoveInUpdate();
        }

        public void MoveToFirePoint()
        {
            HasArrived = false;
            _isMoveing = false;

            if(_agent == null || _firePoint == null)
            {
                Debug.Log("Agent 또는 FirePoint 연결이 필요합니다");
                return;
            }

            if(!_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
            {
                Debug.Log("적이 활성화된 NavMesh위에 있어야 합니다");
                return;
            }

            _agent.isStopped = false;
            _agent.updateRotation = true;

            _isMoveing = _agent.SetDestination(_firePoint.position);
        }

        void TestEnemyMoveInUpdate()
        {
            if (!_isMoveing) { return; }

            if(_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
            {
                _isMoveing = false;
                return;
            }

            if (_agent.pathPending)
            {
                return;
            }

            if (_agent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
                _isMoveing = false;

                Debug.LogWarning("FirePoint까지 도달 가능한 경로가 없습니다.", this);
                return;
            }

            float arrivalDistance = Mathf.Max(_agent.stoppingDistance, 0.1f);

            if (_agent.remainingDistance > arrivalDistance)
            {
                return;
            }

            _agent.isStopped = true;
            _agent.ResetPath();

            _isMoveing = false;
            HasArrived = true;

            Debug.Log("사격위치에 도달 하였습니다");
        }

        public void SetFirePosition(Transform firePoint)
        {
            _firePoint = firePoint;
        }
    }
}

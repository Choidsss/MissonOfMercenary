using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    // *** 추가: 적마다 별도의 경로와 진행 위치를 보관한다.
    [RequireComponent(typeof(NavMeshAgent))]
    public class AStarEnemyMover : MonoBehaviour
    {
        NavMeshAgent _agent;
        AStarGrid _grid;
        List<Vector3> _path;
        int _index;
        bool _moving;

        public bool HasArrived { get; private set; }

        public bool MoveTo(AStarGrid grid, Vector3 target)
        {
            _agent = GetComponent<NavMeshAgent>();
            HasArrived = false;
            _moving = false;

            if (grid == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
            {
                return false;
            }

            _agent.isStopped = true;
            _agent.ResetPath();
            _grid = grid;
            _path = grid.FindPath(_agent.transform.position, target);
            if (_path == null || _path.Count == 0)
            {
                return false;
            }

            // *** 추가: 실제 스폰 위치에서 출발 셀 중앙까지 먼저 이동한다.
            _index = 0;
            return SetNextDestination();
        }

        void ApplySettings()
        {
            _agent.speed = _grid.MoveSpeed;
            _agent.angularSpeed = _grid.TurnSpeed;
            _agent.updateRotation = true;
        }

        bool SetNextDestination()
        {
            ApplySettings();
            if (!NavMesh.SamplePosition(_path[_index], out NavMeshHit hit,
                _grid.SampleDistance, _agent.areaMask))
            {
                StopMovement("경유점 주변에 NavMesh가 없습니다.");
                return false;
            }

            _agent.isStopped = false;
            _moving = _agent.SetDestination(hit.position);
            if (!_moving)
            {
                StopMovement("이동 요청에 실패했습니다.");
            }
            return _moving;
        }

        void Update()
        {
            if (!_moving) return;
            if (_grid == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
            {
                StopMovement("이동 중 격자 또는 NavMeshAgent가 유효하지 않습니다.");
                return;
            }

            ApplySettings();
            if (_agent.pathPending) return;
            if (_agent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                StopMovement("경유점까지 도달할 수 없습니다.");
                return;
            }
            if (_agent.remainingDistance > Mathf.Max(_agent.stoppingDistance, 0.1f)) return;

            _index++;
            if (_index >= _path.Count)
            {
                HasArrived = true;
                StopMovement();
                return;
            }
            SetNextDestination();
        }

        void StopMovement(string reason = null)
        {
            _moving = false;
            if (_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
            }
            if (reason != null) Debug.LogWarning(reason, this);
        }

        void OnDisable()
        {
            StopMovement();
        }

        void OnDrawGizmosSelected()
        {
            if (_path == null) return;
            Gizmos.color = Color.green;
            for (int i = 1; i < _path.Count; i++)
                Gizmos.DrawLine(_path[i - 1], _path[i]);
        }
    }
}

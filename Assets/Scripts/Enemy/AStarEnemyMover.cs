using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    // *** 추가: 적마다 별도의 경로와 진행 위치를 보관한다.
    [RequireComponent(typeof(NavMeshAgent))]
    public class AStarEnemyMover : MonoBehaviour
    {
        EnemyAnimation _enemyAnimation;
        NavMeshAgent _agent;
        AStarGrid _grid;
        List<Vector3> _path;
        int _index;
        bool _moving;

        public bool HasArrived { get; private set; }

        private void Awake()
        {
            _enemyAnimation = GetComponent<EnemyAnimation>();
        }

        public bool MoveTo(AStarGrid grid, Vector3 target)
        {
            _agent = GetComponent<NavMeshAgent>();
            HasArrived = false;
            _moving = false;

            // 수정: 이동 시작 실패의 원인을 Grid, Agent 활성 상태, NavMesh 배치로 구분.
            if (grid == null)
            {
                Debug.LogWarning("[Mover] 이동 실패: AStarGrid가 전달되지 않았습니다.", this);
                return false;
            }
            if (_agent == null || !_agent.isActiveAndEnabled)
            {
                Debug.LogWarning("[Mover] 이동 실패: NavMeshAgent가 없거나 비활성 상태입니다.", this);
                return false;
            }
            if (!_agent.isOnNavMesh)
            {
                Debug.LogWarning($"[Mover] 이동 실패: 적이 NavMesh 위에 배치되지 않았습니다. 현재 위치: {transform.position}", this);
                return false;
            }

            _agent.isStopped = true;
            _agent.ResetPath();
            _grid = grid;
            _path = grid.FindPath(_agent.transform.position, target);
            if (_path == null || _path.Count == 0)
            {
                // 수정: 생성 좌표와 엄폐 좌표를 포함해 A* 경로 탐색 실패를 출력.
                Debug.LogWarning($"[Mover] 이동 실패: A* 경로를 찾지 못했습니다. 출발: {transform.position}, 목표: {target}. 그리드 범위와 장애물 셀을 확인하세요.", this);
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

            //이동 중 달리는 애니메이션
            _enemyAnimation.PlayTerroristRunAnimation();

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

            //지정 엄폐물 위치 도착
            if (_index >= _path.Count)
            {
                _enemyAnimation.PlayTerroristStopRunAnimation();

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

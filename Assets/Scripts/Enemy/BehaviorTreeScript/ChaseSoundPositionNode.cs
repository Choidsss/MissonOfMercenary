using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class ChaseSoundPositionNode : BTNode
    {
        readonly float _speed;
        readonly NavMeshAgent _nav;
        EnemyChase _enemyChase;


        public ChaseSoundPositionNode(float speed, NavMeshAgent nav, EnemyChase enemyChase)
        {
            _speed = speed;
            _nav = nav;
            _enemyChase = enemyChase;
        }

        public override State Evaluate()
        {
            // SetDestination 호출 전 _nav.enabled와 _nav.isOnNavMesh도 함께 검사해야 예외를 방지할 수 있다. By Codex
            if(_nav == null) { Debug.Log("NavMesh Agent Component Does Not Exist"); return State.Failure; }
            if(_nav.enabled == false || !_nav.isOnNavMesh) { Debug.Log("NavMesh Agent Component is wrong option I guess"); return State.Failure; }
            if (_enemyChase == null) { Debug.Log("Can't Find Component EnemyChase"); return State.Failure; }
            if (!_enemyChase.HasSoundTarget) { Debug.Log("Can't heard the gunSound"); return State.Failure; }

            _nav.speed = _speed;
            _nav.SetDestination(_enemyChase.SoundTargetPosition);

            if(_nav.pathPending) { return State.Running; }

            if(_nav.remainingDistance <= _nav.stoppingDistance)
            {
                _enemyChase.ClearSoundTarget();

                return State.Success;
            }

            return State.Running;
        }
    }
}

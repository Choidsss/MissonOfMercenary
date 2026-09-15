using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class ChaseSoundPositionNode : BTNode
    {
        readonly float _speed;
        readonly NavMeshAgent _nav;
        EnemyChase _enemyChase;

        Vector3 _lastSoundPosition;
        bool _hasDestinaiton;

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

            Vector3 newTarget = _enemyChase.SoundTargetPosition;
            _nav.isStopped = false;
            _nav.speed = _speed;
            
            if(!_hasDestinaiton || _lastSoundPosition != newTarget)
            {
                _lastSoundPosition = newTarget;
                _hasDestinaiton = true;

                _nav.SetDestination(newTarget);
                return State.Running;
            }

            if(_nav.pathPending) { return State.Running; }

            float arriveDistance = Mathf.Max(_nav.stoppingDistance, 0.2f);

            if(_nav.remainingDistance <= arriveDistance && (!_nav.hasPath || _nav.velocity.sqrMagnitude < 0.01f))
            {
                _hasDestinaiton = false;
                _enemyChase.ClearSoundTarget();

                return State.Success;
            }

            return State.Running;
        }
    }
}

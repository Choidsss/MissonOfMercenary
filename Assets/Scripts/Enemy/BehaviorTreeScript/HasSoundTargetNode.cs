using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class HasSoundTargetNode : BTNode
    {
        readonly EnemyChase _enemy;

        public HasSoundTargetNode(EnemyChase enemyChase)
        {
            _enemy = enemyChase;
        }

        public override State Evaluate()
        {
            if(_enemy == null) { Debug.Log("Can't Find a EnemyChase Component"); return State.Failure; }

            if (_enemy.HasSoundTarget)
            {
                return State.Success;
            }

            return State.Failure;
        }
    }
}

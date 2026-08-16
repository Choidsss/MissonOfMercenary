using UnityEngine;

namespace MIssionOfMercenary
{
    public class IsDetectedPlayerNode : BTNode
    {
        EnemyFindArea _findArea;

        public IsDetectedPlayerNode(EnemyFindArea findArea)
        {
            _findArea = findArea;
        }

        public override State Evaluate()
        {
            if (_findArea == null || !_findArea.IsDetectedPlayer)
            {
                return State.Failure;
            }
            return State.Success;
        }

    }
}

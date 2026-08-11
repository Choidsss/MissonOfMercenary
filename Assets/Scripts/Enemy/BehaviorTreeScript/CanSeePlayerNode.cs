using UnityEngine;

namespace MIssionOfMercenary
{
    public class CanSeePlayerNode : BTNode
    {
        EnemyFindArea _findArea;

        public CanSeePlayerNode(EnemyFindArea findArea)
        {
            _findArea = findArea;
        }

        public override State Evaluate()
        {
            if(_findArea == null || _findArea.DetectedTarget == null)
            {
                return State.Failure;
            }

            return State.Success;
        }

    }
}

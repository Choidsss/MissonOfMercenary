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
            if(_findArea == null)
            {
                return State.Failure;
            }

            return _findArea.IsDetectedPlayer ? State.Success : State.Failure;
        }

    }
}

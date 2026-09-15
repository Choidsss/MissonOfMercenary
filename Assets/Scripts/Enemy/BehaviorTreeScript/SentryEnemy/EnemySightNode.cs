using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemySightNode : BTNode
    {
        SentrySensor _sentrySensor;
        EnemyFindArea _blockState;

        public EnemySightNode(SentrySensor sentrySensor, EnemyFindArea blockState)
        {
            _sentrySensor = sentrySensor;
            _blockState = blockState;
        }

        public override State Evaluate()
        {
            if (!_sentrySensor.SentryDetectedPlayer) { return State.Failure; }

            if(!_blockState.IsBlockedByObtacles(_sentrySensor.DetectedTarget))
            {
                return State.Success;
            }
            else
            {
                return State.Failure;
            }
        }
    }
}

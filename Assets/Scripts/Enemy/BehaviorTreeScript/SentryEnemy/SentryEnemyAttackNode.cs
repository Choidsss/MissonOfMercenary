using UnityEngine;

namespace MIssionOfMercenary
{
    public class SentryEnemyAttackNode : BTNode
    {
        readonly EnemyBT _enemyBT;
        readonly SentryEnemyAttack _sentryAttack;
        readonly SentrySensor _sensor;
        readonly EnemyAnimation _enemyAnimation;
        readonly SentryEnemySaveGunTransform _gunTransform;

        public SentryEnemyAttackNode(EnemyBT bt, SentryEnemyAttack sentryAttack, SentrySensor sensor, EnemyAnimation enemyAnimation)
        {
            _enemyBT = bt; 
            _sentryAttack = sentryAttack; 
            _sensor = sensor;
            _enemyAnimation = enemyAnimation;
            _gunTransform = bt != null ? bt.GetComponent<SentryEnemySaveGunTransform>() : null;
        }

        public override State Evaluate()
        {
            if (_enemyBT == null || _sentryAttack == null || _sensor == null || _enemyAnimation == null) { return State.Failure; }
            if (!_sensor.SentryDetectedPlayer) { return State.Failure; }

            _enemyBT.LookAtTarget(_sensor.DetectedTarget); //바라보는건 계속 해야되는 거고

            _enemyAnimation.PlayEnemyAimAnimation();

            if (_enemyAnimation.IsAnimationFinish)
            {
                _enemyAnimation.StopAnimation();
                if (_gunTransform != null && !_gunTransform.ApplyAimingGunTransform())
                {
                    return State.Running;
                }
                _sentryAttack.Attack(_sensor.DetectedTarget);
                //추후 Attack함수 안에 있는 delay를 건드릴예정 && 탄퍼짐

                return State.Running;
            }

            return State.Running;

        }
    }
}

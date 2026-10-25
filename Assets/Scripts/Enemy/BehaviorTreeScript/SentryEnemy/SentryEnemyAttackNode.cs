using UnityEngine;

namespace MIssionOfMercenary
{
    public class SentryEnemyAttackNode : BTNode
    {
        readonly EnemyBT _enemyBT;
        readonly SentryEnemyAttack _sentryAttack;
        readonly SentrySensor _sensor;
        readonly EnemyAnimation _enemyAnimation;

        public SentryEnemyAttackNode(EnemyBT bt, SentryEnemyAttack sentryAttack, SentrySensor sensor, EnemyAnimation enemyAnimation)
        {
            _enemyBT = bt; 
            _sentryAttack = sentryAttack; 
            _sensor = sensor;
            _enemyAnimation = enemyAnimation;
        }

        public override State Evaluate()
        {
            if (_enemyBT == null || _sentryAttack == null || _sensor == null || _enemyAnimation == null) { return State.Failure; }
            if (!_sensor.SentryDetectedPlayer) { return State.Failure; }

            _enemyBT.LookAtTarget(_sensor.DetectedTarget);
            float distance = (_sensor.DetectedTarget - _sensor.transform.position).magnitude;

            if (distance > _sentryAttack.AttackRange)
            {
                return State.Failure;
            }

            //달리는 애니메이션 없이 쏘는 애니메이션만 필요함

            _sentryAttack.Attack(_sensor.DetectedTarget);
            _enemyAnimation.PlayRobotEnemyShotAnim(); //함수 수정

            if (_sentryAttack.Ammo <= 0)
            {
                //_enemyAnimation.ReloadAnimation();
            }

            return State.Running;

        }
    }
}

namespace MIssionOfMercenary
{
    public class SentryEnemyAttackNode : BTNode
    {
        readonly EnemyBT _enemyBT;
        readonly SentryEnemyAttack _sentryAttack;
        readonly SentrySensor _sensor;

        public SentryEnemyAttackNode(EnemyBT bt, SentryEnemyAttack sentryAttack, SentrySensor sensor)
        {
            _enemyBT = bt; // 플레이어 방향으로 회전시키기 위한 EnemyBT 참조를 저장합니다. By Codex
            _sentryAttack = sentryAttack; // 실제 연사와 풀링을 담당하는 공격 컴포넌트를 저장합니다. By Codex
            _sensor = sensor; // 매 프레임 최신 플레이어 위치를 읽기 위한 센서 참조를 저장합니다. By Codex
        }

        public override State Evaluate()
        {
            if (_enemyBT == null || _sentryAttack == null || _sensor == null) { return State.Failure; } // 필수 참조가 없으면 공격을 실행하지 않습니다. By Codex
            if (!_sensor.SentryDetectedPlayer) { return State.Failure; } // 아직 플레이어를 감지하지 못했다면 공격 시퀀스를 실패시킵니다. By Codex

            _enemyBT.LookAtTarget(_sensor.DetectedTarget); // 센서가 제공하는 현재 플레이어 위치를 계속 바라봅니다. By Codex
            _sentryAttack.Attack(_sensor.DetectedTarget); // 공격 컴포넌트가 쿨다운에 맞춰 풀링 탄환을 연사하게 합니다. By Codex

            return State.Running; // 플레이어가 공격 가능한 동안 이 행동을 계속 평가합니다. By Codex
        }
    }
}

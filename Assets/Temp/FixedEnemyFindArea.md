# FixedEnemyFindArea

## 목표

Enemy가 시야 범위 안에서 Player를 한 번 발견하면 `DetectedTarget`에 Player를 저장한다.
그 이후에는 Player가 최초 감지 범위나 시야각 밖으로 나가더라도 타깃을 잃지 않고 계속 추적하며, 공격 거리 안에 들어오면 멈춰서 Player를 바라보고 공격한다.

## 기존 코드의 문제

기존 `DetectPlayer()`는 호출될 때마다 다음 코드로 타깃을 초기화한다.

```csharp
_detectedTarget = null;
```

따라서 Player가 감지 범위 밖으로 나가거나 장애물에 가려지면 `DetectedTarget`이 다시 `null`이 되어 추적이 중단된다.

또한 타깃을 한 번 저장한 뒤에도 바라보는 방향을 갱신하려면 `_playerPosition`을 현재 타깃 위치로 계속 갱신해야 한다.

## EnemyFindArea 수정 방향

`DetectedTarget`이 이미 존재할 때는 시야 검사를 다시 수행하거나 타깃을 초기화하지 않는다. 대신 Player의 현재 위치만 갱신하고 감지 상태를 유지한다.

```csharp
void Update()
{
    DetectPlayer();

    if (_detectedTarget != null)
    {
        _playerPosition = _detectedTarget.position;
    }
}

bool DetectPlayer()
{
    // 한 번 발견한 타깃은 계속 유지한다.
    if (_detectedTarget != null)
    {
        _isDetected = true;
        _playerPosition = _detectedTarget.position;
        return true;
    }

    Collider[] colliders = Physics.OverlapSphere(
        transform.position,
        _distance,
        _playerLayer,
        QueryTriggerInteraction.Ignore
    );

    foreach (Collider col in colliders)
    {
        Vector3 playerPosition = col.transform.position;

        if (IsPlayerInEnemyDegree(playerPosition) &&
            !IsBlockedByObstacles(playerPosition))
        {
            _detectedTarget = col.transform;
            _playerPosition = playerPosition;
            _isDetected = true;
            _isSense = false;

            return true;
        }
    }

    _isDetected = false;
    return false;
}
```

중요한 점은 기존의 무조건적인 `_detectedTarget = null` 코드를 제거하는 것이다.

## 시야각 검사 수정

필드에 저장된 이전 위치가 아니라 현재 검사 중인 Player 위치를 직접 전달해서 검사한다.

```csharp
bool IsPlayerInEnemyDegree(Vector3 playerPosition)
{
    Vector3 direction = playerPosition - transform.position;
    float angle = Vector3.Angle(transform.forward, direction);

    return angle < _degree;
}
```

## 장애물 검사 수정

현재 검사 중인 Player 위치를 전달받아서 Raycast의 끝점을 계산한다.

```csharp
bool IsBlockedByObstacles(Vector3 playerPosition)
{
    Vector3 start = transform.position + _eyeHeight;
    Vector3 end = playerPosition + _targetHeight;
    Vector3 direction = end - start;

    return Physics.Raycast(
        start,
        direction.normalized,
        direction.magnitude,
        _obstaclesLayer,
        QueryTriggerInteraction.Ignore
    );
}
```

## Player 바라보기 수정

캐시된 `_playerPosition`만 사용하는 것보다 `DetectedTarget.position`을 직접 사용하는 편이 안전하다.

```csharp
public void LookAtPlayer()
{
    if (_detectedTarget == null)
        return;

    Vector3 direction = _detectedTarget.position - transform.position;
    direction.y = 0f;

    if (direction.sqrMagnitude <= 0.01f)
        return;

    Quaternion targetRotation = Quaternion.LookRotation(direction);
    targetRotation *= Quaternion.Euler(0f, _lookAngleOffset, 0f);

    transform.rotation = Quaternion.Slerp(
        transform.rotation,
        targetRotation,
        _enemyTurnAmount * Time.deltaTime
    );
}
```

## 공격 거리 검사 추가

현재 `EnemyAttack.EnemyAttackOnPlayer()`는 `DetectedTarget`이 있는지만 확인한다. 타깃을 영구적으로 유지하면 먼 거리에서도 공격 코루틴이 시작될 수 있으므로, 코루틴을 실행하기 전에 공격 거리 검사를 추가해야 한다.

```csharp
float distance = Vector3.Distance(
    transform.position,
    _findArea.DetectedTarget.position
);

if (distance > AttackRange)
    return;
```

위 코드는 `EnemyAttackOnPlayer()`에서 `DetectedTarget`의 null 검사를 통과한 다음, `_isAttackRoutineRunning`을 검사하고 코루틴을 시작하기 전에 배치한다.

## 최종 동작 흐름

1. 미발견 상태에서는 `DetectPlayer()`가 범위, 시야각, 장애물을 검사한다.
2. Player를 발견하면 `_detectedTarget`에 Player의 `Transform`을 저장한다.
3. 발견 이후에는 Player가 최초 감지 범위 밖으로 나가도 타깃을 유지한다.
4. 공격 거리 밖에서는 `ChasePlayerNode`가 Player의 현재 위치로 이동한다.
5. 공격 거리 안에서는 `EnemyAttackNode`가 NavMeshAgent를 멈추고 Player를 바라본다.
6. `EnemyAttack`은 공격 거리 안에 있을 때만 공격 코루틴을 시작한다.

## 확인 사항

- `EnemyFindArea.Update()`에서 `DetectPlayer()`의 주석을 해제해야 한다.
- `ChasePlayerNode`는 `DetectedTarget != null`일 때 공격 거리 밖의 Player를 계속 추적한다.
- `EnemyAttackNode`는 현재 `IsDetectedPlayer`도 검사하므로, 타깃을 유지하는 동안 `_isDetected` 역시 `true`로 유지해야 한다.
- Player가 사망하거나 제거됐을 때 추적을 해제할 필요가 생기면 별도의 `ClearTarget()` 메서드를 추가하는 것이 좋다.

```csharp
public void ClearTarget()
{
    _detectedTarget = null;
    _isDetected = false;
    _playerPosition = Vector3.zero;
}
```

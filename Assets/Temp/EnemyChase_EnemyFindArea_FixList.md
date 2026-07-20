# EnemyChase / EnemyFindArea 수정 목록

## 먼저 고쳐야 하는 기능 오류

### 1. FindPlayer 결과를 null 검사하기 전에 사용하고 있음

현재 코드:

```csharp
Vector3 reTarget = FindPlayer().transform.position;
if (reTarget == null)
```

`FindPlayer()`가 `null`이면 `.transform`에서 `NullReferenceException`이 발생한다. 또한 `Vector3`는 값 타입이므로 `null`과 비교할 수 없다.

수정 예시:

```csharp
GameObject player = FindPlayer();

if (player == null)
{
    DoFind = false;
    return;
}

_targetPos = player.transform.position;
```

### 2. EnemyChase의 탐색 범위와 플레이어 레이어가 설정되지 않음

`_radius`와 `_playerLayer`에 `SerializeField`가 없고 코드에서도 값을 할당하지 않는다. 현재 기본값은 반지름 `0`, 빈 LayerMask이므로 `FindPlayer()`가 플레이어를 찾지 못한다.

```csharp
[SerializeField] LayerMask _playerLayer;
[SerializeField] float _radius;
```

추가 후 Enemy 프리팹 인스펙터에서 Player 레이어와 탐색 반지름을 설정해야 한다.

### 3. 소리를 추적할 때 LookAtPlayer를 호출하고 있음

`EnemyChase`의 목표는 소리가 난 `TargetPosition`이지만, `EnemyFindArea.LookAtPlayer()`는 `EnemyFindArea._playerPosition`을 바라본다. 플레이어가 감지되지 않았거나 이전 위치가 남아 있으면 잘못된 방향을 바라본다.

소리 위치를 바라보는 별도 함수가 필요하다.

```csharp
void LookAtTargetPosition()
{
    Vector3 direction = TargetPosition - transform.position;
    direction.y = 0f;

    if (direction.sqrMagnitude <= 0.01f) return;

    Quaternion targetRotation = Quaternion.LookRotation(direction);
    transform.rotation = Quaternion.Slerp(
        transform.rotation,
        targetRotation,
        _amount * Time.deltaTime);
}
```

`EnemyMoveToSoundPosition()`에서는 `_findArea.LookAtPlayer()` 대신 이 함수를 호출한다.

### 4. 시야각 검사 전에 플레이어 방향으로 회전하고 있음

`EnemyFindArea.DetectPlayer()`에서 `LookAtPlayer()`를 먼저 실행하면 원래 Enemy의 정면 방향이 바뀐 뒤 시야각을 검사하게 된다. 이 경우 플레이어가 원래 시야 밖에 있어도 감지될 수 있다.

권장 순서:

```csharp
if (IsPlayerInEnemyDegree() && !IsBlockedByObtacles())
{
    LookAtPlayer();
    // 감지 성공 처리
}
```

## 다음 단계에서 연결해야 하는 기능

### 5. 플레이어 직접 감지와 추적 이동이 아직 연결되지 않음

`EnemyFindArea`는 `DetectedTarget`을 제공하지만 `EnemyChase`는 소리 위치만 따라간다. 직접 발견한 플레이어를 계속 추적하려면 다음 중 하나로 연결해야 한다.

- `DetectedTarget.position`을 `TargetPosition`으로 계속 갱신
- `ChasePlayerNode`를 Behavior Tree에 활성화
- 추후 상태 머신에서 SoundChase와 PlayerChase를 분리

### 6. IsSense 상태가 실제로 사용되지 않음

`_isSense`는 선언되어 있지만 `true`로 바뀌는 코드가 없다. 다음처럼 상태 의미를 정해야 한다.

- `IsDetectedPlayer`: 플레이어의 정확한 위치를 시야로 확인함
- `IsSensePlayer`: 소리만 듣고 대략적인 위치를 조사 중임

소리 위치를 전달받을 때 감지 상태를 설정할 구조가 필요하다.

### 7. 후방 감지 옵션이 사용되지 않음

`_backsideDegree`, `_backsideDistance`는 현재 어떤 검사에도 사용되지 않는다. 후방 인기척 기능을 구현하거나, 사용 계획이 없다면 일단 제거한다.

## 리팩터링할 때 개선할 부분

### 8. Lerp 이동은 벽과 NavMesh를 무시함

현재 이동:

```csharp
transform.position = Vector3.Lerp(
    transform.position,
    TargetPosition,
    _amount * Time.deltaTime);
```

벽을 통과하거나 NavMesh 밖으로 이동할 수 있다. 최종적으로는 `NavMeshAgent.SetDestination()`을 사용하는 것이 좋다.

### 9. Vector3.zero를 목표 없음 표시로 사용 중

월드 원점 `(0, 0, 0)`도 유효한 목표 위치일 수 있다. 목표 존재 여부는 `DoFind` 또는 별도의 `bool _hasTarget`으로만 판단하는 것이 안전하다.

### 10. 총소리 전달 시 동일 Enemy가 여러 번 처리될 수 있음

Enemy의 Ragdoll 콜라이더가 여러 개라서 `OverlapSphere` 결과에 같은 Enemy가 중복으로 들어온다. `HashSet<EnemyChase>`로 중복을 제거하거나 감지 전용 콜라이더 하나만 Enemy 레이어로 분리하는 것이 좋다.

또한 반복문 안에서 `EnemyChase`를 찾지 못했을 때 `return`하면 나머지 Enemy 처리가 모두 중단된다. 이 경우 `continue`를 사용해야 한다.

```csharp
if (ec == null)
{
    Debug.Log("Can't Find a Component<EnemyChase>");
    continue;
}
```

## 권장 작업 순서

1. `FindPlayer()` null 처리 수정
2. `_radius`, `_playerLayer`를 인스펙터에서 설정 가능하게 변경
3. 소리 위치를 바라보는 함수 분리
4. 시야각 검사 후 플레이어를 바라보도록 순서 변경
5. 도착 후 플레이어 발견/미발견 흐름 테스트
6. 직접 발견 추적과 소리 추적 연결
7. NavMeshAgent 이동으로 교체
8. 감지 상태와 클래스 책임 리팩터링

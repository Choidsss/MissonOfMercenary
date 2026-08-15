# EnemyBT 피드백

검토 범위: `Assets/Scripts/Enemy/BehaviorTreeScript`, `EnemyFindArea`, `EnemyAttack`, `EnemyAnimation`, `EnemyChase`

## 한줄 결론

현재 트리의 큰 우선순위인 **플레이어 전투 → 소리 위치 추적 → 순찰**은 좋은 출발이다. 다만 지금 상태에서는 몇 가지 컴파일/실행 오류가 있고, 각 노드가 끝나거나 다른 노드로 전환될 때의 정리가 빠져 있다. 이것부터 고친 다음 `바라보기`를 BT의 별도 액션 노드로 옮기는 것을 추천한다.

## 좋은 점

- `EnemyBT.SetupTree()`만 보면 AI의 행동 우선순위가 바로 읽힌다.
- 감지, 공격 거리 확인, 공격, 추적, 소리 조사, 순찰을 노드로 분리하려는 방향이 좋다.
- `Selector`와 `Sequence`의 기본 반환 규칙은 현재 의도에 맞게 구현되어 있다.
- `NavMeshAgent`가 없거나 NavMesh 위에 없을 때를 검사하려는 방어 코드도 좋은 습관이다.

## 우선순위가 높은 수정 사항

### 1. `Vector3`를 `null`과 비교하고 있다

위치 값인 `DetectedTarget`의 타입은 `Vector3`라서 `null`로 “타깃 없음”을 표현할 수 없다.

- `CanSeePlayerNode.cs:16`
- `ChasePlayerNode.cs:26`
- `EnemyFindArea.cs:71`
- 참고로 기존 `EnemyAttackNode.cs:24`도 같은 문제

이미 `EnemyFindArea.IsDetectedPlayer`가 있으므로 조건 노드는 이 값을 확인하면 된다. 더 명확하게 만들려면 `Transform DetectedTarget`을 공개하고 위치가 필요할 때 `.position`을 쓰거나, `bool TryGetDetectedPosition(out Vector3 position)` 형태를 사용할 수 있다.

### 2. `ChaseToPlayerNode`의 NavMesh 검사 조건이 반대다

`ChaseToPlayerNode.cs:23`의 아래 조건은 `_nav`가 **존재하면** 실패한다.

```csharp
if (_enemyAnimation == null || _findArea == null || _nav)
```

의도는 `_nav == null`일 가능성이 높다. 아울러 `enabled`, `isOnNavMesh`, `_enemyAttack`, 감지 여부도 `SetDestination` 전에 검사해야 한다. 현재 상태라면 정상적으로 NavMeshAgent가 붙어 있는 적은 추적 노드에서 계속 `Failure`가 난다.

### 3. 감지가 한 번 성공하면 영원히 풀리지 않는다

`EnemyFindArea.cs:43`에서 `_isDetected`가 참이면 플레이어 위치만 갱신하고 즉시 반환한다. 이후에는 거리, 시야각, 벽 가림을 다시 검사하지 않으므로 한 번 본 플레이어를 벽 너머에서도 영원히 안다.

이것이 의도한 “기억”이라면 다음 둘을 분리하는 편이 좋다.

- `CanCurrentlySeePlayer`: 현재 실제로 보이는가
- `LastKnownPlayerPosition`: 마지막으로 본 위치
- 필요하다면 기억 제한 시간

현재 이름인 `IsDetectedPlayer`만으로는 “지금 보임”인지 “전에 본 적 있음”인지 알기 어렵다.

### 4. 시야 거리 계산에서 제곱값과 일반값을 비교한다

`EnemyFindArea.cs:51`에서 `direction.sqrMagnitude`와 `_eyeSightDistance`를 바로 비교한다. 둘 중 하나로 단위를 맞춰야 한다.

```csharp
direction.sqrMagnitude <= _eyeSightDistance * _eyeSightDistance
```

현재 설정값이 10이라면 실제 감지 거리는 약 3.16이 된다. 그리고 `_player`가 할당되지 않았을 때의 null 검사도 필요하다. `_playerLayer`, `_backsideDegree`, `_backsideDistance`는 현재 사용되지 않는다.

### 5. 공격 코루틴과 발사 애니메이션이 매 프레임 누적된다

`EnemyFireNode.Evaluate()`는 `Running`을 반환하면서 매 프레임 다음을 실행한다.

- `EnemyAttack.AttackStart()` → 매 프레임 새 코루틴 시작
- `PlayEnemyShotAnim()` → 매 프레임 새 코루틴과 Trigger 실행

따라서 공격 범위에 머무르면 코루틴이 대량으로 겹치고 총알도 프레임 단위로 생성될 수 있다. 공격 노드는 `Enter / Tick / Exit` 개념을 갖거나, 최소한 `EnemyAttack` 내부에 `_isAttacking`과 쿨다운을 두어 중복 시작을 막아야 한다.

또한 `StopCoroutine(EnemyFireRoutine())`는 새 IEnumerator를 만들어 넘기므로 이미 실행 중인 코루틴을 중단하지 못한다. 시작할 때 받은 `Coroutine` 참조를 저장해 중단해야 한다.

### 6. 행동 전환 시 이전 행동의 상태가 남는다

예를 들어 추적이 공격으로 전환될 때 `_nav.isStopped = true`가 되고, 다시 소리 추적이나 순찰로 내려갔을 때 해당 노드가 이를 `false`로 되돌리지 않는다. 속도, `isStopped`, `updateRotation`, 경로, 애니메이션도 여러 노드가 동시에 소유하고 있다.

각 이동 노드가 시작할 때 최소한 아래 상태를 명시적으로 설정하는 것이 안전하다.

```csharp
_nav.isStopped = false;
_nav.speed = wantedSpeed;
```

장기적으로는 노드에 `OnEnter`, `Evaluate/Tick`, `OnExit`를 두면 노드 변경 시 조준 해제, 공격 중단, 경로 정리 같은 작업을 한곳에서 처리할 수 있다.

### 7. `ChaseToPlayerNode`의 도착 판정이 너무 이르다

`SetDestination()` 직후에는 새 경로가 아직 계산되지 않아 `remainingDistance`가 0 또는 이전 경로 값일 수 있다. `pathPending`을 먼저 확인하고, `hasPath` 및 유효 경로 여부도 고려하는 편이 좋다.

또한 공격 가능 여부는 단순 거리뿐 아니라 현재 시야 확보 여부까지 요구할지 결정해야 한다. 그렇지 않으면 벽을 사이에 두고 사격할 수 있다.

### 8. 죽음 처리가 공격보다 늦다

`EnemyFireNode.cs:24-27`은 먼저 공격과 애니메이션을 실행한 뒤 죽었는지 검사한다. 죽은 적도 한 번 이상 발사할 수 있으므로 죽음과 필수 컴포넌트 검사를 맨 앞에 두는 것이 좋다. 가능하면 루트 최우선에 `IsDead → Death` 분기를 두어 모든 이동과 공격을 중단시키는 편이 명확하다.

### 9. 런타임 코드의 불필요한 의존성과 중복 노드 정리

- `BehaviorSelector.cs`의 `NUnit.Framework`, `System.Data`, `UnityEngine` using은 사용하지 않는다. 특히 런타임 스크립트에서 NUnit 의존은 빌드 문제를 만들 수 있어 제거하는 것이 좋다.
- `EnemyAttack.cs`에도 NUnit 및 Visual Scripting 등 불필요한 using이 있다.
- `ChasePlayerNode`와 `ChaseToPlayerNode`, `EnemyAttackNode`와 현재 공격 시퀀스가 역할상 겹친다. 실제로 사용할 구현 하나만 남겨야 수정 지점이 분산되지 않는다.
- 매 프레임 실패하는 조건에서 `Debug.Log`를 호출하면 콘솔이 과도하게 쌓인다. 구성 오류는 초기화 시 한 번 검증하고, 정상적인 조건 실패는 로그를 남기지 않는 편이 좋다.
- 깨져 보이는 한글 주석은 파일 인코딩을 UTF-8로 통일해 복구하는 것이 좋다.

## 바라보기는 어디에서 처리하는 것이 좋은가?

말씀하신 판단에 동의한다. `EnemyFindArea`는 **감지 센서** 역할만 맡고, 실제 회전은 행동을 결정하는 BT 쪽에서 수행하는 편이 덜 꼬인다.

현재 `EnemyFindArea.DetectPlayer()` 안에서 회전하면 다음 문제가 생긴다.

- 회전 자체가 다음 프레임의 시야각 판정에 영향을 주어 센서와 행동이 서로 피드백을 만든다.
- 순찰 이동 중 NavMeshAgent 자동 회전과 `LookAtPlayer()` 수동 회전이 충돌할 수 있다.
- 소리를 조사하는 중인지, 추적 중인지, 공격 조준 중인지와 상관없이 센서가 회전을 결정한다.
- 현재 코드는 감지에 성공한 그 프레임에만 `LookAtPlayer()`를 부르고 다음부터 조기 반환하므로 지속 회전도 되지 않는다.

다만 회전 코드를 `EnemyBT.Update()`에 직접 넣기보다는 `FaceTargetNode` 또는 `AimAtTargetNode`라는 **별도 BT 액션 노드**로 두는 것을 추천한다. 그러면 회전이 필요한 분기에서만 명시적으로 사용할 수 있다.

추천 구조는 다음과 같다.

```text
Root Selector
├─ Dead Sequence
│  ├─ IsDead
│  └─ Death
├─ Combat Sequence
│  ├─ HasCombatTarget
│  └─ Combat Selector
│     ├─ Attack Sequence
│     │  ├─ IsInAttackRange
│     │  ├─ HasLineOfSight
│     │  ├─ StopMoving
│     │  ├─ FaceTarget
│     │  └─ FireWithCooldown
│     └─ ChaseTarget
├─ InvestigateSound Sequence
│  ├─ HasSoundTarget
│  └─ MoveToSoundPosition
└─ Patrol
```

공격 중에는 `_nav.updateRotation = false`로 두고 `FaceTargetNode`가 수동 회전을 담당하며, 추적/순찰로 돌아갈 때 다시 `true`로 복원하는 식이 자연스럽다. 회전 완료가 사격 조건이라면 각도 오차가 허용치 안일 때 `Success`, 아직 도는 중이면 `Running`을 반환하면 된다.

주의할 점은 `Sequence`가 현재 매 프레임 첫 자식부터 다시 평가된다는 것이다. 반응형 BT로 사용할 수 있는 구현이지만 `FaceTarget` 다음의 발사 노드는 회전이 완료된 프레임부터 실행된다. 이동하면서 동시에 바라보게 만들고 싶다면 순차 `Sequence`만으로는 부족하며, `ChaseTargetNode` 안에서 이동 방향 회전을 NavMeshAgent에 맡기거나 별도의 `Parallel` 노드를 추가해야 한다.

## 추천 수정 순서

1. `Vector3 == null`과 `ChaseToPlayerNode`의 `_nav` 조건 등 컴파일/즉시 실패 문제 수정
2. `EnemyFindArea`를 현재 시야와 마지막 기억으로 분리하고 거리 제곱 비교 수정
3. `EnemyAttack`의 코루틴 중복 실행과 쿨다운 처리
4. 노드 전환 때 `isStopped`, 속도, 회전, 애니메이션 상태 복원
5. `FaceTargetNode`를 만들어 공격 시퀀스에 배치
6. 죽음 분기 및 시야선(Line of Sight) 공격 조건 추가
7. 중복 노드와 불필요한 using, 과도한 로그 정리

## 총평

처음 만든 BT의 뼈대로는 방향이 괜찮다. 특히 우선순위를 트리 조립부에 모은 점은 이후 확장하기 좋다. 지금 가장 큰 문제는 “트리 모양”보다 **타깃 상태의 의미와 Running 노드의 생명주기**다. 이 둘을 먼저 정리하면 바라보기, 재장전, 엄폐, 수색 같은 행동도 훨씬 안정적으로 추가할 수 있다.

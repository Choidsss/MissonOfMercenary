# Enemy NavMesh 순찰 구현 순서

현재 목표는 Enemy의 이동을 `NavMeshAgent`로 통일하고, 아래 우선순위로 행동하게 만드는 것이다.

```text
플레이어를 직접 발견함
    → Player Chase
아니지만 총소리 조사 위치가 있음
    → Sound Investigate
둘 다 아님
    → Patrol
```

## 먼저 내릴 결론

- 순찰 기능을 만드는 방향은 맞다.
- 다만 `PatrolNode`만 먼저 실행하면 안 된다.
- 현재 `EnemyChase.Update()`가 `transform.position = Vector3.Lerp(...)`로 직접 이동시키므로, `NavMeshAgent`와 동시에 실행되면 서로 충돌할 수 있다.
- 따라서 **EnemyBT가 어떤 행동을 할지 결정하고, 실제 이동은 모두 NavMeshAgent가 담당하도록 통일한 다음 Patrol을 연결**하는 순서가 안전하다.
- `_targetPos == Vector3.zero`는 총소리 조사 종료를 표현하는 임시 방법으로는 쓸 수 있지만 최종 상태 판단으로는 권장하지 않는다. 월드 원점 `(0, 0, 0)`도 실제 목적지가 될 수 있기 때문이다.
- 최종적으로는 `HasSoundTarget` 또는 현재의 `DoFind` 같은 bool을 조사 위치의 유무를 나타내는 기준으로 사용한다.

---

## 1단계: 이동 책임을 하나로 통일

현재 `EnemyChase`의 다음 이동 코드를 제거하거나 비활성화할 준비를 한다.

```csharp
transform.position = Vector3.Lerp(...);
```

`MoveToOriginPosition()`의 직접 위치 변경도 Patrol 구현 후에는 사용하지 않는다.

이후 Enemy 이동은 모두 다음 방식으로 처리한다.

```csharp
_agent.SetDestination(targetPosition);
```

역할은 다음처럼 나누는 것이 좋다.

- `EnemyFindArea`: 플레이어가 시야에 있고 벽에 막히지 않았는지 감지
- `SoundPositionGiveToEnemy`: 총을 쏜 순간 조사할 위치를 Enemy에게 전달
- `EnemyChase`: 총소리 조사 위치와 조사 중 여부를 보관
- `EnemyBT`: Chase / Sound Investigate / Patrol 중 무엇을 실행할지 결정
- 각 행동 노드: NavMeshAgent에 실제 목적지를 전달

## 2단계: 총소리 목표 상태를 명확하게 만들기

`EnemyChase`에 아래 의미의 API가 필요하다.

```text
HasSoundTarget
SoundTargetPosition
SetSoundTarget(position)
ClearSoundTarget()
```

현재 필드를 최대한 유지한다면 다음처럼 대응시킬 수 있다.

```text
DoFind == true  → 조사할 총소리 위치가 있음
DoFind == false → 조사할 총소리 위치가 없음
```

중요한 점:

- 행동 전환 조건은 `_targetPos == Vector3.zero`보다 `DoFind` 또는 `HasSoundTarget`을 사용한다.
- 조사 위치에 도착해 주변 확인까지 끝난 순간 `ClearSoundTarget()`을 호출한다.
- `ClearSoundTarget()` 내부에서 `_targetPos = Vector3.zero`와 `DoFind = false`를 함께 처리하면 값이 어긋나는 것을 방지할 수 있다.
- 총소리가 다시 발생했을 때 위치를 갱신할 정책도 이 단계에서 결정한다.

현재 의도대로라면 추천 정책은 다음과 같다.

```text
순찰 중 총소리 발생      → 즉시 새 위치 저장
총소리 조사 중 재발생    → 필요하면 최신 위치로 갱신
플레이어 직접 추격 중    → 시야 추격을 우선하고 총소리는 무시하거나 보조 기억으로만 저장
```

`SoundPositionGiveToEnemy`의 foreach 안에서 한 Enemy의 중복 Collider를 만났다고 `return`하면 나머지 Enemy에게 전달하지 못할 수 있다. 이후 `HashSet<EnemyChase>`로 중복을 제거하고 각 Enemy를 한 번씩 처리하는 작업도 필요하다.

## 3단계: EnemyBT의 최종 뼈대부터 구성

`EnemyBT`에 Patrol만 단독으로 추가하기보다 전체 우선순위를 먼저 잡는다.

```text
Root Selector
├─ Visual Chase Sequence
│  ├─ CanSeePlayerNode
│  └─ ChasePlayerNode
├─ Sound Investigate Sequence
│  ├─ HasSoundTargetNode
│  └─ InvestigateSoundNode
└─ PatrolNode
```

Selector는 위에서부터 평가하므로 자식 순서가 중요하다.

```text
Visual Chase → Sound Investigate → Patrol
```

Patrol을 가장 앞에 두면 `PatrolNode`가 계속 `Running`을 반환하여 Chase와 Sound Investigate가 평가되지 않을 수 있다.

현재 `EnemyBT`에서는 `ChasePlayerNode` 연결이 주석 처리되어 있으므로 이를 다시 활성화해야 한다.

## 4단계: Patrol 방식 결정

첫 구현은 고정 Waypoint 방식이 확인하기 쉽다.

Enemy 또는 별도 PatrolRoute 오브젝트에 다음을 둔다.

```csharp
[SerializeField] Transform[] _patrolPoints;
```

`PatrolNode`의 기본 동작:

1. 현재 순찰 지점으로 `SetDestination()` 호출
2. `pathPending`이면 `Running`
3. 도착하지 않았으면 `Running`
4. 도착하면 잠시 대기하거나 다음 인덱스로 변경
5. 다음 지점으로 이동
6. 순찰 행동 자체는 계속 유지되므로 일반적으로 `Running` 반환

도착 판정 시에는 아래 조건을 같이 확인한다.

```text
!agent.pathPending
agent.remainingDistance <= agent.stoppingDistance
```

필요하면 `agent.hasPath`와 `agent.velocity.sqrMagnitude`도 보조 조건으로 사용한다.

## 5단계: Sound Investigate 노드 구현

`HasSoundTargetNode`:

- `EnemyChase.DoFind` 또는 `HasSoundTarget`이 true면 `Success`
- 아니면 `Failure`

`InvestigateSoundNode`:

1. SoundTargetPosition을 NavMesh 위의 유효한 위치로 보정
2. `_agent.SetDestination(soundTargetPosition)` 호출
3. 이동 중에는 `Running`
4. 도착하면 주변 탐색 또는 대기
5. 플레이어를 새로 발견했다면 다음 프레임에 Visual Chase가 우선 실행됨
6. 찾지 못하고 조사가 끝났다면 `ClearSoundTarget()` 호출
7. `Success` 또는 `Failure`로 종료
8. 다음 BT 평가부터 자동으로 Patrol 실행

즉, `_targetPos`가 비워졌을 때 Patrol로 직접 명령을 보내는 것이 아니라, **Sound 조건이 Failure가 되어 Selector가 자연스럽게 Patrol을 선택하도록 만드는 것**이 좋다.

## 6단계: ChasePlayerNode 연결 및 전환 확인

현재 `ChasePlayerNode`는 `EnemyFindArea.DetectedTarget.position`을 목적지로 사용한다.

먼저 아래 흐름을 확인한다.

```text
Patrol 중 플레이어 발견
→ CanSeePlayerNode Success
→ ChasePlayerNode Running
→ 시야에서 놓침
→ CanSeePlayerNode Failure
→ SoundTarget이 있으면 조사
→ 없으면 Patrol
```

플레이어를 놓친 마지막 위치까지 가게 만들고 싶다면, 이후 별도의 `LastKnownPlayerPosition`을 추가한다. 최초 Patrol 테스트에서는 넣지 않아도 된다.

## 7단계: EnemyFindArea와 회전 책임 정리

현재 `EnemyFindArea.DetectPlayer()`는 감지 후보를 검사하면서 `LookAtPlayer()`를 호출한다.

NavMeshAgent가 이동 회전까지 담당할 경우 다음 충돌을 확인해야 한다.

- `NavMeshAgent.updateRotation == true`
- `EnemyFindArea.LookAtPlayer()`가 transform.rotation을 직접 변경

최초 테스트에서는 다음 중 하나로 통일한다.

- 이동 중 방향은 NavMeshAgent에 맡기고, 정지/공격할 때만 직접 회전
- 또는 `updateRotation = false`로 두고 별도 회전 코드가 항상 담당

두 방식이 동시에 회전을 변경하지 않도록 한다.

## 8단계: Inspector 연결

Enemy 프리팹에서 확인할 항목:

- `EnemyBT._findArea`
- `EnemyBT._agent`
- `EnemyChase` 참조가 BT 또는 Sound 노드에 전달되는지
- Patrol Point 배열
- NavMeshAgent가 시작 시 NavMesh 위에 있는지
- Agent Type이 NavMeshSurface와 같은지
- Agent Radius / Height / Speed / Stopping Distance

## 9단계: 최소 동작 테스트 순서

한 번에 전부 테스트하지 말고 아래 순서로 확인한다.

1. 플레이어와 총소리 없이 Enemy가 Waypoint를 순서대로 순찰하는지
2. 순찰 중 총소리를 내면 조사 위치로 우선 이동하는지
3. 조사 완료 후 `HasSoundTarget == false`가 되고 다시 순찰하는지
4. 순찰 중 플레이어를 발견하면 즉시 추격하는지
5. 총소리 조사 중 플레이어를 발견하면 추격이 우선되는지
6. 벽 뒤 플레이어는 Visual Chase를 시작하지 않는지
7. 여러 Enemy가 있을 때 총소리 위치가 각각 한 번씩 전달되는지
8. 목적지가 NavMesh 밖일 때 오류 없이 가까운 NavMesh 위치로 보정되는지

## 실제 구현 권장 순서 요약

```text
1. EnemyChase의 직접 transform 이동 중단
2. SoundTarget 상태 API 정리
3. EnemyBT에 EnemyChase 참조 추가
4. Selector 우선순위를 Visual Chase → Sound Investigate → Patrol로 구성
5. PatrolNode 구현 및 단독 테스트
6. HasSoundTargetNode / InvestigateSoundNode 구현
7. ChasePlayerNode 활성화
8. 총소리 중복 전달과 갱신 정책 수정
9. 회전 제어 충돌 정리
10. 행동 전환 통합 테스트
```

핵심은 `_targetPos == Vector3.zero`일 때 Patrol 코드를 직접 호출하는 것이 아니다. 총소리 목표가 없어지면 Sound Investigate 조건이 실패하고, EnemyBT의 Selector가 다음 후보인 Patrol을 선택하게 만들어야 한다.

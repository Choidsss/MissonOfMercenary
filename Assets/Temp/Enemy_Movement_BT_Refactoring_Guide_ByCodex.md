# Enemy 이동 / 감지 / BT 리팩토링 가이드

이 문서는 현재 코드를 한 번에 전부 갈아엎지 않고, 컴파일과 동작을 중간마다 확인하면서 정리하기 위한 작업 지침이다.

대상 파일:

- `Assets/Scripts/Enemy/EnemyChase.cs`
- `Assets/Scripts/Enemy/Vision/EnemyFindArea.cs`
- `Assets/Scripts/Player/Player/ToEnemy/SoundPositionGiveToEnemy.cs`
- `Assets/Scripts/Enemy/BehaviorTreeScript/EnemyBT.cs`
- `Assets/Scripts/Enemy/BehaviorTreeScript/ChasePlayerNode.cs`
- 이후 추가할 `HasSoundTargetNode`, `InvestigateSoundNode`, `PatrolNode`

---

## 1. 현재 코드에서 가장 먼저 정리해야 하는 문제

### 문제 A: 이동시키는 코드가 여러 곳으로 분산될 예정

현재 `EnemyChase`는 아래처럼 Transform을 직접 이동시킨다.

```csharp
transform.position = Vector3.Lerp(
    transform.position,
    TargetPosition,
    _amount * Time.deltaTime);
```

`MoveToOriginPosition()`도 Transform을 직접 이동시킨다.

반면 `ChasePlayerNode`는 다음처럼 NavMeshAgent를 사용한다.

```csharp
_agent.SetDestination(_findArea.DetectedTarget.position);
```

여기에 Patrol까지 NavMeshAgent로 만들면 Transform 직접 이동과 NavMeshAgent 이동이 동시에 실행될 수 있다.

### 정리 방향

- 위치 이동은 BT 행동 노드와 NavMeshAgent만 담당한다.
- `EnemyChase`는 이동하지 않고 총소리 조사 상태만 보관한다.
- `EnemyBT`는 어떤 행동을 실행할지만 결정한다.
- `EnemyFindArea`는 플레이어 감지만 담당한다.

---

## 2. 파일별 최종 책임

```text
SoundPositionGiveToEnemy
    총을 쏜 순간 범위 안 Enemy에게 소리 위치 전달

EnemyChase
    소리 목표 위치와 소리 조사 여부 저장
    이동 코드 없음

EnemyFindArea
    플레이어가 시야각/거리 안에 있는지 검사
    벽에 가려졌는지 검사
    감지된 Player Transform 제공

EnemyBT
    행동 트리를 생성하고 매 프레임 Evaluate
    필요한 컴포넌트를 각 노드에 전달

ChasePlayerNode
    보이는 플레이어 위치로 NavMeshAgent 이동

InvestigateSoundNode
    저장된 소리 위치로 NavMeshAgent 이동
    조사 종료 시 Sound Target 제거

PatrolNode
    지정된 순찰 지점 사이를 NavMeshAgent로 이동
```

---

## 3. EnemyChase.cs 리팩토링

### 3-1. EnemyChase에 남길 것

총소리 조사에 필요한 상태만 남긴다.

```csharp
Vector3 _soundTargetPosition;
bool _hasSoundTarget;

public bool HasSoundTarget => _hasSoundTarget;
public Vector3 SoundTargetPosition => _soundTargetPosition;
```

외부에서 상태를 변경하는 메서드를 만든다.

```csharp
public void SetSoundTarget(Vector3 position)
{
    _soundTargetPosition = position;
    _hasSoundTarget = true;
}

public void ClearSoundTarget()
{
    _soundTargetPosition = Vector3.zero;
    _hasSoundTarget = false;
}
```

이렇게 하면 다른 스크립트가 `_targetPos`와 `DoFind`를 따로 변경하여 상태가 어긋나는 일을 막을 수 있다.

### 3-2. EnemyChase에서 제거할 것

아래 필드와 메서드는 BT/NavMeshAgent 전환 후 제거한다.

```text
_amount
_arriveDistance
_moveSpeed
_findArea
_enemyOriginPosition
_radius
_playerLayer
_findPlayer
Start()
Update()
EnemyMoveToSoundPosition()
GetPlayerObject()
MoveToOriginPosition()
```

제거 이유:

- `_amount`, `_moveSpeed`: Transform Lerp 이동용
- `_findArea`, `_radius`, `_playerLayer`, `_findPlayer`: 감지 책임이 EnemyFindArea와 중복됨
- `_enemyOriginPosition`: Waypoint Patrol로 대체됨
- `Update()`: 행동 실행은 EnemyBT가 담당함

### 3-3. 기존 API를 바로 삭제하지 않는 안전한 중간 단계

먼저 새 API를 추가하고 기존 호출부를 교체한다.

```csharp
public bool HasSoundTarget => _hasSoundTarget;
public Vector3 SoundTargetPosition => _soundTargetPosition;
public void SetSoundTarget(Vector3 position) { ... }
public void ClearSoundTarget() { ... }
```

그 다음 `SoundPositionGiveToEnemy`와 새 BT 노드가 새 API만 사용하는 것을 확인한 뒤 기존 코드를 제거한다.

---

## 4. SoundPositionGiveToEnemy.cs 리팩토링

### 4-1. 현재 foreach의 문제

현재 코드는 Collider 하나에서 `EnemyChase`를 찾지 못하거나 조사 중인 Enemy를 만나면 `return`한다.

```csharp
if (ec == null) { return; }

if (ec.DoFind)
{
    return;
}
```

`return`은 현재 Collider만 넘기는 것이 아니라 `PositionGiveToEnemy()` 전체를 끝낸다. 따라서 뒤에 있는 다른 Enemy에게 총소리가 전달되지 않을 수 있다.

### 4-2. 최소 수정

해당 Collider만 무시하려면 `continue`를 사용한다.

```csharp
if (ec == null)
{
    continue;
}
```

### 4-3. Ragdoll Collider 중복 제거

한 Enemy에 여러 Collider가 있으므로 `HashSet<EnemyChase>`로 한 번만 처리한다.

권장 흐름:

```csharp
HashSet<EnemyChase> enemies = new HashSet<EnemyChase>();

foreach (Collider col in cols)
{
    EnemyChase enemy = col.GetComponentInParent<EnemyChase>();

    if (enemy != null)
    {
        enemies.Add(enemy);
    }
}

foreach (EnemyChase enemy in enemies)
{
    enemy.SetSoundTarget(transform.position);
}
```

파일 위쪽에 다음 using이 필요하다.

```csharp
using System.Collections.Generic;
```

### 4-4. 총소리 위치 갱신 정책

현재는 `DoFind == true`면 새 총소리를 무시한다.

첫 구현에서는 더 단순하게 최신 총소리 위치로 갱신하는 것을 권장한다.

```csharp
enemy.SetSoundTarget(transform.position);
```

이 방식이면:

- 순찰 중 총소리: 조사 시작
- 조사 중 다시 총소리: 최신 위치로 목적지 갱신
- 직접 추격 중 총소리: 위치는 저장되지만 BT 우선순위상 플레이어 추격이 계속 우선

직접 추격 중 총소리를 아예 저장하지 않을지는 기능이 안정된 뒤 정책으로 추가한다.

### 4-5. 제거 가능한 값

`_didGive`는 현재 외부에서 사용되지 않고 행동 결정에도 쓰이지 않는다. 디버깅 목적이 아니라면 제거한다.

`_ar`도 실제 검사 코드가 주석 처리되어 있으므로 총 발사 가능 여부를 검사하지 않을 거라면 제거한다.

---

## 5. EnemyFindArea.cs 리팩토링

### 5-1. EnemyFindArea에 남길 것

```text
시야 거리
시야각
Player Layer
Obstacle Layer
눈/목표 높이 Offset
DetectedTarget
IsDetectedPlayer
OverlapSphere 검사
Raycast 벽 검사
```

### 5-2. 감지와 회전을 분리

현재 `DetectPlayer()`는 Collider 후보를 검사할 때마다 `LookAtPlayer()`를 호출한다.

이러면 시야에 들어왔는지 판정하기 전에 Enemy가 Player 쪽으로 회전하면서 결과적으로 시야가 자동으로 따라갈 수 있다.

권장 순서:

```text
1. Player 후보 위치 계산
2. 현재 Enemy forward 기준으로 시야각 검사
3. Raycast로 벽 검사
4. 둘 다 통과하면 DetectedTarget 저장
5. 회전은 ChasePlayerNode 또는 별도 회전 코드에서 처리
```

따라서 `DetectPlayer()` 내부의 선행 `LookAtPlayer()` 호출은 제거 대상으로 둔다.

### 5-3. LookAtPlayer 공개 메서드 정리

`EnemyChase`에서 더 이상 호출하지 않으면 `public`일 필요가 없다.

선택지는 두 가지다.

```text
A. NavMeshAgent.updateRotation을 사용
   → LookAtPlayer 제거

B. 별도 회전을 직접 구현
   → LookAtPlayer를 행동 노드에서만 호출
   → NavMeshAgent.updateRotation = false
```

첫 구현에서는 A 방식이 단순하다.

### 5-4. 매 프레임 Debug.Log 제거

현재 Raycast 결과를 매 프레임 출력한다.

```csharp
Debug.Log($"{hitWall}");
```

Enemy 수가 늘어나면 Console이 과도하게 쌓이므로 감지 확인이 끝나면 제거한다.

### 5-5. 사용하지 않는 필드 정리

현재 아래 필드는 실제 감지 로직에서 사용되지 않는다.

```text
_backsideDegree
_backsideDistance
_isSense
IsSensePlayer
System.Linq using
```

후방 감지를 바로 구현하지 않을 거라면 제거한다. 나중에 기능을 만들 때 다시 추가하는 편이 현재 상태를 이해하기 쉽다.

### 5-6. 이름 오타 정리

```text
IsBlockedByObtacles
→ IsBlockedByObstacles
```

기능에는 영향이 없지만 이후 호출과 검색이 쉬워진다.

---

## 6. EnemyBT.cs 리팩토링

### 6-1. EnemyChase 참조 추가

Sound Target을 노드에 넘기기 위해 참조를 추가한다.

```csharp
[SerializeField] EnemyChase _enemyChase;
```

`Awake()`에서 자동 연결한다.

```csharp
if (_enemyChase == null)
{
    _enemyChase = GetComponent<EnemyChase>();
}
```

### 6-2. Patrol Point 참조 추가

가장 단순한 형태:

```csharp
[SerializeField] Transform[] _patrolPoints;
```

나중에 여러 Enemy가 같은 경로를 공유해야 하면 `PatrolRoute` 컴포넌트로 분리한다. 최초 구현에서는 배열로 시작해도 충분하다.

### 6-3. SetupTree 구조 변경

최종 형태:

```csharp
BTNode SetupTree()
{
    BehaviorSelector selector = new BehaviorSelector();

    Sequence visualChaseSequence = new Sequence();
    visualChaseSequence.AddChild(new CanSeePlayerNode(_findArea));
    visualChaseSequence.AddChild(new ChasePlayerNode(_agent, _findArea));

    Sequence soundInvestigateSequence = new Sequence();
    soundInvestigateSequence.AddChild(new HasSoundTargetNode(_enemyChase));
    soundInvestigateSequence.AddChild(new InvestigateSoundNode(_agent, _enemyChase));

    selector.AddChild(visualChaseSequence);
    selector.AddChild(soundInvestigateSequence);
    selector.AddChild(new PatrolNode(_agent, _patrolPoints));

    return selector;
}
```

순서는 반드시 다음과 같이 둔다.

```text
Visual Chase
Sound Investigate
Patrol
```

### 6-4. 필수 참조 방어

`Awake()` 이후에도 필수 컴포넌트가 없다면 명확한 오류를 한 번 출력하고 BT 실행을 중단하는 편이 좋다.

```text
NavMeshAgent 없음
EnemyFindArea 없음
EnemyChase 없음
Patrol Point 없음
```

Patrol Point가 없을 때는 제자리 대기 Patrol로 처리하거나 `PatrolNode`가 Failure를 반환하도록 정책을 정한다.

---

## 7. ChasePlayerNode.cs 리팩토링

### 현재 유지 가능한 부분

아래 검사는 그대로 유지할 수 있다.

```csharp
_agent == null
!_agent.enabled
!_agent.isOnNavMesh
_findArea == null
_findArea.DetectedTarget == null
```

### 목적지 갱신

플레이어 추격 중에는 Player 위치가 움직이므로 `Evaluate()`마다 `SetDestination()`을 호출하는 현재 방식이 맞다.

### 도착 결과 정책

현재는 도착하면 `Success`를 반환한다.

```csharp
return _agent.remainingDistance <= _agent.stoppingDistance
    ? State.Success
    : State.Running;
```

추격 노드는 Player가 계속 보이는 동안 행동을 유지해야 하므로, 공격 노드가 아직 없다면 도착해도 `Running`을 유지하는 방식이 더 자연스러울 수 있다.

이후 Attack Sequence를 추가할 때는 다음처럼 분리한다.

```text
공격 거리 안 → Attack Sequence
보이지만 공격 거리 밖 → Chase Sequence
```

최초 순찰 테스트에서는 현재 반환값을 유지해도 되지만, 도착 후 Selector가 같은 프레임/다음 프레임에 다시 평가된다는 점을 확인한다.

---

## 8. 새로 추가할 BT 노드

### HasSoundTargetNode

책임은 조건 확인 하나뿐이다.

```text
EnemyChase가 없으면 Failure
HasSoundTarget이 true면 Success
아니면 Failure
```

### InvestigateSoundNode

책임:

```text
SoundTargetPosition을 목적지로 설정
NavMesh 경로를 따라 이동
도착 후 잠시 조사
조사가 끝나면 ClearSoundTarget 호출
```

중요:

- `transform.position`을 직접 변경하지 않는다.
- 목적지가 NavMesh 밖일 수 있으므로 `NavMesh.SamplePosition()` 보정을 고려한다.
- 조사가 끝나기 전에는 `Running`을 반환한다.
- `ClearSoundTarget()` 후 `Success`를 반환하면 다음 평가부터 Patrol로 넘어간다.

### PatrolNode

책임:

```text
현재 Waypoint 인덱스 보관
현재 Waypoint로 SetDestination
도착하면 다음 인덱스로 변경
계속 Running 반환
```

PatrolNode는 가장 낮은 우선순위에 있어야 한다.

---

## 9. BehaviorSelector / Sequence에서 알아둘 점

현재 구현은 매 프레임 첫 번째 자식부터 다시 평가한다.

이 특성 덕분에:

```text
Patrol 중 Player 감지
→ 다음 Evaluate에서 첫 번째 Visual Chase가 선택됨

Sound 조사 중 Player 감지
→ 다음 Evaluate에서 Visual Chase가 Sound보다 먼저 선택됨

Sound Target 제거
→ Sound Sequence가 Failure
→ Patrol 선택
```

따라서 `_targetPos == Vector3.zero`를 감지해 Patrol을 직접 호출할 필요가 없다.

단, 행동이 바뀔 때 이전 NavMesh 경로가 잠깐 남을 수 있으므로 각 새 행동이 선택되면 자신의 목적지를 즉시 `SetDestination()`해야 한다.

---

## 10. 컴파일을 유지하는 실제 리팩토링 순서

### 작업 1

`EnemyChase`에 새 API만 먼저 추가한다.

```text
HasSoundTarget
SoundTargetPosition
SetSoundTarget()
ClearSoundTarget()
```

기존 코드는 아직 삭제하지 않는다.

### 작업 2

`SoundPositionGiveToEnemy`를 새 API로 교체한다.

```text
TargetPosition 프로퍼티 사용 중단
DoFind 검사 사용 중단
SetSoundTarget() 호출
HashSet으로 중복 Enemy 제거
foreach 내부 return 제거
```

여기까지 컴파일하고 총소리 전달 여부를 확인한다.

### 작업 3

`EnemyBT`에 `_enemyChase`, `_patrolPoints` 참조를 추가한다.

아직 새 노드를 SetupTree에 연결하지 않아도 된다.

### 작업 4

`PatrolNode`를 만들고 Selector의 마지막에 연결한다.

이 시점에는 `EnemyChase.Update()`를 비활성화하여 Transform 이동과 NavMeshAgent가 충돌하지 않게 한다.

플레이어와 총소리 없이 순찰만 테스트한다.

### 작업 5

`HasSoundTargetNode`, `InvestigateSoundNode`를 만들고 Patrol보다 앞에 연결한다.

총소리 위치 이동과 조사 후 Patrol 복귀를 테스트한다.

### 작업 6

`ChasePlayerNode` 주석을 해제하고 가장 높은 우선순위에 둔다.

감지 → 추격 → 놓침 → Sound 또는 Patrol 전환을 테스트한다.

### 작업 7

모든 이동이 BT/NavMeshAgent로 동작하는 것을 확인한 후 `EnemyChase`의 이전 이동/감지 코드를 삭제한다.

### 작업 8

`EnemyFindArea`에서 불필요한 회전, Debug.Log, 미사용 필드를 정리한다.

---

## 11. 리팩토링 완료 기준

다음 조건을 모두 만족하면 1차 리팩토링 완료다.

- Enemy 위치를 직접 변경하는 `transform.position = ...` 코드가 행동 스크립트에 없음
- 이동 목적지는 NavMeshAgent의 `SetDestination()`으로만 설정
- EnemyFindArea는 이동하지 않음
- EnemyChase는 Update 없이 Sound Target 상태만 보관
- SoundPositionGiveToEnemy는 한 Enemy에게 한 번만 위치 전달
- `_targetPos == Vector3.zero`를 행동 전환 조건으로 사용하지 않음
- EnemyBT의 우선순위가 Visual Chase → Sound Investigate → Patrol
- Sound Target이 제거되면 별도 명령 없이 Patrol로 복귀
- Patrol 중 Player를 발견하면 다음 BT 평가에서 Chase로 전환

---

## 12. 이번 단계에서 굳이 하지 않아도 되는 것

아래 기능은 1차 순찰/조사 전환이 안정된 후 추가한다.

- 공격 행동
- 마지막으로 본 Player 위치 기억
- 후방 감지
- 복잡한 수색 패턴
- Patrol Route 공용 컴포넌트화
- 애니메이션 상태 연동
- BT 노드 중단/진입/종료 콜백
- 완전한 상태 머신 또는 Blackboard 도입

먼저 `Patrol → Sound Investigate → Patrol`, `Patrol → Visual Chase` 두 흐름을 안정시키는 것이 우선이다.

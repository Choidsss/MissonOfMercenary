# Enemy 움직임 스크립트와 Behavior Tree 연결 가이드

이 문서는 현재 프로젝트에 있는 다음 스크립트가 각각 무슨 일을 하고, 서로 어떻게 연결되는지를 정리한 문서다.

- `BehaviorSelector.cs`
- `EnemyFindArea.cs`
- `CanSeePlayerNode.cs`
- `ChasePlayerNode.cs`
- `EnemyPatrolNode.cs`
- `EnemyChase.cs`
- `EnemyBT.cs`

현재 먼저 완성할 동작은 다음과 같다.

```text
플레이어가 보임
    → 플레이어 추격

플레이어가 보이지 않음
    → Waypoint 순찰
```

총소리 조사는 그다음 단계에서 다음과 같이 추가한다.

```text
플레이어가 보임
    → 플레이어 추격

플레이어는 안 보이지만 총소리 목표가 있음
    → 총소리 위치 조사

둘 다 아님
    → Waypoint 순찰
```

---

## 1. 전체 구조부터 이해하기

각 스크립트의 역할은 다음과 같이 나누는 것이 좋다.

```text
EnemyFindArea
    플레이어가 보이는지 감지
            ↓
CanSeePlayerNode
    감지 결과가 true인지 확인
            ↓
ChasePlayerNode
    보이는 플레이어 위치를 NavMeshAgent 목적지로 설정

EnemyPatrolNode
    플레이어가 보이지 않을 때 Waypoint를 왕복

EnemyBT
    위 행동들의 우선순위를 정하고 매 프레임 평가

EnemyChase
    현재는 총소리 목표 위치와 총소리 목표 존재 여부를 보관
```

중요한 원칙은 다음과 같다.

```text
EnemyFindArea는 감지만 한다.
BT 조건 노드는 상태만 확인한다.
BT 행동 노드는 NavMeshAgent에 목적지를 전달한다.
EnemyBT는 어떤 행동을 실행할지 결정한다.
```

---

## 2. MonoBehaviour 스크립트와 일반 BTNode의 차이

### MonoBehaviour인 클래스

현재 다음 클래스는 `MonoBehaviour`다.

```text
EnemyBT
EnemyFindArea
EnemyChase
```

이 클래스들은 Enemy GameObject에 컴포넌트로 붙일 수 있다.

따라서 다음 Unity 기능을 사용할 수 있다.

- Inspector의 `[SerializeField]`
- `GetComponent`
- `Awake`
- `Start`
- `Update`
- `transform`
- `gameObject`

### 일반 BTNode인 클래스

다음 클래스는 `MonoBehaviour`가 아니라 일반 C# 객체다.

```text
CanSeePlayerNode
ChasePlayerNode
EnemyPatrolNode
BehaviorSelector
Sequence
```

이 객체들은 GameObject에 컴포넌트로 붙이지 않는다.

따라서 Unity가 해당 노드의 `Start()`나 `Update()`를 자동으로 호출하지 않는다. 필요한 참조는 노드를 생성할 때 생성자로 전달한다.

예:

```csharp
new CanSeePlayerNode(_findArea);
new ChasePlayerNode(_agent, _findArea);
new EnemyPatrolNode(_agent, _wayPoint, _patrolSpeed);
```

일반 BTNode에서도 필드는 정상적으로 사용할 수 있다. 차이는 필드를 사용할 수 있느냐가 아니라, 그 필드에 값이 어떻게 들어오느냐다.

```text
EnemyBT의 필드
    Inspector에서 값 설정 가능

EnemyPatrolNode의 필드
    EnemyBT가 생성자를 통해 값 전달
```

---

## 3. BehaviorSelector와 Sequence의 역할

### BehaviorSelector

Selector는 자식들을 위에서부터 확인하고, `Failure`가 아닌 첫 번째 자식의 결과를 반환한다.

```text
첫 번째 자식이 Failure
    → 두 번째 자식 평가

첫 번째 자식이 Running
    → 첫 번째 자식 실행 유지

첫 번째 자식이 Success
    → 첫 번째 자식의 Success 반환
```

그래서 우선순위가 높은 행동을 먼저 추가해야 한다.

현재 필요한 순서:

```text
1. Chase Sequence
2. Patrol Node
```

Patrol은 정상 작동 중 계속 `Running`을 반환한다. Patrol을 먼저 추가하면 Selector가 Patrol에서 멈추기 때문에 Chase를 확인하지 못한다.

### Sequence

Sequence는 자식들을 순서대로 확인한다.

```text
자식이 Success
    → 다음 자식 평가

자식이 Failure
    → Sequence Failure

자식이 Running
    → Sequence Running
```

Chase Sequence는 다음처럼 구성한다.

```text
CanSeePlayerNode
    Success
        ↓
ChasePlayerNode
```

플레이어가 보이지 않으면 `CanSeePlayerNode`가 `Failure`를 반환한다. 그러면 Chase Sequence도 `Failure`가 되고, Selector는 다음 행동인 Patrol을 평가한다.

---

## 4. EnemyFindArea의 역할과 연결

`EnemyFindArea`는 Enemy GameObject에 붙는 감지 컴포넌트다.

현재 `Update()`에서 매 프레임 `DetectPlayer()`를 실행한다.

감지 흐름:

```text
OverlapSphere로 Player Layer 검색
    ↓
시야각 확인
    ↓
Raycast로 벽 확인
    ↓
감지 성공 시
IsDetectedPlayer = true
DetectedTarget = Player Transform
```

BT가 사용하는 공개 값은 다음 두 가지다.

```csharp
public bool IsDetectedPlayer => _isDetected;
public Transform DetectedTarget => _detectedTarget;
```

사용처:

```text
CanSeePlayerNode
    IsDetectedPlayer 사용

ChasePlayerNode
    DetectedTarget 사용
```

### 현재 EnemyFindArea에서 주의할 점

현재 `DetectPlayer()`는 시야각과 벽을 확인하기 전에 `LookAtPlayer()`를 호출한다.

따라서 Player가 감지 거리 안에만 들어오면 Enemy가 먼저 Player 쪽으로 회전하고, 회전한 결과 Player가 시야각 안으로 들어올 수 있다. 엄격한 전방 시야를 원한다면 다음 순서가 되어야 한다.

```text
현재 Enemy의 forward 기준으로 시야각 검사
    ↓
벽 검사
    ↓
둘 다 통과했을 때만 감지 성공
```

또한 `IsBlockedByObtacles()`의 다음 로그는 Enemy가 많아지면 매 프레임 Console을 채운다.

```csharp
Debug.Log($"{hitWall}");
```

감지 테스트가 끝나면 제거하는 것이 좋다.

---

## 5. CanSeePlayerNode의 역할과 연결

`CanSeePlayerNode`는 이동하지 않는다. `EnemyFindArea`의 감지 결과만 확인하는 조건 노드다.

생성할 때 `EnemyFindArea`를 받는다.

```csharp
new CanSeePlayerNode(_findArea)
```

반환 결과:

```text
_findArea가 null
    → Failure

IsDetectedPlayer가 true
    → Success

IsDetectedPlayer가 false
    → Failure
```

이 노드는 Chase Sequence의 첫 번째 자식이어야 한다.

현재 `EnemyBT`에는 `CanSeePlayerNode`가 두 번 추가되어 있다.

```csharp
chaseSeq.AddChild(new CanSeePlayerNode(_findArea));
chaseSeq.AddChild(new CanSeePlayerNode(_findArea));
```

두 번째 것은 중복이므로 제거하고 한 번만 추가한다.

---

## 6. ChasePlayerNode의 역할과 연결

`ChasePlayerNode`는 `EnemyFindArea.DetectedTarget` 위치를 NavMeshAgent 목적지로 설정한다.

생성할 때 다음 두 참조를 받는다.

```csharp
new ChasePlayerNode(_agent, _findArea)
```

실행 흐름:

```text
Agent와 FindArea가 유효한지 확인
    ↓
DetectedTarget이 있는지 확인
    ↓
Player 위치로 SetDestination
    ↓
이동 중이면 Running
    ↓
도착하면 Success
```

이 노드는 Chase Sequence의 두 번째 자식이어야 한다.

```text
Chase Sequence
├─ CanSeePlayerNode
└─ ChasePlayerNode
```

현재는 공격 노드가 없기 때문에 Player에게 도착하면 `Success`를 반환한다. 하지만 다음 프레임에 BT 전체가 다시 평가되므로 Player가 계속 보이는 동안 다시 Chase Sequence가 선택된다.

---

## 7. EnemyPatrolNode의 역할과 연결

`EnemyPatrolNode`는 다음 세 값을 생성자로 받는다.

```text
NavMeshAgent
Waypoint 배열
Patrol Speed
```

생성:

```csharp
EnemyPatrolNode patrolNode =
    new EnemyPatrolNode(_agent, _wayPoint, _patrolSpeed);
```

현재 왕복 인덱스의 의도:

```text
Waypoint가 3개일 때

0 → 1 → 2 → 1 → 0 → 1 ...
```

Patrol은 끝나는 행동이 아니므로 정상 순찰 중에는 계속 `Running`을 반환한다.

### 현재 PatrolNode에서 보완할 점

#### Agent가 NavMesh 위에 있는지 확인

`SetDestination()` 전에 다음 상태까지 확인해야 한다.

```csharp
if (_nav == null || !_nav.enabled || !_nav.isOnNavMesh)
{
    return State.Failure;
}
```

#### Patrol Speed 적용

현재 생성자로 받은 `_speed`가 실제로 사용되지 않는다.

Patrol 속도를 적용하려면 목적지를 설정하기 전에 다음 코드를 실행한다.

```csharp
_nav.speed = _speed;
```

나중에 Chase 속도를 별도로 사용할 경우 `ChasePlayerNode`도 실행될 때 Chase 속도를 설정해야 한다. 그렇지 않으면 Patrol에서 설정한 느린 속도가 Chase에서도 그대로 유지된다.

#### Waypoint가 하나일 때

현재 Waypoint가 하나면 그 위치로 이동한 후 계속 `Running`을 반환한다. 이것은 “지정된 한 지점으로 이동한 뒤 그곳에 머무름”이라는 정책으로 사용할 수 있다.

---

## 8. EnemyChase의 현재 역할

이름은 `EnemyChase`지만 현재 이 스크립트는 Enemy를 직접 이동시키지 않는다.

현재 책임은 총소리 목표 상태 보관이다.

```text
_hasSoundTarget
    조사할 총소리 위치가 있는가?

_sondTargetPosition
    조사할 총소리 위치

SetSoundTarget(position)
    새 총소리 위치 저장

ClearSoundTarget()
    총소리 목표 제거
```

현재 Chase / Patrol만 사용하는 1차 BT에는 `EnemyChase`를 연결하지 않아도 된다.

총소리 조사를 추가할 때 다음 두 노드가 `EnemyChase`를 사용하게 된다.

```text
HasSoundTargetNode
    EnemyChase.HasSoundTarget 확인

InvestigateSoundNode
    EnemyChase.SoundTargetPosition으로 이동
    조사 종료 후 EnemyChase.ClearSoundTarget 호출
```

향후 최종 구조:

```text
Root Selector
├─ Visual Chase Sequence
│  ├─ CanSeePlayerNode
│  └─ ChasePlayerNode
├─ Sound Investigate Sequence
│  ├─ HasSoundTargetNode
│  └─ InvestigateSoundNode
└─ EnemyPatrolNode
```

현재 `EnemyChase.Update()`는 `HasSoundTarget`을 `DoFind`로 다시 복사하고 있다. 두 bool은 같은 의미가 되므로 이후에는 `HasSoundTarget` 하나만 행동 조건으로 사용하는 편이 단순하다.

---

## 9. EnemyBT의 역할

`EnemyBT`는 전체 BT의 조립과 실행을 담당한다.

```text
Awake
    EnemyFindArea와 NavMeshAgent 참조 확보

Start
    SetupTree로 트리 한 번 생성

Update
    매 프레임 Root Evaluate 호출
```

Waypoint와 Patrol Speed는 Unity Inspector에서 설정해야 하므로 `EnemyBT`에 `[SerializeField]`로 둔다.

```csharp
[Header("Patrol Options")]
[SerializeField] float _patrolSpeed;
[SerializeField] Transform[] _wayPoint;
```

그리고 `SetupTree()`에서 이 값을 `EnemyPatrolNode`의 생성자로 전달한다.

현재의 다음 필드는 제거한다.

```csharp
[SerializeField] EnemyPatrolNode _patrol;
```

`EnemyPatrolNode`는 GameObject에 붙은 컴포넌트를 가져오는 것이 아니라 `SetupTree()`에서 생성할 일반 BTNode이기 때문이다.

---

## 10. 현재 단계의 EnemyBT 최종 결과

현재 Chase와 Patrol만 연결한 `EnemyBT`는 다음 형태가 되어야 한다.

```csharp
using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class EnemyBT : MonoBehaviour
    {
        [Header("Required Components")]
        [SerializeField] EnemyFindArea _findArea;
        [SerializeField] NavMeshAgent _agent;

        [Header("Patrol Options")]
        [SerializeField] float _patrolSpeed;
        [SerializeField] Transform[] _wayPoint;

        BTNode _root;

        void Awake()
        {
            if (_findArea == null)
            {
                _findArea = GetComponent<EnemyFindArea>();
            }

            if (_agent == null)
            {
                _agent = GetComponent<NavMeshAgent>();
            }
        }

        void Start()
        {
            _root = SetupTree();
        }

        void Update()
        {
            _root?.Evaluate();
        }

        BTNode SetupTree()
        {
            BehaviorSelector selector = new BehaviorSelector();

            Sequence chaseSequence = new Sequence();
            chaseSequence.AddChild(new CanSeePlayerNode(_findArea));
            chaseSequence.AddChild(new ChasePlayerNode(_agent, _findArea));

            EnemyPatrolNode patrolNode =
                new EnemyPatrolNode(_agent, _wayPoint, _patrolSpeed);

            selector.AddChild(chaseSequence);
            selector.AddChild(patrolNode);

            return selector;
        }
    }
}
```

자식을 만드는 순서와 Selector에 추가하는 순서를 분리해서 보면 다음과 같다.

```text
1. Selector 생성
2. Chase Sequence 생성
3. CanSeePlayerNode를 Chase Sequence에 추가
4. ChasePlayerNode를 Chase Sequence에 추가
5. PatrolNode 생성
6. Chase Sequence를 Selector에 첫 번째로 추가
7. PatrolNode를 Selector에 두 번째로 추가
8. Selector를 Root로 반환
```

---

## 11. 한 프레임의 실제 실행 흐름

### 플레이어가 보일 때

```text
EnemyFindArea.Update
    IsDetectedPlayer = true
    DetectedTarget = Player

EnemyBT.Update
    Root Selector Evaluate
        ↓
    Chase Sequence Evaluate
        ↓
    CanSeePlayerNode = Success
        ↓
    ChasePlayerNode = Running 또는 Success
        ↓
    Selector는 Patrol을 평가하지 않음
```

### 플레이어가 보이지 않을 때

```text
EnemyFindArea.Update
    IsDetectedPlayer = false
    DetectedTarget = null

EnemyBT.Update
    Root Selector Evaluate
        ↓
    Chase Sequence Evaluate
        ↓
    CanSeePlayerNode = Failure
        ↓
    Chase Sequence = Failure
        ↓
    Selector가 다음 자식 PatrolNode 평가
        ↓
    PatrolNode = Running
```

### Patrol 중 Player를 발견할 때

```text
이전 프레임
    PatrolNode = Running

다음 프레임
    Selector는 항상 첫 번째 자식부터 다시 평가
        ↓
    CanSeePlayerNode = Success
        ↓
    ChasePlayerNode가 Player 위치로 목적지를 변경
        ↓
    이전 Patrol 목적지는 새 Chase 목적지로 덮어써짐
```

별도의 “Patrol 중단 함수”가 없어도 각 행동 노드가 선택될 때 자신의 목적지를 다시 설정하므로 행동이 전환된다.

---

## 12. Enemy GameObject의 Inspector 연결

Enemy GameObject에는 최소한 다음 컴포넌트가 필요하다.

```text
NavMeshAgent
EnemyFindArea
EnemyBT
```

총소리 기능까지 유지한다면 다음도 붙인다.

```text
EnemyChase
```

`EnemyBT` Inspector:

```text
Find Area
    같은 Enemy의 EnemyFindArea
    비워도 Awake의 GetComponent로 찾음

Agent
    같은 Enemy의 NavMeshAgent
    비워도 Awake의 GetComponent로 찾음

Patrol Speed
    순찰 속도

Way Point
    순찰할 Transform 배열
```

Waypoint 배열 예:

```text
Size = 3
Element 0 = Waypoint_A
Element 1 = Waypoint_B
Element 2 = Waypoint_C
```

Waypoint 오브젝트들은 NavMesh 위 또는 NavMesh에서 충분히 가까운 곳에 배치한다.

NavMeshAgent 확인 항목:

```text
Agent Type이 Bake한 NavMesh와 같은가?
Enemy 시작 위치가 NavMesh 위인가?
Speed가 0보다 큰가?
Stopping Distance가 지나치게 크지 않은가?
Is Stopped가 false인가?
```

EnemyFindArea 확인 항목:

```text
Player Layer가 Player GameObject의 Layer와 같은가?
Obstacle Layer에 벽 Layer가 포함되어 있는가?
Distance가 0보다 큰가?
Degree가 원하는 시야각인가?
Eye Height와 Target Height가 적절한가?
```

---

## 13. 테스트 순서

### 테스트 1: Patrol만 확인

Player를 감지 범위 밖에 둔다.

예상 결과:

```text
Enemy가 Waypoint 0 → 1 → 마지막 → 이전 → 0 순서로 왕복
```

움직이지 않는다면 다음을 확인한다.

```text
EnemyBT의 _root가 생성되었는가?
PatrolNode가 Selector에 추가되었는가?
Waypoint 배열이 비어 있지 않은가?
NavMeshAgent가 NavMesh 위에 있는가?
Patrol Speed가 Agent에 적용되었는가?
```

### 테스트 2: Chase 확인

Patrol 중 Player를 시야 안에 둔다.

예상 결과:

```text
다음 BT 평가부터 Patrol 목적지가 Player 위치로 변경
Enemy가 Player를 추격
```

### 테스트 3: Patrol 복귀 확인

Player를 시야와 감지 범위 밖으로 이동한다.

예상 결과:

```text
CanSeePlayerNode가 Failure
Selector가 PatrolNode 선택
마지막으로 저장된 Patrol 인덱스의 Waypoint부터 순찰 재개
```

현재는 마지막으로 본 Player 위치를 조사하는 노드가 없으므로 Player를 놓치는 순간 바로 Patrol로 돌아가는 것이 정상이다.

---

## 14. 현재 코드에서 바로 수정할 목록

### EnemyBT

```text
EnemyPatrolNode _patrol SerializeField 제거
_patrolSpeed 필드 추가
_wayPoint 배열 필드 추가
중복된 두 번째 CanSeePlayerNode 제거
SetupTree에서 EnemyPatrolNode 생성
Selector에 Chase 다음 Patrol 추가
```

### EnemyPatrolNode

```text
_nav.isOnNavMesh 검사 추가
_nav.speed = _speed 적용
```

### EnemyFindArea

```text
감지 전에 LookAtPlayer를 호출하는 현재 정책 검토
테스트 완료 후 매 프레임 Debug.Log 제거
사용하지 않는 System.Linq 제거
```

### BehaviorSelector

런타임 코드에서 사용하지 않는 다음 using을 제거하는 것이 좋다.

```csharp
using NUnit.Framework;
using System.Data;
using UnityEngine;
```

이 파일에서 실제로 필요한 것은 다음이다.

```csharp
using System.Collections.Generic;
```

---

## 15. 다음 구현 순서

현재 단계에서는 한 번에 총소리까지 연결하지 말고 다음 순서로 확인한다.

```text
1. EnemyBT를 Chase → Patrol 구조로 수정
2. Player 없이 Waypoint 왕복 확인
3. Patrol 중 Player 감지 시 Chase 전환 확인
4. Player를 놓쳤을 때 Patrol 복귀 확인
5. NavMeshAgent velocity로 Idle / Walk 애니메이션 연결
6. EnemyChase를 사용하는 총소리 조사 노드 추가
```

지금 가장 먼저 완성해야 하는 기준은 다음 세 가지다.

```text
Player가 없으면 계속 왕복 순찰한다.
Player가 보이면 즉시 추격한다.
Player가 사라지면 다시 순찰한다.
```

# Dictionary 기반 WayPoint 구조 변경

## 1. 변경 목적

현재 `EnemyBT`는 적마다 Inspector에서 `Transform[] _wayPoints`를 직접 연결해야 한다.
이 구조는 적이 늘어날 때마다 같은 연결 작업을 반복해야 하므로, Scene의 경로를 한곳에서 관리하고 적이 ID를 이용해 자신의 경로를 가져오도록 변경한다.

경로는 `PatrolRouteManager`가 다음 Dictionary에 보관한다.

```csharp
Dictionary<string, Transform[]> _routes;
```

Unity는 일반 Dictionary를 Inspector에 직접 직렬화하지 않으므로, Manager의 자식 Transform을 `Awake()`에서 읽어서 Dictionary를 생성한다.

## 2. 권장 Hierarchy 구조

```text
PatrolRouteManager
├─ EnemySoldier_01
│  ├─ Point_01
│  ├─ Point_02
│  └─ Point_03
├─ WarBot_01
│  ├─ Point_01
│  └─ Point_02
└─ WarBot_02
   ├─ Point_01
   ├─ Point_02
   └─ Point_03
```

- `PatrolRouteManager` 바로 아래의 오브젝트 이름이 경로 ID가 된다.
- 경로 ID 오브젝트의 자식들이 배열 순서대로 WayPoint가 된다.
- WayPoint의 이동 순서는 Hierarchy의 자식 순서로 결정된다.
- 경로 오브젝트는 Enemy의 자식으로 두지 않는다. Enemy가 파괴될 때 경로까지 파괴되는 것을 방지하기 위해 Manager 아래에 둔다.
- 제자리에서 대기하는 고정형 Enemy는 경로를 만들지 않아도 된다.

## 3. PatrolRouteManager.cs

`Assets/Scripts/Enemy` 또는 별도의 Manager 폴더에 다음 스크립트를 만든다.

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class PatrolRouteManager : MonoBehaviour
    {
        public static PatrolRouteManager Instance { get; private set; }

        readonly Dictionary<string, Transform[]> _routes =
            new Dictionary<string, Transform[]>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("PatrolRouteManager가 Scene에 두 개 이상 있습니다.");
                return;
            }

            Instance = this;
            BuildRoutes();
        }

        void BuildRoutes()
        {
            _routes.Clear();

            foreach (Transform routeRoot in transform)
            {
                string routeId = routeRoot.name;

                if (_routes.ContainsKey(routeId))
                {
                    Debug.LogError($"중복된 Patrol ID입니다: {routeId}");
                    continue;
                }

                Transform[] wayPoints = new Transform[routeRoot.childCount];

                for (int i = 0; i < routeRoot.childCount; i++)
                {
                    wayPoints[i] = routeRoot.GetChild(i);
                }

                _routes.Add(routeId, wayPoints);
            }
        }

        public bool TryGetRoute(string routeId, out Transform[] wayPoints)
        {
            return _routes.TryGetValue(routeId, out wayPoints);
        }
    }
}
```

Scene에 빈 GameObject를 만들고 이름을 `PatrolRouteManager`로 지정한 뒤 이 컴포넌트를 붙인다.

## 4. EnemyBT 필드 변경

기존 필드:

```csharp
[SerializeField] Transform[] _wayPoints;
```

다음과 같이 변경한다.

```csharp
[Header("Patrol Node Options")]
[SerializeField] string _patrolId;
[SerializeField] float _patrolSpeed;

Transform[] _wayPoints;
```

`_patrolId`는 Enemy의 표시 이름과 분리된 경로 식별자다. Enemy GameObject의 이름을 변경해도 경로 연결을 유지할 수 있고, 여러 Enemy가 하나의 경로를 공유할 수도 있다.

## 5. EnemyBT에서 경로 가져오기

`EnemyBT.Start()`에서 BT를 만들기 전에 경로를 조회한다.

```csharp
void Start()
{
    _enemyChase = GetComponent<EnemyChase>();

    LoadWayPoints();
    _root = SetupTree();
}

void LoadWayPoints()
{
    if (string.IsNullOrWhiteSpace(_patrolId))
    {
        _wayPoints = null;
        return;
    }

    if (PatrolRouteManager.Instance == null)
    {
        Debug.LogError("PatrolRouteManager가 Scene에 없습니다.");
        _wayPoints = null;
        return;
    }

    if (!PatrolRouteManager.Instance.TryGetRoute(
            _patrolId,
            out _wayPoints))
    {
        Debug.LogWarning(
            $"{name}의 Patrol 경로를 찾지 못했습니다. ID: {_patrolId}");
    }
}
```

`EnemyPatrolNode` 생성 코드는 기존 방식을 그대로 사용한다.

```csharp
selectorTree.AddChild(
    new EnemyPatrolNode(_nav, _wayPoints, _patrolSpeed));
```

경로가 없으면 현재 `EnemyPatrolNode`가 `State.Failure`를 반환하므로 예외가 발생하지 않는다.

## 6. Enemy 이름을 직접 ID로 사용하는 방법

별도의 `_patrolId` 없이 Enemy GameObject 이름을 바로 사용할 수도 있다.

```csharp
string patrolId = gameObject.name;

PatrolRouteManager.Instance.TryGetRoute(
    patrolId,
    out _wayPoints);
```

이 방식을 사용하면 다음 두 이름이 정확히 일치해야 한다.

```text
Enemy GameObject: EnemySoldier_01
경로 오브젝트:   EnemySoldier_01
```

하지만 다음 문제가 생길 수 있다.

- Enemy 이름을 바꾸면 연결이 끊어진다.
- 같은 이름의 Enemy를 여러 개 만들 수 없다.
- 복제 시 `(1)`, `(2)`가 붙으면 등록된 경로 ID와 달라진다.
- 이름 오타가 있어도 컴파일 단계에서는 발견되지 않는다.

따라서 Enemy 이름을 ID로 직접 사용하는 것보다 별도의 `_patrolId` 필드를 사용하는 방식을 권장한다.

## 7. 적용 예시

Hierarchy:

```text
PatrolRouteManager
└─ SoldierRoute_A
   ├─ Point_01
   ├─ Point_02
   └─ Point_03
```

EnemySoldier Inspector:

```text
Patrol Id: SoldierRoute_A
Patrol Speed: 3
```

여러 적이 같은 경로를 사용해야 한다면 각 Enemy의 `Patrol Id`를 모두 `SoldierRoute_A`로 지정하면 된다. 각 `EnemyPatrolNode`가 WayPoint 인덱스를 별도로 가지고 있으므로 경로 배열을 공유해도 진행 상태는 서로 섞이지 않는다.

## 8. 고정형 Enemy 처리

순찰하지 않고 제자리에서 공격하는 Enemy는 `_patrolId`를 비워둔다. 다만 현재 BT는 플레이어를 감지하고 공격 사거리 밖에 있으면 `ChaseToPlayerNode`를 실행하므로, WayPoint만 비우는 것으로 고정형 행동이 완성되지는 않는다.

`EnemyBT`에 다음과 같은 행동 타입을 추가하고 타입에 따라 BT를 다르게 조립해야 한다.

```csharp
public enum EnemyBehaviorType
{
    Patrol,
    Stationary
}

[SerializeField] EnemyBehaviorType _behaviorType;
```

- `Patrol`: 공격, 추격, 소리 조사, 순찰을 사용한다.
- `Stationary`: 공격 가능한 경우에만 공격하고, 나머지 상황에는 정지 상태를 유지한다.

## 9. 적용 순서

1. `PatrolRouteManager.cs`를 생성한다.
2. Scene에 `PatrolRouteManager` GameObject를 만든다.
3. Manager 아래에 경로 ID 오브젝트와 Point들을 만든다.
4. `EnemyBT`의 Inspector용 `_wayPoints` 배열을 `_patrolId`로 교체한다.
5. `EnemyBT.Start()`에서 `LoadWayPoints()`를 호출한다.
6. 각 순찰형 Enemy에 사용할 `Patrol Id`를 지정한다.
7. 중복 ID 및 찾을 수 없는 ID 관련 Console 메시지를 확인한다.
8. 고정형 Enemy는 `Patrol Id`를 비우고 별도의 `Stationary` 행동 타입을 사용한다.

## 10. 확인 사항

- Scene에 `PatrolRouteManager`가 하나만 존재하는가?
- 각 경로 ID가 중복되지 않는가?
- `_patrolId`와 Manager 아래의 경로 이름이 정확히 일치하는가?
- Point들의 Hierarchy 순서가 실제 이동 순서와 일치하는가?
- Enemy가 파괴돼도 경로 Transform이 함께 파괴되지 않는가?
- 고정형 Enemy가 플레이어를 추격하거나 총소리 위치로 이동하지 않는가?

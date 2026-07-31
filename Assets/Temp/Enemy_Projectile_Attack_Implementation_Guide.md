# Enemy 발사체 공격 구현 가이드

## 결론

`EnemyAttackNode` 안에 탐지, 회전, 쿨다운, 발사체 생성, 충돌, 데미지까지 전부 넣지 않는다.

현재 프로젝트 구조에서는 아래처럼 책임을 나누는 것이 좋다.

```text
EnemyBT
  └─ 공격할 상황인지 판단하고 노드를 구성
      └─ EnemyAttackNode
          └─ EnemyAttack.TryAttack(target) 호출
              └─ EnemyProjectile 생성
                  └─ 이동 및 충돌 후 PlayerHealth에 데미지 전달
```

- `EnemyAttackNode`: BT 판단과 상태 반환
- `EnemyAttack`: 공격 설정, 쿨다운, 조준, 발사체 생성
- `EnemyProjectile`: 발사체 이동, 충돌, 데미지, 수명
- `PlayerHealth` 또는 `IDamageable`: 실제 체력 감소와 사망 처리

노드는 일반 C# 객체라 Unity 생명주기와 프리팹 참조를 직접 관리하기 불편하다. 반면 `EnemyAttack`은 `MonoBehaviour`이므로 Inspector에서 총구와 발사체 프리팹을 연결하기 쉽다.

---

## 권장 파일 구성

```text
Assets/Scripts/Enemy/
├─ EnemyAttack.cs
├─ EnemyProjectile.cs
└─ BehaviorTreeScript/
   ├─ EnemyAttackNode.cs
   └─ IsInAttackRangeNode.cs     // 선택 사항
```

현재 `EnemyAttackNode.cs`가 `Enemy` 폴더 바로 아래에 있는데, 다른 노드와 맞추려면 `BehaviorTreeScript` 폴더로 옮겨도 된다. 기능상 필수는 아니다.

---

## 1. EnemyAttack의 책임

`EnemyAttack`은 실제 무기 역할을 한다.

- 총구 위치 보관
- 발사체 프리팹 보관
- 공격력, 사거리, 연사 간격, 발사체 속도 설정
- 타깃이 사거리 안인지 확인
- 타깃 방향으로 적 회전
- 쿨다운이 끝났을 때 발사체 생성
- 죽을 때 공격하지 않도록 컴포넌트 비활성화

### 권장 기본 형태

```csharp
using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyAttack : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Transform _muzzle;
        [SerializeField] EnemyProjectile _projectilePrefab;

        [Header("Attack Options")]
        [SerializeField] int _damage = 15;
        [SerializeField] float _attackRange = 15f;
        [SerializeField] float _attackInterval = 1f;
        [SerializeField] float _projectileSpeed = 20f;
        [SerializeField] float _turnSpeed = 10f;

        float _nextAttackTime;

        public float AttackRange => _attackRange;

        public bool IsInRange(Transform target)
        {
            if (target == null)
                return false;

            float sqrDistance = (target.position - transform.position).sqrMagnitude;
            return sqrDistance <= _attackRange * _attackRange;
        }

        public bool TryAttack(Transform target)
        {
            if (target == null || _muzzle == null || _projectilePrefab == null)
                return false;

            if (!IsInRange(target))
                return false;

            FaceTarget(target);

            if (Time.time < _nextAttackTime)
                return false;

            _nextAttackTime = Time.time + _attackInterval;

            Vector3 targetPoint = target.position + Vector3.up;
            Vector3 direction = (targetPoint - _muzzle.position).normalized;

            EnemyProjectile projectile = Instantiate(
                _projectilePrefab,
                _muzzle.position,
                Quaternion.LookRotation(direction));

            projectile.Initialize(direction, _projectileSpeed, _damage, gameObject);
            return true;
        }

        void FaceTarget(Transform target)
        {
            Vector3 direction = target.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                _turnSpeed * Time.deltaTime);
        }
    }
}
```

처음에는 이 정도로 구현하고, 발사 애니메이션을 붙일 때 `TryAttack()`에서 바로 생성하지 않고 애니메이션 이벤트가 실제 발사 메서드를 호출하도록 확장하면 된다.

---

## 2. EnemyAttackNode의 책임

`EnemyAttackNode`는 직접 총알을 만들 필요가 없다.

매 프레임 다음만 확인한다.

1. 필요한 참조가 유효한가?
2. 플레이어가 탐지되어 있는가?
3. 플레이어가 공격 사거리 안인가?
4. 가능하다면 `EnemyAttack.TryAttack()` 호출
5. 공격 가능한 동안 `Running`, 조건이 깨지면 `Failure` 반환

### 권장 형태

```csharp
using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyAttackNode : BTNode
    {
        readonly EnemyAttack _attack;
        readonly EnemyFindArea _findArea;

        public EnemyAttackNode(EnemyAttack attack, EnemyFindArea findArea)
        {
            _attack = attack;
            _findArea = findArea;
        }

        public override State Evaluate()
        {
            if (_attack == null || !_attack.enabled || _findArea == null)
                return State.Failure;

            Transform target = _findArea.DetectedTarget;

            if (!_findArea.IsDetectedPlayer || target == null)
                return State.Failure;

            if (!_attack.IsInRange(target))
                return State.Failure;

            _attack.TryAttack(target);
            return State.Running;
        }
    }
}
```

공격 쿨다운 중에도 타깃을 계속 바라보고 공격 브랜치를 유지하기 위해, 사거리 안이라면 `TryAttack()`의 발사 성공 여부와 관계없이 `Running`을 반환한다.

타깃이 사거리 밖으로 나가면 `Failure`가 되므로 Selector가 다음 추적 브랜치를 평가한다.

현재 `EnemyAttackNode`의 `IWeapons` 구현과 `AimType`, `WeaponType` 필드는 제거해도 된다. BT 노드는 무기 자체가 아니고 무기를 사용하는 판단 객체이기 때문이다. 무기 종류가 실제로 여러 개 생기면 그 정보는 `EnemyAttack` 또는 별도의 무기 컴포넌트에 둔다.

---

## 3. EnemyProjectile의 책임

발사체는 다음만 담당한다.

- 지정된 방향과 속도로 이동
- 일정 시간 후 자동 제거
- 플레이어 또는 `IDamageable`과 충돌하면 데미지 적용
- 발사한 적 자신과의 충돌 무시
- 한 번 충돌한 뒤 중복 데미지 방지

### Rigidbody 기반 권장 형태

```csharp
using UnityEngine;

namespace MIssionOfMercenary
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] float _lifeTime = 5f;

        Rigidbody _rigidbody;
        int _damage;
        GameObject _owner;
        bool _hasHit;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void Initialize(
            Vector3 direction,
            float speed,
            int damage,
            GameObject owner)
        {
            _damage = damage;
            _owner = owner;
            _rigidbody.linearVelocity = direction.normalized * speed;

            Destroy(gameObject, _lifeTime);
        }

        void OnTriggerEnter(Collider other)
        {
            if (_hasHit)
                return;

            if (_owner != null && other.transform.root.gameObject == _owner)
                return;

            IDamageable damageable = other.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                _hasHit = true;
                damageable.TakeDameged(_damage);
                Destroy(gameObject);
                return;
            }

            // 벽이나 지형에 맞아도 제거한다.
            _hasHit = true;
            Destroy(gameObject);
        }
    }
}
```

### Unity 버전 확인

프로젝트 Unity 버전에서 `Rigidbody.linearVelocity`를 지원하지 않으면 아래처럼 사용한다.

```csharp
_rigidbody.velocity = direction.normalized * speed;
```

둘 중 프로젝트에서 컴파일되는 하나만 사용한다.

### Projectile 프리팹 Inspector 설정

- Rigidbody
  - Use Gravity: 처음에는 Off 권장
  - Is Kinematic: Off
  - Collision Detection: Continuous Dynamic 권장
- Collider
  - Is Trigger: On
- Layer
  - `EnemyProjectile` 전용 레이어 권장
- Physics Layer Collision Matrix
  - Enemy와 EnemyProjectile 충돌 해제
  - Player, 지형과는 충돌 활성화

빠른 발사체는 프레임 사이에 Collider를 통과할 수 있다. `Continuous Dynamic`을 사용하거나, 더 안정적으로 만들려면 이전 위치에서 현재 위치까지 Raycast/SphereCast하는 방식을 추가한다.

---

## 4. Player 데미지 처리

현재 프로젝트에는 `IDamageable` 인터페이스는 있지만, 검색된 코드 기준으로 이를 구현하는 Player 체력 컴포넌트가 아직 없다.

먼저 Player 쪽에 다음 역할을 가진 컴포넌트가 필요하다.

```csharp
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] int _health = 100;

    public bool IsDeath { get; set; }

    public void TakeDameged(int damage)
    {
        if (IsDeath)
            return;

        _health -= damage;

        if (_health <= 0)
        {
            _health = 0;
            IsDeath = true;
            Death();
        }
    }

    public void Death()
    {
        Debug.Log("플레이어 사망");
    }
}
```

현재 인터페이스의 메서드 이름이 `TakeDameged`로 되어 있다. 가능하면 나중에 `TakeDamage`로 철자를 정리하는 것이 좋지만, 변경할 때는 인터페이스와 모든 구현/호출부를 함께 수정해야 한다.

---

## 5. EnemyBT에 공격 브랜치 연결

Selector는 앞에 추가된 자식부터 평가한다. 따라서 우선순위는 아래가 적절하다.

```text
1. 플레이어 공격
2. 플레이어 추적
3. 총소리 위치 추적
4. 순찰
```

`EnemyBT`에 참조를 추가한다.

```csharp
EnemyAttack _enemyAttack;
```

`Awake()` 또는 `Start()`에서 가져온다.

```csharp
_enemyAttack = GetComponent<EnemyAttack>();
```

`SetupTree()`에서 공격 노드를 가장 먼저 등록한다.

```csharp
BTNode SetupTree()
{
    BehaviorSelector selector = new BehaviorSelector();

    EnemyAttackNode attackNode =
        new EnemyAttackNode(_enemyAttack, _findArea);

    Sequence chaseSeq = new Sequence();
    chaseSeq.AddChild(new CanSeePlayerNode(_findArea));
    chaseSeq.AddChild(new ChasePlayerNode(_nav, _findArea));

    Sequence soundPositionSeq = new Sequence();
    soundPositionSeq.AddChild(new HasSoundTargetNode(_enemyChase));
    soundPositionSeq.AddChild(
        new ChaseSoundPositionNode(
            _moveSoundPositionSpeed,
            _nav,
            _enemyChase));

    EnemyPatrolNode patrolNode =
        new EnemyPatrolNode(_nav, _wayPoints, _patrolSpeed);

    selector.AddChild(attackNode);
    selector.AddChild(chaseSeq);
    selector.AddChild(soundPositionSeq);
    selector.AddChild(patrolNode);

    return selector;
}
```

공격 중에는 NavMeshAgent가 계속 이전 목적지를 향해 움직일 수 있다. 공격 사거리 안에 들어오면 `EnemyAttackNode`에서 다음 처리를 추가하는 것이 좋다.

```csharp
_agent.isStopped = true;
```

다만 공격 브랜치가 끝나 추적으로 돌아갈 때 반드시 다시 풀어야 한다.

```csharp
_agent.isStopped = false;
```

이를 깔끔하게 하려면 `EnemyAttackNode`에 `NavMeshAgent`도 전달하여 사거리 안에서는 정지시키고, 공격 조건 실패 시 해제한다.

---

## 6. 조준 방식 선택

### A. 현재 위치를 향해 직선 발사

```csharp
Vector3 direction = (targetPoint - muzzlePosition).normalized;
```

가장 간단하지만 플레이어가 옆으로 달리면 잘 빗나간다. 우선 이 방식으로 전체 흐름부터 완성하는 것을 권장한다.

### B. 플레이어의 이동을 예측해서 발사

플레이어 속도와 발사체 속도로 미래 위치를 계산한다. 직선 발사가 정상 작동한 다음 추가한다.

### C. 약간의 오차 추가

항상 정확히 조준하면 불공평해질 수 있다. 난이도에 따라 방향에 작은 각도 오차를 적용할 수 있다.

```csharp
Quaternion spread = Quaternion.Euler(
    Random.Range(-2f, 2f),
    Random.Range(-2f, 2f),
    0f);

direction = spread * direction;
```

초기 구현에서는 플레이어 몸통 높이를 고정값 `Vector3.up`으로 보정하고, 이후 Player에 별도의 `AimTarget` Transform을 만들어 참조하는 방식이 더 정확하다.

---

## 7. 사망 시 함께 꺼야 할 컴포넌트

현재 Ragdoll 전환 시 다음 컴포넌트를 비활성화하고 있다.

- Animator
- EnemyBT
- NavMeshAgent
- EnemyFindArea

여기에 `EnemyAttack`도 추가해야 한다.

```csharp
EnemyAttack _attack;
```

```csharp
_attack = GetComponent<EnemyAttack>();
```

```csharp
if (_attack != null)
    _attack.enabled = !active;
```

BT가 꺼지면 새 공격 판단은 실행되지 않지만, 공격 애니메이션 이벤트나 별도 예약 로직을 나중에 추가할 수 있으므로 공격 컴포넌트 자체도 끄는 편이 안전하다. 이미 발사된 Projectile은 독립 오브젝트이므로 적이 죽어도 계속 날아가게 할지, 함께 제거할지는 게임 규칙으로 선택하면 된다.

---

## 8. 구현 권장 순서

### 1단계: PlayerHealth 준비

- Player에 `IDamageable` 구현
- 임시 코드로 데미지와 사망 로그 확인

### 2단계: EnemyProjectile 프리팹 제작

- 구체 같은 임시 Mesh 사용
- Rigidbody와 Trigger Collider 설정
- 이동, 벽 충돌, Player 데미지 확인

### 3단계: EnemyAttack 구현

- Muzzle Transform 생성
- 발사체 프리팹 연결
- `TryAttack(target)` 단독 테스트

### 4단계: EnemyAttackNode 연결

- 공격 노드를 Selector 최우선으로 추가
- 사거리 안에서는 공격
- 사거리 밖에서는 Chase로 전환 확인

### 5단계: 이동과 회전 다듬기

- 공격 중 NavMeshAgent 정지
- 플레이어 방향으로 회전
- 공격 종료 후 추적 재개

### 6단계: 연출 추가

- 발사 애니메이션
- Muzzle Flash
- 발사음
- 피격 이펙트
- 명중 오차

### 7단계: 최적화

- 적과 총알 수가 많아질 때 Projectile Object Pool 적용

---

## 9. 테스트 체크리스트

- [ ] 플레이어를 발견하지 못하면 발사하지 않는다.
- [ ] 플레이어가 공격 사거리 밖이면 추적한다.
- [ ] 공격 사거리 안에 들어오면 이동을 멈춘다.
- [ ] 공격 중 플레이어 방향으로 회전한다.
- [ ] 설정된 공격 간격보다 빠르게 연사하지 않는다.
- [ ] 발사체가 적 자신의 Collider에 맞지 않는다.
- [ ] 발사체가 Player에게 한 번만 데미지를 준다.
- [ ] 발사체가 벽에 맞으면 제거된다.
- [ ] 빗나간 발사체도 수명 이후 제거된다.
- [ ] 적이 죽으면 탐지, BT, 이동, 공격이 모두 정지한다.
- [ ] 적이 죽기 직전에 발사한 발사체의 처리 규칙이 의도와 맞다.
- [ ] 여러 적이 동시에 공격해도 각자 쿨다운이 독립적으로 작동한다.

---

## 최종 권장 범위

처음부터 예측 사격, 오브젝트 풀, 애니메이션 이벤트까지 한꺼번에 만들 필요는 없다.

첫 완성 목표는 아래 흐름이다.

```text
Player 발견
→ 사거리 확인
→ 사거리 밖이면 Chase
→ 사거리 안이면 정지 및 회전
→ 쿨다운마다 Projectile 생성
→ Projectile이 Player 또는 벽과 충돌
→ Player 체력 감소
```

이 흐름이 안정적으로 작동한 뒤 애니메이션, 사운드, 조준 오차, 예측 사격, Object Pool 순으로 확장한다.

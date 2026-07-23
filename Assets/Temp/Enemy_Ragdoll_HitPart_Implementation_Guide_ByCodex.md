# Enemy Ragdoll / 부위별 데미지 구현 가이드

## 1. 현재 프리팹에서 먼저 고쳐야 하는 문제

대상 프리팹:

```text
Assets/Prefabs/Enemy/EnemySoldier.prefab
```

현재 확인된 상태:

- Enemy 루트에는 `NavMeshAgent`가 있다.
- 모델의 머리, 몸통, 팔, 다리 뼈에는 여러 `Collider`와 `Rigidbody`가 있다.
- 현재 래그돌 Rigidbody가 살아 있는 상태부터 `Is Kinematic = false`, `Use Gravity = true`로 저장되어 있다.
- 이 상태에서는 Animator/NavMesh가 뼈를 움직이는 동시에 물리 엔진도 Rigidbody를 움직인다.
- 그 결과 모델과 Collider가 어긋나거나, 캐릭터 이동 중 래그돌 부위가 뒤처지는 현상이 생길 수 있다.

가장 먼저 모든 래그돌 Rigidbody를 아래처럼 바꾼다.

```text
평상시(살아 있음)
Rigidbody.isKinematic = true
Rigidbody.useGravity = false
Animator.enabled = true
NavMeshAgent.enabled = true

사망 후(래그돌)
Rigidbody.isKinematic = false
Rigidbody.useGravity = true
Animator.enabled = false
NavMeshAgent.enabled = false
```

뼈의 자식으로 Collider와 Rigidbody가 올바르게 붙어 있다면, 평상시에는 Animator가 뼈를 움직일 때 Collider도 자동으로 따라온다. 별도의 추적 코드는 원래 필요하지 않다.

---

## 2. 권장 오브젝트 구조

```text
EnemySoldier (루트)
├─ NavMeshAgent
├─ Animator 또는 Animator가 있는 모델 참조
├─ EnemyHit                 // 체력과 실제 데미지 처리
├─ EnemyRagdollController   // 평상시/래그돌 전환
├─ 이동용 CapsuleCollider   // 살아 있을 때 몸 전체 이동 충돌용
└─ Soldier_demo (모델)
   └─ Skeleton
      ├─ Head Bone
      │  ├─ Rigidbody
      │  ├─ Collider
      │  ├─ Joint
      │  └─ EnemyBodyPart (Head)
      ├─ Chest/Pelvis Bone
      │  ├─ Rigidbody
      │  ├─ Collider
      │  ├─ Joint
      │  └─ EnemyBodyPart (Chest)
      ├─ Left/Right Arm Bones
      │  └─ EnemyBodyPart (Arm)
      └─ Left/Right Leg Bones
         └─ EnemyBodyPart (Leg)
```

머리, 가슴, 팔, 다리는 데미지 분류이고, 실제 래그돌은 관절이 자연스럽게 꺾이도록 상완/하완, 허벅지/종아리처럼 더 많은 뼈와 Collider를 사용해도 된다. 예를 들어 왼쪽 상완과 오른쪽 하완은 서로 다른 Collider지만 둘 다 `Arm`으로 지정한다.

---

## 3. Enum으로 네 부위 분류

새 파일 예시:

```csharp
namespace MIssionOfMercenary
{
    public enum EnemyHitPart
    {
        Head,
        Chest,
        Arm,
        Leg
    }
}
```

추천 기본 배율:

```text
Head  = 2.0배
Chest = 1.0배
Arm   = 0.7배
Leg   = 0.7배
```

배율은 나중에 Inspector에서 조절할 수 있게 각 부위 컴포넌트에 직렬화하는 편이 좋다.

---

## 4. 각 Collider에 붙일 부위 판정 컴포넌트

각 래그돌 Collider가 붙은 뼈 오브젝트에 아래 컴포넌트를 붙인다.

```csharp
using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyBodyPart : MonoBehaviour
    {
        [SerializeField] EnemyHitPart _hitPart;
        [SerializeField] float _damageMultiplier = 1f;
        [SerializeField] EnemyHit _enemyHit;

        public EnemyHitPart HitPart => _hitPart;

        private void Reset()
        {
            _enemyHit = GetComponentInParent<EnemyHit>();
        }

        public void TakeDamage(int baseDamage)
        {
            if (_enemyHit == null)
            {
                return;
            }

            int finalDamage = Mathf.RoundToInt(baseDamage * _damageMultiplier);
            _enemyHit.TakeDameged(finalDamage);
        }
    }
}
```

Inspector 설정 예:

```text
머리 Collider      → Head,  2.0
가슴/골반 Collider → Chest, 1.0
양팔 Collider      → Arm,   0.7
양다리 Collider    → Leg,   0.7
```

`EnemyHit`은 루트에 하나만 둔다. 부위 Collider마다 체력을 따로 만들지 말고, `EnemyBodyPart`가 계산한 최종 데미지를 루트 `EnemyHit`에 전달한다.

---

## 5. 총알 Raycast에서 부위 판정하기

총알이 맞은 `RaycastHit.collider`에서 `EnemyBodyPart`를 찾는다.

```csharp
if (Physics.Raycast(origin, direction, out RaycastHit hit, range, hitLayer))
{
    EnemyBodyPart bodyPart = hit.collider.GetComponent<EnemyBodyPart>();

    if (bodyPart != null)
    {
        bodyPart.TakeDamage(weaponDamage);
    }
}
```

컴포넌트를 Collider와 같은 GameObject에 붙이면 `GetComponent`만 사용해도 된다. 부모나 자식을 매번 검색하는 것보다 구조가 명확하고 판정 실수가 적다.

총알이 루트 이동용 CapsuleCollider를 먼저 맞지 않도록 다음 중 하나로 분리한다.

- 부위 Collider를 `EnemyHitbox` Layer로 지정하고 총알은 이 Layer만 Raycast한다.
- 루트 이동용 Collider는 총알 Raycast LayerMask에서 제외한다.
- 평상시 충돌용 Layer와 총알 피격용 Layer를 별도로 운영한다.

---

## 6. 래그돌 전환 코드 구조

```csharp
using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class EnemyRagdollController : MonoBehaviour
    {
        [SerializeField] Animator _animator;
        [SerializeField] NavMeshAgent _agent;
        [SerializeField] Collider _rootMoveCollider;
        [SerializeField] Rigidbody[] _ragdollBodies;

        private void Awake()
        {
            SetRagdoll(false);
        }

        public void SetRagdoll(bool active)
        {
            if (_agent != null)
            {
                _agent.enabled = !active;
            }

            if (_rootMoveCollider != null)
            {
                _rootMoveCollider.enabled = !active;
            }

            if (_animator != null)
            {
                _animator.enabled = !active;
            }

            foreach (Rigidbody body in _ragdollBodies)
            {
                body.isKinematic = !active;
                body.useGravity = active;
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
        }
    }
}
```

Unity 버전에 따라 `Rigidbody.velocity` 대신 `linearVelocity`를 사용해야 할 수 있으므로 현재 프로젝트 API에 맞춰 선택한다.

사망 처리 흐름:

```text
EnemyHit.TakeDameged()
→ 체력 감소
→ 체력이 0 이하이면 Death()
→ BT/공격/시야 스크립트 중단
→ EnemyRagdollController.SetRagdoll(true)
→ Animator와 NavMeshAgent 중단
→ 뼈 Rigidbody 물리 활성화
```

래그돌 전환 직전에 Animator가 현재 자세를 이미 뼈에 적용하고 있으므로, 그 자세 그대로 물리가 시작된다.

---

## 7. Collider가 모델을 따라오지 않을 때 확인 순서

### 7-1. Collider가 실제 뼈에 붙어 있는지

Collider가 `EnemySoldier` 루트나 모델 바깥의 별도 오브젝트에 있으면 스킨 메시의 뼈 변형을 자동으로 따라가지 않는다.

올바른 위치:

```text
Skeleton/Hips/Spine/Chest/... 뼈 GameObject
```

각 Collider를 담당하는 실제 bone GameObject에 붙인다.

### 7-2. 살아 있는데 Rigidbody가 Non-Kinematic인지

현재 프리팹에서 가장 의심되는 원인이다.

살아 있는 동안 모든 뼈 Rigidbody:

```text
Is Kinematic = true
Use Gravity = false
```

사망할 때만 반대로 전환한다.

### 7-3. Animator와 Rigidbody를 동시에 제어하는지

동일한 뼈를 Animator와 Non-Kinematic Rigidbody가 동시에 제어하면 안 된다.

```text
생존: Animator가 제어
사망: Rigidbody가 제어
```

두 모드는 명확히 나눈다.

### 7-4. 모델 루트의 Scale 확인

래그돌 부모 또는 조상 Transform에 비균일 Scale이 있으면 Collider 크기와 Joint 동작이 어긋날 수 있다.

가능하면 다음을 유지한다.

```text
EnemySoldier scale = (1, 1, 1)
Model root scale   = (1, 1, 1)
Skeleton scale     = (1, 1, 1)
```

FBX 크기는 Import Settings의 Scale Factor에서 맞추는 편이 안전하다.

### 7-5. Animator의 Update Mode 확인

NavMesh/일반 Update 기반 캐릭터라면 우선 `Normal`을 사용한다. 물리와 애니메이션 타이밍 때문에 떨림이 생길 때만 `Animate Physics`를 시험한다. 핵심 해결책은 Update Mode가 아니라 생존 중 Rigidbody를 Kinematic으로 두는 것이다.

### 7-6. Joint 연결 확인

Ragdoll Wizard로 만들었다면 각 자식 뼈의 `CharacterJoint.connectedBody`가 부모 쪽 Rigidbody를 가리키는지 확인한다. Joint가 누락되면 사망 순간 부위가 분리되어 날아갈 수 있다.

---

## 8. 별도 Hitbox를 코드로 억지로 따라오게 하는 방법

권장 방식은 Collider를 뼈에 직접 붙이는 것이다. FBX 구조를 수정할 수 없거나 별도의 Hitbox 오브젝트를 유지해야 할 때만 아래 방식을 사용한다.

```csharp
using UnityEngine;

namespace MIssionOfMercenary
{
    public class FollowBoneHitbox : MonoBehaviour
    {
        [SerializeField] Transform _targetBone;
        [SerializeField] Vector3 _positionOffset;
        [SerializeField] Vector3 _rotationOffset;

        private void LateUpdate()
        {
            if (_targetBone == null)
            {
                return;
            }

            transform.position =
                _targetBone.TransformPoint(_positionOffset);

            transform.rotation =
                _targetBone.rotation * Quaternion.Euler(_rotationOffset);
        }
    }
}
```

`LateUpdate()`를 쓰는 이유는 Animator가 해당 프레임의 뼈 자세를 적용한 다음 Hitbox를 맞추기 위해서다.

주의:

- 따라오는 Hitbox에는 Non-Kinematic Rigidbody를 붙이지 않는다.
- Rigidbody가 꼭 필요하면 생존 중 `isKinematic = true`로 둔다.
- Non-Kinematic Rigidbody의 Transform을 `LateUpdate()`에서 강제로 덮어쓰면 떨림과 물리 충돌 누락이 생긴다.
- 사망 래그돌용 Collider와 강제 추적 Hitbox를 같은 오브젝트로 운영하면 제어권이 충돌하므로 가능하면 분리한다.

권장 분리 구조:

```text
생존 중 피격 판정: 뼈 자식 Trigger Hitbox, Kinematic
사망 후 물리 판정: Ragdoll Collider + Rigidbody
```

단, 처음 구현할 때는 구조가 복잡해질 수 있으므로 기존 뼈 Collider를 피격 판정과 래그돌에 함께 사용하고 상태만 전환하는 방식부터 완성하는 것이 좋다.

---

## 9. Layer와 충돌 설정

추천 Layer:

```text
EnemyRoot       // 루트 이동 Collider
EnemyHitbox     // 머리/가슴/팔/다리 피격 Collider
EnemyRagdoll    // 사망 후 물리 충돌이 필요할 경우
PlayerBullet
```

Physics Collision Matrix에서 확인할 것:

- `PlayerBullet`은 `EnemyHitbox`를 감지한다.
- 같은 Enemy의 래그돌 부위끼리 과도하게 충돌하지 않도록 Joint와 Ignore Collision 정책을 확인한다.
- 소리 감지용 `_enemyLayer`가 모든 래그돌 Collider를 잡으면 현재처럼 중복 결과가 생긴다.
- 소리 감지는 가능하면 부위 Hitbox가 아니라 Enemy 루트 전용 Layer/Collider 하나만 찾게 한다.

현재 `SoundPositionGiveToEnemy`는 `HashSet<EnemyChase>`로 중복을 제거하므로 기능상 여러 Collider를 처리할 수 있다. 그래도 성능과 구조를 위해 소리 감지용 LayerMask는 루트 감지 Collider만 포함하는 편이 더 좋다.

---

## 10. Unity Editor에서 작업하는 순서

1. `EnemySoldier` 프리팹을 Prefab Mode로 연다.
2. 현재 래그돌 Collider가 실제 Skeleton 뼈에 붙어 있는지 확인한다.
3. 모든 뼈 Rigidbody를 `Is Kinematic = true`, `Use Gravity = false`로 바꾼다.
4. 머리/가슴/양팔/양다리 Collider 크기와 Center를 Scene 뷰에서 몸에 맞춘다.
5. `EnemyHitPart` enum을 만든다.
6. `EnemyBodyPart`를 각 Collider GameObject에 붙이고 부위를 지정한다.
7. 루트에 실제 체력을 관리할 `EnemyHit`을 완성한다.
8. 루트에 `EnemyRagdollController`를 붙이고 Animator, Agent, 이동 Collider, 모든 뼈 Rigidbody를 연결한다.
9. 총알 Raycast가 `EnemyBodyPart`를 호출하도록 연결한다.
10. 생존 상태에서 애니메이션을 재생하며 Collider가 정확히 따라오는지 확인한다.
11. 각 부위를 한 발씩 쏴서 배율이 적용되는지 확인한다.
12. 사망 시 Animator/Agent가 꺼지고 래그돌 물리가 켜지는지 확인한다.

---

## 11. 테스트 체크리스트

- 걷기/달리기 애니메이션 중 Collider가 몸에서 떨어지지 않는다.
- 회전과 NavMesh 이동 중에도 Collider가 뒤처지지 않는다.
- 머리는 `Head`, 가슴과 골반은 `Chest`로 판정된다.
- 왼팔/오른팔 모두 `Arm`으로 판정된다.
- 왼다리/오른다리 모두 `Leg`로 판정된다.
- 한 발에 데미지가 한 번만 들어간다.
- 루트 이동 Collider가 부위 Raycast를 가로막지 않는다.
- 사망 전에는 래그돌 Rigidbody가 Kinematic이다.
- 사망 후에는 Animator와 NavMeshAgent가 꺼진다.
- 사망 후에만 래그돌 Rigidbody의 물리와 중력이 활성화된다.
- 사망 순간 몸이 폭발하듯 튀거나 각 부위가 분리되지 않는다.

---

## 12. 이 프로젝트에서 권장하는 바로 다음 작업

코드를 추가하기 전에 `EnemySoldier.prefab`의 모든 래그돌 Rigidbody를 생존 기본 상태인 `Is Kinematic = true`, `Use Gravity = false`로 바꾸고, Play Mode에서 걷기 애니메이션과 Collider가 일치하는지 먼저 확인한다. 이 단계에서 일치하면 강제 추적 코드는 만들 필요가 없다.

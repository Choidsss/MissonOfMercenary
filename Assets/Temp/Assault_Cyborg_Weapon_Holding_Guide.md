# Assault Cyborg 총기 장착 및 모션 연동 가이드

## 1. 결론

Assault Cyborg처럼 캐릭터와 총기 모델이 분리되어 있다면 **총을 오른손 본에 고정하고, 왼손을 총기의 그립 지점에 IK로 붙이는 방식**이 가장 관리하기 쉽다.

- 오른손: 총기의 위치와 회전을 결정하는 기준 손
- 총기: 오른손의 `WeaponSocket` 자식
- 왼손: 총기 자식의 `LeftHandGrip`을 따라가는 IK
- Muzzle: 총기 총구에 배치하며 총기와 같이 움직임
- 애니메이션: 몸과 오른팔의 큰 움직임을 담당
- IK: 애니메이션마다 달라지는 왼손 오차를 마지막에 보정

총에 Rigidbody를 붙여 손에 물리적으로 매달 필요는 없다. 적이 들고 있는 동안에는 Transform 계층으로 고정하고, 총을 떨어뜨릴 때만 Rigidbody를 활성화하거나 별도의 드롭 프리팹을 생성한다.

---

## 2. 권장 프리팹 구조

현재 `WarBot` 안의 `WeaponBox`는 총기 관련 오브젝트를 모으는 용도로 사용할 수 있지만, 실제 장착 기준은 반드시 캐릭터의 오른손 본을 따라가야 한다.

```text
WarBot
├─ AssaultCyborg2
│  └─ Armature
│     └─ ...
│        ├─ RightHand
│        │  └─ WeaponSocket
│        │     └─ AssaultRifle
│        │        ├─ Muzzle
│        │        └─ LeftHandGrip
│        └─ LeftHand
├─ Rig
│  └─ LeftArmRig
│     ├─ LeftHandIKTarget
│     └─ LeftElbowHint
└─ WeaponBox (선택: 장비 관리용 루트)
```

`WeaponSocket`은 빈 GameObject로 만들고 오른손 본의 자식으로 둔다. 총기 자체의 피벗을 억지로 수정하지 말고, `WeaponSocket`의 Local Position/Rotation으로 손에 맞춘다.

> 현재 `WarBot.prefab`의 `EnemyAttack._muzzle` 참조가 비어 있으므로, 총기 아래의 실제 `Muzzle` Transform을 연결해야 한다.

---

## 3. Unity 에디터 작업 순서

### 3-1. 오른손에 총 붙이기

1. Assault Cyborg의 계층에서 오른손 본을 찾는다.
2. 오른손 본 아래에 빈 오브젝트 `WeaponSocket`을 만든다.
3. 총기 프리팹을 `WeaponSocket`의 자식으로 넣는다.
4. 장착 모션을 재생한 상태에서 총의 손잡이가 오른손 손바닥에 오도록 `WeaponSocket`의 위치와 회전을 조절한다.
5. 총구 아래에 `Muzzle`, 앞손잡이 위치에 `LeftHandGrip`을 만든다.

이 상태에서 애니메이션을 재생했을 때 총이 오른손을 정확히 따라가면 1차 장착은 완료다.

### 3-2. 왼손 IK 만들기

Animation Rigging 패키지를 사용한다.

1. 캐릭터 루트에 `RigBuilder`를 추가한다.
2. `Rig` 오브젝트와 `LeftArmRig`을 만든다.
3. `LeftArmRig`에 `Two Bone IK Constraint`를 추가한다.
4. Root/Mid/Tip에 왼쪽 UpperArm/LowerArm/Hand 본을 연결한다.
5. Target에는 `LeftHandIKTarget`, Hint에는 `LeftElbowHint`를 연결한다.
6. `LateUpdate`에서 `LeftHandIKTarget`을 총기의 `LeftHandGrip` 위치와 회전으로 이동시킨다.

```csharp
void LateUpdate()
{
    if (_leftHandGrip == null || _leftHandIKTarget == null)
        return;

    _leftHandIKTarget.SetPositionAndRotation(
        _leftHandGrip.position,
        _leftHandGrip.rotation);
}
```

손목 방향이 뒤집히면 팔 본을 돌리지 말고 `LeftHandGrip`의 Local Rotation을 조절한다. 팔꿈치가 몸을 뚫으면 `LeftElbowHint`를 캐릭터의 왼쪽·약간 뒤쪽으로 옮긴다.

---

## 4. 모션마다 총을 다르게 쥐게 하는 방법

총을 든 모든 애니메이션에 대해 총의 위치를 따로 움직이는 방식은 관리가 어렵다. 다음처럼 역할을 나눈다.

### 총기 전용 모션인 경우

Idle Rifle, Walk Rifle, Run Rifle, Fire Rifle처럼 총기용으로 제작된 애니메이션이라면:

- 애니메이션이 몸통과 오른팔 자세를 만든다.
- 총은 오른손 `WeaponSocket`을 따라간다.
- 왼손 IK가 총기의 `LeftHandGrip`에 붙는다.
- 모션마다 총기 Transform을 별도로 변경하지 않는다.

이 방법을 기본으로 사용하는 것이 좋다.

### 애니메이션마다 오른손 그립 위치가 크게 다른 경우

애니메이션 자체가 서로 다른 총기나 자세를 기준으로 제작됐다면 모션별 Socket 보정값을 둔다.

```text
WeaponPose
├─ Idle:  position / rotation
├─ Walk:  position / rotation
├─ Run:   position / rotation
└─ Attack: position / rotation
```

Animator State가 바뀔 때 `WeaponSocket.localPosition`과 `localRotation`을 해당 보정값으로 보간한다. 단, 처음부터 이 기능을 만들지 말고 실제 애니메이션 테스트에서 오른손 오차가 확인될 때만 추가한다.

### 총을 내리고 드는 상태가 나뉘는 경우

Animator에 다음 정도의 상태를 둔다.

```text
Unarmed/Lowered Locomotion
        ↓ Aim 또는 Player 발견
RaiseWeapon
        ↓
Armed Locomotion ↔ Fire
        ↓ 전투 종료
LowerWeapon
```

- `IsArmed`: 총을 든 이동 Blend Tree 선택
- `IsAiming`: 상체 조준 레이어 또는 조준 자세 활성화
- `Fire`: 발사 애니메이션 Trigger
- `Speed`: 현재처럼 이동 Blend Tree에 사용

`RaiseWeapon` 전환 중에는 IK Weight를 0에서 1로 올리고, `LowerWeapon` 중에는 1에서 0으로 내리면 왼손이 갑자기 순간이동하지 않는다.

---

## 5. 권장 Animator 구성

현재 `EnemyAnimation`은 `Speed`만 전달하므로 우선 아래 구성이면 충분하다.

```text
Base Layer
├─ Rifle Idle
├─ Rifle Walk
├─ Rifle Run
└─ Death

Upper Body Layer (Avatar Mask: Spine 이상)
├─ Aim
└─ Fire
```

이동 중에도 사격해야 한다면 하체 이동은 Base Layer가 맡고, 상체 조준/사격은 Upper Body Layer가 맡게 한다. Upper Body Layer에는 척추, 양팔, 머리만 포함하는 Avatar Mask를 사용한다.

사격 애니메이션의 정확한 발사 프레임에는 Animation Event를 두어 `EnemyAttack`의 공개 발사 함수를 호출하는 방법이 자연스럽다. 다만 게임 로직이 Animation Event에 완전히 종속되지 않도록 실제 쿨다운과 발사 가능 여부는 공격 코드에서 검사한다.

---

## 6. 조준과 손 고정의 처리 순서

한 프레임에서 권장되는 순서는 다음과 같다.

1. AI가 플레이어 위치와 공격 여부를 결정한다.
2. Animator가 이동/조준/발사 애니메이션을 계산한다.
3. 상체 Aim Rig가 플레이어 방향을 향하게 한다.
4. 총은 오른손 `WeaponSocket`을 따라간다.
5. 왼손 IK Target이 총의 `LeftHandGrip`을 따라간다.
6. 총기의 `Muzzle.forward`를 기준으로 탄착군과 발사 방향을 계산한다.

조준 방향을 만들기 위해 총을 월드 좌표에서 직접 회전시키면 오른손과 분리되어 보일 수 있다. 가능하면 상체/오른팔 조준 Rig가 총을 돌리게 하고, 총은 끝까지 손의 자식으로 유지한다.

---

## 7. 구현 시 주의점

- 장착된 총에는 Rigidbody가 필요 없다. Collider도 적 자신과 충돌하지 않게 하거나 비활성화한다.
- 총알 Rigidbody와 장착된 총의 Rigidbody는 별개의 문제다.
- `Muzzle`은 캐릭터 루트가 아니라 총기의 자식이어야 한다.
- `LeftHandGrip`도 총기의 자식이어야 총기 반동과 조준 회전을 자동으로 따라간다.
- `WeaponSocket`과 본 사이의 Scale은 `(1, 1, 1)`을 유지하는 편이 안전하다.
- IK Target을 왼손 본의 자식으로 두면 순환 구조처럼 움직이므로 총기 쪽 Grip을 기준으로 삼아야 한다.
- 사망 래그돌 전환 시 Rig/IK Weight를 0으로 만들고 총을 손에서 분리한다.
- 총을 떨어뜨릴 때는 손에 붙은 총의 Rigidbody를 즉석에서 켜기보다 드롭용 총기 프리팹을 생성하는 방식이 단순하다.

---

## 8. 이 프로젝트에서 바로 진행할 체크리스트

- [ ] AssaultCyborg2의 오른손 본 확인
- [ ] 오른손 아래에 `WeaponSocket` 생성
- [ ] 총기 프리팹을 Socket 자식으로 배치
- [ ] 총기에 `Muzzle`과 `LeftHandGrip` 생성
- [ ] `EnemyAttack._muzzle`에 실제 Muzzle 연결
- [ ] 캐릭터에 `RigBuilder`와 왼팔 Two Bone IK 구성
- [ ] Idle/Walk/Run 애니메이션에서 오른손과 총의 결합 확인
- [ ] 왼손 IK Weight 1 상태로 Grip과 손목 방향 확인
- [ ] Armed 이동 Blend Tree 구성
- [ ] Fire 애니메이션과 실제 발사 타이밍 연결
- [ ] 이동 중 사격이 필요하면 Upper Body Layer와 Avatar Mask 구성
- [ ] 사망 시 IK 해제 및 총기 드롭 처리

우선은 **Rifle Idle 한 모션에서 오른손 Socket과 왼손 IK를 완성한 뒤**, Walk, Run, Fire 순서로 확장하는 것이 가장 안전하다.

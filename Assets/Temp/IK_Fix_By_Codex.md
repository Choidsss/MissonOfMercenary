# 무기 교체를 위한 양손 IK 구조 정리

## 1. 이 문서의 목표

현재 프로젝트의 주무기 IK를 먼저 정리한 뒤, 나중에 권총과 칼을 추가하고 무기 교체 시스템과 연결할 수 있는 구조를 만드는 것이 목표다.

지금 당장 모든 무기의 IK를 한 번에 완성할 필요는 없다. 다음 순서로 진행한다.

1. 공용 오른손·왼손 IK 기반을 만든다.
2. 현재 주무기의 왼손 IK를 공용 구조로 변경한다.
3. 주무기가 기존처럼 작동하는지 확인한다.
4. 권총의 양손 IK를 만든다.
5. 칼의 오른손 IK를 만든다.
6. 마지막에 무기 교체 시스템이 IK Target과 Weight를 변경하도록 연결한다.

---

## 2. 먼저 이해해야 하는 핵심

### GripPoint와 IKTarget은 서로 다른 역할이다

#### GripPoint

각 무기에 들어가는 손잡이 위치다.

- 주무기의 `LeftGripPoint`: 주무기를 잡는 왼손 손목 위치
- 권총의 `RightGripPoint`: 권총을 잡는 오른손 손목 위치
- 권총의 `LeftGripPoint`: 오른손을 감싸는 왼손 손목 위치
- 칼의 `RightGripPoint`: 칼을 잡는 오른손 손목 위치

GripPoint는 무기마다 위치와 회전이 다르다.

#### IKTarget

캐릭터의 `Two Bone IK Constraint`가 항상 바라보는 공용 Transform이다.

- `RightHandIKTarget`: 오른손 IK가 따라가는 공용 Target
- `LeftHandIKTarget`: 왼손 IK가 따라가는 공용 Target

무기가 바뀌어도 Constraint의 Target 설정 자체는 바꾸지 않는다. 대신 공용 IKTarget을 현재 무기의 GripPoint 아래로 옮긴다.

```text
현재 무기의 GripPoint
└─ 공용 HandIKTarget
       ↑
       └─ Two Bone IK Constraint가 이 Transform을 따라감
```

이 구조를 사용하면 무기가 바뀔 때마다 `Two Bone IK Constraint`를 새로 만들거나 Inspector 설정을 다시 할 필요가 없다.

---

## 3. 무기별 최종 IK 사용 계획

| 무기 | 오른손 IK | 왼손 IK | 설명 |
|---|---:|---:|---|
| 주무기 | 0 | 1 | 오른손은 기존 애니메이션, 왼손만 앞 손잡이에 고정 |
| 권총 | 1 | 1 | 오른손은 권총 손잡이, 왼손은 오른손을 감싸는 위치에 고정 |
| 칼 | 1 | 0 | 오른손은 칼 손잡이에 고정, 왼손은 우선 애니메이션 사용 |
| 맨손 | 0 | 0 | 기본 애니메이션만 사용 |

`0`은 Constraint Weight가 0이라는 뜻이고, `1`은 Weight가 1이라는 뜻이다.

주무기까지 반드시 양손 IK로 변경할 필요는 없다. 현재 주무기의 오른손이 애니메이션으로 자연스럽게 유지되고 있다면 그대로 사용한다.

---

## 4. 최종 Hierarchy 구조

아래와 같은 구조를 목표로 한다.

```text
Player
├─ WeaponBox
│  ├─ AssaultRifle
│  │  └─ LeftGripPoint
│  ├─ HandGun
│  │  ├─ RightGripPoint
│  │  └─ LeftGripPoint
│  └─ Knife
│     └─ RightGripPoint
│
├─ IKTargets
│  ├─ RightHandIKTarget
│  ├─ LeftHandIKTarget
│  ├─ RightElbowHint
│  └─ LeftElbowHint
│
└─ Rig1
   ├─ RightHandIK
   └─ LeftHandIK
```

실제로 무기를 장착한 동안에는 공용 IKTarget이 해당 무기의 GripPoint 자식으로 이동한다.

주무기를 들었을 때:

```text
AssaultRifle
└─ LeftGripPoint
   └─ LeftHandIKTarget
```

권총을 들었을 때:

```text
HandGun
├─ RightGripPoint
│  └─ RightHandIKTarget
└─ LeftGripPoint
   └─ LeftHandIKTarget
```

칼을 들었을 때:

```text
Knife
└─ RightGripPoint
   └─ RightHandIKTarget
```

---

## 5. 지금 가장 먼저 할 작업: 공용 IK 기반 만들기

권총이나 칼의 손 위치를 먼저 잡지 않는다. 현재 주무기가 공용 IK 구조에서 정상 작동하도록 변경하는 것이 첫 번째 작업이다.

### 5-1. IKTargets 생성

Player 프리팹을 열고 빈 GameObject를 만든다.

```text
IKTargets
```

그 아래에 다음 빈 GameObject 네 개를 만든다.

```text
IKTargets
├─ RightHandIKTarget
├─ LeftHandIKTarget
├─ RightElbowHint
└─ LeftElbowHint
```

각 오브젝트의 Scale은 `(1, 1, 1)`로 유지한다.

### 5-2. 기존 LeftElbowHint 이동

현재 Player 프리팹의 `LeftElbowHint`는 왼쪽 아래팔 본의 자식으로 설정되어 있다.

Hint가 IK로 움직이는 팔 본의 자식이면 팔이 움직일 때 Hint도 같이 움직여 계산이 불안정해질 수 있다. 기존 `LeftElbowHint`를 팔 본 밖의 `IKTargets` 아래로 옮긴다.

```text
변경 전

LeftForeArm
└─ LeftElbowHint
```

```text
변경 후

IKTargets
└─ LeftElbowHint
```

옮길 때 World Position이 유지되도록 하고, 이후 팔꿈치보다 약간 왼쪽·아래쪽·뒤쪽에 배치한다.

오른손 IK용 `RightElbowHint`는 오른쪽 팔꿈치보다 약간 오른쪽·아래쪽·뒤쪽에 배치한다.

---

## 6. 기존 주무기 왼손 IK 변경

### 6-1. LeftHandIK의 Target 교체

현재 `LeftHandIK`의 Target은 주무기의 `LeftGripPoint`를 직접 참조한다.

```text
변경 전

LeftHandIK.Target = AssaultRifle/LeftGripPoint
```

이를 공용 Target으로 변경한다.

```text
변경 후

LeftHandIK.Target = IKTargets/LeftHandIKTarget
LeftHandIK.Hint   = IKTargets/LeftElbowHint
```

### 6-2. LeftHandIKTarget을 주무기에 연결

Hierarchy에서 `LeftHandIKTarget`을 주무기의 `LeftGripPoint` 자식으로 옮긴다.

```text
AssaultRifle
└─ LeftGripPoint
   └─ LeftHandIKTarget
```

자식으로 옮긴 뒤 다음 값으로 초기화한다.

```text
LeftHandIKTarget

Local Position = (0, 0, 0)
Local Rotation = (0, 0, 0)
Local Scale    = (1, 1, 1)
```

이제 주무기가 반동이나 Sway로 움직이면 다음 순서로 왼손이 따라간다.

```text
주무기 이동
    ↓
LeftGripPoint 이동
    ↓
LeftHandIKTarget 이동
    ↓
LeftHandIK가 왼손을 Target에 고정
```

### 6-3. LeftHandIK 테스트 값

먼저 정확한 위치를 확인하기 위해 다음 값으로 테스트한다.

```text
LeftHandIK Constraint Weight = 1
Target Position Weight       = 1
Target Rotation Weight       = 1
Hint Weight                  = 1
```

현재 Player 프리팹의 전체 Weight는 약 `0.094`, Position Weight는 약 `0.612`로 설정되어 있다. 이 상태에서는 손이 GripPoint에 정확히 붙지 않아 위치 조정이 어렵다.

손 위치가 어색할 때는 우선 Weight를 낮추지 말고 주무기의 `LeftGripPoint` 위치와 회전을 수정한다. 손 위치가 완성된 뒤 필요하면 Weight 전환을 부드럽게 만든다.

---

## 7. 기존 IKController에서 제거할 작업

현재 `IKController.UpdateIK()`에는 다음과 같이 왼쪽 위팔 본을 직접 이동시키는 코드가 있다.

```csharp
void UpdateIK()
{
    _leftArm.position = _leftGripPoint.position;
    _leftArm.rotation = _leftGripPoint.rotation;
}
```

이 코드는 손을 움직이는 것이 아니라 `LeftUpperArm`을 GripPoint 위치로 직접 이동시킨다. 또한 `Two Bone IK Constraint`와 동시에 같은 팔을 움직이기 때문에 충돌할 수 있다.

공용 IK 구조로 변경한 뒤에는 이 코드를 사용하지 않는다.

```csharp
private void LateUpdate()
{
    // UpdateIK(); 호출 제거
}
```

최종적으로 `UpdateIK()` 함수와 IK에만 사용되던 다음 필드도 정리할 수 있다.

```csharp
_leftGripPoint
_leftArm
_leftForceArm
_leftHand
```

단, 다른 기능에서 사용하는지 검색한 후 제거한다. 현재 구조에서는 `UpdateIK()`와 초기 위치 저장에 주로 사용되고 있다.

주무기의 반동과 Sway 코드는 그대로 유지한다. 무기가 움직이면 그 자식인 GripPoint와 IKTarget도 같이 움직이므로 왼손이 자동으로 따라온다.

---

## 8. 오른손 IK 틀 만들기

주무기 테스트가 정상적으로 끝나면 `Rig1` 아래에 `RightHandIK`를 만든다.

`RightHandIK`에 `Two Bone IK Constraint`를 추가하고 다음과 같이 연결한다.

```text
Root   = RightUpperArm
Mid    = RightForeArm
Tip    = RightHand
Target = RightHandIKTarget
Hint   = RightElbowHint
```

현재 주무기를 들고 있을 때는 오른손 IK를 사용하지 않는다.

```text
RightHandIK Constraint Weight = 0
```

따라서 이 단계에서 오른팔은 기존 애니메이션과 동일하게 보여야 한다. 오른손 IK는 권총과 칼을 위해 틀만 준비한 것이다.

---

## 9. 1차 완료 검사: 주무기만 테스트

아직 권총과 칼을 건드리지 말고 Play Mode에서 주무기만 검사한다.

- [ ] 주무기가 기존 위치에 정상적으로 보인다.
- [ ] 오른팔은 기존 애니메이션 그대로 유지된다.
- [ ] `RightHandIK` Weight가 0이다.
- [ ] `LeftHandIK` Weight가 1이다.
- [ ] 왼손 손목이 `LeftGripPoint`에 정확히 붙는다.
- [ ] 주무기 Sway가 발생해도 왼손이 따라간다.
- [ ] 반동이 발생해도 왼손이 총에서 분리되지 않는다.
- [ ] 왼팔이 뒤집히거나 비정상적으로 꺾이지 않는다.
- [ ] Console에 Missing Reference 또는 Rig 관련 오류가 없다.

팔꿈치가 이상하게 꺾이면 `LeftGripPoint`보다 `LeftElbowHint` 위치를 먼저 조정한다.

이 검사가 모두 끝나면 공용 IK 기반 작업이 완료된 것이다.

---

## 10. 그다음 작업: 권총 양손 IK

공용 기반이 정상 작동한 뒤 권총을 작업한다.

### 10-1. 권총 GripPoint 생성

권총 오브젝트 아래에 다음 빈 GameObject를 만든다.

```text
HandGun
├─ RightGripPoint
└─ LeftGripPoint
```

#### RightGripPoint 배치

- 오른손 손목이 와야 할 위치에 둔다.
- 권총 손잡이가 오른손 손바닥 안으로 들어오게 한다.
- 오른손 검지가 방아쇠 근처에 오도록 손목 회전을 맞춘다.
- 손가락 모양은 Two Bone IK가 아니라 별도의 손가락 애니메이션으로 만든다.

#### LeftGripPoint 배치

- 권총 손잡이의 왼쪽 또는 왼쪽 아래에 둔다.
- 왼손 손바닥이 오른손 바깥을 감싸도록 회전시킨다.
- 먼저 손목 위치만 맞추고 손가락 겹침은 나중에 수정한다.

### 10-2. 공용 Target 연결

권총 테스트 중에는 다음 구조로 공용 Target을 옮긴다.

```text
HandGun
├─ RightGripPoint
│  └─ RightHandIKTarget
└─ LeftGripPoint
   └─ LeftHandIKTarget
```

옮긴 뒤 두 Target 모두 다음 값으로 초기화한다.

```text
Local Position = (0, 0, 0)
Local Rotation = (0, 0, 0)
Local Scale    = (1, 1, 1)
```

### 10-3. 권총 IK Weight

```text
RightHandIK Weight = 1
LeftHandIK Weight  = 1
```

권총처럼 양팔을 앞으로 뻗는 자세는 IK만으로 처음부터 만들지 않는다. 권총용 기본 애니메이션 또는 한 프레임 포즈에서 어깨와 팔을 대략 앞으로 뻗게 한 뒤 IK로 손목 위치를 정확하게 보정한다.

Two Bone IK는 주로 `UpperArm → ForeArm → Hand`를 계산한다. 기본 포즈에서 팔이 아래로 내려간 상태로 Target만 멀리 보내면 어깨가 부자연스럽고 팔이 완전히 펴질 수 있다.

권총 작업 순서:

1. 권총용 기본 상체 포즈를 적용한다.
2. 왼손 IK Weight는 0으로 두고 오른손만 먼저 맞춘다.
3. `RightGripPoint` 위치와 회전을 조정한다.
4. 오른손이 완성되면 왼손 IK Weight를 1로 올린다.
5. `LeftGripPoint` 위치와 회전을 조정한다.
6. 양쪽 ElbowHint 위치를 조정한다.
7. 마지막에 검지와 나머지 손가락 애니메이션을 만든다.

---

## 11. 그다음 작업: 칼 오른손 IK

칼 오브젝트 아래에 오른손 GripPoint를 만든다.

```text
Knife
└─ RightGripPoint
```

칼 테스트 중에는 다음처럼 연결한다.

```text
Knife
└─ RightGripPoint
   └─ RightHandIKTarget
```

기본 Weight는 다음과 같이 시작한다.

```text
RightHandIK Weight = 1
LeftHandIK Weight  = 0
```

왼손이 필요한 칼 공격 모션이 있다면 그때 애니메이션 또는 별도의 왼손 GripPoint를 추가한다. 처음부터 양손 칼 IK를 만들 필요는 없다.

---

## 12. 나중에 무기 교체 시스템이 담당할 일

IK 기반이 완성되면 무기 교체 시스템은 다음 세 가지만 처리한다.

1. 선택한 무기를 활성화한다.
2. 공용 IKTarget을 선택한 무기의 GripPoint 아래로 옮긴다.
3. 무기 종류에 맞게 오른손·왼손 IK Weight를 설정한다.

예상 동작은 다음과 같다.

```text
주무기 장착
- LeftHandIKTarget을 주무기 LeftGripPoint에 연결
- RightHandIK Weight = 0
- LeftHandIK Weight  = 1

권총 장착
- RightHandIKTarget을 권총 RightGripPoint에 연결
- LeftHandIKTarget을 권총 LeftGripPoint에 연결
- RightHandIK Weight = 1
- LeftHandIK Weight  = 1

칼 장착
- RightHandIKTarget을 칼 RightGripPoint에 연결
- RightHandIK Weight = 1
- LeftHandIK Weight  = 0
```

이때 Constraint의 Target 참조는 계속 공용 `RightHandIKTarget`, `LeftHandIKTarget`으로 유지한다. 런타임에 Constraint가 참조하는 Transform 자체를 무기별 GripPoint로 계속 교체하지 않는다.

---

## 13. 주의사항

### 무기와 손 사이에 순환 구조를 만들지 않는다

다음 구조는 피한다.

```text
RightHand
└─ HandGun
   └─ RightGripPoint
       └─ RightHandIKTarget
```

오른손이 권총을 움직이고, 오른손 IK가 다시 권총의 Target을 따라가면 순환 관계가 생긴다.

권총과 칼에서 오른손 IK를 사용할 때 무기는 `WeaponBox`처럼 손 본 바깥에 둔다.

```text
WeaponBox
└─ HandGun
   └─ RightGripPoint
       └─ RightHandIKTarget
```

### 손가락은 Two Bone IK가 처리하지 않는다

Two Bone IK는 위팔, 아래팔, 손목을 맞추는 용도다.

- 오른손 검지를 방아쇠에 올리는 동작
- 나머지 손가락으로 손잡이를 감싸는 동작
- 왼손으로 오른손을 감싸는 손가락 모양

위 작업은 권총 전용 손가락 애니메이션이나 손가락 본 회전으로 만든다.

### Target이 팔 도달 범위 밖으로 나가지 않게 한다

손이 Target에 닿지 않거나 팔이 완전히 일자로 펴지면 다음 순서로 확인한다.

1. 무기와 GripPoint가 몸에서 너무 멀지 않은지 확인한다.
2. 권총 기본 포즈에서 어깨와 위팔이 앞으로 나와 있는지 확인한다.
3. ElbowHint가 팔 반대편에 있지 않은지 확인한다.
4. Target Position Weight와 Constraint Weight가 1인지 확인한다.

---

## 14. 실제 작업 체크리스트

### 지금 해야 할 작업

- [ ] Player에 `IKTargets` 생성
- [ ] `RightHandIKTarget` 생성
- [ ] `LeftHandIKTarget` 생성
- [ ] `RightElbowHint` 생성
- [ ] 기존 `LeftElbowHint`를 팔 본 밖으로 이동
- [ ] 기존 `LeftHandIK.Target`을 `LeftHandIKTarget`으로 변경
- [ ] 기존 `LeftHandIK.Hint`를 이동한 `LeftElbowHint`로 연결
- [ ] `LeftHandIKTarget`을 주무기 `LeftGripPoint` 아래에 배치
- [ ] `LeftHandIKTarget` Local Transform 초기화
- [ ] `IKController.UpdateIK()` 호출 제거
- [ ] 왼손 IK Weight를 1로 설정하고 주무기 테스트
- [ ] 오른팔 `Two Bone IK Constraint` 생성
- [ ] 오른팔 IK Target과 Hint 연결
- [ ] 주무기 상태에서 오른손 IK Weight를 0으로 유지

### 주무기 테스트가 끝난 뒤

- [ ] 권총에 `RightGripPoint` 생성
- [ ] 권총에 `LeftGripPoint` 생성
- [ ] 권총 기본 상체 포즈 준비
- [ ] 권총 오른손 IK부터 조정
- [ ] 권총 왼손 IK 조정
- [ ] 권총 양쪽 ElbowHint 조정
- [ ] 권총 손가락 포즈 제작
- [ ] 칼에 `RightGripPoint` 생성
- [ ] 칼 오른손 IK 조정
- [ ] 마지막으로 무기 교체 시스템과 Target·Weight 전환 연결

---

## 15. 가장 짧은 요약

지금은 권총과 칼을 먼저 만들지 않는다.

1. 오른손과 왼손의 공용 IKTarget을 만든다.
2. 기존 주무기의 왼손 IK부터 공용 Target 방식으로 바꾼다.
3. 팔 본을 직접 움직이는 기존 `UpdateIK()`를 제거한다.
4. 주무기가 정상인지 확인한다.
5. 오른손 IK는 만들어 두되 주무기에서는 Weight를 0으로 둔다.
6. 그다음 권총에서는 양손 Weight를 1로 사용한다.
7. 칼에서는 오른손 Weight만 1로 사용한다.
8. 마지막에 무기 교체 시스템이 Target의 부모와 Weight만 변경하도록 만든다.


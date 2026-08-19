# 3슬롯 총기 교체 기능 구현 순서

## 구현 전제

플레이어는 무기를 최대 3개까지 동시에 보유한다.

1. 주무기(Primary): 돌격소총, 산탄총, 저격총 등
2. 보조무기(Secondary): 권총 등
3. 근접무기(Melee): 칼 등

각 슬롯에는 한 개의 무기만 장착한다. 이번 구현에서는 인벤토리 전체를 순환하는 구조보다 슬롯 번호로 원하는 무기를 바로 선택하는 구조를 우선한다.

## 전체 구조

```text
Player
└─ WeaponHolder
   ├─ PrimaryWeaponHolder
   │  └─ AssultRifle
   ├─ SecondaryWeaponHolder
   │  └─ Pistol
   └─ MeleeWeaponHolder
      └─ Knife
```

```text
InputReader
    ↓ 무기 슬롯 선택 입력
WeaponManager
    ↓ 기존 무기 비활성화 / 선택 무기 활성화
Primary / Secondary / Melee 무기 스크립트
```

`WeaponManager`는 어떤 슬롯이 현재 장착되어 있는지 관리한다. 발사, 탄약, 재장전, 근접 공격 방식은 각 무기 스크립트가 담당한다.

## 1. 무기 슬롯 타입 만들기

숫자 인덱스만 사용하면 각 숫자의 의미를 알아보기 어려우므로 슬롯을 enum으로 정의한다.

```csharp
public enum WeaponSlot
{
    Primary = 0,
    Secondary = 1,
    Melee = 2
}
```

배열 순서는 반드시 주무기, 보조무기, 근접무기 순으로 유지한다.

## 2. 가장 바깥쪽에 WeaponManager 만들기

기존 `AssultRifle` 코드를 바로 옮기지 않고, 먼저 세 무기의 활성화만 관리하는 스크립트를 만든다.

```csharp
using UnityEngine;

namespace MIssionOfMercenary
{
    public enum WeaponSlot
    {
        Primary = 0,
        Secondary = 1,
        Melee = 2
    }

    public class WeaponManager : MonoBehaviour
    {
        [Header("Weapon Slots")]
        [SerializeField] GameObject _primaryWeapon;
        [SerializeField] GameObject _secondaryWeapon;
        [SerializeField] GameObject _meleeWeapon;

        GameObject[] _weapons;
        WeaponSlot _currentSlot;

        public WeaponSlot CurrentSlot => _currentSlot;
        public GameObject CurrentWeapon => _weapons[(int)_currentSlot];

        private void Awake()
        {
            _weapons = new GameObject[]
            {
                _primaryWeapon,
                _secondaryWeapon,
                _meleeWeapon
            };
        }

        private void Start()
        {
            EquipWeapon(WeaponSlot.Primary);
        }

        public void EquipWeapon(WeaponSlot slot)
        {
            int selectedIndex = (int)slot;

            if (_weapons[selectedIndex] == null)
            {
                Debug.LogWarning($"{slot} 슬롯에 무기가 없습니다.");
                return;
            }

            for (int i = 0; i < _weapons.Length; i++)
            {
                if (_weapons[i] != null)
                {
                    _weapons[i].SetActive(i == selectedIndex);
                }
            }

            _currentSlot = slot;
        }
    }
}
```

초기 단계에서는 필드 3개를 명시적으로 두는 방식이 인스펙터 연결 실수를 찾기 쉽다. 나중에 무기 종류와 슬롯 수가 늘어나면 슬롯 데이터 클래스나 배열 구조로 확장한다.

## 3. 입력 없이 슬롯별 활성화부터 테스트하기

`Start()`에서 다음 값을 하나씩 넣어본다.

```csharp
EquipWeapon(WeaponSlot.Primary);
EquipWeapon(WeaponSlot.Secondary);
EquipWeapon(WeaponSlot.Melee);
```

확인 사항:

- 선택한 슬롯의 무기 하나만 활성화되는가
- 다른 두 무기는 비활성화되는가
- 빈 슬롯을 선택했을 때 현재 무기가 사라지지 않는가
- 무기를 다시 선택했을 때 입력 이벤트가 중복 실행되지 않는가

## 4. 기존 무기의 이벤트 구독과 해제 확인하기

현재 구조를 보존한다면 각 무기는 활성화될 때 `InputReader` 이벤트를 구독하고, 비활성화될 때 반드시 전부 해제해야 한다.

```csharp
void OnEnable()
{
    _inputReader.OnshotEvent += HandleShot;
    _inputReader.OnShotCancled += HandleShotCancled;
    _inputReader.OnReloadEvent += HandledReload;
}

void OnDisable()
{
    _inputReader.OnshotEvent -= HandleShot;
    _inputReader.OnShotCancled -= HandleShotCancled;
    _inputReader.OnReloadEvent -= HandledReload;
}
```

비활성화된 무기가 입력을 계속 받는다면 이벤트 해제가 빠졌는지 먼저 확인한다.

근접무기는 총기와 입력이 다를 수 있으므로 억지로 발사 및 재장전 이벤트를 모두 구현하지 않는다. 공통으로 필요한 기능만 이후 인터페이스로 분리한다.

## 5. InputReader에 3개 슬롯 입력 추가하기

가장 단순하고 명확한 방식은 숫자 키로 각 슬롯을 직접 선택하는 것이다.

```text
1 키 → Primary
2 키 → Secondary
3 키 → Melee
```

`InputReader`에서는 슬롯 선택 결과를 하나의 이벤트로 전달할 수 있다.

```csharp
public event Action<int> OnWeaponSelectEvent;
```

각 입력 콜백에서 `0`, `1`, `2`를 전달하거나 `Action<WeaponSlot>`을 사용해 enum을 직접 전달한다.

```csharp
public event Action<WeaponSlot> OnWeaponSelectEvent;
```

enum을 `InputReader`에서 참조하게 만들고 싶지 않다면 `int`를 전달하고 `WeaponManager`에서 enum으로 변환해도 된다.

## 6. WeaponManager에서 교체 입력 받기

```csharp
private void OnEnable()
{
    _inputReader.OnWeaponSelectEvent += EquipWeapon;
}

private void OnDisable()
{
    _inputReader.OnWeaponSelectEvent -= EquipWeapon;
}
```

현재 들고 있는 슬롯을 다시 선택하면 교체하지 않도록 검사한다.

```csharp
if (_currentSlot == slot)
{
    return;
}
```

## 7. 마우스 휠 교체는 그다음에 추가하기

숫자 키 교체가 정상 작동한 뒤 필요하면 마우스 휠로 다음/이전 슬롯을 선택한다.

```csharp
void EquipNextWeapon()
{
    int nextIndex = ((int)_currentSlot + 1) % 3;
    EquipWeapon((WeaponSlot)nextIndex);
}

void EquipPreviousWeapon()
{
    int previousIndex = ((int)_currentSlot - 1 + 3) % 3;
    EquipWeapon((WeaponSlot)previousIndex);
}
```

빈 슬롯을 허용할 예정이라면 한 번만 계산하지 말고, 최대 3개 슬롯을 순회하며 실제 무기가 있는 다음 슬롯을 찾아야 한다.

## 8. 교체 중 상태와 코루틴 정리하기

기본 교체가 정상 작동한 후 다음 상태를 추가한다.

```csharp
public bool IsSwitchingWeapon { get; private set; }
```

교체를 시작할 때 기존 무기의 동작을 정리한다.

- 연사 코루틴 중지
- 재장전 코루틴 중지 또는 재장전 상태 취소
- 발사 차단
- 조준 해제
- 기존 무기 내리기 애니메이션
- 선택 무기 꺼내기 애니메이션

암살은 별도 키를 사용하므로 캐스트끼리 충돌하지 않는다. 다만 교체 애니메이션 도중 암살을 허용할지는 게임 규칙에 따라 상태값으로 결정한다.

## 9. 무기별로 유지해야 하는 상태

각 무기의 탄약은 무기를 교체해도 보존되어야 한다.

예시:

```text
주무기 17/30 상태에서 보조무기로 교체
→ 다시 주무기로 돌아왔을 때 17/30 유지
```

따라서 탄약을 `WeaponManager`에서 하나의 값으로 관리하지 않고 각 총기 인스턴스가 자신의 탄약을 보관하게 한다.

슬롯이 담당하는 정보:

- 현재 슬롯
- 현재 활성 무기
- 교체 가능 여부
- 교체 애니메이션 상태

개별 무기가 담당하는 정보:

- 현재 탄약과 최대 탄약
- 공격력과 사거리
- 연사 또는 단발 방식
- 재장전
- 무기별 공격 구현

## 10. 공통 코드는 마지막에 분리하기

주무기, 보조무기, 근접무기가 모두 동작한 뒤 실제 중복 코드를 확인하고 공통 인터페이스 또는 부모 클래스를 만든다.

```csharp
public interface IWeapon
{
    void OnEquip();
    void OnUnequip();
}
```

총기 전용 발사와 재장전을 근접무기까지 강제로 구현하게 만들 필요는 없다. 필요하다면 총기용 인터페이스를 별도로 둔다.

```csharp
public interface IFirearm : IWeapon
{
    void Attack(float input);
    void Reload();
}
```

## 권장 구현 순서 요약

```text
WeaponSlot enum 생성
→ WeaponManager에 주무기/보조무기/근접무기 슬롯 생성
→ 세 무기를 플레이어 WeaponHolder 아래에 배치
→ 입력 없이 슬롯별 SetActive 테스트
→ 기존 무기의 이벤트 구독/해제 점검
→ InputReader에 1/2/3 슬롯 입력 추가
→ WeaponManager에 교체 입력 연결
→ 마우스 휠 교체 추가
→ 교체 중 발사/재장전/조준 상태 정리
→ 교체 애니메이션 추가
→ 실제 중복 코드만 인터페이스 또는 부모 클래스로 분리
```

## 맵 레벨링과 함께 확인할 테스트 구역

- 근거리 구역: 근접무기와 암살 테스트
- 중거리 구역: 주무기 교전 테스트
- 좁은 실내: 보조무기 전환 테스트
- 엄폐 구역: 재장전 중 교체 및 조준 해제 테스트
- 고저차 구역: 카메라와 총구의 2단 Raycast 테스트
- 연속 교전 구역: 세 슬롯을 빠르게 교체할 때 이벤트 중복 여부 테스트


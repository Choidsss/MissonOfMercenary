# 무기 픽업 기능 구현 가이드

## 1. 첫 번째 구현 목표

처음부터 모든 무기와 드롭 기능을 완성하려고 하지 말고 아래 동작부터 완성한다.

> 바닥의 권총을 바라보면 픽업 UI가 나타나고, E 키를 누르면 Secondary 슬롯의 무기가 교체된다.

이 동작이 완성되면 Primary와 Melee 슬롯에도 같은 구조를 적용할 수 있다.

## 2. 필요한 역할 분리

무기 픽업 기능은 다음 세 부분으로 나누는 것이 좋다.

1. `DroppedWeapon`
   - 바닥에 떨어진 무기의 정보를 보관한다.
   - 무기 슬롯, 장착용 프리팹, 표시 이름을 가진다.
2. `WeaponPickup`
   - 플레이어가 바라보는 바닥 무기를 찾는다.
   - 픽업 입력을 받으면 `WeaponManager`에 교체를 요청한다.
3. `WeaponManager`
   - 플레이어의 실제 무기 슬롯을 관리한다.
   - 기존 무기와 새 무기를 교체하고 IK와 UI를 갱신한다.

`PickupWeaponUI`는 현재 발견한 무기 정보를 표시하는 역할만 담당한다.

## 3. 바닥 무기 정보 컴포넌트 만들기

새 파일 예시:

`Assets/Scripts/Player/Guns/DroppedWeapon.cs`

```csharp
using UnityEngine;

namespace MIssionOfMercenary
{
    public class DroppedWeapon : MonoBehaviour
    {
        [SerializeField] WeaponSlot _slot;
        [SerializeField] GameObject _equippedWeaponPrefab;
        [SerializeField] string _displayName;

        public WeaponSlot Slot => _slot;
        public GameObject EquippedWeaponPrefab => _equippedWeaponPrefab;
        public string DisplayName => _displayName;
    }
}
```

바닥 무기 오브젝트에는 다음 항목을 설정한다.

- `Slot`: 들어갈 슬롯
- `Equipped Weapon Prefab`: 플레이어 손에 생성할 장착용 무기 프리팹
- `Display Name`: UI에 표시할 무기 이름
- Pickup 전용 Layer
- Collider 또는 자식 `PickupTrigger`

바닥용 오브젝트와 장착용 프리팹을 분리하면 Rigidbody, Collider, 발사 스크립트 및 IK 설정을 관리하기 쉽다.

## 4. 플레이어가 바라보는 무기 찾기

현재 `WeaponPickup`의 `OverlapSphere`와 `Raycast`는 각각 무기의 존재 여부만 반환한다. 실제로 어떤 무기를 발견했는지 저장하도록 변경해야 한다.

`SphereCast` 하나로 가까운 범위와 바라보는 방향을 함께 검사할 수 있다.

```csharp
[SerializeField] WeaponManager _weaponManager;

DroppedWeapon _targetWeapon;

public bool CanPickup => _targetWeapon != null;
public DroppedWeapon TargetWeapon => _targetWeapon;

void Update()
{
    UpdateTarget();
}

void UpdateTarget()
{
    _targetWeapon = null;

    Vector3 origin = transform.position
                   + transform.right * _offsetX
                   + transform.forward * _offsetZ;

    bool hitWeapon = Physics.SphereCast(
        origin,
        _radius,
        transform.forward,
        out RaycastHit hit,
        _maxDistance,
        _layer,
        QueryTriggerInteraction.Collide);

    if (!hitWeapon)
        return;

    _targetWeapon = hit.collider.GetComponentInParent<DroppedWeapon>();
}
```

플레이어 루트의 `transform.forward`가 카메라가 보는 방향과 다르다면, 카메라 또는 별도의 탐색 기준 Transform을 직렬화해서 그 위치와 방향을 사용한다.

```csharp
[SerializeField] Transform _pickupRayOrigin;
```

## 5. 기존 감지 코드의 문제점

현재 코드에는 아래 구조가 있다.

```csharp
foreach (Collider col in colliders)
{
    if (colliders.Length == 0)
```

`colliders.Length`가 0이면 `foreach`가 시작되지 않으므로 내부 조건은 실행될 수 없다.

또한 현재 Raycast는 충돌 여부만 받아서 어떤 오브젝트가 맞았는지 알 수 없다. `RaycastHit` 또는 `SphereCast`의 `RaycastHit`에서 `DroppedWeapon`을 가져와야 한다.

## 6. Pickup 입력 추가하기

`MOMInputAction.inputactions` 또는 실제 플레이어가 사용하는 Input Action Asset에 `Pickup` 액션을 추가하고 E 키를 연결한다.

`InputReader`에는 다음 필드를 추가한다.

```csharp
InputAction _pickupAction;

public event Action OnPickupAction;
```

`OnEnable()`에서 액션을 찾아 활성화하고 이벤트를 등록한다.

```csharp
_pickupAction = _inputActionAsset.FindAction("Pickup");
_pickupAction.Enable();
_pickupAction.performed += PickupActionCallback;
```

`OnDisable()`에서는 반대로 해제한다.

```csharp
_pickupAction.performed -= PickupActionCallback;
_pickupAction.Disable();
```

콜백 함수:

```csharp
void PickupActionCallback(InputAction.CallbackContext context)
{
    OnPickupAction?.Invoke();
}
```

`WeaponPickup`은 이벤트를 구독한다.

```csharp
void OnEnable()
{
    _inputReader.OnPickupAction += TryPickup;
}

void OnDisable()
{
    _inputReader.OnPickupAction -= TryPickup;
}
```

## 7. WeaponManager에 슬롯 교체 기능 추가하기

현재 `WeaponManager`의 `_weapons` 배열은 `Awake()`에서 생성된 후 변경할 방법이 없다. 외부에서는 슬롯 교체 메서드만 호출하도록 만든다.

초기 형태의 예시:

```csharp
public void ReplaceWeapon(WeaponSlot slot, GameObject newWeaponPrefab)
{
    if (newWeaponPrefab == null)
        return;

    int index = (int)slot;
    GameObject oldWeapon = _weapons[index];

    Transform weaponParent = oldWeapon != null
        ? oldWeapon.transform.parent
        : transform;

    GameObject newWeapon = Instantiate(newWeaponPrefab, weaponParent);
    newWeapon.transform.SetLocalPositionAndRotation(
        Vector3.zero,
        Quaternion.identity);

    _weapons[index] = newWeapon;

    if (oldWeapon != null)
        Destroy(oldWeapon);

    EquipWeapon(slot);
}
```

이 코드는 우선 교체 기능을 확인하기 위한 최소 형태다. 무기마다 장착 위치가 다르다면 프리팹의 로컬 Transform을 유지하거나 별도의 무기 장착 지점을 사용해야 한다.

## 8. 실제 픽업 실행하기

`WeaponPickup`에 다음 흐름을 추가한다.

```csharp
void TryPickup()
{
    if (_targetWeapon == null)
        return;

    DroppedWeapon pickedWeapon = _targetWeapon;
    _targetWeapon = null;

    _weaponManager.ReplaceWeapon(
        pickedWeapon.Slot,
        pickedWeapon.EquippedWeaponPrefab);

    Destroy(pickedWeapon.gameObject);
}
```

처음에는 기존 무기를 바닥에 떨어뜨리지 않고 제거해도 된다. 교체와 IK 갱신이 정상 동작하는 것을 확인한 후 기존 무기의 드롭 기능을 추가한다.

## 9. Pickup UI 연결

UI에는 `WeaponPickup.CanPickup`만 사용하는 것보다 현재 타깃 정보도 제공하는 편이 좋다.

```csharp
void ShowPickupText()
{
    DroppedWeapon target = _player.TargetWeapon;

    if (target == null)
    {
        _pickupPanel.SetActive(false);
        return;
    }

    _text.text = $"Press 'E' to pick up {target.DisplayName}";
    _pickupPanel.SetActive(true);
}
```

현재 `PickupWeaponUI`의 `gameObject.name`은 UI 오브젝트의 이름이므로 실제 바닥 무기 이름으로 사용할 수 없다.

## 10. 기존 무기를 바닥에 떨어뜨리는 기능

기본 교체가 완성된 다음 구현한다.

추천 방식은 각 장착 무기에 대응하는 바닥용 프리팹 정보를 두는 것이다.

예시 흐름:

1. 새 무기를 줍기 전에 현재 슬롯의 무기 정보를 저장한다.
2. 플레이어 앞이나 발 근처에 기존 무기의 바닥용 프리팹을 생성한다.
3. 생성된 바닥 무기의 Rigidbody를 활성화한다.
4. 장착 중인 기존 무기를 제거한다.
5. 새 장착용 프리팹을 무기 부모 아래에 생성한다.
6. `_weapons` 배열을 갱신한다.
7. `EquipWeapon(slot)`을 호출해 IK와 UI를 갱신한다.

장착용 무기와 바닥용 무기를 서로 연결하려면 나중에 `WeaponDefinition` ScriptableObject를 만드는 방법이 좋다.

```text
WeaponDefinition
├─ 표시 이름
├─ WeaponSlot
├─ 장착용 프리팹
└─ 바닥용 프리팹
```

무기 종류가 많아지기 전에는 `DroppedWeapon`의 필드만으로 시작해도 충분하다.

## 11. 구현 순서 체크리스트

- [ ] `DroppedWeapon` 컴포넌트 생성
- [ ] 권총 바닥 오브젝트에 `DroppedWeapon`, Layer, Collider 설정
- [ ] `WeaponPickup`에서 타깃 `DroppedWeapon` 검출
- [ ] 발견한 무기 이름을 Pickup UI에 표시
- [ ] Input Action Asset에 `Pickup` 액션과 E 키 추가
- [ ] `InputReader.OnPickupAction` 연결
- [ ] `WeaponManager.ReplaceWeapon()` 구현
- [ ] E 키로 Secondary 슬롯 교체 확인
- [ ] 교체 후 발사, 탄약 UI, 무기 전환 확인
- [ ] 교체 후 오른손/왼손 IK 확인
- [ ] 기존 무기를 바닥에 생성하는 드롭 기능 추가
- [ ] Primary와 Melee 슬롯으로 확장

## 12. 테스트할 항목

- 무기가 없을 때 E를 눌러도 오류가 발생하지 않는가?
- 여러 무기가 가까이 있을 때 바라보는 무기만 선택되는가?
- Collider가 자식 `PickupTrigger`에 있어도 부모의 `DroppedWeapon`을 찾는가?
- 무기 교체 후 숫자 키로 슬롯 전환이 되는가?
- 교체 직후 무기 UI의 종류와 탄약이 갱신되는가?
- 양손 무기와 한손 무기의 IK가 각각 올바르게 적용되는가?
- 주운 바닥 무기가 씬에서 제거되는가?
- 기존 무기를 드롭한 뒤 즉시 다시 줍는 과정에서 중복 픽업이 발생하지 않는가?

## 13. 현재 코드에서 함께 확인할 부분

`WeaponManager.EquipWeapon()`의 아래 Debug 코드는 `CurrentWeaponIKData`가 null인 무기에서 NullReferenceException을 일으킬 수 있다.

```csharp
Debug.Log($"오른손 그립: {CurrentWeaponIKData.RightGripPoint.name}");
```

안전하게 사용하려면 null 검사를 통과한 블록 안으로 옮긴다.

또한 `WeaponUI`는 `_currentWeapon`이 지정되기 전에 `Update()`가 실행되면 오류가 날 수 있으므로 null 검사를 추가하는 편이 안전하다.

```csharp
if (_currentWeapon == null)
    return;
```

이 두 부분은 픽업 후 새 프리팹의 컴포넌트나 IK 데이터가 빠졌을 때 오류 원인을 찾는 데도 도움이 된다.

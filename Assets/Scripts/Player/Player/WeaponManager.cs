using MissionOfMercenary;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Animations;
using System.Collections.Generic;

namespace MIssionOfMercenary
{
    //무기의 종류를 열거로 나눠놓음
    public enum WeaponSlot
    {
        Primary = 0,
        Secondary = 1,
        Melee = 2
    }

    public class WeaponManager : MonoBehaviour
    {
        IFirearm _weapon;
        Knife _knife;

        [SerializeField] PlayerWeaponContext _weaponContext;
        [SerializeField] Transform _weaponMount;

        [Header("Equip Motion")]
        [SerializeField] WeaponStartEquipMotion _equipMotion;

        [SerializeField] WeaponIKController _weaponIKController;

        [Header("InputReader Asset")]
        [SerializeField] InputReader _inputReader;

        [Header("UI Changed")]
        [SerializeField] WeaponUI _weaponUI;

        [Header("Weapon Slot")]
        [SerializeField] GameObject _primaryWeapon; //주무기
        [SerializeField] GameObject _secondaryWeapon; //보조무기
        [SerializeField] GameObject _meleeWeapon; //근접무기

        GameObject[] _weapons; //내가 들고있는 무기
        WeaponSlot _currentSlot; //현재 슬롯
        GameObject _equippedObject;

        public Knife WeaponKnife { get { return _knife; } }
        public IFirearm Weapon { get { return _weapon; } }
        public WeaponSlot CurrentSlot { get { return _currentSlot; } }
        public GameObject CurrentWeapon => _weapons[(int)_currentSlot];

        public bool IsEquipping =>
            _equipMotion != null && _equipMotion.IsEquipping;

        public WeaponIKData CurrentWeaponIKData
        {
            get
            { 
                if (CurrentWeapon == null) return null; 

                return CurrentWeapon.GetComponentInChildren<WeaponIKData>(true); 
            }
        }

        private void OnEnable()
        {
            _inputReader.OnEquipPrimaryAction += EquipPrimary;
            _inputReader.OnEquipSecondaryAction += EquipSecondary;
            _inputReader.OnEquipMeleeAction += EquipMelee;
        }

        private void OnDisable()
        {
            _inputReader.OnEquipPrimaryAction -= EquipPrimary;
            _inputReader.OnEquipSecondaryAction -= EquipSecondary;
            _inputReader.OnEquipMeleeAction -= EquipMelee;
        }


        private void Awake()
        {
            _weapons = new GameObject[] { _primaryWeapon, _secondaryWeapon, _meleeWeapon }; 
        }

        void Start()
        {
            EquipWeapon(WeaponSlot.Primary);
        }

        public void EquipWeapon(WeaponSlot slot)
        {
            int selectedSlot = (int)slot;

            if (_weapons == null ||
                selectedSlot < 0 ||
                selectedSlot >= _weapons.Length)
            {
                return;
            }

            GameObject selectedWeapon = _weapons[selectedSlot];

            if (selectedWeapon == null)
            {
                Debug.Log("슬롯에 무기가 장착되어있지 않습니다.");
                return;
            }

            if (_equippedObject == selectedWeapon && selectedWeapon.activeSelf)
            {
                return;
            }

            _weapon?.TriggeredReleased();

            if (_equippedObject != null)
            {
                _equippedObject.SetActive(false);
            }

            for (int i = 0; i < _weapons.Length; i++)
            {
                if (_weapons[i] != null)
                {
                    _weapons[i].SetActive(false);
                }
            }

            _equipMotion?.Cancel();

            _currentSlot = slot;
            _equippedObject = selectedWeapon;
            selectedWeapon.SetActive(true);

            _weapon = selectedWeapon.GetComponent<IFirearm>();
            _knife = selectedWeapon.GetComponent<Knife>();

            IWeapons currentWeaponInterface =
                selectedWeapon.GetComponentInParent<IWeapons>();

            _weaponUI.GetCurrentWeaponType(currentWeaponInterface);

            WeaponRecoil recoil =
                selectedWeapon.GetComponentInChildren<WeaponRecoil>(true);

            recoil?.InitializeRecoil();

            _equipMotion?.Play(slot);
            _weaponIKController.BlindWeapon(CurrentWeaponIKData);

            //int selectedSlot = (int)slot;

            //if (_weapons[selectedSlot] == null) { Debug.Log("슬롯에 무기가 장착되어있지 않습니다."); return; }

            //for(int i = 0;i < _weapons.Length; i++)
            //{
            //    if(_weapons[i] == null) { continue; }

            //    _weapons[i].SetActive(i == selectedSlot);

            //}

            //_currentSlot = slot;

            //_weapon = CurrentWeapon.GetComponent<IFirearm>();
            //_knife = CurrentWeapon.GetComponent<Knife>();

            //IWeapons currentWeaponInterface = CurrentWeapon.GetComponentInParent<IWeapons>();
            //_weaponUI.GetCurrentWeaponType(currentWeaponInterface);

            //WeaponIKData weaponIKData = CurrentWeaponIKData;

            //_weaponIKController.BlindWeapon(weaponIKData);

            //if (weaponIKData != null && weaponIKData.RightGripPoint != null)
            //{
            //    Debug.Log($"Current Right Grip: {weaponIKData.RightGripPoint.name}"); 
            //}

            //WeaponRecoil recoil = CurrentWeapon.GetComponentInChildren<WeaponRecoil>(true);
        }

        public void EquipPrimary()
        {
            EquipWeapon(WeaponSlot.Primary);
        }
        public void EquipSecondary()
        {
            EquipWeapon(WeaponSlot.Secondary);
        }

        public void EquipMelee()
        {
            EquipWeapon(WeaponSlot.Melee);
        }

        // 변경: 외부에서는 TryReplacedWeapon을 통해서만 교체를 요청하고, 실제 처리 결과를 bool로 반환한다.
        // 새 무기를 비활성 상태로 생성한 뒤 Player 참조를 연결한다.
        // 연결 실패 시 새 인스턴스만 제거하며 기존 슬롯과 무기는 유지한다.
        // 연결 성공 후에는 기존 드롭 처리를 유지하고 새 무기를 장착한다. By Codex
        bool ReplacedWeapon(WeaponSlot slot, GameObject newWeaponPrefab, Vector3 dropPosition, Quaternion dropRotation)
        {
            if (newWeaponPrefab == null || _weaponMount == null || _weaponContext == null)
            {
                return false;
            }

            int index = (int)slot;
            if (_weapons == null || index < 0 || index >= _weapons.Length)
            {
                return false;
            }

            GameObject oldWeapon = _weapons[index];

            GameObject newWeapon = CreateInActiveWeapon(newWeaponPrefab);
            if (!BindWeaponToPlayer(newWeapon))
            {
                Destroy(newWeapon);
                return false;
            }

            DroppedWeapons newDroppedWeapon = newWeapon.GetComponentInChildren<DroppedWeapons>(true);
            newDroppedWeapon?.SetEquippedState(); 

            _weapons[index] = newWeapon;

            if(oldWeapon != null)
            {
                EquippedWeaponDropData dropData = oldWeapon.GetComponentInChildren<EquippedWeaponDropData>(true);

                if(dropData != null && dropData.DroppedWeaponPrefab != null)
                {
                    Instantiate(dropData.DroppedWeaponPrefab, dropPosition, dropRotation);
                }
                else
                {
                    DroppedWeapons oldDroppedData = oldWeapon.GetComponentInChildren<DroppedWeapons>(true);

                    if(oldDroppedData != null)
                    {
                        GameObject droppedClone = Instantiate(oldWeapon, dropPosition, dropRotation);
                        DroppedWeapons droppedCloneData = droppedClone.GetComponentInChildren<DroppedWeapons>(true);
                        droppedCloneData.SetDroppedState();
                    }
                    else
                    {
                        Debug.LogWarning($"{oldWeapon.name}: DroppedWeapons 또는 Dropped Weapon Prefab이 없습니다.", oldWeapon); 
                    }
                }

                Destroy(oldWeapon);
            }

            EquipWeapon(slot);
            return true;
        }

        // 변경: Player 참조, 장착 부모, 슬롯 범위와 무기 구성을 생성 전에 검사한다.
        // 검사를 통과하면 실제 교체 결과를 그대로 반환한다.
        // 픽업 호출부는 true일 때만 바닥 무기를 제거한다. By Codex
        public bool TryReplacedWeapon(WeaponSlot slot, GameObject newWeaponPrefab, Vector3 droppedPosition, Quaternion dropRotation)
        {
            int index = (int)slot;

            if (newWeaponPrefab == null || _weaponMount == null || _weaponContext == null)
            {
                return false;
            }

            if (_weapons == null || index < 0 || index >= _weapons.Length)
            {
                return false;
            }

            bool hasWeapon = newWeaponPrefab.GetComponent<IFirearm>() != null || newWeaponPrefab.GetComponent<Knife>() != null;

            if(!hasWeapon || newWeaponPrefab.GetComponentInChildren<WeaponIKData>(true) == null)
            {
                return false;
            }

            return ReplacedWeapon(slot, newWeaponPrefab, droppedPosition, dropRotation);
        }

        bool BindWeaponToPlayer(GameObject weapon)
        {
            if (weapon == null || _weaponContext == null)
            {
                return false;
            }

            // 초기화 대기 중인 비활성 무기의 자식도 모두 연결한다. By Codex
            MonoBehaviour[] components = weapon.GetComponentsInChildren<MonoBehaviour>(true);

            foreach (MonoBehaviour component in components)
            {
                if (component is IPlayerWeaponBindable bindable)
                {
                    if (!bindable.BindPlayer(_weaponContext))
                    {
                        Debug.Log("초기화 실패");
                        return false;
                    }
                }
            }

            return true;
        }

        GameObject CreateInActiveWeapon(GameObject prefab)
        {
            GameObject staging = new GameObject("WeaponInitialization");
            staging.SetActive(false);
            staging.transform.SetParent(_weaponMount, false);

            GameObject weapon = Instantiate(prefab, staging.transform);

            weapon.SetActive(false);
            weapon.transform.SetParent(_weaponMount, false);
            weapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            Destroy(staging);

            return weapon;
        }
    }
}

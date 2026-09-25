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

            // 같은 무기를 다시 선택하면 장착 모션을 재시작하지 않는다. By Codex
            if (_equippedObject == selectedWeapon && selectedWeapon.activeSelf)
            {
                return;
            }

            // 이전 무기의 연사를 중단한다. By Codex
            _weapon?.TriggeredReleased();

            // OnDisable에서 반동, 재장전, 칼 공격을 정리한다. By Codex
            if (_equippedObject != null)
            {
                _equippedObject.SetActive(false);
            }

            // 처음 시작할 때 활성화되어 있던 무기도 정리한다. By Codex
            for (int i = 0; i < _weapons.Length; i++)
            {
                if (_weapons[i] != null)
                {
                    _weapons[i].SetActive(false);
                }
            }

            // 이전 무기의 정리가 끝난 후 공유 Pivot을 기본 자세로 복구한다. By Codex
            _equipMotion?.Cancel();

            _currentSlot = slot;
            _equippedObject = selectedWeapon;
            selectedWeapon.SetActive(true);

            _weapon = selectedWeapon.GetComponent<IFirearm>();
            _knife = selectedWeapon.GetComponent<Knife>();

            IWeapons currentWeaponInterface =
                selectedWeapon.GetComponentInParent<IWeapons>();

            _weaponUI.GetCurrentWeaponType(currentWeaponInterface);

            // Pivot이 기본 자세일 때 반동의 기준을 먼저 저장한다. By Codex
            WeaponRecoil recoil =
                selectedWeapon.GetComponentInChildren<WeaponRecoil>(true);

            recoil?.InitializeRecoil();

            // 준비 자세를 적용한 다음 손의 IK를 연결한다. By Codex
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

        public void ReplacedWeapon(WeaponSlot slot, GameObject newWeaponPrefab, Vector3 dropPosition, Quaternion dropRotation)
        {
            if(newWeaponPrefab == null) { return; }

            int index = (int)slot;
            GameObject oldWeapon = _weapons[index];

            Transform weaponParent = oldWeapon != null ? oldWeapon.transform.parent : transform;

            GameObject newWeapon = Instantiate(newWeaponPrefab, weaponParent);
            newWeapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            DroppedWeapons newDroppedWeapon = newWeapon.GetComponentInChildren<DroppedWeapons>(true);
            newDroppedWeapon?.SetEquippedState(); // 바닥용 물리와 Pickup Trigger를 끄고 총기 스크립트를 활성화합니다. By_Codex

            _weapons[index] = newWeapon;

            if(oldWeapon != null)
            {
                EquippedWeaponDropData dropData = oldWeapon.GetComponentInChildren<EquippedWeaponDropData>(true);

                if(dropData != null && dropData.DroppedWeaponPrefab != null)
                {
                    Instantiate(dropData.DroppedWeaponPrefab, dropPosition, dropRotation); // 기존 무기의 바닥용 프리팹을 생성합니다. By_Codex
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
        }
    }
}

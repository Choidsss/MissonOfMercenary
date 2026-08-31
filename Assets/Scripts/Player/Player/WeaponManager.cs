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

    /*
     * 숫자버튼을 누르면 맞는 장비를 장착하도록 하는 클래스
     * 아직은 오직 맞는무기를 장착하도록만 하며, 무기를 바꾸는 기능은 아직 만들지 않음
     */
    public class WeaponManager : MonoBehaviour
    {
        [Header("InputReader Asset")]
        [SerializeField] InputReader _inputReader;

        [Header("UI Changed")]
        [SerializeField] WeaponUI _weaponUI;

        [Header("Weapon Slot")]
        [SerializeField] GameObject _primaryWeapon; //주무기
        [SerializeField] GameObject _secondaryWeapon; //보조무기
        [SerializeField] GameObject _meleeWeapon; //근접무기

        [Header("Parent Constraint")]
        [SerializeField] ParentConstraint _leftHandTargetConstraint;
        [SerializeField] ParentConstraint _rightHandTargetConstraint;

        GameObject[] _weapons; //내가 들고있는 무기
        WeaponSlot _currentSlot; //현재 슬롯

        public WeaponSlot CurrentSlot => _currentSlot; // 프로퍼티로 현재슬롯 반환
        public GameObject CurrentWeapon => _weapons[(int)_currentSlot];

        public WeaponIKData CurrentWeaponIKData
        {
            get
            { 
                if (CurrentWeapon == null) return null; 

                return CurrentWeapon.GetComponent<WeaponIKData>();
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
            _weapons = new GameObject[] { _primaryWeapon, _secondaryWeapon, _meleeWeapon }; // 내가 들고있는 무기칸 생성(칸만 생성)
        }

        void Start()
        {
            EquipWeapon(WeaponSlot.Primary);
        }

        public void EquipWeapon(WeaponSlot slot)
        {
            int selectedSlot = (int)slot;

            if (_weapons[selectedSlot] == null) { Debug.Log("슬롯에 무기가 장착되어있지 않습니다."); return; }

            for(int i = 0;i < _weapons.Length; i++)
            {
                if(_weapons[i] == null) { continue; }

                //코드 확인, 아래 주석이랑 같은 코드를 이렇게 쓴거 같은데
                _weapons[i].SetActive(i == selectedSlot);


                //if (i == selectedSlot)
                //{
                //    _weapons[i].SetActive(true);
                //}
            }

            
            _currentSlot = slot;

            IWeapons currentWeaponInterface = CurrentWeapon.GetComponentInParent<IWeapons>();
            _weaponUI.GetCurrentWeaponType(currentWeaponInterface);

            WeaponIKData weaponIKData = CurrentWeaponIKData;

            if(weaponIKData != null)
            {
                SetConstraintData(_rightHandTargetConstraint, weaponIKData.RightGripPoint, true);
                SetConstraintData(_leftHandTargetConstraint, weaponIKData.LeftGripPoint, weaponIKData.UseLeftHandIK);
            }

            Debug.Log($"오른손 그립: {CurrentWeaponIKData.RightGripPoint.name}");

            //Debug.Log($"장착 무기: {CurrentWeapon.name}");
            //Debug.Log($"오른손 그립: {CurrentWeaponIKData.RightGripPoint.name}");
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

        void SetConstraintData(ParentConstraint constraint, Transform newSource, bool shouldUseConstraint)
        {
            if (constraint == null) { Debug.Log("Does Not Exist ParentConstraint!"); return; }

            // 한 손 무기이거나 필요한 GripPoint가 없으면 이 손의 IK 추적을 끈다.
            if (!shouldUseConstraint || newSource == null)
            {
                constraint.weight = 0f;
                return;
            }

            var sources = new List<ConstraintSource>
            {
                new ConstraintSource
                {
                    sourceTransform = newSource,
                    weight = 1f
                }
            };

            constraint.SetSources(sources);
            constraint.weight = 1f;
        }

        public void ReplacedWeapon(WeaponSlot slot, GameObject newWeaponPrefab)
        {
            if(newWeaponPrefab == null) { return; }

            int index = (int)slot;
            GameObject oldWeapon = _weapons[index];

            Transform weaponParent = oldWeapon != null ? oldWeapon.transform.parent : transform;

            GameObject newWeapon = Instantiate(newWeaponPrefab, weaponParent);
            newWeapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            _weapons[index] = newWeapon;

            if(oldWeapon != null) { Destroy(oldWeapon); }

            EquipWeapon(slot);
        }
    }
}

using UnityEngine;

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

        [Header("Weapon Slot")]
        [SerializeField] GameObject _primaryWeapon; //주무기
        [SerializeField] GameObject _secondaryWeapon; //보조무기
        [SerializeField] GameObject _meleeWeapon; //근접무기

        GameObject[] _weapons; //내가 들고있는 무기
        WeaponSlot _currentSlot; //현재 슬롯

        public WeaponSlot CurrentSlot => _currentSlot; // 프로퍼티로 현재슬롯 반환
        public GameObject CurrentWeapon => _weapons[(int)_currentSlot]; //얘가 좀 헷갈리네.......

        int _currentindex;

        private void Awake()
        {
            _weapons = new GameObject[] { _primaryWeapon, _secondaryWeapon, _meleeWeapon }; // 내가 들고있는 무기칸 생성(칸만 생성)
        }

        void Start()
        {
            EquipWeapon(WeaponSlot.Primary);
        }

        void EquipWeapon(WeaponSlot slot)
        {
            int selectedSlot = (int)slot;

            if (_weapons[selectedSlot] == null) { Debug.Log("슬롯에 무기가 장착되어있지 않습니다."); return; }

            for(int i = 0;i < _weapons.Length; i++)
            {
                //코드 확인, 아래 주석이랑 같은 코드를 이렇게 쓴거 같은데
                _weapons[i].SetActive(i == selectedSlot);


                //if (i == selectedSlot)
                //{
                //    _weapons[i].SetActive(true);
                //}
            }

            _currentSlot = slot;
        }
    }
}

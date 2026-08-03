using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyAttack : MonoBehaviour
    {
        [SerializeField] GameObject _ammo;
        [SerializeField] GameObject _muzzle;
        [SerializeField] GameObject _muzzleFire;

        public AimType aimType { get; } = AimType.None;

        public WeaponType weaponType { get; set; } = WeaponType.AR;

        public int Damage { get; } = 30;

        public float AttackRange { get; } = 150;


        void Start()
        {
            
        }

        private void Update()
        {
            
        }

        void EnemyAttackOnPlayer()
        {

        }

        void CalculateShotArea()
        {
            //이거 계산 하려면 어떻게 하지?
        }


        //정면으로 특정 넓이의 도형을 하나 만들어주는 함수를 만듦(이 안에서 랜덤하게 지점을 가져올것)
        //적이 투사체를 쏘는 함수를 하나 만듦(위의 지점을 향해 발사)
        //맞았는지 체크는 플레이어에서 투사체 체크하면 되고
        //머즐에서 불빛 나가게 만드는 함수도 만들기
        //계속 나가게 하면 안되므로, 3,4,5 발의 숫자중 랜덤하게 발사하도록 만드는 함수
    }
}

using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyAttack : MonoBehaviour
    {
        [SerializeField] float _spreadAngle;
        [SerializeField] float _bulletSpeed;
        [SerializeField] GameObject _ammo;
        [SerializeField] GameObject _muzzle;
        [SerializeField] GameObject _muzzleFire;

        EnemyAnimation _enemyAnim;

        EnemyFindArea _findArea;

        Vector3 _target = Vector3.zero;


        public AimType aimType { get; } = AimType.None;

        public WeaponType weaponType { get; set; } = WeaponType.AR;

        public int Damage { get; } = 30;

        public float AttackRange { get; } = 150;

        Vector3 Target { get { return _target; } }


        void Start()
        {
            _findArea = GetComponent<EnemyFindArea>();
            _enemyAnim = GetComponent<EnemyAnimation>();
        }

        private void Update()
        {
            EnemyAttackOnPlayer();
            CalculateShotArea();
        }

        //돌린 머즐의 방향대로 그대로 탄약을 생성시켜 물리로 발사
        void EnemyAttackOnPlayer()
        {
            if (_muzzle == null) { Debug.Log("Muzzle does not exist! Please Check the Component"); return; }
            if(_findArea == null) { Debug.Log("EnemyFindArea Script does not exist! Please Check the Component"); return; }
            if(_findArea.DetectedTarget == null || _findArea.DetectedTarget.position == Vector3.zero) { Debug.Log("Nobody In Area."); return; }

            

            GameObject ammo = Instantiate(_ammo, _muzzle.transform.position, _muzzle.transform.rotation);

            Rigidbody ammoRb = ammo.GetComponent<Rigidbody>();

            if(ammoRb == null) { Debug.Log("RigidBody does not exist! Please Check the Component"); return; }

            Vector3 shot = Target * _bulletSpeed;

            _enemyAnim.PlayEnemyShotAnim();
            ammoRb.AddForce(shot, ForceMode.Impulse);
        }

        //공격하는 범위를 원뿔로 계산, 이후 방향을 구해서, 그 방향대로 레이캐스트
        //정리하면 머즐을 돌리는 함수
        void CalculateShotArea()
        {
            Vector2 spread = Random.insideUnitCircle * _spreadAngle;
            Quaternion rotation = Quaternion.Euler(-spread.y, spread.x, 0);
            Vector3 shotDirection = rotation * _muzzle.transform.forward;

            _target = shotDirection;
            Physics.Raycast(_muzzle.transform.position, shotDirection, out RaycastHit hit, AttackRange);
        }
    }
}

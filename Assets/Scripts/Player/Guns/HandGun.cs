using MissionOfMercenary;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class HandGun : MonoBehaviour, IWeapons
    {
        [SerializeField] InputReader _inputReader;
        [SerializeField] GameObject _muzzle;

        public AimType aimType { get; } = AimType.None;

        public WeaponType weaponType => WeaponType.HG;

        public int Damage { get; } = 10;

        public float AttackRange { get; } = 70;

        public float Ammo { get; } = 10;

        private void OnEnable()
        {
            _inputReader.OnshotEvent += Attack;
        }

        private void OnDisable()
        {
            _inputReader.OnshotEvent += Attack;
        }

        public void Attack(float isShot)
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}

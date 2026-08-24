using MissionOfMercenary;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class Knife : MonoBehaviour, IWeapons
    {
        [Header("Input Reader")]
        [SerializeField] InputReader _inputReader;

        [Header("IK")]
        [SerializeField] Transform _rightGripPoint;
        [SerializeField] Transform _leftGripPoint;

        [Header("Trail Renderer")]
        [SerializeField] GameObject _trail;

        public AimType aimType => AimType.None;

        public WeaponType weaponType => WeaponType.Knife;

        public int Damage { get; private set; } = 100;

        public float AttackRange { get; private set; } = 50;

        public int Ammo { get; private set; } = 0;

        private void OnEnable()
        {
            _inputReader.OnshotEvent += Attack;
        }

        private void OnDisable()
        {
            _inputReader.OnshotEvent -= Attack;
        }


        public void Attack(float isShot)
        {
            throw new System.NotImplementedException();
        }
    }
}

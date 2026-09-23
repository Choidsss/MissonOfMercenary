using MissionOfMercenary;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class Knife : MonoBehaviour, IWeapons
    {
        [Header("Definition")]
        [SerializeField] FirearmDefinition _firearmDef;
        [SerializeField] WeaponEffectDefinition _weaponEffectDef;

        [Header("Slash Timing")]
        [SerializeField, Min(0.01f)] float _windUpDuration = 0.12f;
        [SerializeField, Min(0.01f)] float _slashDuration = 0.1f;
        [SerializeField, Min(0.01f)] float _returnDuration = 0.22f;

        [Header("Slash Timing")]
        [SerializeField] Vector3 _windUpPosition = new Vector3(0.08f, 0.04f, -0.08f);
        [SerializeField] Vector3 _windUpRotation = new Vector3(-15f, -25f, 25f);

        [Header("Slash Pose")]
        [SerializeField] Vector3 _slashPosition = new Vector3(-0.15f, -0.05f, 0.15f);
        [SerializeField] Vector3 _slashRotation = new Vector3(25f, 40f, -50f);

        Vector3 _restPosition;
        Quaternion _restRotation;
        float _attackTime;
        bool _isAttacking;
        bool _hasRestPose;

        public int Damage { get { return _firearmDef.Damage; } }

        public float AttackRange { get { return _firearmDef.Range; } }

        public AimType AimType { get { return _firearmDef.GunAimType; } }

        public WeaponType WeaponType { get { return _firearmDef.GunWeaponType; } }

        public void ExecuteAttack()
        {
            if (!isActiveAndEnabled || _isAttacking) { return; }

            _restPosition = transform.localPosition;
            _restRotation = transform.localRotation;
            _hasRestPose = true;

            _attackTime = 0f;
            _isAttacking = true;
        }


    }
}

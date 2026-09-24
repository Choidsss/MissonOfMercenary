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

        private void OnDisable()
        {
            ResetAttack();
        }

        private void Update()
        {
            KnifeMovement();
        }

        //공격 실행
        public void ExecuteAttack()
        {
            if (!isActiveAndEnabled || _isAttacking) { return; }

            _restPosition = transform.localPosition;
            _restRotation = transform.localRotation;
            _hasRestPose = true;

            _attackTime = 0f;
            _isAttacking = true;
        }

        //칼의 움직임
        void KnifeMovement()
        {
            if (!_isAttacking) { return; }

            _attackTime += Time.deltaTime;

            float windup = Mathf.Max(0.01f, _windUpDuration);
            float slash = Mathf.Max(0.01f, _slashDuration);
            float recovery = Mathf.Max(0.01f, _returnDuration);

            if (_attackTime < windup)
            {
                ApplyPose(Vector3.zero, Vector3.zero, _windUpPosition, _windUpRotation, _attackTime / windup);
            }
            else if (_attackTime < windup + slash)
            {
                ApplyPose(_windUpPosition, _windUpRotation, _slashPosition, _slashRotation, (_attackTime - windup) / slash);
            }
            else if (_attackTime < windup + slash + recovery)
            {
                ApplyPose(_slashPosition, _slashRotation, Vector3.zero, Vector3.zero, (_attackTime - slash - windup) / recovery);
            }
            else
            {
                ResetAttack();
            }
        }

        void ApplyPose(Vector3 fromPosition, Vector3 fromRotation, Vector3 toPosition, Vector3 toRotation, float progress)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress));

            Vector3 position = _restPosition + Vector3.Lerp(fromPosition, toPosition, t);
            Quaternion rotation = _restRotation * Quaternion.Slerp(Quaternion.Euler(fromRotation) , Quaternion.Euler(toRotation), t);

            transform.SetLocalPositionAndRotation(position, rotation);
        }

        void ResetAttack()
        {
            if (_hasRestPose) { transform.SetLocalPositionAndRotation(_restPosition, _restRotation); }

            _isAttacking = false;
            _hasRestPose = false;
            _attackTime = 0f;
        }
    }
}

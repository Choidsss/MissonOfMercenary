using System;
using UnityEngine;

namespace MIssionOfMercenary
{
    [DefaultExecutionOrder(10500)]
    public class WeaponStartEquipMotion : MonoBehaviour
    {
        [Serializable]
        public class EquipPose
        {
            public Vector3 PositionOffset;
            public Vector3 RotationOffset;

            [Min(0.01f)]
            [SerializeField] public float _duration = 0.4f;
        }


        [Header("Reference")]
        [SerializeField] Transform _weaponPivot;

        [Header("Primary - High Ready")]
        [SerializeField]
        EquipPose _primaryPose = new EquipPose
        {
            RotationOffset = new Vector3(-45f, 0f, 0f)
        };

        [Header("Secondary - Low Ready")]
        [SerializeField]
        EquipPose _secondaryPose = new EquipPose
        {
            RotationOffset = new Vector3(45f, 0f, 0f)
        };

        [Header("Melee - Close To Body")]
        [SerializeField]
        EquipPose _meleePose = new EquipPose
        {
            PositionOffset = new Vector3(0f, 0f, -0.15f)
        };

        Vector3 _defaultPosition;
        Quaternion _defaultRotation;

        Vector3 _startPosition;
        Quaternion _startRotation;

        float _elapsed;
        float _duration;
        bool _hasDefaultPose;

        public bool IsEquipping { get; private set; }

        private void Awake()
        {
            if (_weaponPivot == null)
            {
                Debug.LogError("WeaponPivot을 연결해주세요.", this);
                return;
            }

            // 장착 모션이 시작되기 전의 기본 자세를 한 번 저장한다. By Codex
            _defaultPosition = _weaponPivot.localPosition;
            _defaultRotation = _weaponPivot.localRotation;
            _hasDefaultPose = true;
        }

        public void Play(WeaponSlot slot)
        {
            if (!_hasDefaultPose || _weaponPivot == null)
            {
                return;
            }

            Cancel();

            EquipPose pose;

            switch (slot)
            {
                case WeaponSlot.Primary:
                    pose = _primaryPose;
                    break;

                case WeaponSlot.Secondary:
                    pose = _secondaryPose;
                    break;

                case WeaponSlot.Melee:
                    pose = _meleePose;
                    break;

                default:
                    return;
            }

            // 위치는 부모의 로컬 축, 회전은 기본 자세 기준의 추가 회전이다. By Codex
            _startPosition = _defaultPosition + pose.PositionOffset;
            _startRotation =
                _defaultRotation * Quaternion.Euler(pose.RotationOffset);

            _elapsed = 0f;
            _duration = Mathf.Max(0.01f, pose._duration);
            IsEquipping = true;

            _weaponPivot.SetLocalPositionAndRotation(
                _startPosition,
                _startRotation);
        }

        private void Update()
        {
            if (!IsEquipping)
            {
                return;
            }

            _elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(_elapsed / _duration);
            float t = Mathf.SmoothStep(0f, 1f, progress);

            _weaponPivot.SetLocalPositionAndRotation(
                Vector3.Lerp(_startPosition, _defaultPosition, t),
                Quaternion.Slerp(_startRotation, _defaultRotation, t));

            if (progress >= 1f)
            {
                Cancel();
            }
        }

        public void Cancel()
        {
            IsEquipping = false;

            if (!_hasDefaultPose || _weaponPivot == null)
            {
                return;
            }

            // 중간 취소와 정상 완료 모두 기본 자세로 정확히 복구한다. By Codex
            _weaponPivot.SetLocalPositionAndRotation(
                _defaultPosition,
                _defaultRotation);
        }

        private void OnDisable()
        {
            Cancel();
        }
    }
}

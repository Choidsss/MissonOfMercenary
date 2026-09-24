using System.Collections;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class ReloadHandMotion : MonoBehaviour
    {
        // 값이 클수록 해당 구간에 더 많은 시간을 배분한다. By Codex
        [Header("Reload Phase Time Weights")]
        [SerializeField, Min(0.01f)] float _reachMagazineTimeWeight = 1f;
        [SerializeField, Min(0.01f)] float _pullOutTimeWeight = 1f;
        [SerializeField, Min(0.01f)] float _insertTimeWeight = 1f;
        [SerializeField, Min(0.01f)] float _returnHandTimeWeight = 1f;

        [SerializeField] WeaponIKController _weaponIKController;

        [SerializeField] Transform _normalGripPoint;
        [SerializeField] Transform _magazineGrapPoint;
        [SerializeField] Transform _reloadHandTarget;

        [Header("Magazine")]
        [SerializeField] Transform _magazine;
        [SerializeField] Transform _magazinePullOutPoint;

        public bool IsReady => _weaponIKController != null && _normalGripPoint != null && _magazineGrapPoint != null && _reloadHandTarget != null && _magazine != null
                          && _magazinePullOutPoint != null;

        Transform _magazineOriginalParent;
        Vector3 _magazineOriginalPosition;
        Quaternion _magazineOriginalRotation;
        Vector3 _magazineOriginalScale;

        bool _isMagazineAttached;

        private void Awake()
        {
            if(_weaponIKController == null)
            {
                _weaponIKController = GetComponentInParent<WeaponIKController>();
            }
        }

        private void OnDisable()
        {
            Cancel();
        }

        public IEnumerator Play(float duration)
        {
            if (!IsReady) { yield break; }

            duration = Mathf.Max(0.1f, duration);

            // 전체 재장전 시간은 유지하고 구간별 비율만 조절한다. By Codex
            float reachWeight = Mathf.Max(0.01f, _reachMagazineTimeWeight);
            float pullWeight = Mathf.Max(0.01f, _pullOutTimeWeight);
            float insertWeight = Mathf.Max(0.01f, _insertTimeWeight);
            float returnWeight = Mathf.Max(0.01f, _returnHandTimeWeight);
            float timePerWeight = duration / (reachWeight + pullWeight + insertWeight + returnWeight);

            _reloadHandTarget.SetPositionAndRotation(_normalGripPoint.position, _normalGripPoint.rotation);

            _weaponIKController.SetLeftHandOverride(_reloadHandTarget);

            // 1. 왼손이 탄창을 잡는 위치로 이동. By Codex
            yield return MoveTarget(
                _normalGripPoint,
                _magazineGrapPoint,
                timePerWeight * reachWeight);

            AttachMagazine();

            // 2. 손과 탄창을 함께 아래로 이동. By Codex
            yield return MoveTarget(
                _magazineGrapPoint,
                _magazinePullOutPoint,
                timePerWeight * pullWeight);

            // 3. 손과 탄창을 다시 삽입 위치로 이동. By Codex
            yield return MoveTarget(
                _magazinePullOutPoint,
                _magazineGrapPoint,
                timePerWeight * insertWeight);

            RestoreMagazine();

            // 4. 탄창은 총에 남기고 왼손만 기본 자세로 복귀. By Codex
            yield return MoveTarget(
                _magazineGrapPoint,
                _normalGripPoint,
                timePerWeight * returnWeight);

            Cancel();
        }

        IEnumerator MoveTarget(Transform from, Transform to, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));

                _reloadHandTarget.SetPositionAndRotation(Vector3.Lerp(from.position, to.position, t), Quaternion.Slerp(from.rotation, to.rotation, t));

                yield return null;
            }

            _reloadHandTarget.SetPositionAndRotation(to.position, to.rotation);
        }

        void AttachMagazine()
        {
            if (_isMagazineAttached || _magazine == null) { return; }
            
            _magazineOriginalPosition = _magazine.localPosition;
            _magazineOriginalRotation = _magazine.localRotation;
            _magazineOriginalParent = _magazine.parent;
            _magazineOriginalScale = _magazine.localScale;

            _magazine.SetParent(_reloadHandTarget);
            _isMagazineAttached = true;
        }

        void RestoreMagazine()
        {
            if (!_isMagazineAttached) { return; }
                
            if(_magazine != null)
            {
                _magazine.SetParent(_magazineOriginalParent, false);
                _magazine.SetLocalPositionAndRotation(_magazineOriginalPosition, _magazineOriginalRotation);
                _magazine.localScale = _magazineOriginalScale;
            }

            _isMagazineAttached = false;
        }
        public void Cancel()
        {
            RestoreMagazine();

            if (_weaponIKController != null)
            {
                _weaponIKController.ClearLeftHandOverride(_reloadHandTarget);
            }
        }

    }
}

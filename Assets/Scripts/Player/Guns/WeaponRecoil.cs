using MissionOfMercenary;
using System.Collections;
using UnityEngine;

namespace MIssionOfMercenary
{
    [DefaultExecutionOrder(10000)] // Animator와 IK 처리 뒤에 반동 Transform을 적용합니다. By Codex
    public class WeaponRecoil : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Transform _weaponPivot; // 무기 가져오기

        [Header("Options")]
        [SerializeField] float _recoilKickBack; // 앞/뒤 반동
        [SerializeField] float _recoilUpDown; // 위/아래 반동
        [SerializeField] float _recoilVibration; // 총 흔들림

        [SerializeField] float _snapSpeed; // 반동 속도
        [SerializeField] float _recoverySpeed; // 반동 회복 속도

        Vector3 _currentRecoilPos;
        Vector3 _currentRecoilRotation;

        Vector3 _targetRecoilPos;
        Vector3 _targetRecoilRotation;

        Vector3 _originPos;
        Quaternion _originRot;
        bool _isRecoilActive;

        private void Start()
        {
            InitializeRecoil(); // 최초 활성화 때 반동 기준 위치를 저장합니다. By Codex
        }

        public void InitializeRecoil()
        {
            if (_weaponPivot == null) { Debug.LogError($"{name}: Weapon Pivot이 없습니다."); return; }

            _originPos = _weaponPivot.localPosition;
            _originRot = _weaponPivot.localRotation;
            _currentRecoilPos = Vector3.zero;
            _currentRecoilRotation = Vector3.zero;
            _targetRecoilPos = Vector3.zero;
            _targetRecoilRotation = Vector3.zero;
            _isRecoilActive = false;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (_weaponPivot == null) { return; }
            WeaponsRecoil();
        }

        void WeaponsRecoil()
        {
            // 다른 무기의 대기 중인 반동 컴포넌트가 공유 Pivot을 덮어쓰지 않게 합니다. By Codex
            if (!_isRecoilActive) { return; }

            //총의 반동 회복
            _targetRecoilPos = Vector3.Lerp(_targetRecoilPos, Vector3.zero, _recoverySpeed * Time.deltaTime);
            _targetRecoilRotation = Vector3.Lerp(_targetRecoilRotation, Vector3.zero, _recoverySpeed * Time.deltaTime);

            //총의 반동
            _currentRecoilPos = Vector3.Lerp(_currentRecoilPos, _targetRecoilPos, _snapSpeed * Time.deltaTime);
            _currentRecoilRotation = Vector3.Lerp(_currentRecoilRotation, _targetRecoilRotation, _snapSpeed * Time.deltaTime);

            _weaponPivot.localPosition = _originPos + _currentRecoilPos;
            _weaponPivot.localRotation = _originRot * Quaternion.Euler(_currentRecoilRotation);

            if (_targetRecoilPos.sqrMagnitude < 0.000001f &&
                _targetRecoilRotation.sqrMagnitude < 0.000001f &&
                _currentRecoilPos.sqrMagnitude < 0.000001f &&
                _currentRecoilRotation.sqrMagnitude < 0.000001f)
            {
                _weaponPivot.SetLocalPositionAndRotation(_originPos, _originRot);
                _isRecoilActive = false;
            }
        }

        public void WeaponRecoilApply()
        {
            ApplyRecoil();
        }

        void ApplyRecoil()
        {
            _isRecoilActive = true;
            _targetRecoilPos = new Vector3(0f, 0f, -_recoilKickBack);
            _targetRecoilRotation = new Vector3(-_recoilUpDown, Random.Range(-_recoilVibration, _recoilVibration), 0f);
        }
    }
}

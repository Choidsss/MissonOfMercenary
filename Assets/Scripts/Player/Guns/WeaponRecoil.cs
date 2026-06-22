using MissionOfMercenary;
using System.Collections;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class WeaponRecoil : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] InputReader _inputReader; // 입력 가져오기
        [SerializeField] Transform _weapon; // 무기 가져오기

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

        private void Start()
        {
            _originPos = _weapon.localPosition;  // 원래 위치 저장
            _originRot = _weapon.localRotation;  // 원래 회전 저장
        }

        // Update is called once per frame
        void Update()
        {
            ARWeaponRecoil();
        }

        void ARWeaponRecoil()
        {
            if(_weapon == null || _inputReader == null) { return; }

            //총의 반동 회복
            _targetRecoilPos = Vector3.Lerp(_targetRecoilPos, Vector3.zero, _recoverySpeed * Time.deltaTime);
            _targetRecoilRotation = Vector3.Lerp(_targetRecoilRotation, Vector3.zero, _recoverySpeed * Time.deltaTime);

            //총의 반동
            _currentRecoilPos = Vector3.Lerp(_currentRecoilPos, _targetRecoilPos, _snapSpeed * Time.deltaTime);
            _currentRecoilRotation = Vector3.Lerp(_currentRecoilRotation, _targetRecoilRotation, _snapSpeed * Time.deltaTime);

            _weapon.localPosition = _originPos + _currentRecoilPos;
            _weapon.localRotation = _originRot * Quaternion.Euler(_currentRecoilRotation);
        }

        public void WeaponRecoilApply()
        {
            ApplyRecoil();
        }

        void ApplyRecoil()
        {
            _targetRecoilPos = new Vector3(0f, 0f, -_recoilKickBack);
            _targetRecoilRotation = new Vector3(-_recoilUpDown, Random.Range(-_recoilVibration, _recoilVibration), 0f);
        }
    }
}

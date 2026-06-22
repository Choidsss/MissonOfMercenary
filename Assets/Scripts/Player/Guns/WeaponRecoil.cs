using MissionOfMercenary;
using System.Collections;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class WeaponRecoil : MonoBehaviour
    {
        /* *******************Be Fix*******************
         * 1. Weapon Recoil Will be Call When Attack Called everytime.
         * 2. Weapon Transform must be not Move, The Weapon comply with position
         */

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

        // Update is called once per frame
        void Update()
        {
            ARWeaponRecoil();
        }

        private void OnEnable()
        {
            _inputReader.OnshotEvent += WeaponRecoilHandle;
        }

        private void OnDisable()
        {
            _inputReader.OnshotEvent -= WeaponRecoilHandle;
        }

        /*
         * 1. 현재 총의 위치에서 총을 쐈는지 안쐈는지 받아옴.
         * 2. 안쐈으면 그냥 리턴, 쐈다면 감지 로직
         * 
         * WeaponRecoil 함수
         * 1. 총의 위치를 가져옴
         * 2. 총을 쏜게 확인됐다면, 총의 각도를 위쪽으로 틀어지게 만듦 && 뒤쪽으로 밀리게 만듦 && 총을 흔들리게 함
         * 3. 반동을 준 후, Recovery 를 줌
         * 4. Lerp로 부드럽게 만들어줌
         */

        void ARWeaponRecoil()
        {
            if(_weapon == null || _inputReader == null) { return; }

            //총의 반동 회복
            _targetRecoilPos = Vector3.Lerp(_targetRecoilPos, Vector3.zero, _recoverySpeed * Time.deltaTime);
            _targetRecoilRotation = Vector3.Lerp(_targetRecoilRotation, Vector3.zero, _recoverySpeed * Time.deltaTime);

            //총의 반동
            _currentRecoilPos = Vector3.Lerp(_currentRecoilPos, _targetRecoilPos, _snapSpeed * Time.deltaTime);
            _currentRecoilRotation = Vector3.Lerp(_currentRecoilRotation, _targetRecoilRotation, _snapSpeed * Time.deltaTime);

            _weapon.localPosition = _currentRecoilPos;
            _weapon.localRotation = Quaternion.Euler(_currentRecoilRotation);
        }

        void WeaponRecoilHandle(float isShot)
        {
            ApplyRecoil();
        }

        void ApplyRecoil()
        {
            _targetRecoilPos = new Vector3(0f, 0f, -_recoilKickBack);
            _targetRecoilRotation = new Vector3(-_recoilUpDown, Random.Range(-_recoilVibration, _recoilVibration), 0f);
        }

        IEnumerator RecoilRoutine()
        {
            yield return new WaitForSeconds(3);
        }
    }
}

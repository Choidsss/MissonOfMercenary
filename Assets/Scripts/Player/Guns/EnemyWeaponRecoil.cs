using UnityEngine;
using UnityEngine.ProBuilder;

namespace MIssionOfMercenary
{
    public class EnemyWeaponRecoil : MonoBehaviour
    {
        [Header("Recoil Weapon")]
        [SerializeField] GameObject _enemyWeapon;

        [Header("Recoil Variable")]
        [SerializeField] float _kickBack;
        [SerializeField] float _upDown;
        [SerializeField] float _maxOffset;
        [SerializeField] float _recoverySpeed;
        [SerializeField] float _snapSpeed;

        Vector3 _originPosition;
        Vector3 _targetPosition;
        Vector3 _currentPosition;

        void Start()
        {
            InitializeEnemyWeapon();//무기 초기화 함수
        }

        void LateUpdate()
        {
            SetRecoilPositionAndRecovery();
        }

        void InitializeEnemyWeapon()
        {
            _originPosition = _enemyWeapon.transform.localPosition;
            _targetPosition = Vector3.zero;
            _currentPosition = Vector3.zero;
        }

        public void RecoilFromEnemyWeapon()
        {
            // 총의 로컬 +Z가 총구 방향일 때, -Z로 밀고 +Y로 들어 올립니다.
            _targetPosition = new Vector3(0f, _upDown, -_kickBack);
            _targetPosition = Vector3.ClampMagnitude(_targetPosition, _maxOffset);
            // 반동 이동량에 기본 위치를 섞지 않습니다. 복귀는 아래 함수에서 0을 향해 처리합니다.
        }

        void SetRecoilPositionAndRecovery()
        {
            _targetPosition = Vector3.Lerp(_targetPosition, Vector3.zero, _recoverySpeed * Time.deltaTime);

            _currentPosition = Vector3.Lerp(_currentPosition, _targetPosition, _snapSpeed * Time.deltaTime);

            // localPosition은 부모 기준이므로, 총 기준 반동을 localRotation으로 변환합니다.
            // 총이 조준 방향으로 회전해도 총 자신의 뒤쪽으로 밀리도록 적용합니다.
            _enemyWeapon.transform.localPosition = _originPosition + _enemyWeapon.transform.localRotation * _currentPosition;
        }
    }
}

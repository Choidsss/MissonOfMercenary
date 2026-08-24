using MissionOfMercenary;
using TMPro;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class HeadBob : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Transform _camera;
        [SerializeField] PlayerMove _playerMove;
        [SerializeField] GameObject _weaponBox;

        [Header("HeadBob Options")]
        [SerializeField] float _bobFrequency;//흔들리는 속도(주파수)
        [SerializeField] float _bobReturnSpeed; //되돌리는 속도
        [SerializeField] float _amountX; // 좌우 흔들리는 폭
        [SerializeField] float _amountY; //상하 흔들리는 폭
        [SerializeField] float _runMultiply;
        [SerializeField] float _weaponBoxMultiply;

        float _bobTimer; 
        Vector3 _bobOffset; //처음 위치 저장

        Vector3 _originPosition;
        Vector3 _weaponBoxOriginPosition; // 추가

        private void Start()
        {
            _originPosition = _camera.localPosition;
            _weaponBoxOriginPosition = _weaponBox.transform.localPosition; // 추가
        }

        // Update is called once per frame
        void Update()
        {
            HeadbobMove();
        }

        void HeadbobMove()
        {
            if(_playerMove.walkSpeed > 0)
            {
                //얼마만큼 흔들리게 할건지 결정
                _bobTimer += _bobFrequency * Time.deltaTime;

                float multiply = _playerMove.DoRun ? _runMultiply : 1f;
                float bobY = Mathf.Sin(_bobTimer) * _amountY * multiply;
                float bobX = Mathf.Sin(_bobTimer * 0.5f) * _amountX * multiply;
                _bobOffset = Vector3.Lerp(_bobOffset, new Vector3(bobX, bobY, 0), _bobReturnSpeed * Time.deltaTime);
            }
            else
            {
                _bobTimer = 0;
                _bobOffset = Vector3.Lerp(_bobOffset, Vector3.zero, _bobReturnSpeed * Time.deltaTime);
            }

            _camera.localPosition = _originPosition + _bobOffset;
            _weaponBox.transform.localPosition = _weaponBoxOriginPosition + _bobOffset * _weaponBoxMultiply; // 추가
        }

    }
}

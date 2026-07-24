using System.Linq;
// 현재 파일에서 Linq 기능을 쓰지 않으면 using System.Linq는 제거해도 됨. By Codex
using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyFindArea : MonoBehaviour
    {
        [Header("Enemy Field Of Vision Options")]
        [SerializeField] float _degree; //전방 시야 +-각도 제한(한쪽으로 70도, 140도)
        [SerializeField] float _distance; //전방 시야 거리 제한
        [SerializeField] float _backsideDegree; //후방 시야 +-각도 제한(암살기능 추가예정 45,45 도)
        [SerializeField] float _backsideDistance; //후방 시야 거리 제한
        
        [Header("Eye Offset")]
        [SerializeField] Vector3 _eyeHeight;
        [SerializeField] Vector3 _targetHeight;

        [Header("Layer Classify")]
        [SerializeField] LayerMask _obstaclesLayer;
        [SerializeField] LayerMask _playerLayer;

        [Header("Value Options")]
        [SerializeField] float _enemyTurnAmount;
        [SerializeField] float _lookAngleOffset;
        
        
        Vector3 _playerPosition;
        Transform _detectedTarget;
        bool _isDetected = false;
        bool _isSense = false;

        //Vector3 _testVec;

        public bool IsDetectedPlayer => _isDetected;
        public bool IsSensePlayer => _isSense;
        public Transform DetectedTarget => _detectedTarget;

        // Update is called once per frame
        void Update()
        {
            DetectPlayer();
        }

        bool DetectPlayer()
        {
            Collider[] collider = Physics.OverlapSphere(transform.position, _distance, _playerLayer, QueryTriggerInteraction.Ignore);

            _detectedTarget = null;

            if(collider.Length == 0) { _isDetected = false;  return false; }

            foreach (Collider col in collider)
            {
                _playerPosition = col.gameObject.transform.position;

                //범위 안에 들었으면 인기척을 느꼈다라는 것이므로 이래야 맞지 않을까?
                //원래는 계속 돌아다니다가, 벽체크가 없다는게 확인 되면 보는게 맞을듯 => 이거도 플레이어한테 은신상태인지 아닌지를 또 체크하도록 해야 현실감이 있지 않을까.
                //일단은 이렇게 가자
                LookAtPlayer();

                //시야 안쪽인지 와 벽체크
                if (IsPlayerInEnemyDegree() && !IsBlockedByObtacles())
                {
                    _detectedTarget = col.transform;
                    _isDetected = true;
                    _isSense = false;
                    

                    return true;
                }
            }
            _isDetected = false;

            return false;
        }

        public void LookAtPlayer()
        {
            Vector3 direction = _playerPosition - transform.position;
            direction.y = 0;

            if(direction.sqrMagnitude <= 0.01f) { return; }

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // 각도 보정
            targetRotation = targetRotation * Quaternion.Euler(0f, _lookAngleOffset, 0f);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _enemyTurnAmount * Time.deltaTime);
        }

        //왜 계속 false가 뜨지...? 캐스트가 벽에 닿으면 true가 떠야 되는데
        //Eyeoffset Issue => 캐스트가 바닥에 붙어 있어서 계속 벽 판정을 못한게 원인 => _eyeHeigh = 2,  _targetHeight = 1.5 로 수정하여 판정 수정 완료
        bool IsBlockedByObtacles()
        {
            Vector3 start = _eyeHeight + transform.position;
            Vector3 end = _targetHeight + _playerPosition;

            //_testVec = start;

            Vector3 direction = end - start;

            bool hitWall = Physics.Raycast(start, direction.normalized, direction.magnitude, _obstaclesLayer, QueryTriggerInteraction.Ignore);
            Debug.Log($"{hitWall}");

            return hitWall;
        }

        //전방으로 각도 만들어서 Enemy의 시야각 안에 Player가 있는지 체크
        bool IsPlayerInEnemyDegree()
        {
            Vector3 enemyLook = transform.forward; //적의 전방 벡터
            Vector3 direction =  _playerPosition - transform.position; //Enemy 에서 Player로 가는 벡터
            float fov = Vector3.Angle(enemyLook, direction); //둘 사이의 각도를 구함

            //각도가 _degree 안쪽인지 체크
            if (fov < _degree) { return true; }
            else return false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _distance);

            if (_playerPosition == Vector3.zero)
            {
                return;
            }

            Vector3 start = transform.position + _eyeHeight;
            Vector3 end = _playerPosition + _targetHeight;

            // Draw one sight-check line only: green when detected, red when not detected. By Codex
            Gizmos.color = _isDetected ? Color.green : Color.red;
            Gizmos.DrawLine(start, end);
        }
    }
}

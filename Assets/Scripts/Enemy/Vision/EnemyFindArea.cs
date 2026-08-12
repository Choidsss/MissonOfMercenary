using System.Linq;
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
        Vector3 _detectedTarget;
        bool _isDetected = false;
        bool _isSense = false;

        public bool IsDetectedPlayer => _isDetected;
        public bool IsSensePlayer => _isSense;
        public Vector3 DetectedTarget => _playerPosition;

        void Update()
        {
            DetectPlayer(_playerPosition);
        }

        //전방으로 _degree기준으로 180도 약간 안되게 반원을 그려서 그 안에 레이어가 플레이어인 오브젝트가 존재하는지
        //존재한다면 위치를 갱신시키고 저장한 다음 계속 함수를 return시켜서 로직이 실행되지 않도록 만듦
        //아니라면 계속 찾음
        void DetectPlayer(Vector3 playerPosition)
        {
            if (_isDetected) { return; }
            Vector3 dir = playerPosition - transform.position;

            float fov = Vector3.Angle(transform.forward, dir);

            _isDetected = fov < _degree ? true : false;

            _playerPosition = playerPosition;
            return;
            //if (_isDetected)
            //{
            //    _playerPosition = _detectedTarget;
            //    return true;
            //}

            //Collider[] collider = Physics.OverlapSphere(transform.position, _distance, _playerLayer, QueryTriggerInteraction.Ignore);

            //if (collider.Length == 0) { _isDetected = false;  return false; }

            //foreach (Collider col in collider)
            //{
            //    _detectedTarget = col.gameObject.transform.position;

            //    LookAtPlayer();

            //    if (IsPlayerInEnemyDegree(_detectedTarget) && !IsBlockedByObtacles(_detectedTarget))
            //    {
            //        _isDetected = true;
            //        _isSense = false;
            //        _playerPosition = _detectedTarget;

            //        return true;
            //    }
            //}
            //_isDetected = false;
            //return false;
        }

        public void LookAtPlayer()
        {
            Debug.Log("before");
            if (_detectedTarget == null) { return; }

            Vector3 direction = _detectedTarget - transform.position;
            direction.y = 0;

            if(direction.sqrMagnitude <= 0.01f) { return; }

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // 각도 보정
            targetRotation = targetRotation * Quaternion.Euler(0f, _lookAngleOffset, 0f);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _enemyTurnAmount * Time.deltaTime);
            Debug.Log("after");
        }

        bool IsBlockedByObtacles(Vector3 playerPosition)
        {
            Vector3 start = _eyeHeight + transform.position;
            Vector3 end = _targetHeight + playerPosition;

            Vector3 direction = end - start;

            bool hitWall = Physics.Raycast(start, direction.normalized, direction.magnitude, _obstaclesLayer, QueryTriggerInteraction.Ignore);

            return hitWall;
        }

        bool IsPlayerInEnemyDegree(Vector3 playerPosition)
        {
            Vector3 direction = playerPosition - transform.position;
            float angle = Vector3.Angle(transform.forward, direction);

            //Vector3 enemyLook = transform.forward; //적의 전방 벡터
            //Vector3 direction = _playerPosition - transform.position; //Enemy 에서 Player로 가는 벡터
            //float fov = Vector3.Angle(enemyLook, direction); //둘 사이의 각도를 구함

            //return fov < _degree ? true : false;

            return angle < _degree;
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

            Gizmos.color = _isDetected ? Color.green : Color.red;
            Gizmos.DrawLine(start, end);
        }
    }
}

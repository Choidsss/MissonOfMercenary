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
        // Header 오타와 변수 의미를 나중에 정리하면 좋음. 지금 Vector3는 높이값이라기보다 위치 보정 오프셋으로 쓰이고 있음. By Codex

        [Header("Layer Classify")]
        [SerializeField] LayerMask _obstaclesLayer;
        [SerializeField] LayerMask _playerLayer;

        Vector3 _playerPosition;
        Transform _detectedTarget;
        bool _isDetected = false;

        public bool IsDetectedPlayer => _isDetected;
        public Transform DetectedTarget => _detectedTarget;

        // Update is called once per frame
        void Update()
        {
            _isDetected = DetectPlayer();
        }

        bool DetectPlayer()
        {
            Collider[] collider = Physics.OverlapSphere(transform.position, _distance, _playerLayer, QueryTriggerInteraction.Ignore);

            _detectedTarget = null;

            if(collider == null) { return false; }

            foreach (Collider col in collider)
            {
                _playerPosition = col.gameObject.transform.position;

                //시야 안쪽인지 와 벽체크
                if (IsPlayerInEnemyDegree() && !IsBlockedByObtacles())
                {
                    _detectedTarget = col.transform;
                    return true;
                }
            }
            return false;
        }

        bool IsBlockedByObtacles()
        {
            Vector3 start = _eyeHeight + transform.position;
            Vector3 end = _targetHeight + _playerPosition;

            Vector3 direction = end - start;

            bool hitWall = Physics.Raycast(start, direction.normalized, direction.magnitude, _obstaclesLayer, QueryTriggerInteraction.Ignore);

            return hitWall;
        }

        //전방으로 부채꼴 만들어서 각도 안에 Player가 있는지 체크
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

using System.Linq;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyFindArea : MonoBehaviour
    {
        [SerializeField] GameObject _player;

        [Header("Enemy Field Of Vision Options")]
        [SerializeField] float _degree; //전방 시야 +-각도 제한(한쪽으로 70도, 140도)
        [SerializeField] float _eyeSightDistance; //전방 시야 거리 제한
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
        bool _isDetected = false;
        bool _canAssasin = false;

        public bool CanAssasin => _canAssasin;
        public bool IsDetectedPlayer => _isDetected;
        public Vector3 DetectedTarget => _playerPosition;

        void Update()
        {
            DetectPlayer();
            AssasinBackside();
        }

        void DetectPlayer()
        {
            //Player가 이미 한번 발견 된 상태라면 계속해서 위치 갱신
            if (_isDetected) { _playerPosition = _player.transform.position; return; }
            Vector3 playerPosition = _player.transform.position;
            Vector3 direction = playerPosition - transform.position;
            direction.y = 0;

            float fov = Vector3.Angle(transform.forward, direction);

            //시야각 안에 있는지
            bool isInSight = fov <= _degree && direction.sqrMagnitude <= _eyeSightDistance ? true : false;
            //벽에 막혔는지
            bool isBlocked = IsBlockedByObtacles(playerPosition);

            if(isInSight && !isBlocked)
            {
                Debug.Log($"InSight");
                _isDetected = true;
                _playerPosition = playerPosition;
                return;
            }

            _isDetected = false;
            return;
        }

        bool IsBlockedByObtacles(Vector3 playerPosition)
        {
            Vector3 start = _eyeHeight + transform.position;
            Vector3 end = _targetHeight + playerPosition;

            Vector3 direction = end - start;

            bool hitWall = Physics.Raycast(start, direction.normalized, direction.magnitude, _obstaclesLayer, QueryTriggerInteraction.Ignore);

            return hitWall;
        }

        void AssasinBackside()
        {
            Vector3 backDirection = (_player.transform.position - transform.forward).normalized;
            backDirection.y = 0;

            if(backDirection.sqrMagnitude > _backsideDistance * _backsideDistance)
            {
                _canAssasin = false;
            }

            float backAngle = Vector3.Angle(-transform.forward, backDirection);

            if(backAngle <= _backsideDegree)
            {
                _canAssasin = true;
            }
            else
            {
                _canAssasin = false;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _eyeSightDistance);

            // By Codex - Draws the forward field of view used by DetectPlayer.
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

            if (forward != Vector3.zero && _eyeSightDistance > 0f)
            {
                const int arcSegments = 30;
                Vector3 center = transform.position;
                Vector3 leftBoundary = Quaternion.AngleAxis(-_degree, Vector3.up) * forward;
                Vector3 rightBoundary = Quaternion.AngleAxis(_degree, Vector3.up) * forward;

                Gizmos.color = new Color(1f, 0.75f, 0f, 0.9f);
                Gizmos.DrawLine(center, center + leftBoundary * _eyeSightDistance);
                Gizmos.DrawLine(center, center + rightBoundary * _eyeSightDistance);

                Vector3 previousPoint = center + leftBoundary * _eyeSightDistance;

                for (int i = 1; i <= arcSegments; i++)
                {
                    float angle = Mathf.Lerp(-_degree, _degree, i / (float)arcSegments);
                    Vector3 arcDirection = Quaternion.AngleAxis(angle, Vector3.up) * forward;
                    Vector3 currentPoint = center + arcDirection * _eyeSightDistance;

                    Gizmos.DrawLine(previousPoint, currentPoint);
                    previousPoint = currentPoint;
                }
            }

            if (forward != Vector3.zero && _backsideDistance > 0f)
            {
                const int arcSegments = 30;
                Vector3 center = transform.position;
                Vector3 backward = -forward;
                Vector3 leftBoundary = Quaternion.AngleAxis(-_backsideDegree, Vector3.up) * backward;
                Vector3 rightBoundary = Quaternion.AngleAxis(_backsideDegree, Vector3.up) * backward;

                Gizmos.color = new Color(0.7f, 0.2f, 1f, 0.9f);
                Gizmos.DrawLine(center, center + leftBoundary * _backsideDistance);
                Gizmos.DrawLine(center, center + rightBoundary * _backsideDistance);

                Vector3 previousPoint = center + leftBoundary * _backsideDistance;

                for (int i = 1; i <= arcSegments; i++)
                {
                    float angle = Mathf.Lerp(-_backsideDegree, _backsideDegree, i / (float)arcSegments);
                    Vector3 arcDirection = Quaternion.AngleAxis(angle, Vector3.up) * backward;
                    Vector3 currentPoint = center + arcDirection * _backsideDistance;

                    Gizmos.DrawLine(previousPoint, currentPoint);
                    previousPoint = currentPoint;
                }
            }

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

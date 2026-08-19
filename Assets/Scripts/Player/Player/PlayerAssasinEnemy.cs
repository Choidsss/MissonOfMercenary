using MissionOfMercenary;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class PlayerAssasinEnemy : MonoBehaviour
    {
        GameObject _enemy;

        [Header("Needed")]
        [SerializeField] InputReader _inputReader;

        [Header("Assasin Distance")]
        [SerializeField] LayerMask _enemyLayer;
        [SerializeField] float _distance;
        [SerializeField] float _offsetX;
        [SerializeField] float _offsetY;
        [SerializeField] int _assasinDamage = 1000;

        bool _willAssasin = false;
        bool _doAssasin = false;

        public bool DoAssasin => _doAssasin;

        public bool WillAssasin => _willAssasin;

        private void OnEnable()
        {
            _inputReader.OnAssasinAction += EnemyAssasin;
        }

        private void OnDisable()
        {
            _inputReader.OnAssasinAction -= EnemyAssasin;
        }

        // Update is called once per frame
        void Update()
        {
            AssasinEnableCheck();
        }

        void EnemyAssasin()
        {
            if (!_willAssasin) { return; }

            EnemyHealth enemyHealth = _enemy.GetComponent<EnemyHealth>();
            EnemyHit enemyHit = _enemy.GetComponent<EnemyHit>();

            if(enemyHealth == null) { Debug.Log("Can't Find A Component. EnemyHealth Can't Get From The Cast"); return; }

            //캐스트에 잡힌 후, 적의 뒤쪽영역에서 F키가 눌린다면 => 그 적의 정보를 가져와서 한번에 큰 데미지를 줌(즉사)
            enemyHealth.TakeDamege(_assasinDamage);
            _doAssasin = true;
        }

        void AssasinEnableCheck()
        {
            RaycastHit hit;
            // 캐스트 시작 위치를 _offsetY만큼 월드 Y축으로 보정한다. ByCodex
            Vector3 originPos = transform.position + Vector3.up * _offsetY;

            bool isEnemy = Physics.Raycast(originPos, transform.forward, out hit, _distance, _enemyLayer);

            if (isEnemy)
            {
                EnemyFindArea findArea = hit.collider.gameObject.GetComponentInParent<EnemyFindArea>();

                if (findArea == null) { Debug.Log("Can't Find A Component. findArea Can't Get From The Cast"); return; }

                _enemy = findArea.gameObject;

                if (findArea.CanAssasin)
                {
                    _willAssasin = true;
                }
                else
                {
                    _willAssasin = false;
                }
            }
            else
            {
                _enemy = null;
                _willAssasin = false;
            }
        }

        private void OnDrawGizmos()
        {
            // 실제 암살 판정과 동일하게 시작 위치를 _offsetY만큼 월드 Y축으로 보정한다. ByCodex
            Vector3 origin = transform.position + Vector3.up * _offsetY;
            Vector3 direction = transform.forward;

            // 적 레이어에 닿았는지 확인하여 기즈모의 색상과 끝 지점을 결정한다. ByCodex
            bool hitEnemy = Physics.Raycast(origin, direction, out RaycastHit hit, _distance, _enemyLayer);

            // 적에게 닿으면 초록색, 닿지 않으면 흰색으로 캐스트를 표시한다. ByCodex
            Gizmos.color = hitEnemy ? Color.green : Color.white;

            // 적에게 닿은 경우 충돌 지점까지, 그렇지 않은 경우 최대 거리까지 선을 그린다. ByCodex
            Vector3 endPoint = hitEnemy ? hit.point : origin + direction * _distance;
            Gizmos.DrawLine(origin, endPoint);
        }
    }
}

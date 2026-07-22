using MissionOfMercenary;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class SoundPositionGiveToEnemy : MonoBehaviour
    {
        AssultRifle _ar;

        [SerializeField] InputReader _inputReader;


        [Header("OverlapSphere Size Setting")]
        [SerializeField] float _radius;
        [SerializeField] LayerMask _enemyLayer;

        bool _didGive = false;

        void Start()
        {
            _ar = GetComponentInChildren<AssultRifle>();
        }

        private void OnEnable()
        {
            _inputReader.OnshotEvent += GiveToPosition;
        }

        private void OnDisable()
        {
            _inputReader.OnshotEvent -= GiveToPosition;
        }

        void GiveToPosition(float shot)
        {
            PositionGiveToEnemy();
        }

        public void PositionGiveToEnemy()
        {
            //if (_ar == null) { return; }

            Collider[] cols = Physics.OverlapSphere(transform.position, _radius, _enemyLayer, QueryTriggerInteraction.Ignore);
            Debug.Log($"{cols.Length}"); //적은 1명, 수는 11개가 잡힘 => Ragdoll콜라이더 분리로 인한 현상으로 추정 *** go도 가져오지만 ec가 없는거로보아 콜라이더가 붙어있는 오브젝트만 가져오는 듯

            foreach (Collider col in cols)
            {
                //콜라이더도 수정 필요
                GameObject go = col.gameObject;

                //여기서 콜라이더 체크한거로 부모에 있는 EnemyChase 가져와서 있는지 확인하는 코드
                EnemyChase ec = go.GetComponentInParent<EnemyChase>();

                if(ec == null) { Debug.Log("Can't Find a Component<EnemyChase>"); return; }
                
                //EnemyChase가 수색중이라면 Player의 위치 정보를 갱신하지 않음
                if(ec.DoFind)
                {
                    _didGive = true; 
                    return; 
                }
                else
                {
                    _didGive = false;
                }

                //얘가 위치를 계속 넘겨주는 코드
                ec.TargetPosition = transform.position;
                _didGive = true;
            }
        }
    }
}

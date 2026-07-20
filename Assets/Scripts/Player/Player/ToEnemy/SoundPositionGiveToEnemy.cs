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

        //Vector3 _soundPosition;

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



            //***********************ToDo : 현재, 부모쪽에서 EnemyChase를 가져오도록 해서 값을 넘겨주고는 있으나 추후 리팩토링 필수***********************
            foreach (Collider col in cols)
            {
                GameObject go = col.gameObject;

                EnemyChase ec = go.GetComponentInParent<EnemyChase>();

                if(ec == null) { Debug.Log("Can't Find a Component<EnemyChase>"); return; }

                
                ec.TargetPosition = transform.position;
            }
        }
    }
}

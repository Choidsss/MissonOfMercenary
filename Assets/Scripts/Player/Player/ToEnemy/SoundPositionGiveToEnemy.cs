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
            _ar = GetComponent<AssultRifle>();
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
            if (_ar == null) { return; }
            Debug.Log("Is ar Called?");
            Collider[] cols = Physics.OverlapSphere(transform.position, _radius, _enemyLayer, QueryTriggerInteraction.Ignore);
            Debug.Log("Exist Enemy Around");

            foreach (Collider col in cols)
            {
                GameObject go = col.gameObject;
                EnemyChase ec = go.GetComponent<EnemyChase>();

                if(ec == null) { Debug.Log("Can't Find a Component<EnemyChase>"); return; }

                Debug.Log("aaa");
                ec.TargetPosition = transform.position;
            }
        }
    }
}

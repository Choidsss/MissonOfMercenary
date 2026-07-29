using System.Collections.Generic;
using MissionOfMercenary;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class SoundPositionGiveToEnemy : MonoBehaviour
    {
        [SerializeField] InputReader _inputReader;

        [Header("OverlapSphere Size Setting")]
        [SerializeField] float _radius;
        [SerializeField] LayerMask _enemyLayer;

        private void OnEnable()
        {
            _inputReader.OnshotEvent += PositionGiveToEnemy;
        }

        private void OnDisable()
        {
            _inputReader.OnshotEvent -= PositionGiveToEnemy;
        }
        public void PositionGiveToEnemy(float shot)
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, _radius, _enemyLayer, QueryTriggerInteraction.Ignore);
            HashSet<EnemyChase> foundEnemies = new HashSet<EnemyChase>();
            HashSet<Collider> enemyColliders = new HashSet<Collider>();


            foreach (Collider col in cols)
            {
                EnemyChase enemy = col.GetComponentInParent<EnemyChase>();

                if (enemy != null && foundEnemies.Add(enemy))
                {
                    enemyColliders.Add(col);
                }
            }

            foreach (Collider col in enemyColliders)
            {
                col.GetComponentInParent<EnemyChase>().SetSoundTarget(transform.position);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.black;

            Gizmos.DrawWireSphere(transform.position, _radius);

        }
    }
}

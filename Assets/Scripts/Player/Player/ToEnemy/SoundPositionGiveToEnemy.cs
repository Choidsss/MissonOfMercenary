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
            Collider[] cols = Physics.OverlapSphere(transform.position, _radius, _enemyLayer, QueryTriggerInteraction.Ignore);
            // 여러 래그돌 콜라이더에서 찾은 동일한 Enemy는 HashSet으로 중복을 제거하여 위치를 한 번만 전달함. By Codex
            HashSet<EnemyChase> enemies = new HashSet<EnemyChase>();

            foreach (Collider col in cols)
            {
                EnemyChase enemy = col.GetComponentInParent<EnemyChase>();

                if (enemy != null)
                {
                    enemies.Add(enemy);
                }
            }

            foreach (EnemyChase enemy in enemies)
            {
                enemy.SetSoundTarget(transform.position);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class EnemyPatrolNode : MonoBehaviour
    {
        [SerializeField] float _patrolSpeed;
        [SerializeField] NavMeshAgent _nav;
        [SerializeField] Transform[] _wayPoint;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            EnemyPatrolMove();
        }

        void EnemyPatrolMove()
        {
            foreach (var way in _wayPoint)
            {
                Vector3 targetPos = way.position;
                _nav.SetDestination(targetPos);
            }
        }

        //public override State Evaluate()
        //{
            
        //}
    }
}

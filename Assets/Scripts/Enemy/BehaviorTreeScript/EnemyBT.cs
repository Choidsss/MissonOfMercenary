using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class EnemyBT : MonoBehaviour
    {
        [SerializeField] EnemyFindArea _findArea;
        [SerializeField] NavMeshAgent _agent;
        [SerializeField] EnemyPatrolNode _patrol;

        BTNode _root;

        void Awake()
        {
            if (_findArea == null)
            {
                _findArea = GetComponent<EnemyFindArea>();
            }

            if (_agent == null)
            {
                _agent = GetComponent<NavMeshAgent>();
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _root = SetupTree();
        }

        // Update is called once per frame
        void Update()
        {
            _root?.Evaluate();
        }

        BTNode SetupTree()
        {
            BehaviorSelector selector = new BehaviorSelector();

            //Add Node(chaseSeq)
            Sequence chaseSeq = new Sequence();
            chaseSeq.AddChild(new CanSeePlayerNode(_findArea));
            chaseSeq.AddChild(new ChasePlayerNode(_agent, _findArea));
            //chaseSeq.AddChild(new EnemyPatrolNode(_patrol));

            selector.AddChild(chaseSeq);

            return selector;
        }
    }
}

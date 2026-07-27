using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class EnemyBT : MonoBehaviour
    {
        [SerializeField] EnemyFindArea _findArea;
        // 일반 BTNode는 Inspector에서 연결할 수 없으므로 이 필드 대신 SetupTree에서 생성해야 한다. By Codex

        [Header("Patrol Node Options")]
        //[SerializeField] EnemyPatrolNode _patrol;
        [SerializeField] float _patrolSpeed;
        [SerializeField] NavMeshAgent _nav;
        [SerializeField] Transform[] _wayPoints;

        BTNode _root;

        void Awake()
        {
            if (_findArea == null)
            {
                _findArea = GetComponent<EnemyFindArea>();
            }

            if (_nav == null)
            {
                _nav = GetComponent<NavMeshAgent>();
            }
        }

        void Start()
        {
            _root = SetupTree();
        }

        void Update()
        {
            _root?.Evaluate();
        }

        BTNode SetupTree()
        {
            BehaviorSelector selector = new BehaviorSelector();

            Sequence chaseSeq = new Sequence();
            chaseSeq.AddChild(new CanSeePlayerNode(_findArea));
            chaseSeq.AddChild(new ChasePlayerNode(_nav, _findArea));

            EnemyPatrolNode patrolNode = new EnemyPatrolNode(_nav, _wayPoints, _patrolSpeed);

            selector.AddChild(chaseSeq);
            selector.AddChild(patrolNode);

            // PatrolNode를 (_agent, _wayPoint, _patrolSpeed)로 생성해 selector에 추가하지 않으면 플레이어가 없을 때 아무 행동도 하지 않는다. By Codex

            return selector;
        }
    }
}

using UnityEngine;
using UnityEngine.AI;

namespace MIssionOfMercenary
{
    public class EnemyBT : MonoBehaviour
    {
        BTNode _root;
        EnemyChase _enemyChase;

        [SerializeField] EnemyFindArea _findArea;
        [SerializeField] NavMeshAgent _nav;

        [Header("Patrol Node Options")]
        [SerializeField] float _patrolSpeed;
        [SerializeField] Transform[] _wayPoints;

        [Header("MoveToSoundPosition Options")]
        [SerializeField] float _moveSoundPositionSpeed;

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
            _enemyChase = GetComponent<EnemyChase>();
            if (_enemyChase == null) { Debug.Log("It's You!!!!!"); }

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

            Sequence soundPositionSeq = new Sequence();
            soundPositionSeq.AddChild(new HasSoundTargetNode(_enemyChase));
            soundPositionSeq.AddChild(new ChaseSoundPositionNode(_moveSoundPositionSpeed, _nav, _enemyChase));

            EnemyPatrolNode patrolNode = new EnemyPatrolNode(_nav, _wayPoints, _patrolSpeed);

            selector.AddChild(chaseSeq);
            selector.AddChild(soundPositionSeq);
            selector.AddChild(patrolNode);


            return selector;
        }
    }
}

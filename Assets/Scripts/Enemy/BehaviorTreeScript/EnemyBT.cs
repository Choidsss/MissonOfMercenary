using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace MIssionOfMercenary
{
    public enum EnemyBehaviorType
    {
        Patrol,
        Sentry
    }

    public class EnemyBT : MonoBehaviour
    {
        BTNode _root;
        EnemyBT _enemyBT;
        EnemyChase _enemyChase;
        EnemyAttack _enemyAttack;
        EnemyAnimation _enemyAnimation;
        EnemyFindArea _findArea;
        EnemyHealth _enemyHealth;

        SentrySensor _sensor;
        SentryEnemyAttack _sentryAttack;


        [Header("BehaviorType")]
        [SerializeField] EnemyBehaviorType _behaviorType;

        [Header("WayPointManager")]
        [SerializeField] WayPointManager _wayPointManager;

        [Header("Enemy Turn Speed Amount")]
        [SerializeField] float _turnSpeed = 1.0f;
        //[SerializeField] float _lookAngleOffset = 1.0f;
        //[SerializeField] float _lookVerticalOffset = 1.0f; //양수면 아래, 음수면 위

        [Header("Nav Mesh Agent")]
        [SerializeField] NavMeshAgent _nav;

        [Header("Patrol Node Options")]
        [SerializeField] string _routeID;
        [SerializeField] float _patrolSpeed;

        [Header("MoveToSoundPosition Options")]
        [SerializeField] float _moveSoundPositionSpeed;

        Transform[] _wayPoint;
        public bool IsBlocked { get; set; } = false;


        void Awake()
        {
            _enemyBT = GetComponent<EnemyBT>();
            _enemyAnimation = GetComponent<EnemyAnimation>();
            _enemyHealth = GetComponent<EnemyHealth>();
            _findArea = GetComponent<EnemyFindArea>();
            _nav = GetComponent<NavMeshAgent>();
            _enemyAttack = GetComponent<EnemyAttack>();

            _sensor = GetComponent<SentrySensor>();
            _sentryAttack = GetComponent<SentryEnemyAttack>(); // Sentry 전용 공격 컴포넌트를 현재 구조에서 가져옵니다. By Codex
        }

        void Start()
        {
            _enemyChase = GetComponent<EnemyChase>();

            GetRouteFromDictionary();
            _root = SetupTree();
        }

        void Update()
        {
            _root?.Evaluate();
        }

        /*
         * 추가 :
         * 적의 WayPoint가 하나만 있고, 플레이어가 일정 거리 안으로 들어온다면, 바로 공격 자세를 취하고 공격하도록 함(쫓지는 않고 제자리에서 게속 공격만)
         */
        BTNode SetupTree()
        {
            switch (_behaviorType)
            {
                case EnemyBehaviorType.Patrol:
                    return SetupPatrolEnemyTree();

                case EnemyBehaviorType.Sentry:
                    return SetupSentryEnemyTree();

                default:
                    return SetupPatrolEnemyTree();
            }
        }

        BTNode SetupPatrolEnemyTree()
        {
            BehaviorSelector selectorTree = new BehaviorSelector();

            BehaviorSelector typeSelector = new BehaviorSelector();
            //typeSelector.AddChild(new EnemySelectType())

            //공격 시퀀스 => 공격범위 안쪽인지 확인하는 노드, 공격하는 노드
            Sequence attackSeq = new Sequence();
            attackSeq.AddChild(new IsPlayerInEnemyAttackRange(_enemyAttack, _enemyAnimation, _findArea, _nav));
            attackSeq.AddChild(new EnemyFireNode(_enemyBT, _enemyHealth, _enemyAnimation, _enemyAttack, _findArea, _nav));

            //셀렉터 생성
            BehaviorSelector combatSelector = new BehaviorSelector();
            combatSelector.AddChild(attackSeq);//공격 시퀀스 실행
            combatSelector.AddChild(new ChaseToPlayerNode(_enemyAttack, _enemyAnimation, _findArea, _nav));//위 시퀀스가 실패했을 경우 Player를 쫓음

            Sequence combatSeq = new Sequence();
            combatSeq.AddChild(new IsDetectedPlayerNode(_findArea)); //먼저 플레이어의 위치를 아는지 확인하는 노드
            combatSeq.AddChild(combatSelector);//시퀀스 조립

            Sequence moveToSoundPositionSeq = new Sequence();
            moveToSoundPositionSeq.AddChild(new HasSoundTargetNode(_enemyChase));
            moveToSoundPositionSeq.AddChild(new ChaseSoundPositionNode(_moveSoundPositionSpeed, _nav, _enemyChase));

            selectorTree.AddChild(combatSeq);
            selectorTree.AddChild(moveToSoundPositionSeq);
            selectorTree.AddChild(new EnemyPatrolNode(_nav, _patrolSpeed, _wayPoint));

            return selectorTree;
        }

        /*
         * 고정형 적 BTTree 구조
         * 방향은 전방 고정, 일정 범위 내로 들어오면 무조건 공격 -> 일단 쏘는건 계속 하되, 똑같이 30발 제한, 30발 다쏘면 일종의 일레이 시키고, 플레이어가 엄폐물에 숨으면 잠시 사격 정지,
         */
        BTNode SetupSentryEnemyTree()
        {
            BehaviorSelector selectorTree = new BehaviorSelector();

            Sequence combatSeq = new Sequence();
            combatSeq.AddChild(new EnemySightNode(_sensor, _findArea));
            combatSeq.AddChild(new SentryEnemyAttackNode(_enemyBT, _sentryAttack, _sensor)); // 시야가 확보되면 풀링 연사 노드를 실행합니다. By Codex
            selectorTree.AddChild(combatSeq); // 완성한 Sentry 공격 시퀀스를 루트 Selector에 연결합니다. By Codex


            //BehaviorSelector typeSelector = new BehaviorSelector();
            ////typeSelector.AddChild(new EnemySelectType())

            ////공격 시퀀스 => 공격범위 안쪽인지 확인하는 노드, 공격하는 노드
            //Sequence attackSeq = new Sequence();
            //attackSeq.AddChild(new IsPlayerInEnemyAttackRange(_enemyAttack, _enemyAnimation, _findArea, _nav));
            //attackSeq.AddChild(new EnemyFireNode(_enemyBT, _enemyHealth, _enemyAnimation, _enemyAttack, _findArea, _nav));

            ////셀렉터 생성
            //BehaviorSelector combatSelector = new BehaviorSelector();
            //combatSelector.AddChild(attackSeq);//공격 시퀀스 실행
            //combatSelector.AddChild(new ChaseToPlayerNode(_enemyAttack, _enemyAnimation, _findArea, _nav));//위 시퀀스가 실패했을 경우 Player를 쫓음

            //Sequence combatSeq = new Sequence();
            //combatSeq.AddChild(new IsDetectedPlayerNode(_findArea)); //먼저 플레이어의 위치를 아는지 확인하는 노드
            //combatSeq.AddChild(combatSelector);//시퀀스 조립

            //Sequence moveToSoundPositionSeq = new Sequence();
            //moveToSoundPositionSeq.AddChild(new HasSoundTargetNode(_enemyChase));
            //moveToSoundPositionSeq.AddChild(new ChaseSoundPositionNode(_moveSoundPositionSpeed, _nav, _enemyChase));

            //selectorTree.AddChild(combatSeq);
            //selectorTree.AddChild(moveToSoundPositionSeq);
            //selectorTree.AddChild(new EnemyPatrolNode(_nav, _patrolSpeed, _wayPoint));

            return selectorTree;
        }

        void GetRouteFromDictionary()
        {
            if (_wayPointManager == null)
            {
                Debug.LogError($"{name}: WayPointManager가 없습니다.");
                return;
            }

            bool Right = _wayPointManager.TryGetRoutePoint(_routeID, out _wayPoint);
            Debug.Log(_wayPoint);
        }

        public void LookAtPlayer()
        {
            Vector3 direction = _findArea.DetectedTarget - transform.position;
            direction.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            //targetRotation = targetRotation * Quaternion.Euler(_lookVerticalOffset, _lookAngleOffset, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _turnSpeed * Time.deltaTime);
        }

        public void LookAtTarget(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - transform.position; // 센서가 넘긴 현재 플레이어 위치로 방향을 계산합니다. By Codex
            direction.y = 0f; // 고정형 적이 위아래로 기울어지지 않게 수평 회전만 사용합니다. By Codex

            if (direction.sqrMagnitude <= 0.001f) { return; } // 방향이 0에 가까울 때 잘못된 회전 생성을 방지합니다. By Codex

            Quaternion targetRotation = Quaternion.LookRotation(direction); // 플레이어를 향하는 목표 회전을 만듭니다. By Codex
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _turnSpeed * Time.deltaTime); // 설정한 속도로 플레이어 방향을 계속 바라봅니다. By Codex
        }
    }
}

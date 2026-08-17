using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace MIssionOfMercenary
{
    public class EnemyBT : MonoBehaviour
    {
        BTNode _root;
        EnemyBT _enemyBT;
        EnemyChase _enemyChase;
        EnemyAttack _enemyAttack;
        EnemyAnimation _enemyAnimation;
        EnemyFindArea _findArea;
        EnemyHealth _enemyHealth;

        //[Header("Look Player options")]
        //[SerializeField] float _aaa;

        [Header("Enemy Turn Speed Amount")]
        [SerializeField] float _turnSpeed = 1.0f;
        [SerializeField] float _lookAngleOffset = 1.0f;
        [SerializeField] float _lookVerticalOffset = 1.0f; //양수면 아래, 음수면 위

        [Header("Nav Mesh Agent")]
        [SerializeField] NavMeshAgent _nav;

        [Header("Patrol Node Options")]
        [SerializeField] float _patrolSpeed;
        [SerializeField] Transform[] _wayPoints;

        [Header("MoveToSoundPosition Options")]
        [SerializeField] float _moveSoundPositionSpeed;


        void Awake()
        {
            _enemyBT = GetComponent<EnemyBT>();
            _enemyAnimation = GetComponent<EnemyAnimation>();
            _enemyHealth = GetComponent<EnemyHealth>();

            if (_findArea == null)
            {
                _findArea = GetComponent<EnemyFindArea>();
            }

            if (_nav == null)
            {
                _nav = GetComponent<NavMeshAgent>();
            }

            if (_enemyAttack == null)
            {
                _enemyAttack = GetComponent<EnemyAttack>();
            }
        }

        void Start()
        {
            _enemyChase = GetComponent<EnemyChase>();

            _root = SetupTree();
        }

        void Update()
        {
            _root?.Evaluate();
        }

        BTNode SetupTree()
        {
            BehaviorSelector selectorTree = new BehaviorSelector();

            //공격 시퀀스 => 공격범위 안쪽인지 확인하는 노드, 공격하는 노드
            Sequence attackSeq = new Sequence();
            attackSeq.AddChild(new IsPlayerInEnemyAttackRange(_enemyAttack, _enemyAnimation,_findArea, _nav));
            attackSeq.AddChild(new EnemyFireNode(_enemyBT , _enemyHealth , _enemyAnimation,_enemyAttack, _findArea, _nav));

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
            selectorTree.AddChild(new EnemyPatrolNode(_nav, _wayPoints, _patrolSpeed));

            return selectorTree;
        }

        public void LookAtPlayer()
        {
            Vector3 direction = _findArea.DetectedTarget - transform.position;
            direction.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            //targetRotation = targetRotation * Quaternion.Euler(_lookVerticalOffset, _lookAngleOffset, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _turnSpeed * Time.deltaTime);
        }
    }
}

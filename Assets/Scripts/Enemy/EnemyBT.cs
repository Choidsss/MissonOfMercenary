using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyBT : MonoBehaviour
    {
        BTNode _root;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            SetupTree();
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        BTNode SetupTree()
        {
            BehaviorSelector selector = new BehaviorSelector();

            Sequence patrolSeq = new Sequence();
            Sequence chaseSeq = new Sequence();
            Sequence attackSeq = new Sequence();



            //patrolSequence Add



            //ChaseSequence Add

            //AttackSequence Add

            selector.AddChild(patrolSeq);
            selector.AddChild(chaseSeq);
            selector.AddChild(attackSeq);

            return selector;
        }
    }
}

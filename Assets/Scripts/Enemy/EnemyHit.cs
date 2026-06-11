using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyHit : MonoBehaviour, IDamageable
    {
        public bool IsDeath { get; set; } = false;



        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void TakeDameged(int damege)
        {
            throw new System.NotImplementedException();
        }

        public void Death()
        {
            throw new System.NotImplementedException();
        }
    }
}

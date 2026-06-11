using UnityEngine;

namespace MIssionOfMercenary
{
    public interface IDamageable
    {
        public bool IsDeath { get; set; }

        void TakeDameged(int damege);

        void Death();
    }
}

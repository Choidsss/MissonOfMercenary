using UnityEngine;

namespace MIssionOfMercenary
{
    public interface IHitReceiver
    {
        void ReceiveHit(RaycastHit hit, int damage);
    }
}

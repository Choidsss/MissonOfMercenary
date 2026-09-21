using UnityEngine;

namespace MIssionOfMercenary
{
    public interface IFirearm : IWeapons
    {
        void TriggeredPressed();//¹æ¾Æ¼è¸¦ ´ç±è
        void TriggeredReleased();//¹æ¾Æ¼è¸¦ ¶À
        void TryReload();//ÀçÀåÀü
    }
}

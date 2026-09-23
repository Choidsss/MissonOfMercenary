using UnityEngine;

namespace MIssionOfMercenary
{
    public interface IFirearm : IWeapons
    {
        public int CurrentAmmo { get; }

        void TriggeredPressed();//¹æ¾Æ¼è¸¦ ´ç±è
        void TriggeredReleased();//¹æ¾Æ¼è¸¦ ¶À
        void TryReload();//ÀçÀåÀü
    }
}

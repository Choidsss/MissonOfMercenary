using UnityEngine;
using System.Collections.Generic;

namespace MIssionOfMercenary
{


    public class WayPointManager : MonoBehaviour
    {
        readonly Dictionary<string, Transform[]> _dicWayPoint = new Dictionary<string, Transform[]>();

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            SaveWayPoints();
        }

        void SaveWayPoints()
        {
            foreach(Transform pointRoute in transform)
            {
                string routeID = pointRoute.name;

                if (_dicWayPoint.ContainsKey(routeID))
                {
                    Debug.Log("중복된 ID입니다.!!! 다시한번 확인하십시오!!!");
                    return;
                }

                Transform[] wayPoint = new Transform[pointRoute.childCount];

                for(int i = 0;i < pointRoute.childCount; i++)
                {
                    wayPoint[i] = pointRoute.GetChild(i);
                }

                _dicWayPoint.Add(routeID, wayPoint);
            }
        }

        public bool TryGetRoutePoint(string routeID, out Transform[] wayPoints)
        {
            return _dicWayPoint.TryGetValue(routeID, out wayPoints);
        }
    }
}

using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyChase : MonoBehaviour
    {
        EnemyFindArea _findArea;

        /*
         * 적이 날 찾았다면 그 쪽으로 이동하도록 만들기
         */

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _findArea = GetComponent<EnemyFindArea>();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}

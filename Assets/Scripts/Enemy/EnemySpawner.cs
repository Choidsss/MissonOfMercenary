using UnityEngine;
using System.Collections.Generic;

namespace MIssionOfMercenary
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Player Transform")]
        [SerializeField] Transform _player;

        [Header("SpawnEnemyList")]
        [SerializeField] List<GameObject> _enemiesList = new List<GameObject>();

        [Header("FirePoint")]
        [SerializeField] List<Transform> _firePoints = new List<Transform>();

        [Header("Total Spawn Count")]
        [SerializeField] int _totalCount;


        int _currentCount = 0;

        // Update is called once per frame
        void Update()
        {
        
        }

        void SpawnEnemy()
        {
            if (_player == null || _enemiesList.Count == 0 || _firePoints.Count == 0)
            {
                return;
            }

            if (_currentCount >= _totalCount)
            {
                return;
            }

            //List에 있는 것들중 Random하게 Spawn시킬예정
            int index = Random.Range(0, _enemiesList.Count);
            GameObject go = _enemiesList[index];

            int firePointIndex = Random.Range(0, _firePoints.Count);
            Transform tr = _firePoints[firePointIndex];

            if(go == null || tr == null)
            {
                return;
            }

            //생성까진 완료
            GameObject enemy = Instantiate(go, transform.position, transform.rotation);

            //적들에게서 Initialize해주는거 까지가 얘의 역할
            //알아서 공격 갈꺼고, 죽을거고. 
            //카운팅 판단해서 빈 firePoint찾아서 넣는거로 수정 필요하긴 함

            EnemyCombatController enemyCombatController = enemy.GetComponent<EnemyCombatController>();

            if (enemyCombatController == null)
            {
                Debug.LogWarning("EnemyCombatController컴포넌트가 존재하지 않습니다");
                return;
            }

            _currentCount++;
            enemyCombatController.Initialize(_player, tr);
        }
    }
}

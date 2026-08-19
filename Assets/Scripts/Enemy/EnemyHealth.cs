using UnityEngine;
using System.Collections;

namespace MIssionOfMercenary
{
    public class EnemyHealth : MonoBehaviour
    {
        EnemyHit _enemyHit;

        [Header("Enemy Health Option")]
        [SerializeField] int _enemyHealth = 100;

        [Header("Enemy Death Delay Sec Option")]
        [SerializeField] float _deathDelay = 3.0f;
        bool _isDeath;

        public bool IsDeath { get { return _isDeath; } private set { _isDeath = value; } }

        private void Awake()
        {
            _enemyHit = GetComponent<EnemyHit>();
        }

        public void TakeDamege(int damaged)
        {
            _enemyHealth = _enemyHealth - damaged;
            _enemyHealth = Mathf.Clamp(_enemyHealth, 0, 100);

            Debug.Log($"현재 남아있는 적의 체력은 {_enemyHealth} 입니다.");

            if (_enemyHealth <= 0)
            {
                _isDeath = true;
                _enemyHit.SetRagdoll(true);
                EnemyOnDeath();
            }
        }

        public void EnemyOnDeath()
        {
            if (_isDeath)
            {
                Debug.Log("적이 사살 되었습니다.");

                StartCoroutine(EnemyDeathDelay());
            }
        }

        IEnumerator EnemyDeathDelay()
        {
            yield return new WaitForSeconds(_deathDelay);
            Debug.Log("Dealyed");
            Destroy(this.gameObject);
        }
    }
}

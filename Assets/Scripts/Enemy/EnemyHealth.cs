using UnityEngine;

namespace MIssionOfMercenary
{
    public class EnemyHealth : MonoBehaviour
    {
        EnemyHit _enemyHit;

        [SerializeField] int _enemyHealth = 100;
        bool _isDeath;

        public bool IsDeath { get { return _isDeath; } private set { _isDeath = value; } }

        public void TakeDamege(int damaged)
        {
            _enemyHealth = _enemyHealth - damaged;
            _enemyHealth = Mathf.Clamp(_enemyHealth, 0, 100);

            Debug.Log($"현재 남아있는 적의 체력은 {_enemyHealth} 입니다.");

            if (_enemyHealth <= 0)
            {
                _isDeath = true;
            }
        }

        public void EnemyOnDeath()
        {
            if (_isDeath)
            {
                Debug.Log("적이 사살 되었습니다.");
                //Death;
            }
        }
    }
}

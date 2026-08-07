using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] EnemyAttack _enemyAttack;

        [Header("Player Health Recovery Option")]
        [SerializeField] int _recoveryAmount = 30;
        [SerializeField] int _maxHealth = 200;
        [SerializeField] float _delay = 5f;

        int _currentHealth;

        private void Start()
        {
            _currentHealth = _maxHealth;
        }

        private void Update()
        {
            PlayerHealthRecovery();
        }

        public void PlayerChangeHealth(int damaged)
        {
            if(damaged != 0)
            {
                _currentHealth = _currentHealth - damaged;
                _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
            }
        }

        //만약 플레이어가 체력이 까여도 일정시간동안 공격을 받지 않으면 체력이 회복되도록 만든다.
        public void PlayerHealthRecovery()
        {
            if(_currentHealth != _maxHealth)
            StartCoroutine(PlayerRecoveryRoutine());

            _currentHealth = _currentHealth + _recoveryAmount;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

            if(_currentHealth == _maxHealth)
            {
                StopCoroutine(PlayerRecoveryRoutine());
            }
        }

        IEnumerator PlayerRecoveryRoutine()
        {
            yield return new WaitForSeconds(_delay);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(collision == null) { return; }

            //LayerName == EnemyAmmo(22)
            if(collision.gameObject.layer == 22)
            {
                PlayerChangeHealth(_enemyAttack.Damage);
                Destroy(collision.gameObject);
            }
        }
    }
}

using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] EnemyAttack _enemyAttack;

        [Header("Player Health Recovery Option")]
        [SerializeField] int _recoveryAmount = 30; //회복량
        [SerializeField] int _maxHealth = 200; //최대 체력
        [SerializeField] float _delay = 5f; // 몇초 뒤에 회복시킬것인가
        [SerializeField] float _recoveryInterval = 1f; //몇초마다 회복되게 할 것인가

        int _currentHealth;//현재 체력
        float _nextRecoveryTime;//맞고 났을때의 시간 저장


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
            //맞은 데미지가 없다면, 현재 체력이 0이라면 리턴
            if (damaged <= 0 || _currentHealth <= 0)
            {
                return;
            }

            _currentHealth = Mathf.Clamp(_currentHealth - damaged, 0, _maxHealth);//여기서 바로 체력까고, Clamp까지

            _nextRecoveryTime = Time.time + _delay;//현재맞은시간 + Delay 해서 총 몇초뒤에 다시 회복시킬것인지 결정
        }

        public void PlayerHealthRecovery()
        {
            if (_currentHealth <= 0 || _currentHealth >= _maxHealth)
            {
                return;
            }

            if (Time.time < _nextRecoveryTime)
            {
                return;
            }

            _currentHealth = Mathf.Min(_currentHealth + _recoveryAmount, _maxHealth);

            _nextRecoveryTime = Time.time + _recoveryInterval;//다음 회복시간 갱신
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(collision == null) { return; }

            //LayerName == EnemyAmmo(22)
            if(collision.gameObject.layer == 22)
            {
                PlayerChangeHealth(_enemyAttack.Damage);
                // EnemyBullet returns pooled bullets; do not destroy them here.
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using cowsins;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    EnemyRagdoll _ragdoll;

    [Header("UI")]
    [SerializeField] Image _fill; 
    [SerializeField] float _maxHealth = 100.0f;

    float _currentHealth = 0.0f;

    void Start()
    {
        _ragdoll = GetComponent<EnemyRagdoll>();
        _currentHealth = _maxHealth;
        UpdateHealthbar();
    }

    void UpdateHealthbar()
    {
        if(_fill != null)
        {
            Debug.Log($"fillAmount: {_currentHealth / _maxHealth}");
            _fill.fillAmount = _currentHealth / _maxHealth;
        }
    }

    public void Damage(float damage, bool isHeadshot)
    {
        if (damage != 0)
        {
            Debug.Log($"damage : {_currentHealth}");
            _currentHealth -= damage;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

            if (_currentHealth <= 0)
            {
                Die();
            }

            UpdateHealthbar();
        }
    }

    void Die()
    {
        _ragdoll.ActiveRagdoll();
    }
}

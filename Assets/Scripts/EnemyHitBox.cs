using cowsins;
using UnityEngine;

public class EnemyHitBox : MonoBehaviour, IDamageable
{
    EnemyHealth _enemyHealth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _enemyHealth = GetComponentInParent<EnemyHealth>();
    }

    public void Damage(float damage, bool isHeadshot)
    {
        _enemyHealth.Damage(damage, isHeadshot);
    }
}

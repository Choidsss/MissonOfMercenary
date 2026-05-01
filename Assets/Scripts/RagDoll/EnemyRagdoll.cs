using UnityEngine;

public class EnemyRagdoll : MonoBehaviour
{
    Animator _anim;
    Rigidbody[] _rigidBodies;
    Collider[] _colliders;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _anim = GetComponent<Animator>();
        _rigidBodies = GetComponentsInChildren<Rigidbody>();
        _colliders = GetComponentsInChildren<Collider>();

        SetRagdoll(false);   
    }

    public void ActiveRagdoll()
    {
        _anim.enabled = false;
        SetRagdoll(true);
    }

    void SetRagdoll(bool active)
    {
        foreach (Rigidbody rb in _rigidBodies)
        {
            rb.isKinematic = !active;
        }
    }
}

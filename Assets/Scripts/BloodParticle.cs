using UnityEngine;

public class BloodParticle : MonoBehaviour
{
    [SerializeField] float lifetime = 0.5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
using UnityEngine;

public class NPCHead : MonoBehaviour
{
    public Rigidbody2D head;
    public GameObject torso;
    public GameObject bloodPrefab;
    public Transform chinTransform;
    public RingFlash ringFlash;
    public NPCKnockdown knockdown;

    float targetAngleOffset = 40f;
    float baseAngleOffset = 40f;

    [SerializeField] float hitRotationAmount = 30f;
    [SerializeField] float returnSpeed = 2f;
    [SerializeField] float detectionRadius = 0.5f;

    int hitCount = 0;
    bool knockedDown = false;
    bool recentlyHit = false;

    void Start()
    {
        head = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (knockedDown) return;

        stickToTorso();
        updateRotation();
        if (!recentlyHit)
        {
            checkForHits();
        }
    }

    public void stickToTorso()
    {
        head.transform.position = torso.transform.position;
    }

    public void updateRotation()
    {
        targetAngleOffset = Mathf.LerpAngle(targetAngleOffset, baseAngleOffset, Time.deltaTime * returnSpeed);
        head.transform.rotation = torso.transform.rotation * Quaternion.Euler(0f, 0f, targetAngleOffset);
    }

    void checkForHits()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("RightHand") || hit.CompareTag("LeftHand"))
            {
                RegisterHit(hit.CompareTag("RightHand"));
                recentlyHit = true;
                Invoke("ResetHit", 0.5f);
                break;
            }
        }
    }

    void RegisterHit(bool isRightHand)
    {
        hitCount++;

        if (hitCount >= 4)
        {
            knockedDown = true;
            if (ringFlash != null) ringFlash.StayHit();
            if (knockdown != null) knockdown.Knockdown();
            return;
        }

        if (isRightHand)
            targetAngleOffset = baseAngleOffset - hitRotationAmount;
        else
            targetAngleOffset = baseAngleOffset + hitRotationAmount;

        SpawnBlood();
        if (ringFlash != null) ringFlash.Flash();
    }

    void ResetHit()
    {
        recentlyHit = false;
    }

    void SpawnBlood()
    {
        if (bloodPrefab != null && chinTransform != null)
        {
            Instantiate(bloodPrefab, chinTransform.position, chinTransform.rotation);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
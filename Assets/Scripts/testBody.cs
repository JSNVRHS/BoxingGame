using UnityEngine;

public class testBody : MonoBehaviour
{
    public GameObject torso;

    [Header("Ownership")]
    [SerializeField] Transform ownerRoot;
    [SerializeField] Transform opponentRoot;

    [Header("Effects / Knockdown")]
    [SerializeField] RingFlash ringFlash;
    [SerializeField] NPCKnockdown knockdown;

    [Header("Body Hit Reaction")]
    [SerializeField] float detectionRadius = 0.5f;
    [SerializeField] float hitCooldown = 0.5f;
    [SerializeField] int hitsToKnockdown = 3;

    Torso ownerTorso;
    int hitCount = 0;
    bool knockedDown = false;
    bool recentlyHit = false;

    void Start()
    {
        ownerTorso = torso != null ? torso.GetComponent<Torso>() : GetComponent<Torso>();
    }

    void Update()
    {
        if (knockedDown) return;

        if (!recentlyHit)
        {
            CheckForHits();
        }
    }

    void CheckForHits()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        foreach (Collider2D hit in hits)
        {
            if (ownerRoot != null && (hit.transform == ownerRoot || hit.transform.IsChildOf(ownerRoot)))
                continue;

            if (opponentRoot != null && hit.transform != opponentRoot && !hit.transform.IsChildOf(opponentRoot))
                continue;

            RightHand rightHand = hit.GetComponent<RightHand>();
            if (rightHand != null && rightHand.IsPunching)
            {
                if (!CanTakeBodyHit() || !rightHand.IsBodyPunch)
                {
                    continue;
                }

                RegisterHit();
                return;
            }

            BrawlerRightHand brawlerRightHand = hit.GetComponent<BrawlerRightHand>();
            if (brawlerRightHand != null && brawlerRightHand.IsPunching)
            {
                if (!CanTakeBodyHit() || !brawlerRightHand.IsBodyPunch)
                {
                    continue;
                }

                RegisterHit();
                return;
            }

            LeftHand leftHand = hit.GetComponent<LeftHand>();
            if (leftHand != null && leftHand.IsPunching)
            {
                if (!CanTakeBodyHit() || !leftHand.IsBodyPunch)
                {
                    continue;
                }

                RegisterHit();
                return;
            }

            BrawlerLeftHand brawlerLeftHand = hit.GetComponent<BrawlerLeftHand>();
            if (brawlerLeftHand != null && brawlerLeftHand.IsPunching)
            {
                if (!CanTakeBodyHit() || !brawlerLeftHand.IsBodyPunch)
                {
                    continue;
                }

                RegisterHit();
                return;
            }
        }
    }

    void RegisterHit()
    {
        hitCount++;
        recentlyHit = true;
        Invoke(nameof(ResetHit), hitCooldown);
        Debug.Log($"{name}: hit on body. Body hits = {hitCount}.");

        if (hitCount >= hitsToKnockdown)
        {
            knockedDown = true;
            Debug.Log($"{name}: knocked down from body hits.");

            if (ringFlash != null)
                ringFlash.StayHit();

            if (knockdown != null)
                knockdown.Knockdown();

            return;
        }

        if (ringFlash != null)
            ringFlash.Flash();
    }

    void ResetHit()
    {
        recentlyHit = false;
    }

    bool CanTakeBodyHit()
    {
        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}

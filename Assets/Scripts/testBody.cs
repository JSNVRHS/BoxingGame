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
        ResolveOwnerTorso();
    }

    void Update()
    {
        if (knockedDown) return;

        ResolveOwnerTorso();
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

                if (IsBlockedByIdleHand(hit))
                {
                    Debug.Log($"{name}: body punch negated by idle guard hand.");
                    return;
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

                if (IsBlockedByIdleHand(hit))
                {
                    Debug.Log($"{name}: body punch negated by idle guard hand.");
                    return;
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

                if (IsBlockedByIdleHand(hit))
                {
                    Debug.Log($"{name}: body punch negated by idle guard hand.");
                    return;
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

                if (IsBlockedByIdleHand(hit))
                {
                    Debug.Log($"{name}: body punch negated by idle guard hand.");
                    return;
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

    bool IsBlockedByIdleHand(Collider2D attackingCollider)
    {
        if (ownerTorso == null || !ownerTorso.duck)
        {
            return false;
        }

        if (ownerRoot == null || attackingCollider == null)
        {
            return false;
        }

        Collider2D[] blockingColliders = ownerRoot.GetComponentsInChildren<Collider2D>(true);
        foreach (Collider2D blockingCollider in blockingColliders)
        {
            if (blockingCollider == null || blockingCollider == attackingCollider)
            {
                continue;
            }

            if (!blockingCollider.CompareTag("LeftHand") && !blockingCollider.CompareTag("RightHand"))
            {
                continue;
            }

            if (!IsIdleDefenderHand(blockingCollider))
            {
                continue;
            }

            if (blockingCollider.IsTouching(attackingCollider))
            {
                return true;
            }
        }

        return false;
    }

    bool IsIdleDefenderHand(Collider2D handCollider)
    {
        RightHand rightHand = handCollider.GetComponent<RightHand>();
        if (rightHand != null)
        {
            return !rightHand.IsPunching;
        }

        LeftHand leftHand = handCollider.GetComponent<LeftHand>();
        if (leftHand != null)
        {
            return !leftHand.IsPunching;
        }

        BrawlerRightHand brawlerRightHand = handCollider.GetComponent<BrawlerRightHand>();
        if (brawlerRightHand != null)
        {
            return !brawlerRightHand.IsPunching;
        }

        BrawlerLeftHand brawlerLeftHand = handCollider.GetComponent<BrawlerLeftHand>();
        if (brawlerLeftHand != null)
        {
            return !brawlerLeftHand.IsPunching;
        }

        return false;
    }

    void ResolveOwnerTorso()
    {
        if (ownerTorso == null)
        {
            if (torso != null)
            {
                ownerTorso = torso.GetComponent<Torso>();
            }

            if (ownerTorso == null && ownerRoot != null)
            {
                ownerTorso = ownerRoot.GetComponentInChildren<Torso>(true);
            }

            if (ownerTorso == null)
            {
                ownerTorso = GetComponentInParent<Torso>();
            }
        }

        if (torso == null && ownerTorso != null)
        {
            torso = ownerTorso.gameObject;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}

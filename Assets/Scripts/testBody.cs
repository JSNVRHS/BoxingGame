using UnityEngine;

public class testBody : MonoBehaviour
{
    public GameObject torso;

    [Header("Ownership")]
    [SerializeField] Transform ownerRoot;
    [SerializeField] Transform opponentRoot;

    [Header("Effects / Knockdown")]
    [SerializeField] GameObject bloodPrefab;
    [SerializeField] Transform bloodSpawnPoint;
    [SerializeField] RingFlash ringFlash;
    [SerializeField] NPCKnockdown knockdown;

    [Header("Body Hit Reaction")]
    [SerializeField] float detectionRadius = 0.5f;
    [SerializeField] float hitCooldown = 0.5f;
    [SerializeField] int maxBodyHp = 10;

    Torso ownerTorso;
    int currentBodyHp;
    bool knockedDown = false;
    bool recentlyHit = false;

    void Start()
    {
        ResolveOwnerTorso();
        currentBodyHp = maxBodyHp;
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

            if (ImpactDamageUtility.TryGetHandImpactInfo(hit, out ImpactDamageUtility.HandImpactInfo handImpactInfo))
            {
                if (!handImpactInfo.isPunching || !CanTakeBodyHit() || !handImpactInfo.isBodyPunch)
                {
                    continue;
                }

                if (IsBlockedByIdleHand(hit))
                {
                    Debug.Log($"{name}: body punch negated by idle guard hand.");
                    return;
                }

                RegisterHit(handImpactInfo, hit.transform.root);
                return;
            }
        }
    }

    void RegisterHit(ImpactDamageUtility.HandImpactInfo handImpactInfo, Transform attackerRoot)
    {
        recentlyHit = true;
        Invoke(nameof(ResetHit), hitCooldown);

        ImpactDamageUtility.ImpactBreakdown impactBreakdown = ImpactDamageUtility.CalculateImpactBreakdown(
            handImpactInfo.reachedPeakRotation,
            ownerTorso,
            ownerRoot,
            attackerRoot
        );
        int damage = impactBreakdown.damage;
        string reasons = ImpactDamageUtility.FormatReasons(impactBreakdown);

        if (damage <= 0)
        {
            Debug.Log($"{name}: body contact caused 0 damage. Reasons: {reasons}.");
            return;
        }

        currentBodyHp = Mathf.Max(0, currentBodyHp - damage);
        Debug.Log($"{name}: hit on body for {damage} damage. Body HP = {currentBodyHp}/{maxBodyHp}. Reasons: {reasons}.");

        if (damage >= 2)
        {
            SpawnBlood();

            if (ringFlash != null)
                ringFlash.Flash();
        }

        if (damage >= 3)
        {
            Debug.Log($"{name}: body injury.");
        }

        if (currentBodyHp <= 0)
        {
            knockedDown = true;
            Debug.Log($"{name}: knocked down from body damage.");

            if (ringFlash != null)
                ringFlash.StayHit();

            if (knockdown != null)
                knockdown.Knockdown();

            return;
        }
    }

    void ResetHit()
    {
        recentlyHit = false;
    }

    bool CanTakeBodyHit()
    {
        return true;
    }

    void SpawnBlood()
    {
        if (bloodPrefab == null)
        {
            return;
        }

        Transform spawnPoint = bloodSpawnPoint != null ? bloodSpawnPoint : transform;
        Instantiate(bloodPrefab, spawnPoint.position, spawnPoint.rotation);
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

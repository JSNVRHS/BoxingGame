using UnityEngine;

public class testHead : MonoBehaviour
{
    public Rigidbody2D head;
    public GameObject torso;
    public Camera cam;
    Torso ownerTorso;

    [SerializeField] bool aimAtMouse = false;
    [SerializeField] float aimAngleOffset = 0f;

    [SerializeField] Transform ownerRoot;
    [SerializeField] Transform opponentRoot;

    [SerializeField] GameObject bloodPrefab;
    [SerializeField] Transform chinTransform;
    [SerializeField] RingFlash ringFlash;
    [SerializeField] NPCKnockdown knockdown;

    [SerializeField] float baseAngleOffset = 40f;
    [SerializeField] float hitRotationAmount = 30f;
    [SerializeField] float returnSpeed = 2f;
    [SerializeField] float detectionRadius = 0.5f;
    [SerializeField] float hitCooldown = 0.5f;
    [SerializeField] float brawlerDodgeWindow = 1f;
    [SerializeField] int maxHeadHp = 5;
    [SerializeField] SpriteRenderer normalHeadSprite;
    [SerializeField] GameObject bruisedFaceObject;
    [SerializeField] int bruisedFaceHpThreshold = 2;

    float lockedHeadAngle = 0f;
    float reactionAngleOffset = 0f;
    float brawlerEntryTime = float.NegativeInfinity;

    int currentHeadHp;
    bool knockedDown = false;
    bool recentlyHit = false;
    bool wasDuckingLastFrame = false;
    bool bruisedFaceShown = false;

    void Start()
    {
        head = GetComponent<Rigidbody2D>();
        ResolveOwnerTorso();
        ResolveBruisedFaceVisuals();
        SetBruisedFaceVisible(false);

        if (aimAtMouse)
        {
            cam = Camera.main;
        }

        currentHeadHp = maxHeadHp;
    }

    void Update()
    {
        if (knockedDown) return;

        ResolveOwnerTorso();
        ResolveOpponentRoot();
        TrackBrawlerEntry();
        StickToTorso();
        reactionAngleOffset = Mathf.LerpAngle(
            reactionAngleOffset,
            0f,
            Time.deltaTime * returnSpeed
        );

        if (aimAtMouse && ownerTorso != null && ownerTorso.AllowPlayerInput)
        {
            RotateAtMouse();
        }
        else
        {
            UpdateReactionRotation();
        }

        if (!recentlyHit)
        {
            CheckForHits();
        }
    }

    void StickToTorso()
    {
        if (torso == null)
        {
            return;
        }

        head.transform.position = torso.transform.position;
    }

    void RotateAtMouse()
    {
        if (cam == null) return;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        float angleRad = Mathf.Atan2(
            mousePos.y - transform.position.y,
            mousePos.x - transform.position.x
        );
        float baseAngle = Mathf.Rad2Deg * angleRad - 90f + aimAngleOffset;

        if (ownerTorso != null && ownerTorso.IsPunchingActive())
        {
            head.transform.rotation = Quaternion.Euler(0f, 0f, lockedHeadAngle + reactionAngleOffset);
        }
        else
        {
            lockedHeadAngle = baseAngle;
            head.transform.rotation = Quaternion.Euler(0f, 0f, baseAngle + reactionAngleOffset);
        }
    }

    void UpdateReactionRotation()
    {
        head.transform.rotation =
            torso.transform.rotation * Quaternion.Euler(0f, 0f, baseAngleOffset + reactionAngleOffset);
    }

    void CheckForHits()
    {
        if (IsInBrawlerDodgeWindow())
        {
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        foreach (Collider2D hit in hits)
        {
            if (ownerRoot != null && (hit.transform == ownerRoot || hit.transform.IsChildOf(ownerRoot)))
                continue;

            if (opponentRoot != null && hit.transform != opponentRoot && !hit.transform.IsChildOf(opponentRoot))
                continue;

            if (ImpactDamageUtility.TryGetHandImpactInfo(hit, out ImpactDamageUtility.HandImpactInfo handImpactInfo))
            {
                if (!handImpactInfo.isPunching || !CanTakeHeadHit() || !handImpactInfo.isHeadPunch)
                {
                    continue;
                }

                if (ShouldSlipOutfighterPunch() && !hit.GetComponent<BrawlerRightHand>() && !hit.GetComponent<BrawlerLeftHand>())
                {
                    Debug.Log($"{name} miss");
                    return;
                }

                if (IsBlockedByIdleHand(hit))
                {
                    Debug.Log($"{name} block");
                    return;
                }

                bool isRightHand = hit.CompareTag("RightHand");
                RegisterHit(isRightHand, handImpactInfo, hit.transform.root);
                return;
            }
        }
    }

    void RegisterHit(bool isRightHand, ImpactDamageUtility.HandImpactInfo handImpactInfo, Transform attackerRoot)
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
            Debug.Log($"{name} 0 damage Reasons: {reasons}.");
            return;
        }

        currentHeadHp = Mathf.Max(0, currentHeadHp - damage);
        Debug.Log($"{name} hit on head for {damage} dmg. Head HP = {currentHeadHp}/{maxHeadHp} Reasons: {reasons}");
        UpdateBruisedFaceVisual();

        if (isRightHand)
            reactionAngleOffset = -hitRotationAmount;
        else
            reactionAngleOffset = hitRotationAmount;

        if (damage >= 2)
        {
            SpawnBlood();

            if (ringFlash != null)
                ringFlash.Flash();
        }

        if (damage >= 3)
        {
            Debug.Log($"{name} head injury");

            CharacterInjurySystem injurySystem = ownerRoot != null
                ? ownerRoot.GetComponentInChildren<CharacterInjurySystem>(true)
                : null;
            if (injurySystem != null)
            {
                injurySystem.ApplyRandomHeadInjury();
            }
        }

        if (currentHeadHp <= 0)
        {
            knockedDown = true;
            Debug.Log($"{name} kod from head dmg");
            VictoryPoseUtility.ShowVictoryPose(attackerRoot);
            KOMessageUtility.ShowKOMessage(attackerRoot, ownerRoot);

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

    void UpdateBruisedFaceVisual()
    {
        if (bruisedFaceShown || bruisedFaceObject == null || currentHeadHp > bruisedFaceHpThreshold)
        {
            return;
        }

        bruisedFaceShown = true;
        SetBruisedFaceVisible(true);
    }

    void SetBruisedFaceVisible(bool visible)
    {
        if (bruisedFaceObject == null)
        {
            return;
        }

        if (normalHeadSprite != null)
        {
            normalHeadSprite.enabled = !visible;
        }

        bruisedFaceObject.SetActive(visible);
    }

    void ResolveBruisedFaceVisuals()
    {
        if (normalHeadSprite == null)
        {
            normalHeadSprite = GetComponent<SpriteRenderer>();
        }

        if (bruisedFaceObject == null)
        {
            Transform bruisedTransform = FindNamedChild(transform, "bruisedFace");
            if (bruisedTransform != null)
            {
                bruisedFaceObject = bruisedTransform.gameObject;
            }
        }
    }

    Transform FindNamedChild(Transform root, string childName)
    {
        if (root == null)
        {
            return null;
        }

        Transform[] descendants = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform descendant in descendants)
        {
            if (descendant != root && descendant.name == childName)
            {
                return descendant;
            }
        }

        return null;
    }

    void SpawnBlood()
    {
        if (bloodPrefab != null && chinTransform != null)
        {
            Instantiate(bloodPrefab, chinTransform.position, chinTransform.rotation);
        }
    }

    public GameObject GetBloodPrefab()
    {
        return bloodPrefab;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    bool CanTakeHeadHit()
    {
        return true;
    }

    bool ShouldSlipOutfighterPunch()
    {
        return IsInBrawlerDodgeWindow();
    }

    bool IsBlockedByIdleHand(Collider2D attackingCollider)
    {
        if (ownerTorso == null || ownerTorso.duck)
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

    void ResolveOpponentRoot()
    {
        if (opponentRoot != null && opponentRoot.gameObject.activeInHierarchy)
        {
            return;
        }

        Torso candidate = FindActiveOpponentTorso();
        if (candidate != null)
        {
            opponentRoot = candidate.transform.root;
        }
    }

    Torso FindActiveOpponentTorso()
    {
        Torso[] torsos = FindObjectsByType<Torso>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Torso fallback = null;

        foreach (Torso candidate in torsos)
        {
            if (candidate == null || candidate == ownerTorso)
            {
                continue;
            }

            if (ownerRoot != null && (candidate.transform == ownerRoot || candidate.transform.IsChildOf(ownerRoot)))
            {
                continue;
            }

            bool expectedOpponentInput = ownerTorso == null || !ownerTorso.AllowPlayerInput;
            if (candidate.AllowPlayerInput == expectedOpponentInput)
            {
                return candidate;
            }

            if (fallback == null)
            {
                fallback = candidate;
            }
        }

        return fallback;
    }

    void TrackBrawlerEntry()
    {
        bool isDucking = ownerTorso != null && ownerTorso.duck;

        if (isDucking && !wasDuckingLastFrame)
        {
            brawlerEntryTime = Time.time;
        }

        wasDuckingLastFrame = isDucking;
    }

    bool IsInBrawlerDodgeWindow()
    {
        return ownerTorso != null &&
               ownerTorso.duck &&
               Time.time - brawlerEntryTime <= brawlerDodgeWindow;
    }
}

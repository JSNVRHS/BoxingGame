using UnityEngine;

public class testHead : MonoBehaviour
{
    public Rigidbody2D head;
    public GameObject torso;
    public Camera cam;
    Torso ownerTorso;

    [Header("Mode")]
    [SerializeField] bool aimAtMouse = false;
    [SerializeField] float aimAngleOffset = 0f;

    [Header("Ownership")]
    [SerializeField] Transform ownerRoot;
    [SerializeField] Transform opponentRoot;

    [Header("Effects / Knockdown")]
    [SerializeField] GameObject bloodPrefab;
    [SerializeField] Transform chinTransform;
    [SerializeField] RingFlash ringFlash;
    [SerializeField] NPCKnockdown knockdown;

    [Header("Hit Reaction")]
    [SerializeField] float baseAngleOffset = 40f;
    [SerializeField] float hitRotationAmount = 30f;
    [SerializeField] float returnSpeed = 2f;
    [SerializeField] float detectionRadius = 0.5f;
    [SerializeField] float hitCooldown = 0.5f;
    [SerializeField] int hitsToKnockdown = 4;

    float lockedHeadAngle = 0f;
    float reactionAngleOffset = 0f;

    int hitCount = 0;
    bool knockedDown = false;
    bool recentlyHit = false;

    void Start()
    {
        head = GetComponent<Rigidbody2D>();
        ownerTorso = torso != null ? torso.GetComponent<Torso>() : null;

        if (aimAtMouse)
        {
            cam = Camera.main;
        }
    }

    void Update()
    {
        if (knockedDown) return;

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
                if (!CanTakeHeadHit() || !rightHand.IsHeadPunch)
                {
                    continue;
                }

                RegisterHit(true);
                return;
            }

            BrawlerRightHand brawlerRightHand = hit.GetComponent<BrawlerRightHand>();
            if (brawlerRightHand != null && brawlerRightHand.IsPunching)
            {
                if (!CanTakeHeadHit() || !brawlerRightHand.IsHeadPunch)
                {
                    continue;
                }

                RegisterHit(true);
                return;
            }

            LeftHand leftHand = hit.GetComponent<LeftHand>();
            if (leftHand != null && leftHand.IsPunching)
            {
                if (!CanTakeHeadHit() || !leftHand.IsHeadPunch)
                {
                    continue;
                }

                RegisterHit(false);
                return;
            }

            BrawlerLeftHand brawlerLeftHand = hit.GetComponent<BrawlerLeftHand>();
            if (brawlerLeftHand != null && brawlerLeftHand.IsPunching)
            {
                if (!CanTakeHeadHit() || !brawlerLeftHand.IsHeadPunch)
                {
                    continue;
                }

                RegisterHit(false);
                return;
            }
        }
    }

    void RegisterHit(bool isRightHand)
    {
        hitCount++;
        recentlyHit = true;
        Invoke(nameof(ResetHit), hitCooldown);
        Debug.Log($"{name}: hit on head. Head hits = {hitCount}.");

        if (hitCount >= hitsToKnockdown)
        {
            knockedDown = true;
            Debug.Log($"{name}: knocked down from head hits.");

            if (ringFlash != null)
                ringFlash.StayHit();

            if (knockdown != null)
                knockdown.Knockdown();

            return;
        }

        if (isRightHand)
            reactionAngleOffset = -hitRotationAmount;
        else
            reactionAngleOffset = hitRotationAmount;

        SpawnBlood();

        if (ringFlash != null)
            ringFlash.Flash();
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

    bool CanTakeHeadHit()
    {
        return ownerTorso == null || !ownerTorso.duck;
    }
}

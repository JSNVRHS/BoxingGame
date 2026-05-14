using System.Collections;
using System.Reflection;
using UnityEngine;

public class SimpleOSNpcAI : MonoBehaviour
{
    [SerializeField] Torso torso;
    [SerializeField] LeftHand leftHand;
    [SerializeField] RightHand rightHand;
    [SerializeField] BrawlerLeftHand brawlerLeftHand;
    [SerializeField] BrawlerRightHand brawlerRightHand;
    [SerializeField] Transform targetHead;
    [SerializeField] float attackInterval = 1f;
    [SerializeField] float moveSpeed = 1.6f;
    [SerializeField] float preferredDistance = 2.1f;
    [SerializeField] float brawlerDistanceMultiplier = 0.5f;
    [SerializeField] float distanceBuffer = 0.25f;
    [SerializeField] float punchRange = 2.45f;
    [SerializeField] float rotationSpeedDegreesPerSecond = 720f;
    [SerializeField] float outfighterPunchDuration = 0.55f;
    [SerializeField] float brawlerPunchDuration = 0.65f;
    [SerializeField] float comboPunchGap = 0.03f;
    [SerializeField] float punchBodyRotationDegrees = 45f;
    [Header("Difficulty")]
    [SerializeField] float hardAttackIntervalMultiplier = 0.7f;
    [SerializeField] float hardMoveSpeedMultiplier = 1.15f;
    [SerializeField] float hardRotationSpeedMultiplier = 1.15f;
    [Header("Debug Damage")]
    [SerializeField] bool overridePunchDamage = false;
    [SerializeField, Range(0, 5)] int overridePunchDamageAmount = 4;

    Rigidbody2D movementBody;
    Animator legsAnimator;
    Torso targetTorso;
    float nextAttackTime;
    bool comboActive;
    bool leftAiPunching;
    bool rightAiPunching;
    FieldInfo leftPunchingField;
    FieldInfo rightPunchingField;
    FieldInfo brawlerLeftPunchingField;
    FieldInfo brawlerRightPunchingField;

    void Start()
    {
        AutoAssignReferences();
        ApplyMenuDifficulty();

        leftPunchingField = typeof(LeftHand).GetField("punching", BindingFlags.Instance | BindingFlags.NonPublic);
        rightPunchingField = typeof(RightHand).GetField("punching", BindingFlags.Instance | BindingFlags.NonPublic);
        brawlerLeftPunchingField = typeof(BrawlerLeftHand).GetField("punching", BindingFlags.Instance | BindingFlags.NonPublic);
        brawlerRightPunchingField = typeof(BrawlerRightHand).GetField("punching", BindingFlags.Instance | BindingFlags.NonPublic);

        nextAttackTime = Time.time + attackInterval;
        DebugAssignmentState();
    }

    void ApplyMenuDifficulty()
    {
        if (PlayerPrefs.GetString("Difficulty", "Normal") != "Hard")
        {
            overridePunchDamage = false;
            return;
        }

        overridePunchDamage = true;
        overridePunchDamageAmount = 5;
        attackInterval *= hardAttackIntervalMultiplier;
        moveSpeed *= hardMoveSpeedMultiplier;
        rotationSpeedDegreesPerSecond *= hardRotationSpeedMultiplier;
    }

    void Update()
    {
        if (torso == null)
        {
            return;
        }

        FacePlayer();
        MoveToPunchingRange();

        if (comboActive || IsAnyPunchActive())
        {
            return;
        }

        if (targetTorso != null && targetTorso.duck && torso.duck)
        {
            SetBrawlerStance(false);
            nextAttackTime = Time.time + attackInterval;
            return;
        }

        if (Time.time < nextAttackTime)
        {
            return;
        }

        ChooseNextAction();
    }

    void MoveToPunchingRange()
    {
        if (targetHead == null)
        {
            SetMovement(Vector2.zero);
            return;
        }

        Vector2 toPlayer = GetTargetPosition() - GetNpcPosition();
        float distance = toPlayer.magnitude;
        if (distance <= 0.01f)
        {
            SetMovement(Vector2.zero);
            return;
        }

        Vector2 direction = toPlayer / distance;
        float attackDistance = GetCurrentPreferredDistance();
        float tooCloseDistance = attackDistance - distanceBuffer;
        float tooFarDistance = attackDistance + distanceBuffer;

        if (distance > tooFarDistance)
        {
            SetMovement(direction * moveSpeed);
        }
        else if (distance < tooCloseDistance)
        {
            SetMovement(-direction * moveSpeed);
        }
        else
        {
            SetMovement(Vector2.zero);
        }
    }

    void SetMovement(Vector2 velocity)
    {
        if (movementBody != null)
        {
            movementBody.linearVelocity = velocity;
        }
        else
        {
            transform.position += (Vector3)(velocity * Time.deltaTime);
        }

        if (legsAnimator != null)
        {
            legsAnimator.SetBool("walking", velocity.sqrMagnitude > 0.01f);
        }
    }

    void FacePlayer()
    {
        if (targetHead == null || torso.torso == null)
        {
            return;
        }

        if (IsAnyPunchActive())
        {
            return;
        }

        Vector2 toPlayer = GetTargetPosition() - (Vector2)torso.transform.position;
        if (toPlayer.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        float targetAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg - 90f + 130f;
        float nextAngle = Mathf.MoveTowardsAngle(
            torso.torso.rotation,
            targetAngle,
            rotationSpeedDegreesPerSecond * Time.deltaTime
        );
        torso.torso.MoveRotation(nextAngle);
    }

    bool IsPlayerInPunchRange()
    {
        if (targetHead == null)
        {
            return false;
        }

        float distance = Vector2.Distance(GetNpcPosition(), GetTargetPosition());
        return distance <= GetCurrentPunchRange() && distance >= GetCurrentPreferredDistance() - distanceBuffer;
    }

    float GetCurrentPreferredDistance()
    {
        return torso != null && torso.duck
            ? preferredDistance * brawlerDistanceMultiplier
            : preferredDistance;
    }

    float GetCurrentPunchRange()
    {
        if (torso != null && torso.duck)
        {
            return Mathf.Max(GetCurrentPreferredDistance() + distanceBuffer, punchRange * brawlerDistanceMultiplier);
        }

        return punchRange;
    }

    Vector2 GetTargetPosition()
    {
        return targetHead != null ? (Vector2)targetHead.position : GetNpcPosition();
    }

    Vector2 GetNpcPosition()
    {
        if (movementBody != null)
        {
            return movementBody.position;
        }

        return torso != null ? (Vector2)torso.transform.position : (Vector2)transform.position;
    }

    void ChooseNextAction()
    {
        bool playerInBrawlerStance = targetTorso != null && targetTorso.duck;
        bool npcInBrawlerStance = torso.duck;

        if (!playerInBrawlerStance && !npcInBrawlerStance)
        {
            if (Random.value < 0.5f)
            {
                TryStartPunchCombo(useBrawlerHands: false, Random.Range(1, 3));
            }
            else
            {
                SetBrawlerStance(true);
                nextAttackTime = Time.time + attackInterval;
            }
            return;
        }

        if (!playerInBrawlerStance && npcInBrawlerStance)
        {
            TryStartPunchCombo(useBrawlerHands: true, Random.Range(1, 4));
            return;
        }

        if (playerInBrawlerStance && !npcInBrawlerStance)
        {
            if (Random.value < 0.5f)
            {
                TryStartPunchCombo(useBrawlerHands: false, Random.Range(1, 3));
            }
            else
            {
                SetBrawlerStance(true);
                nextAttackTime = Time.time + attackInterval;
            }
            return;
        }

        SetBrawlerStance(false);
        nextAttackTime = Time.time + attackInterval;
    }

    void TryStartPunchCombo(bool useBrawlerHands, int punchCount)
    {
        if (!IsPlayerInPunchRange())
        {
            nextAttackTime = Time.time + 0.1f;
            return;
        }

        StartCoroutine(PunchCombo(useBrawlerHands, punchCount));
    }

    IEnumerator PunchCombo(bool useBrawlerHands, int punchCount)
    {
        comboActive = true;

        for (int i = 0; i < punchCount; i++)
        {
            bool useRightHand = Random.value < 0.5f;
            MonoBehaviour hand = GetHand(useBrawlerHands, useRightHand);
            if (hand == null)
            {
                hand = GetHand(useBrawlerHands, !useRightHand);
                useRightHand = !useRightHand;
            }

            if (TryPunch(hand, useRightHand))
            {
                yield return new WaitUntil(() => !IsAnyPunchActive());
                yield return new WaitForSeconds(comboPunchGap);
            }
            else
            {
                yield return null;
            }
        }

        nextAttackTime = Time.time + attackInterval;
        comboActive = false;
    }

    MonoBehaviour GetHand(bool useBrawlerHands, bool useRightHand)
    {
        if (useBrawlerHands)
        {
            return useRightHand ? brawlerRightHand : brawlerLeftHand;
        }

        return useRightHand ? rightHand : leftHand;
    }

    void SetBrawlerStance(bool value)
    {
        torso.SetBrawlerStance(value);
        brawlerLeftHand = FindBrawlerHand<BrawlerLeftHand>("leftHand");
        brawlerRightHand = FindBrawlerHand<BrawlerRightHand>("rightHand");
        leftHand = FindOutfighterHand<LeftHand>("leftHand");
        rightHand = FindOutfighterHand<RightHand>("rightHand");
    }

    bool TryPunch(MonoBehaviour handBehaviour, bool isRightHand)
    {
        if (handBehaviour == null)
        {
            return false;
        }

        if (isRightHand ? rightAiPunching : leftAiPunching)
        {
            return false;
        }

        Animator anim = handBehaviour.GetComponent<Animator>();
        if (anim == null)
        {
            return false;
        }

        anim.SetTrigger("punch");
        StartCoroutine(PunchRoutine(handBehaviour, isRightHand));
        // Debug.Log($"{name} threw {(isRightHand ? "right" : "left")} OS");
        return true;
    }

    IEnumerator PunchRoutine(MonoBehaviour handBehaviour, bool isRightHand)
    {
        SetPunchingState(handBehaviour, isRightHand, true);

        float duration = IsBrawlerHand(handBehaviour) ? brawlerPunchDuration : outfighterPunchDuration;
        float elapsed = 0f;
        float startAngle = torso.torso != null ? torso.torso.rotation : torso.transform.eulerAngles.z;
        float targetAngle = startAngle + GetPunchBodyRotationOffset(isRightHand);

        while (elapsed < duration)
        {
            RotateBodyDuringPunch(startAngle, targetAngle, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        RotateBodyDuringPunch(startAngle, targetAngle, 1f);
        SetPunchingState(handBehaviour, isRightHand, false);
    }

    float GetPunchBodyRotationOffset(bool isRightHand)
    {
        return isRightHand ? punchBodyRotationDegrees : -punchBodyRotationDegrees;
    }

    void RotateBodyDuringPunch(float startAngle, float targetAngle, float progress)
    {
        if (torso == null || torso.torso == null)
        {
            return;
        }

        float easedProgress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress));
        float angle = Mathf.LerpAngle(startAngle, targetAngle, easedProgress);
        torso.torso.MoveRotation(angle);
    }

    void SetPunchingState(MonoBehaviour handBehaviour, bool isRightHand, bool value)
    {
        if (handBehaviour is BrawlerRightHand)
        {
            rightAiPunching = value;
            if (brawlerRightPunchingField != null)
            {
                brawlerRightPunchingField.SetValue(handBehaviour, value);
            }
        }
        else if (handBehaviour is BrawlerLeftHand)
        {
            leftAiPunching = value;
            if (brawlerLeftPunchingField != null)
            {
                brawlerLeftPunchingField.SetValue(handBehaviour, value);
            }
        }
        else if (isRightHand)
        {
            rightAiPunching = value;
            RightHand.isPunching = value;
            if (rightPunchingField != null && handBehaviour is RightHand)
            {
                rightPunchingField.SetValue(handBehaviour, value);
            }
        }
        else
        {
            leftAiPunching = value;
            LeftHand.isPunching = value;
            if (leftPunchingField != null && handBehaviour is LeftHand)
            {
                leftPunchingField.SetValue(handBehaviour, value);
            }
        }
    }

    bool IsBrawlerHand(MonoBehaviour handBehaviour)
    {
        return handBehaviour is BrawlerLeftHand || handBehaviour is BrawlerRightHand;
    }

    bool IsAnyPunchActive()
    {
        return leftAiPunching || rightAiPunching;
    }

    public bool TryGetOverridePunchDamage(out int damage)
    {
        damage = Mathf.Clamp(overridePunchDamageAmount, 0, 5);
        return overridePunchDamage;
    }

    void AutoAssignReferences()
    {
        if (torso == null)
        {
            torso = GetComponentInChildren<Torso>(true);
        }

        if (leftHand == null)
        {
            leftHand = FindOutfighterHand<LeftHand>("leftHand");
        }

        if (rightHand == null)
        {
            rightHand = FindOutfighterHand<RightHand>("rightHand");
        }

        if (brawlerLeftHand == null)
        {
            brawlerLeftHand = FindBrawlerHand<BrawlerLeftHand>("leftHand");
        }

        if (brawlerRightHand == null)
        {
            brawlerRightHand = FindBrawlerHand<BrawlerRightHand>("rightHand");
        }

        if (movementBody == null)
        {
            movementBody = FindMovementBody();
        }

        if (legsAnimator == null && movementBody != null)
        {
            legsAnimator = movementBody.GetComponent<Animator>();
        }

        if (targetHead == null)
        {
            targetHead = FindTargetHead();
        }

        if (targetTorso == null && targetHead != null)
        {
            targetTorso = targetHead.GetComponentInParent<Torso>();
        }

        if (targetTorso == null)
        {
            targetTorso = FindTargetTorso();
        }
    }

    Rigidbody2D FindMovementBody()
    {
        if (torso == null)
        {
            return GetComponentInChildren<Rigidbody2D>(true);
        }

        if (torso.legs != null)
        {
            Rigidbody2D legsBody = torso.legs.GetComponent<Rigidbody2D>();
            if (legsBody != null)
            {
                return legsBody;
            }
        }

        Movement movement = GetComponentInChildren<Movement>(true);
        if (movement != null && movement.body != null)
        {
            return movement.body;
        }

        return torso.GetComponent<Rigidbody2D>();
    }

    Transform FindTargetHead()
    {
        Torso candidate = FindTargetTorso();
        if (candidate == null)
        {
            return null;
        }

        targetTorso = candidate;
        testHead playerHead = candidate.GetComponentInChildren<testHead>(true);
        if (playerHead != null)
        {
            return playerHead.transform;
        }

        Head legacyHead = candidate.GetComponentInChildren<Head>(true);
        if (legacyHead != null)
        {
            return legacyHead.transform;
        }

        return candidate.transform;
    }

    Torso FindTargetTorso()
    {
        Torso[] torsos = FindObjectsByType<Torso>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Torso candidate in torsos)
        {
            if (candidate != null && candidate != torso && candidate.AllowPlayerInput)
            {
                return candidate;
            }
        }

        return null;
    }

    T FindOutfighterHand<T>(string handName) where T : MonoBehaviour
    {
        if (torso == null)
        {
            return null;
        }

        Transform outfighterRoot = torso.transform.Find("outfighterStance");
        if (outfighterRoot != null)
        {
            Transform handTransform = FindNamedChild(outfighterRoot, handName);
            if (handTransform != null)
            {
                T hand = handTransform.GetComponent<T>();
                if (hand != null)
                {
                    return hand;
                }
            }
        }

        return torso.GetComponentInChildren<T>(true);
    }

    T FindBrawlerHand<T>(string handName) where T : MonoBehaviour
    {
        if (torso == null)
        {
            return null;
        }

        Transform brawlerRoot = torso.transform.Find("brawlerStance");
        if (brawlerRoot != null)
        {
            Transform handTransform = FindNamedChild(brawlerRoot, handName);
            if (handTransform != null)
            {
                T hand = handTransform.GetComponent<T>();
                if (hand != null)
                {
                    return hand;
                }
            }
        }

        return torso.GetComponentInChildren<T>(true);
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

    void DebugAssignmentState()
    {
        if (torso == null)
        {
            Debug.LogWarning($"{name} AI cant find Torso");
        }

        if (leftHand == null)
        {
            Debug.LogWarning($"{name} AI cant find OS left hand");
        }

        if (rightHand == null)
        {
            Debug.LogWarning($"{name} AI cant find OS right hand");
        }

        if (brawlerLeftHand == null)
        {
            Debug.LogWarning($"{name} AI  cant find BS left hand");
        }

        if (brawlerRightHand == null)
        {
            Debug.LogWarning($"{name} AI cant find BS right hand");
        }

        if (movementBody == null)
        {
            Debug.LogWarning($"{name} AI cant find Rigidbody2D movement");
        }

        if (targetHead == null)
        {
            Debug.LogWarning($"{name} AI cant find the player head target");
        }

    }
}

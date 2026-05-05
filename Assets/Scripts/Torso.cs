using UnityEngine;

public class Torso : MonoBehaviour
{
    public Rigidbody2D torso;
    public Animator anim;
    [SerializeField] Transform torsoSprite;
    [SerializeField] GameObject outfighterStance;
    [SerializeField] GameObject brawlerStance;
    public GameObject legs;
    public GameObject rightShoulder;
    public GameObject leftShoulder;
    public Camera cam;
    public bool duck = false;
    public LeftHand leftHand;
    public RightHand rightHand;
    public BrawlerLeftHand brawlerLeftHand;
    public BrawlerRightHand brawlerRightHand;
    [SerializeField] bool allowPlayerInput = true;

    float lockedTorsoAngle = 0f;

    [SerializeField] float rightPunchRotateLeft = 100f;
    [SerializeField] float rightPunchRotateRight = 30f;
    [SerializeField] float leftPunchRotateLeft = 30f;
    [SerializeField] float leftPunchRotateRight = 30f;
    [SerializeField] float brawlerSlipWindowDuration = 5f;
    bool lastAppliedDuck;
    float lastBrawlerEntryTime = float.NegativeInfinity;

    void Start()
    {
        torso = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        CacheStanceRoots();
        ApplyStanceState(forceRefresh: true);
    }

    void Update()
    {
        rotateAtMouse();
        Duck();
        stickToLegs();
    }

    public void stickToLegs()
    {
        torso.transform.position = legs.transform.position;
    }

    public void rotateAtMouse()
    {
        if (!allowPlayerInput)
        {
            return;
        }

        Vector3 mousePos = (Vector2)cam.ScreenToWorldPoint(Input.mousePosition);
        float angleRad = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x);
        float angleDeg = (180f / Mathf.PI) * angleRad - 90f + 130f;

        bool rightPunching = IsRightPunching();
        bool leftPunching = IsLeftPunching();

        if (rightPunching || leftPunching)
        {
            float rotateLeft = rightPunching ? rightPunchRotateLeft : leftPunchRotateLeft;
            float rotateRight = rightPunching ? rightPunchRotateRight : leftPunchRotateRight;

            float delta = Mathf.DeltaAngle(lockedTorsoAngle, angleDeg);
            delta = Mathf.Clamp(delta, -rotateRight, rotateLeft);
            torso.transform.rotation = Quaternion.Euler(0f, 0f, lockedTorsoAngle + delta);
        }
        else
        {
            lockedTorsoAngle = angleDeg;
            torso.transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);
        }
    }

    public void Duck()
    {
        if (!allowPlayerInput)
        {
            ApplyStanceState();
            return;
        }

        if (IsPunchingActive())
        {
            ApplyStanceState();
            return;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            duck = true;
        }
        else
        {
            duck = false;
        }

        ApplyStanceState();
    }

    public bool IsPunchingActive()
    {
        return IsLeftPunching() || IsRightPunching();
    }

    public bool AllowPlayerInput => allowPlayerInput;

    void CacheStanceRoots()
    {
        if (outfighterStance == null)
        {
            Transform outfighterTransform = FindNamedChild("outfighterStance");
            if (outfighterTransform != null)
            {
                outfighterStance = outfighterTransform.gameObject;
            }
        }

        if (brawlerStance == null)
        {
            Transform brawlerTransform = FindNamedChild("brawlerStance");
            if (brawlerTransform != null)
            {
                brawlerStance = brawlerTransform.gameObject;
            }
        }
    }

    void ApplyStanceState(bool forceRefresh = false)
    {
        if (!forceRefresh && lastAppliedDuck == duck)
        {
            if (anim != null)
            {
                anim.SetBool("duck", duck);
            }
            return;
        }

        if (outfighterStance != null)
        {
            outfighterStance.SetActive(!duck);
        }

        if (brawlerStance != null)
        {
            brawlerStance.SetActive(duck);
        }

        RefreshActiveRigReferences();

        if (anim != null)
        {
            anim.SetBool("duck", duck);
        }

        if (duck && (!lastAppliedDuck || forceRefresh))
        {
            lastBrawlerEntryTime = Time.time;
        }

        lastAppliedDuck = duck;
    }

    void RefreshActiveRigReferences()
    {
        GameObject activeStance = duck ? brawlerStance : outfighterStance;
        Transform activeRoot = activeStance != null ? activeStance.transform : null;

        torsoSprite = null;
        anim = null;
        leftShoulder = null;
        rightShoulder = null;
        leftHand = null;
        rightHand = null;
        brawlerLeftHand = null;
        brawlerRightHand = null;

        if (activeRoot == null)
        {
            return;
        }

        torsoSprite = FindNamedChild(activeRoot, "torsoSprite");
        if (torsoSprite != null)
        {
            anim = torsoSprite.GetComponent<Animator>();
        }

        Transform leftShoulderTransform = FindNamedChild(activeRoot, "leftShoulder");
        if (leftShoulderTransform != null)
        {
            leftShoulder = leftShoulderTransform.gameObject;
        }

        Transform rightShoulderTransform = FindNamedChild(activeRoot, "rightShoulder");
        if (rightShoulderTransform != null)
        {
            rightShoulder = rightShoulderTransform.gameObject;
        }

        leftHand = activeRoot.GetComponentInChildren<LeftHand>(true);
        rightHand = activeRoot.GetComponentInChildren<RightHand>(true);
        brawlerLeftHand = activeRoot.GetComponentInChildren<BrawlerLeftHand>(true);
        brawlerRightHand = activeRoot.GetComponentInChildren<BrawlerRightHand>(true);
    }

    bool IsLeftPunching()
    {
        return (leftHand != null && leftHand.IsPunching) ||
               (brawlerLeftHand != null && brawlerLeftHand.IsPunching);
    }

    bool IsRightPunching()
    {
        return (rightHand != null && rightHand.IsPunching) ||
               (brawlerRightHand != null && brawlerRightHand.IsPunching);
    }

    public bool HasFreshBrawlerSlipWindow()
    {
        return duck && Time.time - lastBrawlerEntryTime <= brawlerSlipWindowDuration;
    }

    Transform FindNamedChild(string childName)
    {
        Transform[] descendants = GetComponentsInChildren<Transform>(true);
        Transform fallback = null;

        foreach (Transform descendant in descendants)
        {
            if (descendant == transform || descendant.name != childName)
            {
                continue;
            }

            if (fallback == null)
            {
                fallback = descendant;
            }

            if (descendant.gameObject.activeInHierarchy)
            {
                return descendant;
            }
        }

        return fallback;
    }

    Transform FindNamedChild(Transform root, string childName)
    {
        if (root == null)
        {
            return null;
        }

        Transform[] descendants = root.GetComponentsInChildren<Transform>(true);
        Transform fallback = null;

        foreach (Transform descendant in descendants)
        {
            if (descendant == root || descendant.name != childName)
            {
                continue;
            }

            if (fallback == null)
            {
                fallback = descendant;
            }

            if (descendant.gameObject.activeInHierarchy)
            {
                return descendant;
            }
        }

        return fallback;
    }
}

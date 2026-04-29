using UnityEngine;

public class Torso : MonoBehaviour
{
    public Rigidbody2D torso;
    public Animator anim;
    [SerializeField] Transform torsoSprite;
    public GameObject legs;
    public GameObject rightShoulder;
    public GameObject leftShoulder;
    public Camera cam;
    public bool duck = false;
    public LeftHand leftHand;
    public RightHand rightHand;
    [SerializeField] bool allowPlayerInput = true;

    float lockedTorsoAngle = 0f;

    [SerializeField] float rightPunchRotateLeft = 100f;
    [SerializeField] float rightPunchRotateRight = 30f;
    [SerializeField] float leftPunchRotateLeft = 30f;
    [SerializeField] float leftPunchRotateRight = 30f;

    void Start()
    {
        torso = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        AutoAssignBodyParts();
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

        bool rightPunching = rightHand != null && rightHand.IsPunching;
        bool leftPunching = leftHand != null && leftHand.IsPunching;

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
        if (anim == null)
        {
            return;
        }

        if (!allowPlayerInput)
        {
            anim.SetBool("duck", duck);
            return;
        }

        if (IsPunchingActive())
        {
            anim.SetBool("duck", duck);
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

        anim.SetBool("duck", duck);
    }

    public bool IsPunchingActive()
    {
        return (leftHand != null && leftHand.IsPunching) ||
               (rightHand != null && rightHand.IsPunching);
    }

    public bool AllowPlayerInput => allowPlayerInput;

    void AutoAssignBodyParts()
    {
        if (torsoSprite == null)
        {
            torsoSprite = FindNamedChild("torsoSprite");
        }

        if (torsoSprite != null)
        {
            anim = torsoSprite.GetComponent<Animator>();
        }

        if (leftShoulder == null)
        {
            Transform leftShoulderTransform = FindNamedChild("leftShoulder");
            if (leftShoulderTransform != null)
            {
                leftShoulder = leftShoulderTransform.gameObject;
            }
        }

        if (rightShoulder == null)
        {
            Transform rightShoulderTransform = FindNamedChild("rightShoulder");
            if (rightShoulderTransform != null)
            {
                rightShoulder = rightShoulderTransform.gameObject;
            }
        }

        leftHand = FindPreferredComponent<LeftHand>();
        rightHand = FindPreferredComponent<RightHand>();
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

    T FindPreferredComponent<T>() where T : Component
    {
        T[] components = GetComponentsInChildren<T>(true);
        T fallback = null;

        foreach (T component in components)
        {
            if (component == null)
            {
                continue;
            }

            if (fallback == null)
            {
                fallback = component;
            }

            if (component.gameObject.activeInHierarchy)
            {
                return component;
            }
        }

        return fallback;
    }
}

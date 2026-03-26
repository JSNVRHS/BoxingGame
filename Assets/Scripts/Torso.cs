using UnityEngine;

public class Torso : MonoBehaviour
{
    public Rigidbody2D torso;
    public Animator anim;
    public GameObject legs;
    public GameObject rightShoulder;
    public GameObject leftShoulder;
    public Camera cam;
    public bool duck = false;

    float lockedTorsoAngle = 0f;

    [SerializeField] float rightPunchRotateLeft = 100f;
    [SerializeField] float rightPunchRotateRight = 30f;
    [SerializeField] float leftPunchRotateLeft = 30f;
    [SerializeField] float leftPunchRotateRight = 30f;

    void Start()
    {
        torso = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        cam = Camera.main;
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
        Vector3 mousePos = (Vector2)cam.ScreenToWorldPoint(Input.mousePosition);
        float angleRad = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x);
        float angleDeg = (180f / Mathf.PI) * angleRad - 90f + 130f;

        if (RightHand.isPunching || LeftHand.isPunching)
        {
            float rotateLeft = RightHand.isPunching ? rightPunchRotateLeft : leftPunchRotateLeft;
            float rotateRight = RightHand.isPunching ? rightPunchRotateRight : leftPunchRotateRight;

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
}
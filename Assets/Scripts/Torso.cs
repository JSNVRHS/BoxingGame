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

    [SerializeField] float punchRotateLeft = 100f;
    [SerializeField] float punchRotateRight = 30f;

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

        if (RightHand.isPunching)
        {
            float delta = Mathf.DeltaAngle(lockedTorsoAngle, angleDeg);
            delta = Mathf.Clamp(delta, -punchRotateRight, punchRotateLeft);
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
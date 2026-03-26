using UnityEngine;

public class Head : MonoBehaviour
{
    public Rigidbody2D head;
    public GameObject torso;
    public Camera cam;

    float lockedHeadAngle = 0f;

    void Start()
    {
        head = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    void Update()
    {
        stickToLegs();
        rotateAtMouse();
    }

    public void stickToLegs()
    {
        head.transform.position = torso.transform.position;
    }

    public void rotateAtMouse()
    {
        Vector3 mousePos = (Vector2)cam.ScreenToWorldPoint(Input.mousePosition);
        float angleRad = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x);
        float angleDeg = (180f / Mathf.PI) * angleRad - 90f;

        if (RightHand.isPunching)
        {
            head.transform.rotation = Quaternion.Euler(0f, 0f, lockedHeadAngle);
        }
        else
        {
            lockedHeadAngle = angleDeg;
            head.transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);
        }
    }
}
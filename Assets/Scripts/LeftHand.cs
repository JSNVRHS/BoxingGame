using UnityEngine;
using System.Collections;

public class LeftHand : MonoBehaviour
{
    Animator anim;
    Camera cam;
    bool punching = false;
    public static bool isPunching = false;
    Quaternion punchLockedRotation;

    [SerializeField] float guardLocalAngle = 1f;

    void Start()
    {
        anim = GetComponent<Animator>();
        cam = Camera.main;
    }

    void Update()
    {
        if (!punching && Input.GetKeyDown(KeyCode.Mouse1))
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;

            float angleRad = Mathf.Atan2(
                mousePos.y - transform.position.y,
                mousePos.x - transform.position.x
            );
            float angleDeg = (180f / Mathf.PI) * angleRad - 90f;

            punchLockedRotation = Quaternion.Euler(0f, 0f, angleDeg);
            anim.SetTrigger("punch");
            StartCoroutine(PunchCooldown());
        }
    }

    void LateUpdate()
    {
        if (!punching)
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, guardLocalAngle);
        }
    }

    IEnumerator PunchCooldown()
    {
        isPunching = true;
        punching = true;
        yield return new WaitForSeconds(1f);
        isPunching = false;
        punching = false;
    }
}
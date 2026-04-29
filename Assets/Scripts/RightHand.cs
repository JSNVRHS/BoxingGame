using UnityEngine;
using System.Collections;

public class RightHand : MonoBehaviour
{
    Animator anim;
    bool punching = false;
    public static bool isPunching = false;
    Torso ownerTorso;
    Vector3 idleLocalPosition;
    Quaternion idleLocalRotation;
    Vector3 idleLocalScale;

    public bool IsPunching => punching;
    public bool IsBodyPunch => false;
    public bool IsHeadPunch => punching;
    public Torso OwnerTorso => ownerTorso;

    void Start()
    {
        anim = GetComponent<Animator>();
        ownerTorso = GetComponentInParent<Torso>();
        CaptureIdleTransform();
    }

    void Update()
    {
        if (ownerTorso != null && !ownerTorso.AllowPlayerInput)
        {
            return;
        }

        if (!punching && Input.GetKeyDown(KeyCode.Mouse0))
        {
            anim.SetTrigger("punch");
            StartCoroutine(PunchCooldown());
        }
    }

    void LateUpdate()
    {
        if (!punching)
        {
            transform.localPosition = idleLocalPosition;
            transform.localRotation = idleLocalRotation;
            transform.localScale = idleLocalScale;
        }
    }

    IEnumerator PunchCooldown()
    {
        isPunching = true;
        punching = true;
        yield return new WaitForSeconds(0.80f);
        isPunching = false;
        punching = false;
    }

    void CaptureIdleTransform()
    {
        idleLocalPosition = transform.localPosition;
        idleLocalRotation = transform.localRotation;
        idleLocalScale = transform.localScale;
    }
}

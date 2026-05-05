using UnityEngine;
using System.Collections;

public class LeftHand : MonoBehaviour
{
    Animator anim;
    Camera cam;
    bool punching = false;
    public static bool isPunching = false;
    Torso ownerTorso;

    [SerializeField] float guardLocalAngle = 1f;

    public bool IsPunching => punching;
    public bool IsBodyPunch => false;
    public bool IsHeadPunch => punching;
    public Torso OwnerTorso => ownerTorso;

    void Start()
    {
        anim = GetComponent<Animator>();
        cam = Camera.main;
        ownerTorso = GetComponentInParent<Torso>();
    }

    void Update()
    {
        if (ownerTorso != null && !ownerTorso.AllowPlayerInput)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            TryPunch();
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
        yield return new WaitForSeconds(0.80f);
        isPunching = false;
        punching = false;
    }

    public bool TryPunch()
    {
        if (punching || anim == null)
        {
            return false;
        }

        anim.SetTrigger("punch");
        StartCoroutine(PunchCooldown());
        return true;
    }
}

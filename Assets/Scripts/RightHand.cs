using UnityEngine;
using System.Collections;

public class RightHand : MonoBehaviour
{
    Animator anim;
    bool punching = false;
    public static bool isPunching = false;
    Torso ownerTorso;

    public bool IsPunching => punching;
    public bool IsBodyPunch => punching && ownerTorso != null && ownerTorso.duck;
    public bool IsHeadPunch => punching && ownerTorso != null && !ownerTorso.duck;
    public Torso OwnerTorso => ownerTorso;

    void Start()
    {
        anim = GetComponent<Animator>();
        ownerTorso = GetComponentInParent<Torso>();
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

    IEnumerator PunchCooldown()
    {
        isPunching = true;
        punching = true;
        yield return new WaitForSeconds(1f);
        isPunching = false;
        punching = false;
    }
}

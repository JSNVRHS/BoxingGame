using UnityEngine;
using System.Collections;

public class BrawlerRightHand : MonoBehaviour
{
    Animator anim;
    Torso ownerTorso;
    bool punching = false;

    public bool IsPunching => punching;
    public bool IsBodyPunch => punching;
    public bool IsHeadPunch => false;
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
        punching = true;
        yield return new WaitForSeconds(1f);
        punching = false;
    }
}

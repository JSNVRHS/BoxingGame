using UnityEngine;
using System.Collections;

public class RightHand : MonoBehaviour
{
    Animator anim;
    bool punching = false;
    public static bool isPunching = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
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
using UnityEngine;
using System.Collections;

public class BrawlerLeftHand : MonoBehaviour
{
    Animator anim;
    Torso ownerTorso;
    bool punching = false;
    float idleLocalZRotation;
    bool reachedPeakRotationThisPunch = false;
    [SerializeField] float peakRotationThreshold = 20f;

    public bool IsPunching => punching;
    public bool IsBodyPunch => punching;
    public bool IsHeadPunch => false;
    public Torso OwnerTorso => ownerTorso;
    public bool HasReachedPeakRotationThisPunch => reachedPeakRotationThisPunch;

    void Start()
    {
        anim = GetComponent<Animator>();
        ownerTorso = GetComponentInParent<Torso>();
        idleLocalZRotation = transform.localEulerAngles.z;
    }

    void Update()
    {
        TrackPeakRotation();

        if (ownerTorso != null && !ownerTorso.AllowPlayerInput)
        {
            return;
        }

        if (!punching && Input.GetKeyDown(KeyCode.Mouse1))
        {
            anim.SetTrigger("punch");
            StartCoroutine(PunchCooldown());
        }
    }

    IEnumerator PunchCooldown()
    {
        punching = true;
        reachedPeakRotationThisPunch = false;
        yield return new WaitForSeconds(1f);
        punching = false;
    }

    void TrackPeakRotation()
    {
        if (!punching || reachedPeakRotationThisPunch)
        {
            return;
        }

        float currentZ = transform.localEulerAngles.z;
        float rotationDelta = Mathf.Abs(Mathf.DeltaAngle(idleLocalZRotation, currentZ));
        if (rotationDelta >= peakRotationThreshold)
        {
            reachedPeakRotationThisPunch = true;
        }
    }
}

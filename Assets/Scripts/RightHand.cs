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
    float idleLocalZRotation;
    bool reachedPeakRotationThisPunch = false;
    [SerializeField] float peakRotationThreshold = 20f;

    public bool IsPunching => punching;
    public bool IsBodyPunch => false;
    public bool IsHeadPunch => punching;
    public Torso OwnerTorso => ownerTorso;
    public bool HasReachedPeakRotationThisPunch => reachedPeakRotationThisPunch;

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

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            TryPunch();
        }
    }

    void LateUpdate()
    {
        TrackPeakRotation();

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
        reachedPeakRotationThisPunch = false;
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

    void CaptureIdleTransform()
    {
        idleLocalPosition = transform.localPosition;
        idleLocalRotation = transform.localRotation;
        idleLocalScale = transform.localScale;
        idleLocalZRotation = transform.localEulerAngles.z;
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

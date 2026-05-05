using UnityEngine;
using System.Collections;

public class LeftHand : MonoBehaviour
{
    Animator anim;
    Camera cam;
    bool punching = false;
    public static bool isPunching = false;
    Torso ownerTorso;
    float idleLocalZRotation;
    bool reachedPeakRotationThisPunch = false;

    [SerializeField] float guardLocalAngle = 1f;
    [SerializeField] float peakRotationThreshold = 20f;

    public bool IsPunching => punching;
    public bool IsBodyPunch => false;
    public bool IsHeadPunch => punching;
    public Torso OwnerTorso => ownerTorso;
    public bool HasReachedPeakRotationThisPunch => reachedPeakRotationThisPunch;

    void Start()
    {
        anim = GetComponent<Animator>();
        cam = Camera.main;
        ownerTorso = GetComponentInParent<Torso>();
        idleLocalZRotation = transform.localEulerAngles.z;
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
        TrackPeakRotation();

        if (!punching)
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, guardLocalAngle);
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

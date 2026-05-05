using System.Collections;
using System.Reflection;
using UnityEngine;

public class SimpleOSNpcAI : MonoBehaviour
{
    [SerializeField] Torso torso;
    [SerializeField] LeftHand leftHand;
    [SerializeField] RightHand rightHand;
    [SerializeField] float attackInterval = 1f;

    float nextAttackTime;
    bool leftAiPunching;
    bool rightAiPunching;
    FieldInfo leftPunchingField;
    FieldInfo rightPunchingField;

    void Start()
    {
        AutoAssignReferences();

        leftPunchingField = typeof(LeftHand).GetField("punching", BindingFlags.Instance | BindingFlags.NonPublic);
        rightPunchingField = typeof(RightHand).GetField("punching", BindingFlags.Instance | BindingFlags.NonPublic);

        nextAttackTime = Time.time + attackInterval;
        DebugAssignmentState();
    }

    void Update()
    {
        if (torso == null)
        {
            return;
        }

        torso.duck = false;

        if (IsAnyPunchActive())
        {
            return;
        }

        if (Time.time < nextAttackTime)
        {
            return;
        }

        bool punched = TryPunch(leftHand, isRightHand: false);
        if (punched)
        {
            nextAttackTime = Time.time + attackInterval;
        }
    }

    bool TryPunch(MonoBehaviour handBehaviour, bool isRightHand)
    {
        if (handBehaviour == null)
        {
            return false;
        }

        if (isRightHand ? rightAiPunching : leftAiPunching)
        {
            return false;
        }

        Animator anim = handBehaviour.GetComponent<Animator>();
        if (anim == null)
        {
            return false;
        }

        anim.SetTrigger("punch");
        StartCoroutine(PunchRoutine(handBehaviour, isRightHand));
        // Debug.Log($"{name}: NPC threw {(isRightHand ? "right" : "left")} OS straight.");
        return true;
    }

    IEnumerator PunchRoutine(MonoBehaviour handBehaviour, bool isRightHand)
    {
        SetPunchingState(handBehaviour, isRightHand, true);
        yield return new WaitForSeconds(0.8f);
        SetPunchingState(handBehaviour, isRightHand, false);
    }

    void SetPunchingState(MonoBehaviour handBehaviour, bool isRightHand, bool value)
    {
        if (isRightHand)
        {
            rightAiPunching = value;
            RightHand.isPunching = value;
            if (rightPunchingField != null && handBehaviour is RightHand)
            {
                rightPunchingField.SetValue(handBehaviour, value);
            }
        }
        else
        {
            leftAiPunching = value;
            LeftHand.isPunching = value;
            if (leftPunchingField != null && handBehaviour is LeftHand)
            {
                leftPunchingField.SetValue(handBehaviour, value);
            }
        }
    }

    bool IsAnyPunchActive()
    {
        return leftAiPunching || rightAiPunching;
    }

    void AutoAssignReferences()
    {
        if (torso == null)
        {
            torso = GetComponentInChildren<Torso>(true);
        }

        if (leftHand == null)
        {
            leftHand = FindOutfighterHand<LeftHand>("leftHand");
        }

        if (rightHand == null)
        {
            rightHand = FindOutfighterHand<RightHand>("rightHand");
        }
    }

    T FindOutfighterHand<T>(string handName) where T : MonoBehaviour
    {
        if (torso == null)
        {
            return null;
        }

        Transform outfighterRoot = torso.transform.Find("outfighterStance");
        if (outfighterRoot != null)
        {
            Transform handTransform = FindNamedChild(outfighterRoot, handName);
            if (handTransform != null)
            {
                T hand = handTransform.GetComponent<T>();
                if (hand != null)
                {
                    return hand;
                }
            }
        }

        return torso.GetComponentInChildren<T>(true);
    }

    Transform FindNamedChild(Transform root, string childName)
    {
        if (root == null)
        {
            return null;
        }

        Transform[] descendants = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform descendant in descendants)
        {
            if (descendant != root && descendant.name == childName)
            {
                return descendant;
            }
        }

        return null;
    }

    void DebugAssignmentState()
    {
        if (torso == null)
        {
            Debug.LogWarning($"{name}: SimpleOSNpcAI could not find Torso.");
        }

        if (leftHand == null)
        {
            Debug.LogWarning($"{name}: SimpleOSNpcAI could not find OS left hand.");
        }

        if (rightHand == null)
        {
            Debug.LogWarning($"{name}: SimpleOSNpcAI could not find OS right hand.");
        }

    }
}

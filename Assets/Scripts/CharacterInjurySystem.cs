using System.Collections.Generic;
using UnityEngine;

public class CharacterInjurySystem : MonoBehaviour
{
    enum InjuryType
    {
        BrowCut,
        Concussion,
        BodyType1,
        BodyType2
    }

    [SerializeField] float bodyType1SpeedMultiplier = 0.6f;
    [SerializeField] float bodyType2RotationMultiplier = 0.6f;
    [Header("Player Injury UI")]
    [SerializeField] Texture2D browCutOverlayTexture;
    [SerializeField] Vector2 iconSize = new Vector2(42f, 42f);
    [SerializeField] float iconSpacing = 6f;
    [SerializeField] Texture2D browCutIcon;
    [SerializeField] Texture2D concussionIcon;
    [SerializeField] Texture2D bodyType1Icon;
    [SerializeField] Texture2D bodyType2Icon;

    Movement movement;
    Torso torso;
    bool isPlayer;
    readonly HashSet<InjuryType> appliedInjuries = new HashSet<InjuryType>();
    readonly List<InjuryType> appliedInjuryOrder = new List<InjuryType>();

    GUIStyle iconStyle;

    void Start()
    {
        movement = GetComponentInChildren<Movement>(true);
        torso = GetComponentInChildren<Torso>(true);
        isPlayer = torso != null && torso.AllowPlayerInput;
    }

    public void ApplyRandomHeadInjury()
    {
        List<InjuryType> available = new List<InjuryType>();
        if (!appliedInjuries.Contains(InjuryType.BrowCut))
        {
            available.Add(InjuryType.BrowCut);
        }

        if (!appliedInjuries.Contains(InjuryType.Concussion))
        {
            available.Add(InjuryType.Concussion);
        }

        ApplyRandomInjuryFrom(available, "head");
    }

    public void ApplyRandomBodyInjury()
    {
        List<InjuryType> available = new List<InjuryType>();
        if (!appliedInjuries.Contains(InjuryType.BodyType1))
        {
            available.Add(InjuryType.BodyType1);
        }

        if (!appliedInjuries.Contains(InjuryType.BodyType2))
        {
            available.Add(InjuryType.BodyType2);
        }

        ApplyRandomInjuryFrom(available, "body");
    }

    void ApplyRandomInjuryFrom(List<InjuryType> available, string regionName)
    {
        if (available.Count == 0)
        {
            Debug.Log($"{name}: no unique {regionName} injuries left to apply.");
            return;
        }

        InjuryType injury = available[Random.Range(0, available.Count)];
        ApplyInjury(injury);
    }

    void ApplyInjury(InjuryType injury)
    {
        if (!appliedInjuries.Add(injury))
        {
            return;
        }
        appliedInjuryOrder.Add(injury);
        if (isPlayer)
        {
            ManagementGameState.AddInjury();
        }

        switch (injury)
        {
            case InjuryType.BrowCut:
                ApplyBrowCut();
                break;
            case InjuryType.Concussion:
                ApplyConcussion();
                break;
            case InjuryType.BodyType1:
                ApplyBodyType1();
                break;
            case InjuryType.BodyType2:
                ApplyBodyType2();
                break;
        }

        Debug.Log($"{name}: injury applied -> {injury}.");
    }

    void ApplyBrowCut()
    {
        if (!isPlayer)
        {
            return;
        }

    }

    void ApplyConcussion()
    {
        if (!isPlayer || movement == null)
        {
            return;
        }

        movement.SetInputInverted(true);
    }

    void ApplyBodyType1()
    {
        if (movement != null)
        {
            movement.SetSpeedMultiplier(bodyType1SpeedMultiplier);
        }
    }

    void ApplyBodyType2()
    {
        if (torso != null)
        {
            torso.SetRotationSpeedMultiplier(bodyType2RotationMultiplier);
        }
    }

    void OnGUI()
    {
        if (!isPlayer)
        {
            return;
        }

        if (appliedInjuries.Contains(InjuryType.BrowCut))
        {
            DrawBrowCutOverlay();
        }

        EnsureGuiStyle();

        float x = 18f;
        float y = 18f;
        foreach (InjuryType injury in appliedInjuryOrder)
        {
            Rect iconRect = new Rect(x, y, iconSize.x, iconSize.y);
            DrawInjuryIcon(iconRect, injury);
            x += iconSize.x + iconSpacing;
        }
    }

    void DrawInjuryIcon(Rect iconRect, InjuryType injury)
    {
        Texture2D icon = GetIconTexture(injury);
        if (icon != null)
        {
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
            return;
        }

        GUI.Box(iconRect, GetIconLabel(injury), iconStyle);
    }

    void DrawBrowCutOverlay()
    {
        Rect overlayRect = new Rect(0f, 0f, Screen.width * 0.5f, Screen.height);
        Color previousColor = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 0.92f);

        if (browCutOverlayTexture != null)
        {
            GUI.DrawTexture(overlayRect, browCutOverlayTexture, ScaleMode.StretchToFill, true);
        }
        else
        {
            GUI.color = new Color(0.7f, 0f, 0f, 0.22f);
            GUI.DrawTexture(overlayRect, Texture2D.whiteTexture);
        }

        GUI.color = previousColor;
    }

    void EnsureGuiStyle()
    {
        if (iconStyle != null)
        {
            return;
        }

        iconStyle = new GUIStyle(GUI.skin.box);
        iconStyle.alignment = TextAnchor.MiddleCenter;
        iconStyle.fontSize = 15;
        iconStyle.normal.textColor = Color.white;
    }

    string GetIconLabel(InjuryType injury)
    {
        switch (injury)
        {
            case InjuryType.BrowCut:
                return "BC";
            case InjuryType.Concussion:
                return "CON";
            case InjuryType.BodyType1:
                return "T1";
            case InjuryType.BodyType2:
                return "T2";
            default:
                return "?";
        }
    }

    Texture2D GetIconTexture(InjuryType injury)
    {
        switch (injury)
        {
            case InjuryType.BrowCut:
                return browCutIcon;
            case InjuryType.Concussion:
                return concussionIcon;
            case InjuryType.BodyType1:
                return bodyType1Icon;
            case InjuryType.BodyType2:
                return bodyType2Icon;
            default:
                return null;
        }
    }
}

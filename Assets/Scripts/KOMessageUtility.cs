using UnityEngine;
using UnityEngine.SceneManagement;

public static class KOMessageUtility
{
    static KOMessageOverlay overlay;

    public static void ShowKOMessage()
    {
        ShowKOMessage(null, null);
    }

    public static void ShowKOMessage(Transform attackerRoot, Transform defenderRoot)
    {
        GameObject koMessage = FindSceneObject("KOmessage");
        if (koMessage != null)
        {
            koMessage.SetActive(true);
        }

        bool playerWon = DidPlayerWin(attackerRoot, defenderRoot);
        ManagementGameState.CompleteFight(playerWon);
        EnsureOverlay().Show();
    }

    static bool DidPlayerWin(Transform attackerRoot, Transform defenderRoot)
    {
        if (IsPlayerRoot(attackerRoot))
        {
            return true;
        }

        if (IsPlayerRoot(defenderRoot))
        {
            return false;
        }

        Debug.LogWarning("could not identify winner");
        return false;
    }

    static bool IsPlayerRoot(Transform root)
    {
        if (root == null)
        {
            return false;
        }

        Torso torso = root.GetComponentInChildren<Torso>(true);
        if (torso == null)
        {
            return false;
        }

        return torso.AllowPlayerInput;
    }

    static KOMessageOverlay EnsureOverlay()
    {
        if (overlay != null)
        {
            return overlay;
        }

        GameObject overlayObject = new GameObject("KOMessageOverlay");
        Object.DontDestroyOnLoad(overlayObject);
        overlay = overlayObject.AddComponent<KOMessageOverlay>();
        return overlay;
    }

    static GameObject FindSceneObject(string objectName)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject sceneObject in objects)
        {
            if (sceneObject.name == objectName && sceneObject.scene.IsValid())
            {
                return sceneObject;
            }
        }

        return null;
    }

    class KOMessageOverlay : MonoBehaviour
    {
        GUIStyle promptStyle;
        bool visible;
        string managementSceneName = "management scene";

        public void Show()
        {
            visible = true;
        }

        void Update()
        {
            if (!visible)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                visible = false;
                Debug.Log("ko continue pressed");
                SceneManager.LoadScene(managementSceneName);
            }
        }

        void OnGUI()
        {
            if (!visible)
            {
                return;
            }

            if (promptStyle == null)
            {
                promptStyle = new GUIStyle(GUI.skin.label);
                promptStyle.alignment = TextAnchor.MiddleCenter;
                promptStyle.fontSize = 28;
                promptStyle.normal.textColor = Color.white;
                promptStyle.richText = true;
            }

            Rect promptRect = new Rect(0f, Screen.height - 86f, Screen.width, 48f);
            GUI.Label(promptRect, "Press <color=red>Space</color> to continue", promptStyle);
        }
    }
}

using UnityEngine;

public static class KOMessageUtility
{
    static KOMessageOverlay overlay;

    public static void ShowKOMessage()
    {
        GameObject koMessage = FindSceneObject("KOmessage");
        if (koMessage != null)
        {
            koMessage.SetActive(true);
        }

        EnsureOverlay().Show();
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

        public void Show()
        {
            visible = true;
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

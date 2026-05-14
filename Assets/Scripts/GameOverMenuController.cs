using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenuController : MonoBehaviour
{
    [SerializeField] string mainMenuSceneName = "main menu";

    Camera menuCamera;
    GameObject backButton;
    GameObject exitButton;
    TextMesh reasonText;
    TextMesh daysText;
    TextMesh recordText;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        AddToGameOverScene(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AddToGameOverScene(scene);
    }

    static void AddToGameOverScene(Scene scene)
    {
        if (!scene.name.ToLowerInvariant().Contains("gameover"))
        {
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("game over menu: camera missing");
            return;
        }

        if (mainCamera.GetComponent<GameOverMenuController>() != null)
        {
            return;
        }

        mainCamera.gameObject.AddComponent<GameOverMenuController>();
    }










    void Start()
    {
        menuCamera = Camera.main;
        FindObjects();
        RefreshText();
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        if (WasClicked(backButton))
        {
            SceneManager.LoadScene(mainMenuSceneName);
            return;
        }

        if (WasClicked(exitButton))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    void FindObjects()
    {
        backButton = FindSceneObject("backButton");
        exitButton = FindSceneObject("exitButton");
        reasonText = FindText("reason");
        daysText = FindText("daysSurvived");
        recordText = FindText("record");
    }

    void RefreshText()
    {
        SetText(reasonText, "reason: " + ManagementGameState.GameOverReason);
        SetText(daysText, "days survived: " + ManagementGameState.Day);
        SetText(recordText, "record " + ManagementGameState.RecordText + " money: " + ManagementGameState.Money);
    }

    void SetText(TextMesh textMesh, string value)
    {
        if (textMesh != null)
        {
            textMesh.text = value;
        }
    }










    bool WasClicked(GameObject target)
    {
        if (target == null || !target.activeInHierarchy || menuCamera == null)
        {
            return false;
        }

        SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = target.GetComponentInChildren<SpriteRenderer>();
        }

        if (renderer == null)
        {
            return false;
        }

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Mathf.Abs(menuCamera.transform.position.z - renderer.transform.position.z);
        Vector3 worldPosition = menuCamera.ScreenToWorldPoint(mousePosition);
        return renderer.bounds.Contains(worldPosition);
    }

    TextMesh FindText(string objectName)
    {
        GameObject target = FindSceneObject(objectName);
        if (target == null)
        {
            return null;
        }

        return target.GetComponent<TextMesh>();
    }

    GameObject FindSceneObject(string objectName)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        string lowerName = objectName.ToLowerInvariant();

        foreach (GameObject sceneObject in objects)
        {
            if (sceneObject == null || !sceneObject.scene.IsValid())
            {
                continue;
            }

            if (sceneObject.name.ToLowerInvariant() == lowerName)
            {
                return sceneObject;
            }
        }

        return null;
    }
}

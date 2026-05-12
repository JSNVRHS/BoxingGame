using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    const string DifficultyKey = "Difficulty";
    const string NormalDifficulty = "Normal";
    const string HardDifficulty = "Hard";

    [SerializeField] string managementSceneName = "management scene";
    [SerializeField] string backgroundObjectName = "background";
    [SerializeField] string startButtonName = "Start";
    [SerializeField] string exitButtonName = "Exit";
    [SerializeField] string difficultyButtonsName = "difficutlyButtons";
    [SerializeField] string normalButtonName = "Normal";
    [SerializeField] string hardButtonName = "Hard";
    [SerializeField] string backButtonName = "back";
    [SerializeField] float cameraPadding = 1f;

    Camera menuCamera;
    SpriteRenderer backgroundRenderer;
    GameObject startButton;
    GameObject exitButton;
    GameObject difficultyButtons;
    GameObject normalButton;
    GameObject hardButton;
    GameObject backButton;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        AddToMenuScene(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AddToMenuScene(scene);
    }

    static void AddToMenuScene(Scene scene)
    {
        if (!IsMainMenuScene(scene))
        {
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        if (mainCamera.GetComponent<MainMenuController>() != null)
        {
            return;
        }

        mainCamera.gameObject.AddComponent<MainMenuController>();
    }

    static bool IsMainMenuScene(Scene scene)
    {
        string sceneName = scene.name.ToLowerInvariant();
        return sceneName.Contains("main") && sceneName.Contains("menu");
    }

    void Start()
    {
        menuCamera = Camera.main;
        backgroundRenderer = FindBackgroundRenderer();
        FindMenuObjects();
        ShowMainButtons();
        FitCameraToBackground();
    }

    void LateUpdate()
    {
        FitCameraToBackground();
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        if (WasClicked(startButton))
        {
            ShowDifficultyButtons();
            return;
        }

        if (WasClicked(exitButton))
        {
            ExitGame();
            return;
        }

        if (WasClicked(normalButton))
        {
            StartGame(NormalDifficulty);
            return;
        }

        if (WasClicked(hardButton))
        {
            StartGame(HardDifficulty);
            return;
        }

        if (WasClicked(backButton))
        {
            ShowMainButtons();
        }
    }

    void StartGame(string difficulty)
    {
        PlayerPrefs.SetString(DifficultyKey, difficulty);
        ManagementGameState.ResetRun();
        PlayerPrefs.Save();
        SceneManager.LoadScene(managementSceneName);
    }

    void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void FindMenuObjects()
    {
        startButton = FindNamedSceneObject(startButtonName);
        exitButton = FindNamedSceneObject(exitButtonName);
        difficultyButtons = FindNamedSceneObject(difficultyButtonsName);
        normalButton = FindNamedSceneObject(normalButtonName);
        hardButton = FindNamedSceneObject(hardButtonName);
        backButton = FindNamedSceneObject(backButtonName);
    }

    void ShowMainButtons()
    {
        SetObjectActive(startButton, true);
        SetObjectActive(exitButton, true);
        SetObjectActive(difficultyButtons, false);
    }

    void ShowDifficultyButtons()
    {
        SetObjectActive(startButton, true);
        SetObjectActive(exitButton, true);
        SetObjectActive(difficultyButtons, true);
    }

    void SetObjectActive(GameObject target, bool value)
    {
        if (target != null)
        {
            target.SetActive(value);
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

    void FitCameraToBackground()
    {
        if (menuCamera == null || backgroundRenderer == null)
        {
            return;
        }

        Bounds bounds = backgroundRenderer.bounds;
        if (bounds.size.x <= 0f || bounds.size.y <= 0f)
        {
            return;
        }

        menuCamera.orthographic = true;
        float z = menuCamera.transform.position.z;
        menuCamera.transform.position = new Vector3(bounds.center.x, bounds.center.y, z);

        float aspect = Mathf.Max(0.01f, (float)Screen.width / Screen.height);
        float verticalSize = bounds.extents.y;
        float horizontalSize = bounds.extents.x / aspect;
        menuCamera.orthographicSize = Mathf.Max(verticalSize, horizontalSize) * cameraPadding;
    }

    SpriteRenderer FindBackgroundRenderer()
    {
        GameObject backgroundObject = FindNamedSceneObject(backgroundObjectName);
        if (backgroundObject != null)
        {
            SpriteRenderer namedRenderer = backgroundObject.GetComponentInChildren<SpriteRenderer>();
            if (namedRenderer != null)
            {
                return namedRenderer;
            }
        }

        SpriteRenderer[] renderers = FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        SpriteRenderer largestRenderer = null;
        float largestArea = 0f;

        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            if (renderer.name.ToLowerInvariant() == backgroundObjectName.ToLowerInvariant())
            {
                return renderer;
            }

            float area = renderer.bounds.size.x * renderer.bounds.size.y;
            if (area > largestArea)
            {
                largestArea = area;
                largestRenderer = renderer;
            }
        }

        return largestRenderer;
    }

    GameObject FindNamedSceneObject(string objectName)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        string targetName = objectName.ToLowerInvariant();

        foreach (GameObject sceneObject in objects)
        {
            if (sceneObject == null || !sceneObject.scene.IsValid())
            {
                continue;
            }

            if (sceneObject.name.ToLowerInvariant() == targetName)
            {
                return sceneObject;
            }
        }

        return null;
    }
}

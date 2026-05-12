using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagementCameraFit : MonoBehaviour
{
    [SerializeField] float padding = 1.08f;
    [SerializeField] float minOrthographicSize = 5f;

    Camera targetCamera;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        AddToManagementScene(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AddToManagementScene(scene);
    }

    static void AddToManagementScene(Scene scene)
    {
        string sceneName = scene.name.ToLowerInvariant();
        if (!sceneName.Contains("management"))
        {
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("management camera fit: main camera not found");
            return;
        }

        if (mainCamera.GetComponent<ManagementCameraFit>() != null)
        {
            return;
        }

        mainCamera.gameObject.AddComponent<ManagementCameraFit>();
    }

    void Awake()
    {
        targetCamera = GetComponent<Camera>();
        FitCamera();
    }

    void LateUpdate()
    {
        FitCamera();
    }

    void FitCamera()
    {
        if (targetCamera == null)
        {
            return;
        }

        Bounds bounds;
        if (!TryGetSceneRenderBounds(out bounds))
        {
            Debug.LogWarning("management camera fit: no renderers found");
            return;
        }

        targetCamera.orthographic = true;

        Vector3 cameraPosition = targetCamera.transform.position;
        cameraPosition.x = bounds.center.x;
        cameraPosition.y = bounds.center.y;
        targetCamera.transform.position = cameraPosition;

        float aspect = (float)Screen.width / Screen.height;
        if (aspect <= 0f)
        {
            aspect = 1f;
        }

        float verticalSize = bounds.extents.y;
        float horizontalSize = bounds.extents.x / aspect;
        float wantedSize = Mathf.Max(verticalSize, horizontalSize) * padding;

        if (wantedSize < minOrthographicSize)
        {
            wantedSize = minOrthographicSize;
        }

        targetCamera.orthographicSize = wantedSize;
    }

    bool TryGetSceneRenderBounds(out Bounds bounds)
    {
        Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        bounds = new Bounds();
        bool foundRenderer = false;

        foreach (Renderer sceneRenderer in renderers)
        {
            if (sceneRenderer == null)
            {
                continue;
            }

            if (!sceneRenderer.gameObject.scene.IsValid())
            {
                continue;
            }

            if (!foundRenderer)
            {
                bounds = sceneRenderer.bounds;
                foundRenderer = true;
            }
            else
            {
                bounds.Encapsulate(sceneRenderer.bounds);
            }
        }

        return foundRenderer;
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraZoomLock : MonoBehaviour
{
    [SerializeField] float fixedOrthographicSize = 9f;
    [SerializeField] float fixedFieldOfView = 60f;

    Camera targetCamera;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        AddToMainCamera(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AddToMainCamera(scene);
    }

    static void AddToMainCamera(Scene scene)
    {
        if (!ShouldLockZoom(scene))
        {
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null && mainCamera.GetComponent<CameraZoomLock>() == null)
        {
            mainCamera.gameObject.AddComponent<CameraZoomLock>();
        }
    }

    static bool ShouldLockZoom(Scene scene)
    {
        return scene.name.ToLowerInvariant().Contains("fight");
    }

    void Awake()
    {
        targetCamera = GetComponent<Camera>();
        ApplyLockedZoom();
    }

    void LateUpdate()
    {
        ApplyLockedZoom();
    }

    void ApplyLockedZoom()
    {
        if (targetCamera == null)
        {
            return;
        }

        if (targetCamera.orthographic)
        {
            targetCamera.orthographicSize = fixedOrthographicSize;
        }
        else
        {
            targetCamera.fieldOfView = fixedFieldOfView;
        }
    }
}

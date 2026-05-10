using UnityEngine;

public class CameraZoomLock : MonoBehaviour
{
    [SerializeField] float fixedOrthographicSize = 9f;
    [SerializeField] float fixedFieldOfView = 60f;

    Camera targetCamera;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AddToMainCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null && mainCamera.GetComponent<CameraZoomLock>() == null)
        {
            mainCamera.gameObject.AddComponent<CameraZoomLock>();
        }
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

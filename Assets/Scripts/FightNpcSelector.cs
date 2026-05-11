using UnityEngine;
using UnityEngine.SceneManagement;

public class FightNpcSelector : MonoBehaviour
{
    [SerializeField] string firstNpcName = "NPCJason";
    [SerializeField] string secondNpcName = "NPCBob";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        AddToFightScene(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AddToFightScene(scene);
    }

    static void AddToFightScene(Scene scene)
    {
        if (!scene.name.ToLowerInvariant().Contains("fight"))
        {
            return;
        }

        if (FindFirstObjectByType<FightNpcSelector>() != null)
        {
            return;
        }

        new GameObject("FightNpcSelector").AddComponent<FightNpcSelector>();
    }

    void Awake()
    {
        GameObject firstNpc = FindNamedSceneObject(firstNpcName);
        GameObject secondNpc = FindNamedSceneObject(secondNpcName);

        if (firstNpc == null || secondNpc == null)
        {
            Debug.LogWarning("FightNpcSelector could not find both NPCJason and NPCBob.");
            return;
        }

        bool useFirstNpc = Random.value < 0.5f;
        firstNpc.SetActive(useFirstNpc);
        secondNpc.SetActive(!useFirstNpc);
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

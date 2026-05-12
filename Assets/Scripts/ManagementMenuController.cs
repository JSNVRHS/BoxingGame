using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagementMenuController : MonoBehaviour
{
    [SerializeField] string fightSceneName = "fight scene";
    [SerializeField] string gameOverSceneName = "gameover scene";

    Camera menuCamera;
    GameObject fightButton;
    GameObject healButton;
    GameObject billsButton;
    GameObject sleepButton;
    GameObject retireButton;
    GameObject gangsterPanel;
    GameObject startButton;
    GameObject restTextObject;
    TextMesh moneyText;
    TextMesh billsText;
    TextMesh injuryText;
    TextMesh dayText;
    TextMesh recordText;
    TextMesh priceText;
    TextMesh statusText;

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
        if (!scene.name.ToLowerInvariant().Contains("management"))
        {
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("management menu: camera missing");
            return;
        }

        if (mainCamera.GetComponent<ManagementMenuController>() != null)
        {
            return;
        }

        mainCamera.gameObject.AddComponent<ManagementMenuController>();
    }

    void Start()
    {
        menuCamera = Camera.main;
        ManagementGameState.EnsureStarted();
        FindObjects();
        SetObjectActive(gangsterPanel, false);
        RefreshUI();
        CheckDailyLoss();
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        if (WasClicked(startButton))
        {
            LoadFightScene();
            return;
        }

        if (WasClicked(fightButton))
        {
            PressFight();
            return;
        }

        if (WasClicked(healButton))
        {
            PressHeal();
            return;
        }

        if (WasClicked(billsButton))
        {
            PressBills();
            return;
        }

        if (WasClicked(sleepButton))
        {
            PressSleep();
            return;
        }

        if (WasClicked(retireButton))
        {
            PressRetire();
        }
    }

    void FindObjects()
    {
        fightButton = FindSceneObject("fightButton");
        healButton = FindSceneObject("HealButton");
        billsButton = FindSceneObject("billsButton");
        sleepButton = FindSceneObject("sleepButton");
        retireButton = FindSceneObject("retireButton");
        gangsterPanel = FindSceneObject("gangster");
        startButton = FindSceneObject("startButton");
        restTextObject = FindSceneObject("rest");

        moneyText = FindText("counterMoney");
        billsText = FindText("counterBills");
        injuryText = FindText("counterInjury");
        dayText = FindText("daysCounter");
        recordText = FindText("record");
        priceText = FindText("price");
        statusText = FindText("status");
    }

    void PressFight()
    {
        if (ManagementGameState.FightUsedToday)
        {
            SetObjectActive(restTextObject, true);
            Debug.Log("fight blocked, rest needed");
            return;
        }

        ManagementGameState.FightUsedToday = true;

        if (Random.value < 0.2f)
        {
            ManagementGameState.PrepareFight(true);
            SetObjectActive(gangsterPanel, true);
            Debug.Log("gangster encounter");
        }
        else
        {
            ManagementGameState.PrepareFight(false);
            LoadFightScene();
        }

        RefreshUI();
    }

    void PressHeal()
    {
        if (ManagementGameState.Injuries <= 0)
        {
            Debug.Log("heal button inactive");
            return;
        }

        ManagementGameState.TryHealInjury();
        RefreshUI();
        CheckDailyLoss();
    }

    void PressBills()
    {
        if (ManagementGameState.Bills != 0)
        {
            Debug.Log("pay bills button inactive");
            return;
        }

        ManagementGameState.TryPayBills();
        RefreshUI();
    }

    void PressSleep()
    {
        ManagementGameState.SleepOneDay();
        SetObjectActive(gangsterPanel, false);
        RefreshUI();
        CheckDailyLoss();
    }

    void PressRetire()
    {
        ManagementGameState.Retire();
        SceneManager.LoadScene(gameOverSceneName);
    }

    void LoadFightScene()
    {
        SceneManager.LoadScene(fightSceneName);
    }

    void RefreshUI()
    {
        SetText(moneyText, ManagementGameState.Money.ToString());
        SetText(billsText, ManagementGameState.Bills.ToString());
        SetText(injuryText, ManagementGameState.Injuries + "/4");
        SetText(dayText, "Day: " + ManagementGameState.Day);
        SetText(recordText, ManagementGameState.RecordText);

        if (ManagementGameState.Injuries > 0)
        {
            SetText(priceText, ManagementGameState.MedPrice + "MP");
        }
        else
        {
            SetText(priceText, "N/A");
        }

        if (ManagementGameState.Bills == 0)
        {
            SetText(statusText, "today");
        }
        else
        {
            SetText(statusText, "not today");
        }

        SetObjectActive(restTextObject, ManagementGameState.FightUsedToday);
    }

    void CheckDailyLoss()
    {
        if (ManagementGameState.HasDailyLossCondition())
        {
            Debug.Log("daily loss found: " + ManagementGameState.GameOverReason);
            SceneManager.LoadScene(gameOverSceneName);
        }
    }

    void SetText(TextMesh textMesh, string value)
    {
        if (textMesh != null)
        {
            textMesh.text = value;
        }
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

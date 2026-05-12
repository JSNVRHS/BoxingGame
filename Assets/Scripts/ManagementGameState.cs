using UnityEngine;

public static class ManagementGameState
{
    const string Prefix = "ManagementRun_";
    const string InitializedKey = Prefix + "Initialized";
    const string MoneyKey = Prefix + "Money";
    const string BillsKey = Prefix + "Bills";
    const string InjuriesKey = Prefix + "Injuries";
    const string DayKey = Prefix + "Day";
    const string WinsKey = Prefix + "Wins";
    const string LossesKey = Prefix + "Losses";
    const string MedPriceKey = Prefix + "MedPrice";
    const string FightUsedKey = Prefix + "FightUsed";
    const string GangsterFightKey = Prefix + "GangsterFight";
    const string FightResolvedKey = Prefix + "FightResolved";
    const string SelectedNpcKey = Prefix + "SelectedNpc";
    const string GameOverReasonKey = Prefix + "GameOverReason";

    public static int Money
    {
        get { return PlayerPrefs.GetInt(MoneyKey, 0); }
        set { PlayerPrefs.SetInt(MoneyKey, value); }
    }

    public static int Bills
    {
        get { return PlayerPrefs.GetInt(BillsKey, 5); }
        set { PlayerPrefs.SetInt(BillsKey, value); }
    }

    public static int Injuries
    {
        get { return PlayerPrefs.GetInt(InjuriesKey, 0); }
        set { PlayerPrefs.SetInt(InjuriesKey, Mathf.Clamp(value, 0, 4)); }
    }

    public static int Day
    {
        get { return PlayerPrefs.GetInt(DayKey, 1); }
        set { PlayerPrefs.SetInt(DayKey, value); }
    }

    public static int Wins
    {
        get { return PlayerPrefs.GetInt(WinsKey, 0); }
        set { PlayerPrefs.SetInt(WinsKey, value); }
    }

    public static int Losses
    {
        get { return PlayerPrefs.GetInt(LossesKey, 0); }
        set { PlayerPrefs.SetInt(LossesKey, value); }
    }

    public static int MedPrice
    {
        get { return PlayerPrefs.GetInt(MedPriceKey, 20); }
        set { PlayerPrefs.SetInt(MedPriceKey, value); }
    }

    public static bool FightUsedToday
    {
        get { return PlayerPrefs.GetInt(FightUsedKey, 0) == 1; }
        set
        {
            if (value)
            {
                PlayerPrefs.SetInt(FightUsedKey, 1);
            }
            else
            {
                PlayerPrefs.SetInt(FightUsedKey, 0);
            }
        }
    }

    public static bool CurrentFightIsGangster
    {
        get { return PlayerPrefs.GetInt(GangsterFightKey, 0) == 1; }
        set
        {
            if (value)
            {
                PlayerPrefs.SetInt(GangsterFightKey, 1);
            }
            else
            {
                PlayerPrefs.SetInt(GangsterFightKey, 0);
            }
        }
    }

    public static bool FightResolved
    {
        get { return PlayerPrefs.GetInt(FightResolvedKey, 0) == 1; }
        set
        {
            if (value)
            {
                PlayerPrefs.SetInt(FightResolvedKey, 1);
            }
            else
            {
                PlayerPrefs.SetInt(FightResolvedKey, 0);
            }
        }
    }

    public static string SelectedNpcName
    {
        get { return PlayerPrefs.GetString(SelectedNpcKey, ""); }
        set { PlayerPrefs.SetString(SelectedNpcKey, value); }
    }

    public static string GameOverReason
    {
        get { return PlayerPrefs.GetString(GameOverReasonKey, ""); }
        set { PlayerPrefs.SetString(GameOverReasonKey, value); }
    }

    public static string RecordText
    {
        get { return Wins + "-" + Losses; }
    }

    public static void EnsureStarted()
    {
        if (PlayerPrefs.GetInt(InitializedKey, 0) == 1)
        {
            return;
        }

        ResetRun();
    }

    public static void ResetRun()
    {
        PlayerPrefs.SetInt(InitializedKey, 1);
        Money = 0;
        Bills = 5;
        Injuries = 0;
        Day = 1;
        Wins = 0;
        Losses = 0;
        MedPrice = 20;
        FightUsedToday = false;
        CurrentFightIsGangster = false;
        FightResolved = false;
        SelectedNpcName = "";
        GameOverReason = "";
        PlayerPrefs.Save();
        Debug.Log("management state reset");
    }

    public static void PrepareFight(bool gangsterFight)
    {
        CurrentFightIsGangster = gangsterFight;
        FightResolved = false;
        if (Random.value < 0.5f)
        {
            SelectedNpcName = "NPCBob";
        }
        else
        {
            SelectedNpcName = "NPCJason";
        }
        PlayerPrefs.Save();
        Debug.Log("fight prepared: " + SelectedNpcName + ", gangster=" + gangsterFight);
    }

    public static void CompleteFight(bool playerWon)
    {
        if (FightResolved)
        {
            Debug.Log("fight result already saved");
            return;
        }

        FightResolved = true;

        if (playerWon)
        {
            Wins = Wins + 1;
            int prize = Random.Range(5, 10);
            Money = Money + prize;
            Debug.Log("player won, money +" + prize);
        }
        else
        {
            Losses = Losses + 1;
            if (CurrentFightIsGangster)
            {
                Money = Money + 20;
                Debug.Log("player lost gangster fight, money +20");
            }
            else
            {
                Debug.Log("player lost normal fight, money +0");
            }
        }

        PlayerPrefs.Save();
    }

    public static void AddInjury()
    {
        if (Injuries >= 4)
        {
            Debug.Log("injury ignored, already at max");
            return;
        }

        Injuries = Injuries + 1;
        PlayerPrefs.Save();
        Debug.Log("injury added, count=" + Injuries);
    }

    public static bool TryHealInjury()
    {
        if (Injuries <= 0)
        {
            Debug.Log("heal failed, no injuries");
            return false;
        }

        if (Money < MedPrice)
        {
            Debug.Log("heal failed, not enough money");
            return false;
        }

        Money = Money - MedPrice;
        Injuries = Injuries - 1;
        MedPrice = Mathf.CeilToInt(MedPrice * 1.25f);
        PlayerPrefs.Save();
        Debug.Log("injury healed, new price=" + MedPrice);
        return true;
    }

    public static bool TryPayBills()
    {
        if (Bills != 0)
        {
            Debug.Log("bills cannot be paid today");
            return false;
        }

        Money = Money - 50;
        Bills = Bills + 5;
        PlayerPrefs.Save();
        Debug.Log("bills paid");
        return true;
    }

    public static void SleepOneDay()
    {
        Day = Day + 1;
        Bills = Bills - 1;
        FightUsedToday = false;
        PlayerPrefs.Save();
        Debug.Log("sleep done, day=" + Day + ", bills=" + Bills);
    }

    public static bool HasDailyLossCondition()
    {
        if (Injuries >= 4)
        {
            GameOverReason = "Injuries";
            PlayerPrefs.Save();
            return true;
        }

        if (Bills <= -1)
        {
            GameOverReason = "Starvation";
            PlayerPrefs.Save();
            return true;
        }

        return false;
    }

    public static void Retire()
    {
        GameOverReason = "Retired";
        PlayerPrefs.Save();
        Debug.Log("player retired");
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : SingletonMonobehaviour<TimeManager>, ISaveable
{

    private int gameYear = 1;
    private Season gameSeason = Season.Spring;
    private int gameDay = 1;
    private int gameHour = 6;
    private int gameMinute = 30;
    private int gameSecond = 0;
    private string gameDayOfWeek = "Mon";

    private bool gameClockPaused = false;

    private float gameTick = 0f;

    private string _iSaveableUniqueID;
    public string ISaveableUniqueID { get { return _iSaveableUniqueID; } set { _iSaveableUniqueID = value; } }

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }

    protected override void Awake()
    {
        base.Awake();

        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    private void OnEnable()
    {
        ISaveableRegister();

        EventHandler.BeforeSceneUnloadEvent += BeforeSceneUnloadFadeOut;
        EventHandler.AfterSceneLoadEvent += AfterSceneLoadFadeIn;
    }

    private void OnDisable()
    {
        ISaveableDeregister();

        EventHandler.BeforeSceneUnloadEvent -= BeforeSceneUnloadFadeOut;
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoadFadeIn;
    }

    private void BeforeSceneUnloadFadeOut()
    {
        gameClockPaused = true;
    }

    private void AfterSceneLoadFadeIn()
    {
        gameClockPaused = false;
    }


    private void Start()
    {
        EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
    }

    private void Update()
    {
        if (!gameClockPaused)
        {
            GameTick();
        }
    }

    private void GameTick()
    {
        gameTick += Time.deltaTime;

        if (gameTick >= Settings.secondsPerGameSecond)
        {
            gameTick -= Settings.secondsPerGameSecond;

            UpdateGameSecond();
        }
    }

    private void UpdateGameSecond()
    {
        gameSecond++;

        if (gameSecond > 59)
        {
            gameSecond = 0;
            gameMinute++;


            if (gameMinute > 59)
            {
                gameMinute = 0;
                gameHour++;

                if (gameHour > 23)
                {
                    gameHour = 0;
                    gameDay++;

                    if (gameDay > 30)
                    {
                        gameDay = 1;

                        int gs = (int)gameSeason;
                        gs++;

                        gameSeason = (Season)gs;

                        if (gs > 3)
                        {
                            gs = 0;
                            gameSeason = (Season)gs;

                            gameYear++;

                            if (gameYear > 9999)
                                gameYear = 1;


                            EventHandler.CallAdvanceGameYearEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                        }

                        EventHandler.CallAdvanceGameSeasonEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                    }

                    gameDayOfWeek = GetDayOfWeek();
                    EventHandler.CallAdvanceGameDayEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                }

                EventHandler.CallAdvanceGameHourEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
            }

            EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);

        }

        // Call to advance game second event would go here if required
    }

    private string GetDayOfWeek()
    {
        int totalDays = (((int)gameSeason) * 30) + gameDay;
        int dayOfWeek = totalDays % 7;

        switch (dayOfWeek)
        {
            case 1:
                return "Mon";

            case 2:
                return "Tue";

            case 3:
                return "Wed";

            case 4:
                return "Thu";

            case 5:
                return "Fri";

            case 6:
                return "Sat";

            case 0:
                return "Sun";

            default:
                return "";
        }
    }

    public TimeSpan GetGameTime()
    {
        TimeSpan gameTime = new TimeSpan(gameHour, gameMinute, gameSecond);

        return gameTime;
    }

    public Season GetGameSeason()
    {
        return gameSeason;
    }


    //TODO:Remove
    /// <summary>
    /// Advance 1 game minute
    /// </summary>
    public void TestAdvanceGameMinute()
    {
        for (int i = 0; i < 60; i++)
        {
            UpdateGameSecond();
        }
    }

    //TODO:Remove
    /// <summary>
    /// Advance 1 day
    /// </summary>
    public void TestAdvanceGameDay()
    {
        for (int i = 0; i < 86400; i++)
        {
            UpdateGameSecond();
        }
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    public GameObjectSave ISaveableSave()
    {
        // Delete existing scene save if exists
        GameObjectSave.sceneData.Remove(Settings.PersistentScene);

        // Create new scene save
        SceneSave sceneSave = new SceneSave();

        // Create new int dictionary
        sceneSave.intDictionary = new Dictionary<string, int>();

        // Create new string dictionary
        sceneSave.stringDictionary = new Dictionary<string, string>();

        // Add values to the int dictionary
        sceneSave.intDictionary.Add("gameYear", gameYear);
        sceneSave.intDictionary.Add("gameDay", gameDay);
        sceneSave.intDictionary.Add("gameHour", gameHour);
        sceneSave.intDictionary.Add("gameMinute", gameMinute);
        sceneSave.intDictionary.Add("gameSecond", gameSecond);

        // Add values to the string dictionary
        sceneSave.stringDictionary.Add("gameDayOfWeek", gameDayOfWeek);
        sceneSave.stringDictionary.Add("gameSeason", gameSeason.ToString());

        // Add scene save to game object for persistent scene
        GameObjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);

        return GameObjectSave;
    }

    public void ISaveableLoad(GameSave gameSave)
    {
        // Get saved gameobject from gameSave data
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave))
        {
            GameObjectSave = gameObjectSave;

            // Get savedscene data for gameObject
            if (GameObjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave))
            {
                // if int and string dictionaries are found
                if (sceneSave.intDictionary != null && sceneSave.stringDictionary != null)
                {
                    // populate saved int values
                    if (sceneSave.intDictionary.TryGetValue("gameYear", out int savedGameYear))
                        gameYear = savedGameYear;

                    if (sceneSave.intDictionary.TryGetValue("gameDay", out int savedGameDay))
                        gameDay = savedGameDay;

                    if (sceneSave.intDictionary.TryGetValue("gameHour", out int savedGameHour))
                        gameHour = savedGameHour;

                    if (sceneSave.intDictionary.TryGetValue("gameMinute", out int savedGameMinute))
                        gameMinute = savedGameMinute;

                    if (sceneSave.intDictionary.TryGetValue("gameSecond", out int savedGameSecond))
                        gameSecond = savedGameSecond;

                    // populate string saved values
                    if (sceneSave.stringDictionary.TryGetValue("gameDayOfWeek", out string savedGameDayOfWeek))
                        gameDayOfWeek = savedGameDayOfWeek;

                    if (sceneSave.stringDictionary.TryGetValue("gameSeason", out string savedGameSeason))
                    {
                        if (Enum.TryParse<Season>(savedGameSeason, out Season season))
                        {
                            gameSeason = season;
                        }
                    }

                    // Zero gametick
                    gameTick = 0f;

                    // Trigger advance minute event
                    EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);

                    // Refresh game clock
                }
            }
        }
    }
    public void ISaveableStoreScene(string sceneName)
    {
        // Nothing required here since Time Manager is running on the persistent scene
    }

    public void ISaveableRestoreScene(string sceneName)
    {
        // Nothing required here since Time Manager is running on the persistent scene
    }

    private IEnumerator AdvanceGameHourWithFade()
    {
        // Fade to black
        yield return SceneControllerManager.Instance.FadeToBlackCoroutine();

        // Advance the hour
        gameHour++;

        if (gameHour > 23)
        {
            gameHour = 0;
            gameDay++;

            if (gameDay > 30)
            {
                gameDay = 1;

                int gs = (int)gameSeason;
                gs++;
                gameSeason = (Season)gs;

                if (gs > 3)
                {
                    gs = 0;
                    gameSeason = (Season)gs;
                    gameYear++;

                    if (gameYear > 9999)
                        gameYear = 1;

                    EventHandler.CallAdvanceGameYearEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                }

                EventHandler.CallAdvanceGameSeasonEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
            }

            gameDayOfWeek = GetDayOfWeek();
            EventHandler.CallAdvanceGameDayEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
        }

        EventHandler.CallAdvanceGameHourEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);

        // Fade back in
        yield return SceneControllerManager.Instance.FadeFromBlackCoroutine();
        Player.Instance.EnablePlayerInput();
    }

    public void SetTimeToDawn()
    {
        Time.timeScale = 1;
        StartCoroutine(SetTimeToDawnCoroutine());
    }

    private void AdvanceGameHour()
    {
        gameHour++;

        if (gameHour > 23)
        {
            gameHour = 0;
            gameDay++;

            if (gameDay > 30)
            {
                gameDay = 1;

                int gs = (int)gameSeason;
                gs++;
                gameSeason = (Season)gs;

                if (gs > 3)
                {
                    gs = 0;
                    gameSeason = (Season)gs;
                    gameYear++;

                    if (gameYear > 9999)
                        gameYear = 1;

                    EventHandler.CallAdvanceGameYearEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                }

                EventHandler.CallAdvanceGameSeasonEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
            }

            gameDayOfWeek = GetDayOfWeek();
            EventHandler.CallAdvanceGameDayEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
        }

        EventHandler.CallAdvanceGameHourEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
    }

    private IEnumerator SetTimeToDawnCoroutine()
    {
        // Disable player input and fade to black at the beginning
        Player.Instance.DisablePlayerInput();
        yield return SceneControllerManager.Instance.FadeToBlackCoroutine();

        // Reset minutes and seconds first
        gameMinute = 0;
        gameSecond = 0;
        
        int hoursAdvanced = 0;
        
        // If it's already dawn (6 AM) or later, advance to next day's dawn
        if (gameHour >= 6)
        {
            // Calculate hours needed to reach next day's 6 AM
            hoursAdvanced = (24 - gameHour) + 6;
            
            // Advance time hour by hour without fade effects
            for (int i = 0; i < hoursAdvanced; i++)
            {
                AdvanceGameHour();
            }
        }
        else
        {
            // If it's before 6 AM, advance to 6 AM same day
            hoursAdvanced = 6 - gameHour;
            for (int i = 0; i < hoursAdvanced; i++)
            {
                AdvanceGameHour();
            }
        }

        // Wait based on hours slept (e.g., 0.5 seconds per hour)
        float sleepDelay = hoursAdvanced * 0.5f;
        yield return new WaitForSeconds(sleepDelay);

        // Fade back in and re-enable player input at the end
        yield return SceneControllerManager.Instance.FadeFromBlackCoroutine();
        Player.Instance.EnablePlayerInput();
    }

    public void SetTimeToDusk()
    {
        Time.timeScale = 1;
        StartCoroutine(SetTimeToDuskCoroutine());
    }

    private IEnumerator SetTimeToDuskCoroutine()
    {
        // Disable player input and fade to black at the beginning
        Player.Instance.DisablePlayerInput();
        yield return SceneControllerManager.Instance.FadeToBlackCoroutine();

        // Reset minutes and seconds first
        gameMinute = 0;
        gameSecond = 0;

        int hoursAdvanced = 0;

        // If it's already dusk (7 PM) or later, advance to next day's dusk
        if (gameHour >= 19)
        {
            // Calculate hours needed to reach next day's 7 PM (19:00)
            hoursAdvanced = (24 - gameHour) + 19;

            // Advance time hour by hour without fade effects
            for (int i = 0; i < hoursAdvanced; i++)
            {
                AdvanceGameHour();
            }
        }
        else
        {
            // If it's before 7 PM, advance to 7 PM same day
            hoursAdvanced = 19 - gameHour;
            for (int i = 0; i < hoursAdvanced; i++)
            {
                AdvanceGameHour();
            }
        }

        // Wait based on hours advanced (e.g., 0.5 seconds per hour)
        float sleepDelay = hoursAdvanced * 0.5f;
        yield return new WaitForSeconds(sleepDelay);

        // Fade back in and re-enable player input at the end
        yield return SceneControllerManager.Instance.FadeFromBlackCoroutine();
        Player.Instance.EnablePlayerInput();
    }
}


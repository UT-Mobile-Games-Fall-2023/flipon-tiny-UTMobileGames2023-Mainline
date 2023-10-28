using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyRewards : MonoBehaviour
{
    [System.Serializable]
    public class LoginData
    {
        public bool firstTimePlaying;
        public bool claimedReward;
        public string dateLastOpened; // calendar date (1st - 31st)
        public int dayLastOpened; // reward day number (Day 1 - 7)
        public int daysRemaining;
        public bool dailyRewardsLoaded;
    }

    private bool isNewDay;
    private float currentTime;

    private bool firstTimePlaying; // local bool to track if it's still the player's first time playing/seeing rewards
    private int currentDay;
    private int daysRemaining;
    private bool claimedReward;

    private string timeGameOpened;
    private string currentDate; 
    private string currentTimeInGame;

    public string testDate; // date that can be changed for testing

    // UI
    public Button[] rewardButtons = new Button[7];
    public GameObject pip;
    public GameObject daysRemainingText;

    // sounds 
    public AudioSource audioSource;

    // data
    private string savePath;

    // manager instance
    DailyRewards dailyRewardsManager;

    private void Awake()
    {
        if (dailyRewardsManager == null)
        {
            dailyRewardsManager = GetComponent<DailyRewards>();
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        // audio
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // create a save path for data file (data has not been saved yet)
        savePath = Path.Combine(Application.persistentDataPath, "loginData.dat");

        // returns boolean from login data
        firstTimePlaying = CheckFirstTimePlaying();

        // test time
        DateTime testDateTime =  DateTime.Parse(testDate);

        // check if user's first time opening game (need to track this in the title screen, store it/set boolean, check data here)
        if (firstTimePlaying)
        {
            // save some inital data here
            LoginData firstDayData = new LoginData();
            firstDayData.firstTimePlaying = true;
            firstDayData.claimedReward = false;
            firstDayData.dateLastOpened = DateTime.Now.ToString("d");
            firstDayData.dayLastOpened = 1;
            firstDayData.daysRemaining = 7;
            firstDayData.dailyRewardsLoaded = false;

            SaveLoginData(firstDayData);

            // set 1st day rewards
            ResetRewards();
        }
        // if not first time opening, check how much time has passed since last log-in
        else
        {
            // get current date when game is opened 
            currentDate = DateTime.Now.ToString("d");
            DateTime currDateTime = DateTime.Now;

            // load data to get the date that the game was last opened on 
            LoginData data = LoadLoginData();

            if (data != null && data.dailyRewardsLoaded == false) // check that save file exists and that we haven't already done this
            {
                data.dailyRewardsLoaded = true;
                SaveLoginData(data);

                // check if the days are different (one or more days have passed)
                // CHANGE TESTDATE BACK TO DATA.DATELASTOPENED
                if (testDate != currentDate)
                {
                    // test time
                    DateTime dateLastOpenedData = DateTime.Parse(data.dateLastOpened);

                    // NOTE FOR LATER - NEED TO SAVE WHOLE DATE (WITH MONTH ETC.) TO BE ABLE TO CALCULATE TIMESPAN LATER NOT JUST THE DAY
                    // CHANGE TESTDATE BACK TO DATELASTOPENEDDATA
                    int daysPassed = CalculateDateChange(currDateTime, testDateTime); // how many days have passed

                    // days remaining is days remaining - dayspassed? 
                    print("Days Passed: " + daysPassed.ToString());

                    // check if claimed reward
                    claimedReward = data.claimedReward;

                    // one day has passed
                    if (daysPassed == 1)
                    {
                        if (data.claimedReward)
                        {
                            // reward was already claimed last time, just load the next day reward
                            currentDay = data.dayLastOpened + 1; // current day is updated 
                            data.daysRemaining--;
                            data.dayLastOpened++;
                            data.claimedReward = false;

                            if (currentDay > 7 || data.daysRemaining < 1)
                            {
                                currentDay = 1;
                                data.dayLastOpened = 1;
                                daysRemaining = 7;
                                data.daysRemaining = 7;
                            }
                            
                            SaveLoginData(data);
                            UnlockReward(currentDay); // only unlock the NEXT day IF they have already claimed the reward, otherwise keep the original day unlocked
                                                      // change it in the data too and save
                        }
                        else
                        {
                            // have not yet claimed reward, keep last day's reward unlocked only and decrement days remaining
                            print("STILL NEED TO CLAIM YESTERDAY'S REWARD");
                            currentDay = data.dayLastOpened;
                            data.daysRemaining--; 
                            if (currentDay > 7 || data.daysRemaining < 1)
                            {
                                currentDay = 1;
                                daysRemaining = 7;
                                data.daysRemaining = 7;
                                data.dayLastOpened = 1;
                            }
                            else
                            {
                                daysRemaining = data.daysRemaining - 1;
                            }
                            SaveLoginData(data);
                            UnlockReward(currentDay);
                        }
                        
                    }
                    // if MORE THAN ONE DAY has passed, calculate days remaining in the week OR reset
                    else if (daysPassed > 1)
                    {
                        data.daysRemaining -= daysPassed;
                        print(data.daysRemaining);

                        // reset rewards if week is over
                        if (data.daysRemaining < 1)
                        {
                            print("here");
                            currentDay = 1;
                            daysRemaining = 7;
                            data.daysRemaining = 7;
                            data.dayLastOpened = 1;
                            data.claimedReward = false; // resetting to day 1, should no longer be claimed
                        }
                        else
                        {
                            // last reward was claimed, unlock the next one
                            if (data.claimedReward)
                            {
                                currentDay = data.dayLastOpened + 1; // reward they should NOW be on
                                data.dayLastOpened++;
                                data.claimedReward = false;

                                if (currentDay > 7)
                                {
                                    currentDay = 1;
                                    daysRemaining = 7;
                                    data.daysRemaining = 7;
                                    data.dayLastOpened = 1;
                                }
                            }
                            // last reward was NOT claimed, keep that same one unlocked
                            else
                            {
                                currentDay = data.dayLastOpened; // reward they should be still on

                                if (currentDay > 7)
                                {
                                    currentDay = 1;
                                    daysRemaining = 7;
                                    data.daysRemaining = 7;
                                    data.dayLastOpened = 1;
                                }
                            }
                        }
                        SaveLoginData(data);
                        UnlockReward(currentDay);
                    }
                }
                // same day, coming back to the game
                else
                {
                    // check if they have claimed their reward already or not
                    claimedReward = data.claimedReward;
                    currentDay = data.dayLastOpened;

                    if (claimedReward)
                    {
                        // red notification needs to be disabled
                        print("Your reward was already claimed for today!");
                        // get rid of pip
                        pip.GetComponent<Image>().enabled = false;
                        print("current day " + currentDay.ToString());
                        print("remaining days " + data.daysRemaining.ToString());
                        print("day last opened " + data.dayLastOpened.ToString());
                    }
                    else
                    {
                        // still need to get their reward, data should be same as before already
                        print("Welcome back, you still have a reward to claim for today");
                        print("current day " + currentDay.ToString());
                        print("remaining days " + data.daysRemaining.ToString());
                        print("day last opened " + data.dayLastOpened.ToString());
                    }
                    
                }
            }
            else
            {
                print("Login/Reward data was already loaded");
            }

            // update date
            data.dateLastOpened = DateTime.Now.ToString("d");
            SaveLoginData(data);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // load data
        LoginData data = LoadLoginData();

        // continually get the CURRENT time (date)
        string currTimeInGame = DateTime.Now.ToString("d");
        
        // CHECK if the current date in game ever changes to be different than the saved time in game (goes to the next day at midnight)
        if (currTimeInGame != data.dateLastOpened)
        {
            print("NEW DAY - MIDNIGHT REACHED");
            // unlock the next day, update all data
            data.dateLastOpened = currTimeInGame;
            data.daysRemaining--;

            if (data.daysRemaining < 1)
            {
                // reset rewards
                data.daysRemaining = 7;
                data.dayLastOpened = 1;
                currentDay = 1;
                daysRemaining = 7;
            }
            else
            {
                // if last reward was claimed, unlock the next day
                if (data.claimedReward)
                {
                    currentDay = data.dayLastOpened + 1;
                    data.dayLastOpened++;
                    data.claimedReward = false;

                    if (currentDay > 7)
                    {
                        // reset rewards
                        currentDay = 1;
                        daysRemaining = 7;
                        data.dayLastOpened = 1;
                        data.daysRemaining = 7;
                    }
                }
                // if reward was not claimed, things stay the same (only thing that updates is the text/pip)
                else
                {

                }
            }

            // enable pip
            pip.GetComponent<Image>().enabled = true;

            // update text
            if (data.daysRemaining == 1)
            {
                daysRemainingText.GetComponent<TextMeshProUGUI>().text = data.daysRemaining.ToString() + " Day Remaining!"; // spelling
            }
            else
            {
                daysRemainingText.GetComponent<TextMeshProUGUI>().text = data.daysRemaining.ToString() + " Days Remaining!";
            }

            UnlockReward(currentDay);
            SaveLoginData(data);
        }
    }

    // runs when user clicks on button
    public void ClaimReward(int rewardAmount) // pass login data here, need to update so reward amount is in an array or something else that is editable
    {
        // get rid of pip
        pip.GetComponent<Image>().enabled = false;

        if (!claimedReward)
        {
            // gain currency
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddCurrency(rewardAmount);
                // CurrencyManager.Instance.AddCurrencyWithLimit(rewardAmount);
            }

            // make button no longer interactable 
            rewardButtons[currentDay - 1].interactable = false;

            // change image of button
            rewardButtons[currentDay - 1].transform.GetChild(3).GetComponent<Image>().enabled = false;

            claimedReward = true;
            firstTimePlaying = false;

            // load data, update it, and save
            LoginData data = LoadLoginData();
            data.claimedReward = true;
            data.firstTimePlaying = false;
            SaveLoginData(data);
        }
    }

    public void UnlockReward(int currentDay)
    {
        // load data, update it, and save
        LoginData data = LoadLoginData();

        // get button and set it to interactable 
        if (currentDay == 1)
        {
            ResetRewards();
        }
        else
        {
            for (int i = 0; i < rewardButtons.Length; i++)
            {
                rewardButtons[i].interactable = false;
            }
            rewardButtons[currentDay - 1].interactable = true;
        }

        // update data
        data.claimedReward = false;
        data.dayLastOpened = currentDay;
        SaveLoginData(data);
    }

    // Runs each time the daily rewards button is pressed
    public void LoadRewards()
    {
        LoginData loadedData = LoadLoginData();
        claimedReward = loadedData.claimedReward;
        currentDay = loadedData.dayLastOpened;
        daysRemaining = loadedData.daysRemaining;

        // reset rewards if on day 1
        if (currentDay > 7 || currentDay == 1 || daysRemaining == 0)
        {
            if (!claimedReward && daysRemaining == 7)
            {
                ResetRewards();
            }
            else if (claimedReward)
            {
                for (int i = 0; i < rewardButtons.Length; i++)
                {
                    rewardButtons[i].interactable = false;
                    if (i + 1 <= currentDay) // include the current day
                    {
                        // change the icon to "collected"
                        rewardButtons[i].transform.GetChild(3).GetComponent<Image>().enabled = false;
                    }
                }
            }
        }
        else
        {
            // if claimed reward, everything is not interactable but some buttons need a different icon
            if (claimedReward)
            {
                print("hereererhererererherherERERE");
                for (int i = 0; i < rewardButtons.Length; i++)
                {
                    rewardButtons[i].interactable = false;
                    if (i + 1 <= currentDay) // include the current day
                    {
                        // change the icon to "collected"
                        rewardButtons[i].transform.GetChild(3).GetComponent<Image>().enabled = false;
                    }
                }
            }
            // if not claimed reward, only unlock the current day reward and make sure icons are correct 
            else
            {
                rewardButtons[currentDay - 1].interactable = true; // day 1 button
                for (int i = 0; i < rewardButtons.Length; i++)
                {
                    if (i + 1 != currentDay)
                    {
                        rewardButtons[i].interactable = false; // all other buttons
                    }

                    if (i + 1 < currentDay)
                    {
                        rewardButtons[i].transform.GetChild(3).GetComponent<Image>().enabled = false;
                    }
                }
            }
        }

        // reload data
        LoginData updatedData = LoadLoginData();

        // set text for remaining days after things have been updated with days remaining
        if (updatedData.daysRemaining == 1)
        {
            daysRemainingText.GetComponent<TextMeshProUGUI>().text = updatedData.daysRemaining.ToString() + " Day Remaining!"; // spelling
        }
        else
        {
            daysRemainingText.GetComponent<TextMeshProUGUI>().text = updatedData.daysRemaining.ToString() + " Days Remaining!";
        }
    }

    // runs when past the 7th day or on first playthrough
    public void ResetRewards()
    {
        currentDay = 1;
        daysRemaining = 7;
        rewardButtons[0].interactable = true; // day 1 button
        for (int i = 1; i < rewardButtons.Length; i++)
        {
            rewardButtons[i].interactable = false; // all other buttons
        }

        // reset data
        LoginData data = LoadLoginData();
        data.dayLastOpened = 1;
        data.daysRemaining = 7;
        data.claimedReward = false;
        SaveLoginData(data);
    }

    public bool CheckFirstTimePlaying()
    {
        LoginData data = LoadLoginData();
        if (data != null)
        {
            return data.firstTimePlaying;
        }
        else
        {
            return true; // for now we assume that if there's no data saved then it is the first time they are playing
        }
    }

    // returns number of days that have passed since today and the last date they opened the game rewards
    public int CalculateDateChange(DateTime currentDate, DateTime previousDate)
    {
        TimeSpan interval = currentDate - previousDate;
        return interval.Days;
    }

    // data saving and loading (taken from GameManager script)
    public void SaveLoginData(LoginData data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream fileStream = File.Create(savePath);

        formatter.Serialize(fileStream, data);
        fileStream.Close();
    }

    public LoginData LoadLoginData()
    {
        if (File.Exists(savePath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = File.Open(savePath, FileMode.Open);

            LoginData data = (LoginData)formatter.Deserialize(fileStream);
            fileStream.Close();

            return data;
        }
        else
        {
            Debug.LogWarning("No save file found.");
            return null;
        }
    }

    // save data when exiting the game 
    private void OnApplicationQuit()
    {
        print("CLOSING GAME");
        LoginData data = LoadLoginData();
        data.dateLastOpened = DateTime.Now.ToString("d");
        data.dailyRewardsLoaded = false;
        print(data.dateLastOpened);
        SaveLoginData(data);
    }
    
}
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
    // private string timeGameClosed;


    public string testDate; // date that can be changed for testing

    // UI
    public Button[] rewardButtons = new Button[7];
    public GameObject pip;
    public GameObject daysRemainingText;

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
            // current date is stored ? to track when they have opened the game. Might need to do this at the end of gameplay 
            firstDayData.dateLastOpened = DateTime.Now.ToString("d");
            firstDayData.dayLastOpened = 1;
            firstDayData.daysRemaining = 7;
            firstDayData.dailyRewardsLoaded = false;

            // DATA IS SAVED
            SaveLoginData(firstDayData);

            // set 1st day rewards (should replace this with reset rewards)
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

                    // if only one day has passed, WE ARE GOOD YAY
                    if (daysPassed == 1)
                    {
                        print("ONE DAY PASSED");
                        if (data.claimedReward)
                        {
                            // reward was already claimed last time, just load the next day reward
                            currentDay = data.dayLastOpened + 1; // current day is updated 
                            data.daysRemaining--;
                            data.dayLastOpened++;

                            if (currentDay > 7 || data.daysRemaining < 1)
                            {
                                currentDay = 1;
                                daysRemaining = 7;
                                data.daysRemaining = 7;
                                data.dayLastOpened = 1;
                            }
                            data.claimedReward = false;
                            SaveLoginData(data);
                            UnlockReward(currentDay); // only unlock the NEXT day IF they have already claimed the reward, otherwise keep the original day unlocked
                                                      // change it in the data too and save
                        }
                        else
                        {
                            // have not yet claimed reward, keep last day's reward unlocked only and decrement days remaining
                            print("STILL NEED TO CLAIM LAST DAY'S REWARD");
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
                        // data should be updated
                        data.daysRemaining -= daysPassed;
                        if (data.daysRemaining < 1)
                        {
                            currentDay = 1;
                            data.daysRemaining = 7;
                            data.dayLastOpened = 1;
                        }
                        else
                        {
                            if (claimedReward)
                            {
                                currentDay = data.dayLastOpened + 1; // reward they should NOW be on
                                if (currentDay > 7)
                                {
                                    currentDay = 1;
                                }
                            }
                            else
                            {
                                currentDay = data.dayLastOpened; // reward they should be still on
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
                    if (claimedReward)
                    {
                        // red notification needs to be disabled
                        print("Your reward was already claimed for today!");
                        // get rid of pip
                        pip.GetComponent<Image>().enabled = false;
                    }
                    else
                    {
                        // still need to get their reward, data should be same as before already
                        print("Welcome back, you still have a reward to claim for today");
                    }
                    LoadRewards(); // WORKS YAY
                }
            }
            else
            {
                print("Login/Reward data was already loaded");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        /*
        // time in game (military time 00:00 - 23:59)
        currentTimeInGame = DateTime.Now.ToString("d");
        // print(currentTimeInGame);

        DateTime currHour = DateTime.Parse(currentTimeInGame);

        // if the days are different the DAY HAS CHANGED - update the previous day here!!! 
        if (currHour >= 0 && isNewDay)
        {
            if (firstTimePlaying)
            {
                firstTimePlaying = false;
            }
            else
            {
                isNewDay = false;

                // new reward needs to unlock 
                if (currentDay == 7)
                {
                    currentDay = 1;
                }
                else
                {
                    currentDay++;
                }

                // UnlockReward(currentDay);
            }
                
        }
        // otherwise, we are still in the same day 
        */
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

            // make sure color is normal?
            /*
            var colors = btn.colors;
            colors.pressedColor = new Color(0f, 0f, 0f, 0f);
            btn.colors = colors;
            */

            claimedReward = true; // NOTE FOR LATER: NEED TO UPDATE THE DATA SOMEHOW
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
        }
        else
        {
            // if claimed reward, everything is not interactable but some buttons need a different icon
            if (claimedReward)
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
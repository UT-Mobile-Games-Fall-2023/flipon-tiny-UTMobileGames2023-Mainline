using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	[System.Serializable]
	public class PlayerData
	{
		public string level = "Level 1";
	}

	private string savePath;
	public static GameManager gameManager;
	public Transform lvlParent;
	public LvlUnlockContainer lvlUnlocks;
	public GameObject CorruptBackgroundsParent;
	public Image[] CorruptBackgrounds = null;

	private int index = 0;
	private void Awake()
	{
		if (gameManager == null)
		{
			gameManager = this.GetComponent<GameManager>();
		}
		else
		{
			Destroy(this.gameObject);
		}
		if (lvlParent == null)
		{
			lvlParent = GameObject.FindFirstObjectByType<MapTouchDetection>().lvlParent;
		}
		if (CorruptBackgroundsParent != null)
		{
			CorruptBackgrounds = CorruptBackgroundsParent.GetComponentsInChildren<Image>();
		}
		DontDestroyOnLoad(this.gameObject);
		savePath = Path.Combine(Application.persistentDataPath, "playerData.dat");
		lvlUnlocks.LvlUnlockStates = new bool[lvlParent.childCount];
	}
	private void Start()
	{
		MapUIScript.mapInstance.currentLevelName = LoadLevel();
	}
	private void Update()
	{
		if (SceneManager.GetActiveScene().name == "Map_t")
		{
			//MapUIScript.mapInstance.currentLevelName = LoadLevel();
			if (lvlParent == null)
			{
				lvlParent = GameObject.Find("Level Buttons").transform;
			}
			if (CorruptBackgroundsParent != null && CorruptBackgrounds == null)
			{
				CorruptBackgrounds = CorruptBackgroundsParent.GetComponentsInChildren<Image>();
			}
		}
	}
	public void RevealMap(int level)
	{
		CheckIfStartDialogue(level);
		if (CorruptBackgrounds != null)
		{
			int region = level / 5;
			if (region <= 1)
			{
				CorruptBackgrounds[0].fillAmount = 1 - ((level - 1) % 5 * 0.2f);
			}
			else if (1 < region && region <= 2)
			{
				CorruptBackgrounds[1].fillAmount = 1 - ((level - 1) % 5 * 0.2f);
			}
			else if (2 < region && region <= 3)
			{
				CorruptBackgrounds[2].fillAmount = 1 - ((level - 1) % 5 * 0.2f);
			}
			else if (3 < region && region <= 4)
			{
				CorruptBackgrounds[3].fillAmount = 1 - ((level - 1) % 5 * 0.2f);
			}
			else if (region > 4)
			{
				CorruptBackgrounds[4].fillAmount = 0;
			}
		}
	}
	public void SavePlayerData(PlayerData data)
	{
		BinaryFormatter formatter = new BinaryFormatter();
		FileStream fileStream = File.Create(savePath);

		formatter.Serialize(fileStream, data);
		fileStream.Close();
	}

	public PlayerData LoadPlayerData()
	{
		if (File.Exists(savePath))
		{
			BinaryFormatter formatter = new BinaryFormatter();
			FileStream fileStream = File.Open(savePath, FileMode.Open);

			PlayerData data = (PlayerData)formatter.Deserialize(fileStream);
			fileStream.Close();

			return data;
		}
		else
		{
			Debug.LogWarning("No save file found.");
			return null;
		}
	}
	public void SaveLevel(string level = null)
	{
		PlayerData data = new PlayerData();
		data.level = level;
		SavePlayerData(data);
	}

	public string LoadLevel()
	{
		PlayerData data = LoadPlayerData();
		if (data != null)
		{
			LoadUnlocks(Int32.Parse(Regex.Match(data.level, @"\d+").Value));
			return data.level;
		}
		else
		{
			// If no save file exists, return a default value.
			LoadUnlocks(1);
			return "Level 1";
		}
	}
	public void CheckIfStartDialogue(int level)
	{
		if(level % 5 == 1)
		{
			DialogueManager.dialogueManager.StartDialogue();
		}
		if (level % 5 == 3 || level % 5 == 5)
		{
			DialogueManager.dialogueManager.StartDialogue();
		}
	}

	public void LoadUnlocks(int level)
	{
		if (SceneManager.GetActiveScene().name == "Map_t")
		{
			lvlParent = GameObject.Find("Level Buttons").transform;
		}
		RevealMap(level);
		for (int i = 0; i < level; i++)
		{
			lvlUnlocks.LvlUnlockStates[i] = true;
			lvlParent.GetChild(i).GetComponent<MapLvlButton>().SetUnlocked(true);
		}
	}

	public void LoadUnlocks()
	{
		if (lvlParent == null)
		{
			lvlParent = GameObject.FindFirstObjectByType<MapTouchDetection>().lvlParent;
		}
		//Set Level Values to Array
		for (int i = 0; i < lvlParent.childCount; i++)
		{
			lvlParent.GetChild(i).GetComponent<MapLvlButton>().SetUnlocked(lvlUnlocks.LvlUnlockStates[i]);
		}
		Debug.Log("Loading Unlocks");
		lvlUnlocks.PrintArray();
	}

	public void SaveUnlocks()
	{
		//Set Array To Level Values
		for (int i = 0; i < lvlParent.childCount; i++)
		{
			lvlUnlocks.SetupState(i, lvlParent.GetChild(i).GetComponent<MapLvlButton>().GetUnlocked());
		}
		Debug.Log("Saving Unlocks");
		lvlUnlocks.PrintArray();
	}
	public void ClearSavedData()
	{
		if (File.Exists(savePath))
		{
			File.Delete(savePath);
		}
	}
}

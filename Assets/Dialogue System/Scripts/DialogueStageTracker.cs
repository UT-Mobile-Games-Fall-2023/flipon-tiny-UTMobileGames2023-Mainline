using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DialogueStageTracker : MonoBehaviour
{
	// All canvases used by this script to create the dialogue box should match the names in the CreateDialogueBox() function

	public static DialogueStageTracker stageTracker;
	DialogueController dialogueController;
	public SingleDialogueDataArray array;
	public DialogueDataManager dialogueDataManager;
	public GameObject dialoguePrefab;
	public float currentStage = 0f;
	public List<SingleDialogueData> dialogueEntries = new List<SingleDialogueData>();
	public bool isPlaying = false;

	private void Awake()
	{
		if (stageTracker == null)
		{
			stageTracker = this.GetComponent<DialogueStageTracker>();
		}
		else
		{
			Destroy(this.gameObject);
		}
		dialogueController = this.GetComponent<DialogueController>();
		dialogueDataManager = LoadDialogueData();
	}
	private void Start()
	{
		string level = "Level 1";
		if (GameManager.gameManager.CurrentLevel != null)
		{
			level = GameManager.gameManager.CurrentLevel;
		}
		else
		{
			GameManager.gameManager.lvlParent.gameObject.SetActive(false);
		}
		currentStage = Int32.Parse(Regex.Match(level, @"\d+").Value) / 5.0f;
		foreach (SingleDialogueData singleDialogue in dialogueDataManager.allDialogues)
		{
			dialogueEntries.Add(singleDialogue);
		}
	}
	public void RemoveEntry()
	{
		stageTracker.dialogueEntries.RemoveAt(0);
		stageTracker.dialogueDataManager.shownDialogues.Add(dialogueDataManager.allDialogues[0]);
		stageTracker.dialogueDataManager.allDialogues.RemoveAt(0);
	}
	private void Update()
	{
		stageTracker.isPlaying = dialogueController.isPlaying;
		
		if (stageTracker.isPlaying)
		{
			CheckTouch();
		}
		else if(currentStage > 4.4f)
		{
			Destroy(this.gameObject);
		}
		else
		{
			//if (((currentStage * 5 < stageTracker.dialogueDataManager.shownDialogues.Count) && stageTracker.dialogueDataManager.allDialogues.Count >0 )|| currentStage != 0.2f)
			if (!(currentStage * 5 < stageTracker.dialogueDataManager.shownDialogues.Count) || currentStage != 0.2f)
			{
				switch (currentStage)
				{
					case 0.6f:
						if (stageTracker.dialogueDataManager.shownDialogues.Count <2)
						{
							SetDialogueObjects();
							dialogueController.StartSingleDialogue(dialogueEntries[0]);
							currentStage += .1f;
						}
						break;
					case 1.0f:
						if (stageTracker.dialogueDataManager.shownDialogues.Count < 3)
						{
							SetDialogueObjects();
							dialogueController.StartSingleDialogue(dialogueEntries[0]);
							currentStage += .1f;
						}
						break;
					case 1.2f:
						if (stageTracker.dialogueDataManager.shownDialogues.Count < 4 && GameManager.gameManager.endRegion)
						{
							//Snowy Lvl 1 (Lvl 6)
							EndRegionDialogue();
							currentStage += .1f;
						}
						break;
					case 1.6f:
						if (stageTracker.dialogueDataManager.shownDialogues.Count < 5)
						{
							SetDialogueObjects();
							dialogueController.StartSingleDialogue(dialogueEntries[0]);
							currentStage += .1f;
						}
						break;
					case 2.0f:
						if (stageTracker.dialogueDataManager.shownDialogues.Count < 6)
						{
							SetDialogueObjects();
							dialogueController.StartSingleDialogue(dialogueEntries[0]);
							currentStage += .1f;
						}
						break;
					case 2.2f:
						//Windy Lvl 1 (Lvl 11)
						if (stageTracker.dialogueDataManager.shownDialogues.Count < 7 && GameManager.gameManager.endRegion)
						{
							EndRegionDialogue();
							currentStage += .1f;
						}
						break;
					case 2.6f:
						if (stageTracker.dialogueDataManager.shownDialogues.Count < 8)
						{
							SetDialogueObjects();
							dialogueController.StartSingleDialogue(dialogueEntries[0]);
							currentStage += .1f;
						}
						break;
					case 3.0f:
						if (stageTracker.dialogueDataManager.shownDialogues.Count < 9)
						{
							SetDialogueObjects();
							dialogueController.StartSingleDialogue(dialogueEntries[0]);
							currentStage += .1f;
						}
						break;
					case 3.2f:
						if (stageTracker.dialogueDataManager.shownDialogues.Count < 10 && GameManager.gameManager.endRegion)
						{
							//Swampy Lvl 1 (Lvl 16)
							EndRegionDialogue();
							currentStage += .1f;
						}
						break;
					case 3.6f:
						if (stageTracker.dialogueDataManager.shownDialogues.Count < 11)
						{
							SetDialogueObjects();
							dialogueController.StartSingleDialogue(dialogueEntries[0]);
							currentStage += .1f;
						}
						break;
					case 4.0f:
						if (stageTracker.dialogueDataManager.shownDialogues.Count < 12)
						{
							SetDialogueObjects();
							dialogueController.StartSingleDialogue(dialogueEntries[0]);
							currentStage += .1f;
						}
						break;
					case 4.2f:
						if (stageTracker.dialogueDataManager.shownDialogues.Count < 13 && GameManager.gameManager.endRegion)
						{
							//Stormy Lvl 1 (Lvl 21)
							EndRegionDialogue();
							currentStage += .1f;
						}
						break;
					case 4.4f:
						//After Castle Lvl 1 (Lvl 2)
						StartRegionDialogue();
						currentStage += .1f;
						break;
					default:
						GameManager.gameManager.lvlParent.gameObject.SetActive(true);
						break;
				}

			}
		}
	}

	internal void StartFirstDialouge()
	{
		SetDialogueObjects();
		dialogueController.StartSingleDialogue(dialogueDataManager.allDialogues[0]);
		currentStage += .1f;
	}

	public DialogueDataManager LoadDialogueData()
	{
		if (File.Exists(Application.persistentDataPath + "/dialogueDataManager.dat"))
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + "/dialogueDataManager.dat", FileMode.Open);
			DialogueDataManagerData loadedData = (DialogueDataManagerData)bf.Deserialize(file);
			file.Close();

			// Set the loaded data to the DialogueDataManager
			string lastPlayedEntryName = loadedData.lastPlayedEntryName;
			bool found = false;
			dialogueDataManager.allDialogues.Clear();
			dialogueDataManager.shownDialogues.Clear();
			Debug.Log(loadedData.lastPlayedEntryName);
			Debug.Log(dialogueDataManager.shownDialogues);

			if (dialogueDataManager.shownDialogues != null)
			{
				SingleDialogueData lastItem;
				if (dialogueDataManager.shownDialogues.Count > 0)
				{
					lastItem = dialogueDataManager.shownDialogues[dialogueDataManager.shownDialogues.Count - 1];
				}
				else
				{
					lastItem = ScriptableObject.CreateInstance<SingleDialogueData>();
					lastItem.name = " ";
				}

				if (!lastItem.name.Equals(lastPlayedEntryName))
				{
					foreach (SingleDialogueData dialogueData in array.GetArray())
					{
						if (!found)
						{
							dialogueDataManager.shownDialogues.Add(dialogueData);
						}
						else
						{
							dialogueDataManager.allDialogues.Add(dialogueData);
						}

						if (dialogueData.name.Equals(lastPlayedEntryName))
						{
							found = true;
						}
					}
				}
			}
			else
			{
				foreach (SingleDialogueData dialogueData in array.GetArray())
				{
					if (dialogueData.name.Equals(lastPlayedEntryName))
					{
						found = true;
					}

					if (!found)
					{
						dialogueDataManager.allDialogues.Add(dialogueData);
					}
					else
					{
						dialogueDataManager.shownDialogues.Add(dialogueData);
					}
				}
			}
		}
		else
		{
			dialogueDataManager.allDialogues = array.GetArray().ToList<SingleDialogueData>();
			dialogueDataManager.shownDialogues.Clear();
		}
		return dialogueDataManager;
	}


	public void ResetProgress()
	{
		dialogueDataManager.allDialogues.Clear();
		dialogueDataManager.shownDialogues.Clear();

		if (File.Exists(Application.persistentDataPath + "/dialogueDataManager.dat"))
		{
			File.Delete(Application.persistentDataPath + "/dialogueDataManager.dat");
		}
#if UNITY_EDITOR

		EditorUtility.SetDirty(dialogueDataManager); // Mark the scriptable object as dirty.
#endif
	}
	public void SaveDialogueData(string name)
	{
		DialogueDataManagerData dataToSave = new DialogueDataManagerData();
		dataToSave.lastPlayedEntryName = name;
		// Serialize to a .dat file
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(Application.persistentDataPath + "/dialogueDataManager.dat");
		bf.Serialize(file, dataToSave);
		file.Close();
	}
	void StartRegionDialogue()
	{
		Debug.Log($"UPDATE:{dialogueEntries[0]}");
		//dialoguePrefab.SetActive(true);
		SetDialogueObjects();
		dialogueController.StartSingleDialogue(stageTracker.dialogueEntries[0]);
	}
	public void UpdateStage(int level)
	{
		currentStage = level / 5.0f;
	}
	public void EndRegionDialogue()
	{
		dialoguePrefab.SetActive(true);
		SetDialogueObjects();
		Debug.Log($"EndRegionDialogue:{dialogueEntries[0]}");

		dialogueController.StartSingleDialogue(dialogueEntries[0]);
		Debug.Log($"EndRegionDialogue: After {GameManager.gameManager.LoadPlayerData().level} Dialogue");


	}

	private void CheckTouch()
	{
		if (Input.touchCount > 0)
		{
			if (Input.touchCount == 1)
			{
				// Touch Started (Could be changed to touch ended)
				if (Input.GetTouch(0).phase == TouchPhase.Began)
				{
					stageTracker.dialogueController.DisplayNextSentenceSingleCharacter();
				}
			}
		}
	}

	static public void SetStageStage(float desStage)
	{
		stageTracker.currentStage = desStage;
	}

	private void SetDialogueObjects()
	{
		// if names of children in prefab are different, change name strings below
		GameManager.gameManager.lvlParent.gameObject.SetActive(false);
		GameObject dialogueObject = Instantiate(dialoguePrefab);
		dialogueController.DialoguePanel = dialogueObject;
		Image[] diaImgs = dialogueObject.GetComponentsInChildren<Image>();
		foreach (Image child in diaImgs)
		{
			if (child.name == "Character Image")
			{
				dialogueController.characterImage = child;
				break;
			}
		}
		TMP_Text[] diaText = dialogueObject.GetComponentsInChildren<TMP_Text>();
		foreach (TMP_Text child in diaText)
		{
			if (child.name == "Name")
			{
				dialogueController.characterName = child;
			}
			else if (child.name == "Dialogue Text")
			{
				dialogueController.dialogueText = child;
			}
		}
	}
}

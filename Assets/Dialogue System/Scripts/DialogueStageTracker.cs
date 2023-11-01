using Pon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class DialogueStageTracker : MonoBehaviour
{
	// All canvases used by this script to create the dialogue box should match the names in the CreateDialogueBox() function

	public static DialogueStageTracker stageTracker;
	DialogueController dialogueController;
	public SingleDialogueDataArray array;
	public DialogueDataManager dialogueDataManager;
	public GameObject dialoguePrefab;
	//GameObject currDiaBox;
	public static float currentStage = 0f;
	Image charImg;
	PonGameScript gameScript;
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
		//DontDestroyOnLoad(this.gameObject);

		dialogueController = this.GetComponent<DialogueController>();
		foreach (SingleDialogueData singleDialogue in dialogueDataManager.allDialogues)
		{
			dialogueEntries.Add(singleDialogue);
		}
	}
	private void Start()
	{
		string level = "Level 1";
		if (GameManager.gameManager.CurrentLevel != null)
		{
			level = GameManager.gameManager.CurrentLevel;
		}
		currentStage = Int32.Parse(Regex.Match(level, @"\d+").Value) / 5.0f;
	}
	public void RemoveEntry()
	{
		dialogueEntries.RemoveAt(0);
		dialogueDataManager.shownDialogues.Add(dialogueDataManager.allDialogues[0]);
		dialogueDataManager.allDialogues.RemoveAt(0);
	}
	private void Update()
	{
		stageTracker.isPlaying = dialogueController.isPlaying;
		if (stageTracker.isPlaying)
		{
			CheckTouch();
		}
		else
		{
			if (!(currentStage * 5 <= dialogueDataManager.shownDialogues.Count))
			{
				switch (currentStage)
				{
					case 0.2f:
						// start of dialogue
						Debug.Log("Level 1 Dialogue");
						//Display first dialogue line
						dialoguePrefab.SetActive(true);
						SetDialogueObjects();
						dialogueController.StartSingleDialogue(dialogueEntries[0]);
						//	CheckTouch();
						currentStage += .01f;

						break;
					case 0.6f:
						Debug.Log("Level 3 Dialogue");
						dialoguePrefab.SetActive(true);
						SetDialogueObjects();
						dialogueController.StartSingleDialogue(dialogueEntries[0]);
						currentStage += .1f;
						break;
					/*case 0.8f:
						// second line of dialogue
						dialoguePrefab.SetActive(true);
						SetDialogueObjects();
						dialogueController.StartSingleDialogue(dialogueEntries[0]);
						Debug.Log("Before Level 5 Dialogue");
						currentStage += .1f;
						break;*/
					case 1.0f:
						// second line of dialogue
						dialoguePrefab.SetActive(true);
						SetDialogueObjects();
						dialogueController.StartSingleDialogue(dialogueEntries[0]);
						Debug.Log("After Level 5 Dialogue");
						currentStage += .1f;
						break;
					case 1.2f:
						//Waiting for player to click through Dialogue
						dialoguePrefab.SetActive(true);
						SetDialogueObjects();
						dialogueController.StartSingleDialogue(dialogueEntries[0]);
						Debug.Log("After Level 6 Dialogue");
						currentStage += .1f;
						break;
					case 1.6f:
						// third line of dialogue
						dialoguePrefab.SetActive(true);
						SetDialogueObjects();
						dialogueController.StartSingleDialogue(dialogueEntries[0]);
						Debug.Log("After Level 8 Dialogue");
						currentStage += .1f;
						break;
					/*	case 1.8f:
							//Waiting for player to click through Dialogue
							dialoguePrefab.SetActive(true);
							SetDialogueObjects();
							dialogueController.StartSingleDialogue(dialogueEntries[0]);
							Debug.Log("Before Level 10 Dialogue");
							break;*/
					case 2.0f:
						dialoguePrefab.SetActive(true);
						SetDialogueObjects();
						// description-only line (which does not include a character sprite)
						dialogueController.StartSingleDialogue(dialogueEntries[0]);
						currentStage += .1f;

						break;
					case 2.2f:
						//Waiting for player to click through Dialogue
						dialoguePrefab.SetActive(true);
						SetDialogueObjects();
						dialogueController.StartSingleDialogue(dialogueEntries[0]);
						currentStage += .1f;
						break;
					case 2.6f:
						// fourth line of dialogue (with character sprite re-enabled)
						dialoguePrefab.SetActive(true);
						SetDialogueObjects();
						dialogueController.StartSingleDialogue(dialogueEntries[0]);
						currentStage += .1f;

						break;
					/*	case 2.8f:
							//Waiting for player to click through Dialogue
							dialoguePrefab.SetActive(true);
							SetDialogueObjects();
							dialogueController.StartSingleDialogue(dialogueEntries[0]);
							currentStage += .1f;

							break;*/
					case 3.0f:
						// load game (Loading function may need to be replaced to work with level objectives attached to level button, unless dialogue is activated by level-ending and not level button)
						dialoguePrefab.SetActive(true);
						SetDialogueObjects();
						dialogueController.StartSingleDialogue(dialogueEntries[0]);
						currentStage += .1f;

						break;
					case 3.2f:
						// load game (Loading function may need to be replaced to work with level objectives attached to level button, unless dialogue is activated by level-ending and not level button)
						dialoguePrefab.SetActive(true);
						SetDialogueObjects();
						dialogueController.StartSingleDialogue(dialogueEntries[0]);
						currentStage += .1f;

						break;
					case 3.6f:
						// load game (Loading function may need to be replaced to work with level objectives attached to level button, unless dialogue is activated by level-ending and not level button)
						dialoguePrefab.SetActive(true);
						SetDialogueObjects();
						dialogueController.StartSingleDialogue(dialogueEntries[0]);
						currentStage += .1f;

						break;
					/*case 3.8f:
						// load game (Loading function may need to be replaced to work with level objectives attached to level button, unless dialogue is activated by level-ending and not level button)
						dialoguePrefab.SetActive(true);
						SetDialogueObjects();
						dialogueController.StartSingleDialogue(dialogueEntries[0]);
						currentStage += .1f;

						break;*/
					case 4.0f:
						// load game (Loading function may need to be replaced to work with level objectives attached to level button, unless dialogue is activated by level-ending and not level button)
						dialoguePrefab.SetActive(true);
						SetDialogueObjects();
						dialogueController.StartSingleDialogue(dialogueEntries[0]);
						currentStage += .1f;

						break;
				}
			}
		}
	}
	public void UpdateStage(int level)
	{
		currentStage = level / 5.0f;
	}
	public void EndRegionDialogue()
	{
		dialoguePrefab.SetActive(true);
		SetDialogueObjects();
		dialogueController.StartSingleDialogue(dialogueEntries[0]);
		Debug.Log($"After {GameManager.gameManager.LoadPlayerData().level} Dialogue");

	}
	private void StartLoad(int destination)
	{
		if (destination == 0)
		{
			StartCoroutine(AsyncLoadIntoMap());
		}
		else if (destination == 1)
		{
			StartCoroutine(AsyncLoadIntoGame());
		}
		else
		{
			Debug.Log("Destination out of range");
		}

	}

	private IEnumerator AsyncLoadIntoMap()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Map_t");

		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}

	private IEnumerator AsyncLoadIntoGame()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Game");

		while (!asyncLoad.isDone)
		{
			yield return null;
		}
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
					dialogueController.DisplayNextSentenceSingleCharacter();
				}
			}
		}
	}

	private void SetActiveDialogueBox(bool desActive)
	{
		// sets all children of dialogue box to be active or inactive
		for (int i = 0; i < dialoguePrefab.transform.childCount; i++)
		{
			dialoguePrefab.transform.GetChild(i).gameObject.SetActive(desActive);
		}
	}


	static public void SetStageStage(float desStage)
	{
		currentStage = desStage;
	}

	private void SetDialogueIndex(int desIndex)
	{
		dialogueController.SetDialogueIndex(desIndex);
	}

	private void SetDialogueObjects()
	{
		// if names of children in prefab are different, change name strings below
		dialogueController.DialoguePanel = dialoguePrefab;
		Image[] diaImgs = dialoguePrefab.GetComponentsInChildren<Image>();
		foreach (Image child in diaImgs)
		{
			if (child.name == "Character Image")
			{
				dialogueController.characterImage = child;
				charImg = child;
				break;
			}
		}
		TMP_Text[] diaText = dialoguePrefab.GetComponentsInChildren<TMP_Text>();
		foreach (TMP_Text child in diaText)
		{
			if (child.name == "Name")
			{
				dialogueController.characterName = child;
			}
			else if (child.name == "Dialogue")
			{
				dialogueController.dialogueText = child;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
	public GameObject DialoguePanel;

	public TMP_Text characterName;
	public TMP_Text dialogueText;
	public Image characterImage;

	private List<string> sentences = new List<string>();
	private List<string> names = new List<string>();
	private List<Sprite> sprites = new List<Sprite>();
	private bool isMulti = false;
	private int currentIndex;
	public bool isPlaying = false;
	private SingleDialogueData activeSingleDialogue;

	private Animator shake;
	void Start()
	{
		sentences = new List<string>();
		names = new List<string>();
		sprites = new List<Sprite>();

	}

	public void StartSingleDialogue(SingleDialogueData dialogueData)
	{
		isMulti = false;
		isPlaying = true;
		if(activeSingleDialogue == null)
		{
			activeSingleDialogue = dialogueData;
		}
		if (dialogueData == null)
		{
			Debug.LogWarning("Dialogue Data is null. Cannot start dialogue.");
			return;
		}

		if (dialogueData.dialogueEntries.Count == 0)
		{
			Debug.LogWarning("Dialogue Data does not contain any dialogue entries.");
			return;
		}


		sentences.Clear();
		names.Clear();
		sprites.Clear();


		foreach (SingleDialogueData.DialogueEntry entry in dialogueData.dialogueEntries)
		{
			characterName.text = entry.characterName;
			characterImage.sprite = entry.characterSprite;
			sentences.Add(entry.sentence);
			names.Add(entry.characterName);
			sprites.Add(entry.characterSprite);

		}
		currentIndex = 0;
		DisplayNextSentenceSingleCharacter();
	}


	public void StartMultiDialogue(MultiDialogueData dialogueData)
	{
		isMulti = true;
		if (dialogueData == null)
		{
			Debug.LogWarning("Dialogue Data is null. Cannot start dialogue.");
			return;
		}

		if (dialogueData.dialogueEntries.Count == 0)
		{
			Debug.LogWarning("Dialogue Data does not contain any dialogue entries.");
			return;
		}


		sentences.Clear();
		names.Clear();
		sprites.Clear();

		foreach (MultiDialogueData.DialogueEntry entry in dialogueData.dialogueEntries)
		{
			characterName.text = entry.characterName;
			characterImage.sprite = entry.characterSprite;
			sentences.Add(entry.sentence);
			names.Add(entry.characterName);
			sprites.Add(entry.characterSprite);

		}
		currentIndex = 0;
		DisplayNextSentenceMultiCharacters();
	}

	public void DisplayNextSentenceSingleCharacter()
	{
		
		if (sentences.Count == 0)
		{
			if (activeSingleDialogue != null)
			{
				StartSingleDialogue(activeSingleDialogue);
			}
			else
			{
				EndDialogue();
				return;
			}
		}

		if (currentIndex < sentences.Count)
		{
			DialogueStageTracker stageTracker = GetComponent<DialogueStageTracker>();
			shake = characterImage.GetComponent<Animator>();
			DisplayCurrentEntry();
			if (stageTracker.currentStage - Math.Truncate(stageTracker.currentStage) == .6f && currentIndex ==1)
			{
				//shake.Play("Character shake");
				shake.SetBool("Shake", true);
			}
			else
			{
				shake.SetBool("Shake", false);

			}
			Debug.Log(stageTracker.currentStage - Math.Truncate(stageTracker.currentStage));
		}
		else
		{
			EndDialogue();
			return;
		}
		currentIndex++;
	}
	public void DisplayNextSentenceMultiCharacters()
	{
		if (sentences.Count == 0)
		{
			EndDialogue();
			return;
		}

		if (currentIndex < sentences.Count)
		{
			DisplayCurrentEntry();
		}
		else
		{
			EndDialogue();
			return;
		}
		currentIndex++;
	}
	private void DisplayCurrentEntry()
	{
		characterName.text = names[currentIndex];
		dialogueText.text = sentences[currentIndex];
		characterImage.sprite = sprites[currentIndex];
	}

	void EndDialogue()
	{
		Debug.Log("I'm in EndDialogue");
		if (!isMulti)
		{
			activeSingleDialogue = null;
			isPlaying = false;
			Debug.Log(GetComponent<DialogueStageTracker>().dialogueEntries[0].name);
			string name = GetComponent<DialogueStageTracker>().dialogueEntries[0].name;
			DialogueStageTracker.stageTracker.SaveDialogueData(name);
			DialogueStageTracker.stageTracker.RemoveEntry();

			if (GameManager.gameManager.endRegion)
			{
				GameManager.gameManager.endRegion = false;
			}
		}
		Destroy(DialoguePanel);
	}

	public void SetDialogueIndex(int desIndex)
	{
		currentIndex = desIndex;
	}
}

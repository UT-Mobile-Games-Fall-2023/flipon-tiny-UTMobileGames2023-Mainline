using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DialogueManager : MonoBehaviour
{
	public static DialogueManager dialogueManager;
	public SingleDialogueDataArray array;
    public List<SingleDialogueData> dialogueEntries = new List<SingleDialogueData>();
    private DialogueController dialogueController;
	public bool isPlaying = false;
	private void Awake()
	{
		if (dialogueManager == null)
		{
			dialogueManager = this.GetComponent<DialogueManager>();
		}
		else
		{
			Destroy(this.gameObject);
		}
		//DontDestroyOnLoad(this.gameObject);
        dialogueController = GetComponent<DialogueController>();

        foreach (SingleDialogueData singleDialogue in array.GetArray())
		{
			dialogueEntries.Add(singleDialogue);
		}


	}
	private void Update()
	{
		dialogueManager.isPlaying = dialogueController.isPlaying;
		if (dialogueManager.isPlaying)
		{
			CheckTouch();
		}

	}
	public void RemoveEntry()
	{
		dialogueEntries.RemoveAt(0);
		array.DialogueEntries.RemoveAt(0);
	}
	private void CheckTouch()
	{
		if (Input.touchCount > 0)
		{
			if (Input.touchCount == 1)
			{
				// Touch Started
				if (Input.GetTouch(0).phase == TouchPhase.Began)
				{
					dialogueController.DisplayNextSentenceSingleCharacter();
				}
			}
		}
	}
	public void StartDialogue()
	{
		
		//Start a dialogue for the specific level
		dialogueController.StartSingleDialogue(dialogueEntries[0]);
		dialogueController.DialoguePanel.SetActive(true);
	}
}

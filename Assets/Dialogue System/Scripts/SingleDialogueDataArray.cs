using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SingleDialogueDataArray : MonoBehaviour
{
	public List<SingleDialogueData> DialogueEntries = new List<SingleDialogueData>();
	public SingleDialogueData[] GetArray()
	{
		return DialogueEntries.ToArray();
	}
}

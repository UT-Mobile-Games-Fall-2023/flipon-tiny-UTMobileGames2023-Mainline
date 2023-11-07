using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleDialogueDataArray : MonoBehaviour
{
    [SerializeField] private List<SingleDialogueData> dialogueEntries = new List<SingleDialogueData>();
    public ReadOnlyCollection<SingleDialogueData> DialogueEntriesReadOnly { get; private set; }

    private void Awake()
    {
        // Initialize the read-only collection
        DialogueEntriesReadOnly = new ReadOnlyCollection<SingleDialogueData>(dialogueEntries);
    }

    public SingleDialogueData[] GetArray()
    {
        return dialogueEntries.ToArray();
    }
}

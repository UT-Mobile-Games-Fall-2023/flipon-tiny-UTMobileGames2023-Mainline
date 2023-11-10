using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


[CreateAssetMenu(fileName = "DialogueManagerData", menuName = "Dialogue System/Dialogue Manager Data")]
public class DialogueDataManager : ScriptableObject
{   
    public List<SingleDialogueData> allDialogues = new List<SingleDialogueData>();
    public List<SingleDialogueData> shownDialogues = new List<SingleDialogueData>();
   
    public void MarkDialogueAsShown(SingleDialogueData dialogue)
    {
        if (!IsDialogueAlreadyShown(dialogue))
        {
            allDialogues.Add(dialogue);
#if UNITY_EDITOR
            EditorUtility.SetDirty(this); // Mark the scriptable object as dirty.
#endif
        }
		else
		{
            shownDialogues.Add(dialogue);
        }
    }


    public bool IsDialogueAlreadyShown(SingleDialogueData dialogue)
    {
        return shownDialogues.Contains(dialogue);
    }

    public void ResetShownDialogues()
    {
        shownDialogues.Clear();
        allDialogues.Clear();
#if UNITY_EDITOR
        EditorUtility.SetDirty(this); // Mark the scriptable object as dirty.
#endif
    }
}

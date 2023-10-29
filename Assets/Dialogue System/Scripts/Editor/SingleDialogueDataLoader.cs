using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

public class SingleDialogueDataLoader : EditorWindow
{
	[MenuItem("Custom Tools/Load Single Dialogue Data")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(SingleDialogueDataLoader));
	}

	private void OnGUI()
	{
		if (GUILayout.Button("Load Single Dialogue Data"))
		{
			LoadSingleDialogueData();
		}
	}

    private void LoadSingleDialogueData()
    {
        string reEngagementFolder = "Assets/Re-engagement"; // Adjust the folder path as needed.
        GameObject holder = Selection.activeGameObject;

        if (holder == null)
        {
            Debug.LogError("SingleDialogueDataArray was not found.");
            return;
        }

        if (holder.GetComponent<SingleDialogueDataArray>() == null)
        {
            holder.AddComponent<SingleDialogueDataArray>();
        }

        SingleDialogueDataArray array = holder.GetComponent<SingleDialogueDataArray>();

        SingleDialogueData[] dialogueEntries = FindSingleDialogueDataInFolder(reEngagementFolder);

        if (dialogueEntries.Length > 0)
        {
            // Sort the dialogueEntries array based on the folder names.
            dialogueEntries = SortDialogueEntries(dialogueEntries);

            array.DialogueEntries = dialogueEntries.ToList();
            EditorUtility.SetDirty(array);
            Debug.Log("Single Dialogue Data loaded into GameManager.");
        }
        else
        {
            Debug.LogWarning("No Single Dialogue Data found in the specified folder.");
        }
    }

    private SingleDialogueData[] SortDialogueEntries(SingleDialogueData[] entries)
    {
        return entries
            .OrderBy(entry => GetParentFolderName(entry))
            .ToArray();
    }

    private string GetParentFolderName(SingleDialogueData entry)
    {
        string path = AssetDatabase.GetAssetPath(entry);
        string parentFolder = path.Substring(0, path.LastIndexOf("/"));
        string[] folders = parentFolder.Split('/');
        return folders[folders.Length - 1];
    }


    private SingleDialogueData[] FindSingleDialogueDataInFolder(string folderPath)
	{
		string[] guids = AssetDatabase.FindAssets("t:SingleDialogueData", new[] { folderPath });
		return guids
			.Select(guid => AssetDatabase.LoadAssetAtPath<SingleDialogueData>(AssetDatabase.GUIDToAssetPath(guid)))
			.ToArray();
	}
}

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(DialogueDataManager))]
public class DialogueManagerDataEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DialogueDataManager dialogueManagerData = (DialogueDataManager)target;

		// Display the default fields
		base.OnInspectorGUI();

		GUILayout.Space(10);

		GUILayout.Label("Add Dialogues from Folder");
		GUILayout.Space(10);

		// Create a drag and drop area for folders with a border.
		GUILayout.Label("Drag and drop a folder containing SingleDialogueData assets here:");
		EditorGUILayout.BeginVertical("Box");
		Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
		EditorGUILayout.EndVertical();

		Event evt = Event.current;

		switch (evt.type)
		{
			case EventType.DragUpdated:
			case EventType.DragPerform:
				if (!dropArea.Contains(evt.mousePosition))
				{
					break;
				}

				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

				if (evt.type == EventType.DragPerform)
				{
					DragAndDrop.AcceptDrag();

					foreach (Object draggedObject in DragAndDrop.objectReferences)
					{
						if (draggedObject is DefaultAsset)
						{
							string folderPath = AssetDatabase.GetAssetPath(draggedObject);
							AddDialoguesFromFolder(dialogueManagerData, folderPath);
							EditorUtility.SetDirty(dialogueManagerData);
						}
					}
				}
				break;
		}
	}

	private void AddDialoguesFromFolder(DialogueDataManager dialogueManagerData, string folderPath)
	{
		string[] guids = AssetDatabase.FindAssets("t:SingleDialogueData", new string[] { folderPath });

		foreach (string guid in guids)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(guid);
			SingleDialogueData dialogueData = AssetDatabase.LoadAssetAtPath<SingleDialogueData>(assetPath);

			if (dialogueData.dialogueEntries.Count > 0)
			{


				if (!dialogueManagerData.IsDialogueAlreadyShown(dialogueData))
				{
					dialogueManagerData.MarkDialogueAsShown(dialogueData);
				}
			}

		}
	}
}

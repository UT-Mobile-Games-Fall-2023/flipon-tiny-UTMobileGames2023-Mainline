using UnityEditor;
using UnityEngine;
using System.IO;

[CustomEditor(typeof(SingleDialogueData))]
public class SingleDialogueDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SingleDialogueData dialogueData = (SingleDialogueData)target;

        // Display the default fields
        base.OnInspectorGUI();

        GUILayout.Space(10);

        GUILayout.Label("CSV Data");
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();

        // Allow specifying the sprite folder.
        SingleDialogueData.spriteFolder = EditorGUILayout.TextField("Sprite Folder", SingleDialogueData.spriteFolder);
        GUILayout.Space(10);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();

        // Add a drop-down menu for selecting and set the file delimiter.
        dialogueData.csvDelimiter = (SingleDialogueData.CSVDelimiter)EditorGUILayout.EnumPopup("CSV Delimiter", dialogueData.csvDelimiter);
        GUILayout.Space(10);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        // Display a button to automatically find and set the CSV File
        if (GUILayout.Button("Auto Find CSV File"))
        {
            string filePath = AssetDatabase.GetAssetPath(dialogueData);
            string folderPath = Path.GetDirectoryName(Path.GetDirectoryName(filePath));
            string csvFileName = dialogueData.name + ".txt";
            string csvFilePath = Path.Combine(folderPath, "Text files", csvFileName);

            TextAsset csvFile = AssetDatabase.LoadAssetAtPath<TextAsset>(csvFilePath);

            if (csvFile != null)
            {
                dialogueData.csvFile = csvFile;
                Debug.Log("CSV File found and set: " + csvFileName);
            }
            else
            {
                Debug.LogWarning("CSV File not found for: " + csvFileName);
            }
        }

        GUILayout.Space(10);

        // Allow specifying the CSV File manually
        dialogueData.csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", dialogueData.csvFile, typeof(TextAsset), false);

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        // Button to populate sentences
        if (GUILayout.Button("Populate Sentences"))
        {
            dialogueData.PopulateSentencesFromCSV();
            EditorUtility.SetDirty(dialogueData);
        }

        GUILayout.EndHorizontal();
    }
}

using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[System.Serializable]
[CreateAssetMenu(fileName = "New Single-Character Dialogue Data", menuName = "Dialogue System/Dialogue Data for Single Character")]
public class SingleDialogueData : ScriptableObject
{
	[System.Serializable]
	public class DialogueEntry
	{
		[TextArea(3, 10)]
		public string sentence;
		public Sprite characterSprite;
	}

	public string characterName; // Fixed character name
	public List<DialogueEntry> dialogueEntries = new List<DialogueEntry>();

	[HideInInspector] public CSVDelimiter csvDelimiter = CSVDelimiter.Tab; // Set to tab as default

	public enum CSVDelimiter
	{
		Tab,
		Space,
		Comma,
		Period,
		Semicolon
		// Add more options as needed.
	}

	[SerializeField]
	[HideInInspector]
	public TextAsset csvFile;

	[HideInInspector] public static string spriteFolder { get; set; }
	public Dictionary<string, Sprite> characterToSpriteMap = new Dictionary<string, Sprite>();

	SingleDialogueData()
	{
		spriteFolder = "Assets/Resources/Characters";
	}

	public char GetDelimiterCharacter()
	{
		// Get the selected delimiter character based on the enum value.
		switch (csvDelimiter)
		{
			case CSVDelimiter.Tab:
				return '\t';
			case CSVDelimiter.Space:
				return ' ';
			case CSVDelimiter.Comma:
				return ',';
			case CSVDelimiter.Period:
				return '.';
			case CSVDelimiter.Semicolon:
				return ';';
			// Add more cases as needed.
			default:
				return ',';
		}
	}

	public Sprite[] LoadSpritesFromFolder()
	{
		//Sprite[] sprites = (Sprite[])Resources.LoadAll(spriteFolder);
		List<Sprite> sprites = new List<Sprite>();
		string[] assetPaths = AssetDatabase.FindAssets("t:Sprite", new string[] { spriteFolder });
		foreach (string assetPath in assetPaths)
		{
			Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(assetPath));
			sprites.Add(sprite);
		}
		return sprites.ToArray();
		//return sprites;
	}
	public void PopulateCharacterToSpriteMap()
	{
		Sprite[] sprites = LoadSpritesFromFolder();
		characterToSpriteMap.Clear();

		foreach (Sprite sprite in sprites)
		{
			characterToSpriteMap[sprite.name] = sprite;
			Debug.Log($"The sprite name is: {sprite.name}");
		}
	}
	public void PopulateSentencesFromCSV()
	{
		PopulateCharacterToSpriteMap(); // Load sprites before populating sentences.
		dialogueEntries = ReadCSV(csvFile, GetDelimiterCharacter()); // Pass the delimiter.
	}

	public List<DialogueEntry> ReadCSV(TextAsset csv, char delimiter)
	{
		List<DialogueEntry> entries = new List<DialogueEntry>();

		if (csv != null)
		{
			string defaultCharacterName = null;
			string[] lines = csv.text.Split('\n');
			for (int i = 1; i < lines.Length; i++) // Start from index 1 to skip the header
			{
				string line = lines[i];
				if (line == null || string.IsNullOrWhiteSpace(line) || line.StartsWith("Notes"))
				{
					continue;
				}
				string[] fields = line.Split(delimiter);
				if (fields.Length >= 2)
				{
					characterName = fields[0];
					if (characterName != null && !string.IsNullOrWhiteSpace(characterName))
					{
						defaultCharacterName = characterName;
					}
					else
					{
						characterName = defaultCharacterName;
					}
					Sprite characterSprite = GetCharacterSprite(fields[1]);
					Debug.Log(characterSprite + " " + fields[1]);
					if (characterSprite == null)
					{
						if (fields[1].Contains("_"))
						{
							Debug.LogWarning("Sprite not found for character: " + fields[1]);

							string[] parts = fields[1].Split('_');
							if (parts.Length > 1)
							{
								characterSprite = GetCharacterSprite(parts[0]); // Use the part before "_"
							}
						}
					}
					DialogueEntry entry = new DialogueEntry
					{
						characterSprite = characterSprite,
						sentence = fields[2],
					};
					entries.Add(entry);
				}
				else
				{
					characterName = fields[0];
					if (characterName != null && !string.IsNullOrWhiteSpace(characterName))
					{
						defaultCharacterName = characterName;
					}
					else
					{
						characterName = defaultCharacterName;
					}
					Sprite characterSprite = GetCharacterSprite(characterName);
					DialogueEntry entry = new DialogueEntry
					{
						characterSprite = characterSprite,
						sentence = fields[2].Replace("\"", ""),
					};
					entries.Add(entry);
				}
			}
		}


		return entries;
	}

	public Sprite GetCharacterSprite(string characterName)
	{
		//Debug.Log(characterName);
		//Debug.Log(characterToSpriteMap.ToString());

		if (characterToSpriteMap.ContainsKey(characterName))
		{
			return characterToSpriteMap[characterName];
		}
		return null;
	}
}

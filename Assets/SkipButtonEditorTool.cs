using Pon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkipButtonEditorTool : MonoBehaviour
{
	public Button SkipGameButton;
	public PonGameScript ponGameScript;
	// Start is called before the first frame update
	void Awake()
	{
		if (SkipGameButton != null)
		{
			SkipGameButton.onClick.AddListener(ponGameScript.SkipGame);
		}
	}
	public void Clicked()
	{
		ponGameScript.SkipGame();

	}
}

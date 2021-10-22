using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMessage : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static PauseMessage _S;
	public static PauseMessage S { get { return _S; } set { _S = value; } }

	public Text 		message; // named "Text" in the Hierarchy/Inspector
	public GameObject	cursorGO;

	[Header ("Set Dynamically")]
	public bool 		dialogueFinished;

	void Awake() {
		S = this;
	}

	public void DisplayText(string text, bool upperLeftAlignment = false) {
		gameObject.SetActive (true);

		StopCoroutine ("DisplayTextCo");
		StartCoroutine(DisplayTextCo(text, upperLeftAlignment));
	}
	IEnumerator DisplayTextCo(string text, bool upperLeftAlignment) {
		// Deactivate Cursor
		cursorGO.SetActive (false);

		// Reset Text Strings
		string dialogueSentences = null;

		// Set Text Alignment
		if (upperLeftAlignment) {
			message.alignment = TextAnchor.UpperLeft;
		} else {
			message.alignment = TextAnchor.MiddleCenter;
		}

		dialogueFinished = false;

		// Split text argument w/ blank space
		string[] dialogueWords = text.Split (' ');
		// Display text one word at a time
		for (int i = 0; i < dialogueWords.Length; i++) {
			dialogueSentences += dialogueWords [i] + " ";
			message.text = dialogueSentences;
			yield return new WaitForSeconds(OptionsScreen.S.textSpeed);
		}
		// Activate cursor
		cursorGO.SetActive(true);

		// Dialogue Finished
		dialogueFinished = true;
	}

	// Set Text Instantly 
	// - No delay/stagger between displaying each word)
	public void SetText(string text, bool upperLeftAlignment = false) {
		StopCoroutine("DisplayTextCo");

		// Set Text Alignment
		if (upperLeftAlignment) {
			message.alignment = TextAnchor.UpperLeft;
		} else {
			message.alignment = TextAnchor.MiddleCenter;
		}

		message.text = text;
	}
}
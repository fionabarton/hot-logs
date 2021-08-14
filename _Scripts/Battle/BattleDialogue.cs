using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogue : MonoBehaviour{
	[Header("Set in Inspector")]
	// Text
	public Text			displayMessageTextBottom;
	public Text			displayMessageTextTop;

	// Cursors
	public GameObject	dialogueCursor;

	[Header("Set Dynamically")]
	// Singleton
	private static BattleDialogue _S;
	public static BattleDialogue S { get { return _S; } set { _S = value; } }

	// Dialogue
	private string[]	dialogueWords;
	public string		dialogueSentences;
	public bool			dialogueFinished = true;
	public int			dialogueNdx = 99;
	public List<string> message;

	void Awake() {
		// Singleton
		S = this;
	}

	public void Loop() {
		if (dialogueNdx <= 0) {
			if (Input.GetButtonDown("SNES A Button") || Input.GetButtonDown("SNES B Button")) {
				dialogueSentences = null;
				dialogueFinished = true;
				dialogueNdx = 0;
			}
		} else if (dialogueNdx > 0) { // For Multiple Lines
			if (Input.GetButtonDown("SNES A Button")) {
				if (message.Count > 0) {
					// Reset Text & Cursor
					ClearForNextLine();

					List<string> tMessage;

					tMessage = message;

					tMessage.RemoveAt(0);

					// Call DisplayText() with one less line of "messages"
					DisplayText(tMessage);
				}
			}
		}
	}

	///////////////////////////////// DIALOGUE/TEXT MANAGER /////////////////////////////////
	//public void DisplayText(string text){
	//	StopCoroutine ("DisplayTextCo");
	//	StartCoroutine ("DisplayTextCo", text);
	//}
	//IEnumerator DisplayTextCo(string text){
	//	// Deactivate Cursor
	//	dialogueCursor.SetActive (false);

	//	// Reset Text Strings
	//	dialogueSentences = null;
	//	dialogueFinished = false;

	//	// Split text argument w/ blank space
	//	dialogueWords = text.Split (' ');
	//	// Display text one word at a time
	//	for (int i = 0; i < dialogueWords.Length; i++) {
	//		dialogueSentences += dialogueWords [i] + " ";
	//		displayMessageText.text = dialogueSentences;
	//		yield return new WaitForSeconds (0.1f);
	//	}
	//	// Activate cursor
	//	dialogueCursor.SetActive (true);
	//	// Dialogue Finished
	//	dialogueFinished = true;
	//}
	// Display a SINGLE string
	public void DisplayText(string messageToDisplay) {
		// Reset Dialogue
		dialogueSentences = null;
		dialogueFinished = true;
		dialogueNdx = 0;

		//DeactivateTextBox();
		List<string> tMessage = new List<string> { messageToDisplay };
		DisplayText(tMessage);
	}

	// Display a LIST of strings
	public void DisplayText(List<string> text) {
		StartCoroutine(DisplayTextCo(text));
	}
	IEnumerator DisplayTextCo(List<string> text) {
		// Deactivate Cursor
		dialogueCursor.SetActive(false);

		// Get amount of Dialogue Strings
		dialogueNdx = text.Count;

		if (text.Count > 0) {
			dialogueFinished = false;

			// Split text argument w/ blank space
			dialogueWords = text[0].Split(' ');
			// Display text one word at a time
			for (int i = 0; i < dialogueWords.Length; i++) {
				// Audio: Dialogue
				AudioManager.S.sfxCS[0].Play();

				dialogueSentences += dialogueWords[i] + " ";
				displayMessageTextBottom.text = dialogueSentences;
				yield return new WaitForSeconds(0.1f);
			}

			// Activate cursor
			dialogueCursor.SetActive(true);

			dialogueNdx -= 1;

			dialogueFinished = true;
		}
	}

	public void ClearForNextLine() {
		StopAllCoroutines();

		// Reset Dialogue
		dialogueSentences = null;
	}
}

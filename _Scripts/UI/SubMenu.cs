using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubMenu : MonoBehaviour {
	[Header ("Set in Inspector")]
	// Cursor Position
	public RectTransform		cursorRT;

	// Sub Menu Button
	public List <GameObject>	subMenuButtonGO;
	public List <Button>		subMenuButtonCS;

	// Sub Menu Text
	public List <Text>			subMenuText;

	// Sub Menu Frame
	public RectTransform		subMenuFrameRT;

	[Header("Set Dynamically")]
	// Singleton
	private static SubMenu _S;
	public static SubMenu S { get { return _S; } set { _S = value; } }

	// Frame & Cursor Position
	private Vector2 			tPos;

	public bool 				canUpdate;

	void Awake() {
		// Singleton
		S = this;
	}

	void OnEnable () {
		canUpdate = true;
	}

	// Cursor Position set to Selected Button
	public void Loop () {
		// Reset canUpdate
		if (Input.GetAxisRaw ("Horizontal") != 0f || Input.GetAxisRaw ("Vertical") != 0f) { 
			canUpdate = true;
		}

		if (canUpdate) {
			// Set Cursor Position to Selected Button
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == subMenuButtonGO[0]) {
				tPos.x = subMenuButtonGO[0].GetComponent<RectTransform> ().anchoredPosition.x;
				tPos.y = subMenuButtonGO[0].GetComponent<RectTransform> ().anchoredPosition.y;
			} else if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == subMenuButtonGO[1]) {
				tPos.x = subMenuButtonGO[1].GetComponent<RectTransform> ().anchoredPosition.x;
				tPos.y = subMenuButtonGO[1].GetComponent<RectTransform> ().anchoredPosition.y;
			}else if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == subMenuButtonGO[2]) {
				tPos.x = subMenuButtonGO[2].GetComponent<RectTransform> ().anchoredPosition.x;
				tPos.y = subMenuButtonGO[2].GetComponent<RectTransform> ().anchoredPosition.y;
			}else if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == subMenuButtonGO[3]) {
				tPos.x = subMenuButtonGO[3].GetComponent<RectTransform> ().anchoredPosition.x;
				tPos.y = subMenuButtonGO[3].GetComponent<RectTransform> ().anchoredPosition.y;
			}

			cursorRT.anchoredPosition = new Vector2 ((tPos.x + 150), (tPos.y));

			canUpdate = false;
		}
	}

	public void SetText(string option1 = "Yes", string option2 = "No", string option3 = "3 sides?", string option4 = "No, 4!", int optionAmount = 2){ 

		// Set Selected GameObject
		switch (option1) {
		case "Yes":
			Utilities.S.SetSelectedGO(subMenuButtonGO[1]);
			break;
		default:
			Utilities.S.SetSelectedGO(subMenuButtonGO[0]);
			break;
		}
			
		// Get Sprite Frame Position
		tPos = subMenuFrameRT.anchoredPosition;

		// Set Text
		subMenuText[0].text = option1;
		subMenuText[1].text = option2;
		subMenuText[2].text = option3;
		subMenuText[3].text = option4;

		switch (optionAmount) {
		case 2:               
			SetTextHelper (false, false, false, false, 150, 0);
			break;
		case 3:
			SetTextHelper (true, false, true, false, 200, -25);
			break;
		case 4:
			SetTextHelper (true, true, true, true, 250, -50);
			break;
		}

		// Set Sprite Frame Position
		subMenuFrameRT.anchoredPosition = tPos;
	}

	void SetTextHelper(bool option3Active, bool option4Active, bool is3Interactable, bool is4Interactable, int frameSizeY, int framePosY){
		// Activate Text
		subMenuText [2].gameObject.SetActive (option3Active);
		subMenuText [3].gameObject.SetActive (option4Active);

		// Interactable
		subMenuButtonCS[2].interactable = is3Interactable;
		subMenuButtonCS[3].interactable = is4Interactable;

		// Set Frame Height
		subMenuFrameRT.sizeDelta = new Vector2(400, frameSizeY);

		// Set Frame Position
		tPos.y = framePosY;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A set of general functions that are HOPEFULLY useful in a multitude of projects
/// </summary>
public class Utilities : MonoBehaviour
{
    [Header("Set Dynamically")]
    // Singleton
    private static Utilities _S;
    public static Utilities S { get { return _S; } set { _S = value; } }

    void Awake() {
        S = this;
    }

    ////////////////////////////////////////////////////////////////////////////////
    // Set GameObject Position
    public void SetPosition(GameObject tGO, float x, float y) {
		Vector3 tPos = tGO.transform.position;
		tPos.x = x;
		tPos.y = y;
		tGO.transform.position = tPos;
	}
	// Set GameObject LOCAL Position
	public void SetLocalPosition(GameObject tGO, float x, float y) {
		Vector3 tPos = tGO.transform.localPosition;
		tPos.x = x;
		tPos.y = y;
		tGO.transform.localPosition = tPos;
	}
	////////////////////////////////////////////////////////////////////////////////
	// Set GameObject Scale
	public void SetScale(GameObject tGO, float x, float y) {
		Vector3 tScale = tGO.transform.localScale;
		tScale.x = x;
		tScale.y = y;
		tGO.transform.localScale = tScale;
	}

	// Flip GameObject Scale/Sprite
	public void Flip(GameObject go, ref bool facingRight) {
		facingRight = !facingRight;
		SetScale(go, go.transform.localScale.x * -1, go.transform.localScale.y);
	}
	////////////////////////////////////////////////////////////////////////////////
	// Set Selected GameObject
	public void SetSelectedGO(GameObject tGO) {
		UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
		UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(tGO);
	}
	////////////////////////////////////////////////////////////////////////////////
	// Gradually Move GameObject's X Position  
	public void MoveXPosition(GameObject tGO, float speed) {
		Vector3 tPos = tGO.transform.position;
		tPos.x += speed * Time.fixedDeltaTime;
		tGO.transform.position = tPos;
	}
	// Gradually Move GameObject's Y Position  
	public void MoveYPosition(GameObject tGO, float speed) {
		Vector3 tPos = tGO.transform.position;
		tPos.y += speed * Time.fixedDeltaTime;
		tGO.transform.position = tPos;
	}
	////////////////////////////////////////////////////////////////////////////////
	// Get Percentage. Returns a float from 0.00...1 to 1.0
	public float GetPercentage(float value, float maxValue) {
		return value / maxValue;
	}
	////////////////////////////////////////////////////////////////////////////////
	// Map a value within a set of numbers to a different set of numbers
	public float Map(float OldMin, float OldMax, float NewMin, float NewMax, float valueToMap) {
		float OldRange = (OldMax - OldMin);
		float NewRange = (NewMax - NewMin);
		float NewValue = (((valueToMap - OldMin) * NewRange) / OldRange) + NewMin;

		return (NewValue);
	}
	////////////////////////////////////////////////////////////////////////////////
	// Calculate Average/Mean of two integers
	public int CalculateAverage(int a, int b) {
		// Account for DivideByZeroException
		if (b == 0) {
			return a;
		} else {
			int c = a / b;
			return c;
		}
	}
	////////////////////////////////////////////////////////////////////////////////
	// Make a list of buttons interactable
	public void ButtonsInteractable(List<Button> buttons, bool isInteractable) {
		for (int i = 0; i <= buttons.Count - 1; i++) {
			buttons[i].interactable = isInteractable;
		}
	}

	// Remove listeners from a list of buttons 
	public void RemoveListeners(List<Button> buttons) {
		for (int i = 0; i <= buttons.Count - 1; i++) {
			buttons[i].onClick.RemoveAllListeners();
		}
	}

	// Set cursor position to currently selected button/gameObject
	public void PositionCursor(GameObject selectedGO, 
		int xAxisDistanceFromCenter, int yAxisDistanceFromCenter = 0, int directionToFace = 2, int cursorNdx = 0) {
		// Get position
		float tPosX = selectedGO.GetComponent<RectTransform>().anchoredPosition.x;
		float tPosY = selectedGO.GetComponent<RectTransform>().anchoredPosition.y;
		float tParentX = selectedGO.transform.parent.GetComponent<RectTransform>().anchoredPosition.x;
		float tParentY = selectedGO.transform.parent.GetComponent<RectTransform>().anchoredPosition.y;

		// Set position
		ScreenCursor.S.rectTrans[cursorNdx].anchoredPosition = new Vector2(
			(tPosX + tParentX + xAxisDistanceFromCenter), 
			(tPosY + tParentY + yAxisDistanceFromCenter)
		);

		// Set rotation
		int angle = (directionToFace + 1) * 90;
		ScreenCursor.S.cursorGO[cursorNdx].transform.localEulerAngles = new Vector3(0, 0, angle);
	}

	////////////////////////////////////////////////////////////////////////////////
	// Returns true if this gameObject is closer to another gameObject
	// horizontally than it is vertically
	public bool isCloserHorizontally(GameObject gameObject1, GameObject gameObject2) {
		float tX = Mathf.Abs(gameObject1.transform.position.x - gameObject2.transform.position.x);
		float tY = Mathf.Abs(gameObject1.transform.position.y - gameObject2.transform.position.y);

		if (tX < tY) { return true; } else { return false; }
	}

	////////////////////////////////////////////////////////////////////////////////
	// Set multiple buttons' text color
	public void SetTextColor(List<Button> buttons, Color32 color) {
		for (int i = 0; i < buttons.Count; i++) {
			buttons[i].GetComponentInChildren<Text>().color = color;
		}
	}

	////////////////////////////////////////////////////////////////////////////////
	// Activate or deactivate all of the gameObject elements stored within a list 
	public void SetActiveList(List<GameObject> objects, bool isActive) {
		for (int i = 0; i < objects.Count; i++) {
			objects[i].SetActive(isActive);
		}
	}

	////////////////////////////////////////////////////////////////////////////////
	// Explicitly set a button's navigation
	public void SetButtonNavigation(Button button, Button buttonSelectOnDown, Button buttonSelectOnUp) {
		// Get the Navigation data
		Navigation navigation = button.navigation;

		// Switch mode to Explicit to allow for custom assigned behavior
		navigation.mode = Navigation.Mode.Explicit;

        // Highlight these buttons if the down or up arrow keys are pressed
        navigation.selectOnDown = buttonSelectOnDown;
        navigation.selectOnUp = buttonSelectOnUp;

		// Reassign the struct data to the button
		button.navigation = navigation;
    }
}
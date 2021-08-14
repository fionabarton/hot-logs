using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attached to RPGMainCamera; used to get InteractableCursorHolder gameObject
/// </summary>
public class InteractableCursor : MonoBehaviour{
	[Header("Set Dynamically")]
	// Singleton
	private static InteractableCursor _S;
	public static InteractableCursor S { get { return _S; } set { _S = value; } }
	
	[Header("Set in Inspector")]
	// Cursor GameObject
	public GameObject cursorGO;

	void Awake() {
		// Singleton
		S = this;
	}

	// Activate, position, and set parent of cursor
	public void Activate (bool activateCursor, GameObject otherGO = null) {
		if (activateCursor) {
			Utilities.S.SetPosition(cursorGO, otherGO.transform.position.x, otherGO.transform.position.y + 0.5f);
			cursorGO.transform.SetParent(otherGO.transform);
			cursorGO.SetActive(activateCursor);
		} else {
			cursorGO.transform.SetParent(CamManager.S.transform);
			cursorGO.SetActive(activateCursor);
		}	
	}
}
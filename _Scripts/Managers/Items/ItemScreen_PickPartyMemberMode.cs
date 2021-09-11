﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScreen_PickPartyMemberMode : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static ItemScreen_PickPartyMemberMode _S;
	public static ItemScreen_PickPartyMemberMode S { get { return _S; } set { _S = value; } }

	void Awake() {
		S = this;
	}

	public void Loop(ItemScreen itemScreen) {
		if (itemScreen.canUpdate) {
			Utilities.S.PositionCursor(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject, 0, 60, 3);

			// Set animation to walk
			PlayerButtons.S.SetAnim("Walk");

			itemScreen.canUpdate = false;
		}

		if (PauseMessage.S.dialogueFinished) {
			if (Input.GetButtonDown("SNES B Button")) {
				// Set animation to idle
				PlayerButtons.S.SetAnim("Idle");

				// Go back to PickItem mode
				ItemScreen_PickItemMode.S.Setup(ItemScreen.S); 
			}
		}
	}
}
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScreen_UsedItemMode : MonoBehaviour { 
	[Header("Set Dynamically")]
	// Singleton
	private static ItemScreen_UsedItemMode _S;
	public static ItemScreen_UsedItemMode S { get { return _S; } set { _S = value; } }

	void Awake() {
		S = this;
	}

	public void Loop(ItemScreen itemScreen) {
		if (PauseMessage.S.dialogueFinished) {
			if (Input.GetButtonDown("SNES A Button")) {
				// Set animation to idle
				PlayerButtons.S.SetAnim("Idle");

				// Go back to PickItem mode
				ItemScreen_PickItemMode.S.Setup(ItemScreen.S);
			}
		}
	}
}
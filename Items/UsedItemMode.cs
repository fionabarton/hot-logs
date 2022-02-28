using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ItemScreen Mode/Step 3: UsedItemMode
/// - Consumed an item
/// </summary>
public class UsedItemMode : MonoBehaviour { 
	public void Loop(ItemScreen itemScreen) {
		if (PauseMessage.S.dialogueFinished) {
			if (Input.GetButtonDown("SNES B Button")) {
				// Set animation to idle
				PlayerButtons.S.SetSelectedAnim("Idle");

				// Go back to PickItem mode
				itemScreen.pickItemMode.Setup(ItemScreen.S);
			}
		}
	}
}
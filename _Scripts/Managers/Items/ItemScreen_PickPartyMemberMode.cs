using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ItemScreen Mode/Step 2: PickPartyMember
/// - Select which party member to use an item on
/// </summary>
public class ItemScreen_PickPartyMemberMode : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static ItemScreen_PickPartyMemberMode _S;
	public static ItemScreen_PickPartyMemberMode S { get { return _S; } set { _S = value; } }

	// Ensures audio is only played once when button is selected
	public GameObject previousSelectedPlayerGO;

	void Awake() {
		S = this;
	}

	public void Loop(ItemScreen itemScreen) {
		if (itemScreen.canUpdate) {
			Utilities.S.PositionCursor(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject, 0, 60, 3);

			// Set animation to walk
			PlayerButtons.S.SetSelectedAnim("Walk");

			// Audio: Selection (when a new gameObject is selected)
			Utilities.S.PlayButtonSelectedSFX(ref previousSelectedPlayerGO);

			itemScreen.canUpdate = false;
		}

		if (PauseMessage.S.dialogueFinished) {
			if (Input.GetButtonDown("SNES B Button")) {
				// Set animation to idle
				PlayerButtons.S.SetSelectedAnim("Idle");

				// Audio: Deny
				AudioManager.S.PlaySFX(eSoundName.deny);

				// Go back to PickItem mode
				ItemScreen_PickItemMode.S.Setup(ItemScreen.S); 
			}
		}
	}
}
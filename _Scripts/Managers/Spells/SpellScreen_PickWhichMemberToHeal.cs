using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SpellScreen Mode/Step 4: PickWhichMemberToHeal
/// - Select which party member to use spell on
/// </summary>
public class SpellScreen_PickWhichMemberToHeal : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static SpellScreen_PickWhichMemberToHeal _S;
	public static SpellScreen_PickWhichMemberToHeal S { get { return _S; } set { _S = value; } }

	// Ensures audio is only played once when button is selected
	public GameObject previousSelectedPlayerGO;

	void Awake() {
		S = this;
	}

	public void Loop(SpellScreen spellScreen) {
		if (spellScreen.canUpdate) {
			Utilities.S.PositionCursor(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject, 0, 60, 3);

			// Set animation to walk
			PlayerButtons.S.SetSelectedAnim("Walk");

			// Audio: Selection (when a new gameObject is selected)
			Utilities.S.PlayButtonSelectedSFX(ref previousSelectedPlayerGO);

			spellScreen.canUpdate = false;
		}

		if (PauseMessage.S.dialogueFinished) {
			if (Input.GetButtonDown("SNES B Button")) {
				// Set animation to idle
				PlayerButtons.S.SetSelectedAnim("Idle");

				// Audio: Deny
				AudioManager.S.PlaySFX(eSoundName.deny);

				spellScreen.LoadSpells(spellScreen.playerNdx); // Go Back
			}
		}
	}
}
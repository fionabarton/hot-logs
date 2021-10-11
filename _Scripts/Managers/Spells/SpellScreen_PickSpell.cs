using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SpellScreen Mode/Step 2: PickSpell
/// - Select which spell to use
/// </summary>
public class SpellScreen_PickSpell : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static SpellScreen_PickSpell _S;
	public static SpellScreen_PickSpell S { get { return _S; } set { _S = value; } }

	void Awake() {
		S = this;
	}

	public void Loop(SpellScreen spellScreen) {
		if (spellScreen.canUpdate) {
			spellScreen.DisplaySpellsDescriptions(spellScreen.playerNdx);
			spellScreen.canUpdate = false;
		}

		if (PauseMessage.S.dialogueFinished) {
			if (Input.GetButtonDown("SNES B Button")) {
				// Go Back
				SpellScreen_PickWhichSpellsToDisplay.S.Setup(spellScreen);

				// Audio: Deny
				AudioManager.S.PlaySFX(eSoundName.deny);
			}
		}
	}
}
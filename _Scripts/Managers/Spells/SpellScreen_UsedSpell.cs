using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SpellScreen Mode/Step 5: UsedSpell
/// - Used a spell
/// </summary
public class SpellScreen_UsedSpell : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static SpellScreen_UsedSpell _S;
	public static SpellScreen_UsedSpell S { get { return _S; } set { _S = value; } }

	void Awake() {
		S = this;
	}

	public void Loop(SpellScreen spellScreen) {
		if (PauseMessage.S.dialogueFinished) {
			if (Input.GetButtonDown("SNES A Button")) {
				// Set animation to idle
				PlayerButtons.S.SetSelectedAnim("Idle");

				spellScreen.LoadSpells(spellScreen.playerNdx);
			}
		}
	}
}
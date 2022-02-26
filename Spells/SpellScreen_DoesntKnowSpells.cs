using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SpellScreen Mode/Step 3: DoesntKnowSpells
/// - This party member doesn't know any spells
/// </summary>
public class SpellScreen_DoesntKnowSpells : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static SpellScreen_DoesntKnowSpells _S;
	public static SpellScreen_DoesntKnowSpells S { get { return _S; } set { _S = value; } }

	void Awake() {
		S = this;
	}

	public void Loop(SpellScreen spellScreen) {
		if (PauseMessage.S.dialogueFinished) {
			if (Input.GetButtonDown("SNES B Button")) {
				SpellScreen_PickWhichSpellsToDisplay.S.Setup(spellScreen);
			}
		}
	}
}
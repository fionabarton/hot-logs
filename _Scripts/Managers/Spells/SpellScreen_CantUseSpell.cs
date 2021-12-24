using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SpellScreen Mode/Step 6: CantUseSpell
/// - Can't use this spell
/// </summary
public class SpellScreen_CantUseSpell : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static SpellScreen_CantUseSpell _S;
	public static SpellScreen_CantUseSpell S { get { return _S; } set { _S = value; } }

	void Awake() {
		S = this;
	}

	public void Loop(SpellScreen spellScreen) {
		if (PauseMessage.S.dialogueFinished) {
			if (Input.GetButtonDown("SNES B Button")) {
				spellScreen.ScreenOffPlayerTurn();
			}
		}
	}
}
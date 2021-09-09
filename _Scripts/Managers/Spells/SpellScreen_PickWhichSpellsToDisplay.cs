using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// SpellScreen Mode/Step 1: PickWhichSpellsToDisplay
/// - Select which party member's spells to use
/// </summary>
public class SpellScreen_PickWhichSpellsToDisplay : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static SpellScreen_PickWhichSpellsToDisplay _S;
	public static SpellScreen_PickWhichSpellsToDisplay S { get { return _S; } set { _S = value; } }

	void Awake() {
		S = this;
	}

	public void Setup(SpellScreen spellScreen) {
		try {
			if (RPG.S.currentSceneName != "Battle") {
				// Buttons Interactable
				Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, true);
				Utilities.S.ButtonsInteractable(spellScreen.spellsButtons, false);
				Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, false);

				spellScreen.canUpdate = true;

				// Remove Listeners
				Utilities.S.RemoveListeners(PlayerButtons.S.buttonsCS);
				// Add Listeners
				PlayerButtons.S.buttonsCS[0].onClick.AddListener(delegate { spellScreen.LoadSpells(0); });
				PlayerButtons.S.buttonsCS[1].onClick.AddListener(delegate { spellScreen.LoadSpells(1); });

				// Activate PlayerButtons
				PlayerButtons.S.gameObject.SetActive(true);

				// Set Selected
				Utilities.S.SetSelectedGO(PlayerButtons.S.buttonsCS[0].gameObject);

				// Display Text
				PauseMessage.S.DisplayText("Use whose spells?!");

				// Switch ScreenMode 
				spellScreen.mode = eSpellScreenMode.pickWhichSpellsToDisplay;

				// Activate Cursor
				ScreenCursor.S.cursorGO.SetActive(true);
			} else {
				// Set Turn Cursor sorting layer BELOW UI
				BattleUI.S.turnCursorSRend.sortingLayerName = "0";
			}
		}
		catch (NullReferenceException) { }
	}

	public void Loop(SpellScreen spellScreen) {
		if (spellScreen.canUpdate) {
			// Display each Member's Spells
			for (int i = 0; i < PlayerButtons.S.buttonsCS.Count; i++) {
				if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == PlayerButtons.S.buttonsCS[i].gameObject) {
					spellScreen.DeactivateUnusedSpellsSlots(i);
					spellScreen.DisplaySpellsDescriptions(i);
					Utilities.S.PositionCursor(PlayerButtons.S.buttonsCS[i].gameObject, 0, 60, 3);
				}
			}

			// Set animation to walk
			PlayerButtons.S.SetAnim("Walk");

			spellScreen.canUpdate = false;
		}
	}
}
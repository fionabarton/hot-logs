using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// SpellScreen Mode/Step 1: PickWhichSpellsToDisplay
/// - Select which party member's spells to use
/// </summary>
public class SpellScreen_PickWhichSpellsToDisplay : MonoBehaviour {
	[Header("Set in Inspector")]
	public Text titleText;

	[Header("Set Dynamically")]
	// Singleton
	private static SpellScreen_PickWhichSpellsToDisplay _S;
	public static SpellScreen_PickWhichSpellsToDisplay S { get { return _S; } set { _S = value; } }

	void Awake() {
		S = this;
	}

	public void Setup(SpellScreen spellScreen) {
		try {
			if (RPG.S.currentScene != "Battle") {
				// Buttons Interactable
				Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, true);
				Utilities.S.ButtonsInteractable(spellScreen.spellsButtons, false);
				Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, false);

				spellScreen.canUpdate = true;

				// Remove Listeners
				Utilities.S.RemoveListeners(PlayerButtons.S.buttonsCS);
				// Add Listeners
				PlayerButtons.S.buttonsCS[0].onClick.AddListener(delegate { spellScreen.LoadSpells(0, true); });
				PlayerButtons.S.buttonsCS[1].onClick.AddListener(delegate { spellScreen.LoadSpells(1, true); });
				PlayerButtons.S.buttonsCS[2].onClick.AddListener(delegate { spellScreen.LoadSpells(2, true); });

				// Activate PlayerButtons
				PlayerButtons.S.gameObject.SetActive(true);

				// Set Slot Headers Text 
				spellScreen.nameHeaderText.text = "Name:";

				// Set Selected gameObject
				Utilities.S.SetSelectedGO(spellScreen.previousSelectedPlayerGO);

				// Reset which spell to select
				spellScreen.previousSelectedSpellGO = spellScreen.spellsButtons[0].gameObject;

				// Display Text
				PauseMessage.S.DisplayText("Use whose spells?!");

				// Switch ScreenMode 
				spellScreen.mode = eSpellScreenMode.pickWhichSpellsToDisplay;

				// Activate Cursor
				ScreenCursor.S.cursorGO[0].SetActive(true);
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
					spellScreen.AssignSpellsNames(i);
					Utilities.S.PositionCursor(PlayerButtons.S.buttonsCS[i].gameObject, 0, 60, 3);
					titleText.text = "Spells: " + "<color=white>" + Party.S.stats[i].name + "</color>";

					// Audio: Selection (when a new gameObject is selected)
					Utilities.S.PlayButtonSelectedSFX(ref spellScreen.previousSelectedPlayerGO);
				}
			}

			// Set animation to walk
			PlayerButtons.S.SetSelectedAnim("Walk");

			spellScreen.canUpdate = false;
		}
	}
}
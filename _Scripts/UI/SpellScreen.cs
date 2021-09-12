using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public enum eSpellScreenMode { pickWhichSpellsToDisplay, pickSpell, doesntKnowSpells, pickWhichMemberToHeal, usedSpell, cantUseSpell };

public class SpellScreen : MonoBehaviour {
	// Overworld & Battle Spells: Antidote, Revive (50 & 100% success rate), Full Heal, Full Party Heal
	// Overworld Spells: Evac, Warp
	// Battle ONLY Spells: Buff (Strength, Agility), AOE (Attack, Heal), Status (Sleep, Poison, Confuse, Blind)

	[Header("Set in Inspector")]
	// Spells "Buttons"
	public List<Button> 	spellsButtons;
	public List<Text> 	  	spellsButtonNameTexts;

	[Header("Set Dynamically")]
	// Singleton
	private static SpellScreen _S;
	public static SpellScreen S { get { return _S; } set { _S = value; } }

	public int 				playerNdx = 0; // Used to set Player 1 or 2 for DisplaySpellsDescriptions(), set in LoadSpells()

	// For Input & Display MessageRemoveAllListeners
	public eSpellScreenMode	mode;

	// Allows parts of Loop() to be called once rather than repeatedly every frame.
	public bool 			canUpdate;

	public GameObject		previousSelectedSpellGO;
	public GameObject		previousSelectedPlayerGO;

	void Awake() {
		S = this;
	}

	void OnEnable () {
		if (RPG.S.currentSceneName != "Battle") {
			// Ensures first slots are selected when screen enabled
			previousSelectedPlayerGO = PlayerButtons.S.buttonsCS[0].gameObject;
			previousSelectedSpellGO = spellsButtons[0].gameObject;
		}

		SpellScreen_PickWhichSpellsToDisplay.S.Setup(S);

		// Add Loop() to Update Delgate
		UpdateManager.updateDelegate += Loop;
	}

	public void Deactivate () {
		// Set Battle Turn Cursor sorting layer ABOVE UI
		BattleUI.S.turnCursorSRend.sortingLayerName = "Above UI";

		// Remove Listeners
		Utilities.S.RemoveListeners(spellsButtons);

		if (RPG.S.currentSceneName != "Battle") {
			// Buttons Interactable
			Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, true);
			// Set Selected Gameobject (Pause Screen: Spells Button)
			Utilities.S.SetSelectedGO(PauseScreen.S.buttonGO[2]);

			PauseMessage.S.DisplayText("Welcome to the Pause Screen!");

			PauseScreen.S.canUpdate = true;

			// Activate Cursor
			ScreenCursor.S.cursorGO.SetActive(true);
		} else {
			// If Player didn't use a Spell, go back to Player Turn
			if (mode != eSpellScreenMode.pickSpell) {
				if (Battle.S.battleMode == eBattleMode.itemOrSpellMenu) {
					Battle.S.PlayerTurn();
				}
			}

			// Deactivate Cursor
			ScreenCursor.S.cursorGO.SetActive(false);
		}

		// Deactivate PlayerButtons
		PlayerButtons.S.gameObject.SetActive(false);

		// Remove Loop() from Update Delgate
		UpdateManager.updateDelegate -= Loop;

		// Deactivate this gameObject
		gameObject.SetActive(false);
	}

	public void Loop () {
		// Reset canUpdate
		if (Input.GetAxisRaw ("Horizontal") != 0f || Input.GetAxisRaw ("Vertical") != 0f) { 
			canUpdate = true;
		}

		// Deactivate SpellScreen
		if (RPG.S.currentSceneName != "Battle") {
			if (mode == eSpellScreenMode.pickWhichSpellsToDisplay) {
				if (Input.GetButtonDown ("SNES B Button")) {
					Deactivate();
				}
			}
		} else {
			if (Input.GetButtonDown ("SNES B Button")) { 
				ScreenOffPlayerTurn ();
			}
		}

		switch (mode) {
		case eSpellScreenMode.pickWhichSpellsToDisplay:
			SpellScreen_PickWhichSpellsToDisplay.S.Loop(S);
			break;
		case eSpellScreenMode.pickSpell:
			SpellScreen_PickSpell.S.Loop(S);
			break;
		case eSpellScreenMode.doesntKnowSpells:
			SpellScreen_DoesntKnowSpells.S.Loop(S);
			break;
		case eSpellScreenMode.pickWhichMemberToHeal:
			SpellScreen_PickWhichMemberToHeal.S.Loop(S);
			break;
		case eSpellScreenMode.usedSpell:
			SpellScreen_UsedSpell.S.Loop(S);
			break;
		// During Battle... "Not Enough MP Message", then back to Player Turn w/ Button Press
		case eSpellScreenMode.cantUseSpell:
			SpellScreen_CantUseSpell.S.Loop(S);
			break;
		}
	}
	
	public void LoadSpells(int playerNdx){
		// Buttons Interactable
		Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, false);
		Utilities.S.ButtonsInteractable(spellsButtons, true);

		this.playerNdx = playerNdx; // Now used to DisplaySpellsDescriptions

		if (RPG.S.currentSceneName == "Battle") {
			// Activate Spell Screen
			gameObject.SetActive (true);
		}

		DeactivateUnusedSpellsSlots (playerNdx);
		AssignSpellsEffect (playerNdx);
		DisplaySpellsDescriptions (playerNdx);
	
		// Empty Inventory
		if (Party.stats[playerNdx].spellNdx == 0) {
			PauseMessage.S.DisplayText(Party.stats[playerNdx].name + " knows no spells, fool!");

			canUpdate = true;
			// Switch ScreenMode
			mode = eSpellScreenMode.doesntKnowSpells;

			// Deactivate Cursor
			ScreenCursor.S.cursorGO.SetActive (false);
		} else {
			canUpdate = true;
			// Switch ScreenMode 
			mode = eSpellScreenMode.pickSpell;

			// Set Selected GameObject 
			// If previousSelectedGameObject is enabled...
			if (previousSelectedSpellGO.activeInHierarchy && RPG.S.currentSceneName != "Battle") {
				// Select previousSelectedGameObject
				Utilities.S.SetSelectedGO(previousSelectedSpellGO);
			} else {
				// Select first spell in the list
				Utilities.S.SetSelectedGO(spellsButtons[0].gameObject);
			}

			// Activate Cursor
			ScreenCursor.S.cursorGO.SetActive (true);
		}
	}

	public void DisplaySpellsDescriptions (int playerNdx) {
		for (int i = 0; i < Party.stats[playerNdx].spellNdx; i++) {
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == spellsButtons[i].gameObject) {
				PauseMessage.S.SetText(Party.stats[playerNdx].spells[i].description);

				// Cursor Position set to Selected Button
				Utilities.S.PositionCursor(spellsButtons[i].gameObject, -160, 0, 0);

				// Set selected button text color	
				spellsButtons[i].gameObject.GetComponentInChildren<Text>().color = new Color32(205, 208, 0, 255);

				// Cache Selected Gameobject 
				previousSelectedSpellGO = spellsButtons[i].gameObject;
			} else {
				// Set non-selected button text color
				spellsButtons[i].gameObject.GetComponentInChildren<Text>().color = new Color32(255, 255, 255, 255);
			}
		}
	}
	
	public void DeactivateUnusedSpellsSlots (int playerNdx) {
		for (int i = 0; i < spellsButtons.Count; i++) {
			if (i < Party.stats[playerNdx].spellNdx) {
				spellsButtons [i].gameObject.SetActive (true);
			} else {
				spellsButtons [i].gameObject.SetActive (false);
			} 
		}
	}

	public void AssignSpellsEffect(int playerNdx) { 
		Utilities.S.RemoveListeners(PlayerButtons.S.buttonsCS);
		Utilities.S.RemoveListeners(spellsButtons);

		for (int i = 0; i < Party.stats[playerNdx].spellNdx; i++) {
			// Add listener to Spell Button
			int copy = i;
			spellsButtons[copy].onClick.AddListener(delegate { UseSpell(Party.stats[playerNdx].spells[copy]); });

			// Assign Button Name Text
			AssignSpellsNames(playerNdx);
		}
	}

	public void AssignSpellsNames(int playerNdx) {
		for (int i = 0; i < Party.stats[playerNdx].spellNdx; i++) {
			// Assign Button Name Text
			spellsButtonNameTexts[i].text = Party.stats[playerNdx].spells[i].name;
		}
	}

	public void ScreenOffPlayerTurn (){
		PauseMessage.S.gameObject.SetActive (false);

		Deactivate();

		if (Battle.S.battleMode == eBattleMode.itemOrSpellMenu) {
			Battle.S.PlayerTurn();
		}
	}

	public void ScreenOffEnemyDeath (int enemyNdx){
		Deactivate();

		BattleEnd.S.EnemyDeath (enemyNdx); 
	}

	// Inspired by ConsumeItem() in ItemScreen.cs
	public void UseSpell(Spell tSpell) {
		canUpdate = true;
		switch (tSpell.name) {
			case "Heal":
				if (RPG.S.currentSceneName == "Battle") { // if Battle
					BattleSpells.S.HealSpell();
				} else { // if Overworld
					WorldSpells.S.SelectPartyMemberToHeal();
				}
				break;
			case "Fireball":
				if (RPG.S.currentSceneName == "Battle") { // if Battle
					BattleSpells.S.FireballSpell();
				} else { // if Overworld
					SpellManager.S.CantUseSpell("You ain't battlin' no one, so ya can't use dis spell!");
				}
				break;
			case "Warp":
				if (RPG.S.currentSceneName == "Battle") { // if Battle
					SpellManager.S.CantUseSpell("Can't use this spell during battle!");
				} else { // if Overworld
					WorldSpells.S.WarpSpell();
				}
				break;
			case "Fireblast":
				if (RPG.S.currentSceneName == "Battle") { // if Battle
					BattleSpells.S.FireblastSpell();
				} else { // if Overworld
					SpellManager.S.CantUseSpell("You ain't battlin' no one, so ya can't use dis spell!");
				}
				break;
		}
	}
}
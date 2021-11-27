using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public enum eSpellScreenMode { pickWhichSpellsToDisplay, pickSpell, doesntKnowSpells, 
	pickWhichMemberToHeal, pickAllMembersToHeal, usedSpell, cantUseSpell, pickWhereToWarp };

public class SpellScreen : MonoBehaviour {
	// Overworld & Battle Spells: Antidote, Revive (50 & 100% success rate), Full Heal, Full Party Heal
	// Overworld Spells: Evac, Warp
	// Battle ONLY Spells: Buff (Strength, Agility), AOE (Attack, Heal), Status (Sleep, Poison, Confuse, Blind)

	[Header("Set in Inspector")]
	// Spells "Buttons"
	public List<Button> 	spellsButtons;
	public List<Text> 	  	spellsButtonNameText;
	public List<Text>		spellsButtonMPCostText;
	
	public Text				nameHeaderText;
	public GameObject		MPCostHeader;

	public GameObject		previousSelectedSpellGO;

	[Header("Set Dynamically")]
	// Singleton
	private static SpellScreen _S;
	public static SpellScreen S { get { return _S; } set { _S = value; } }

	public int 				playerNdx = 0; // Used to set Player 1 or 2 for DisplaySpellsDescriptions(), set in LoadSpells()

	// For Input & Display MessageRemoveAllListeners
	public eSpellScreenMode	mode;

	// Allows parts of Loop() to be called once rather than repeatedly every frame.
	public bool 			canUpdate;

	public GameObject		previousSelectedPlayerGO;

	void Awake() {
		S = this;
	}

	void OnEnable () {
		try {
			if (RPG.S.currentScene != "Battle") {
				// Ensures first slots are selected when screen enabled
				previousSelectedPlayerGO = PlayerButtons.S.buttonsCS[0].gameObject;
				previousSelectedSpellGO = spellsButtons[0].gameObject;
			}

			SpellScreen_PickWhichSpellsToDisplay.S.Setup(S);

			// Add Loop() to Update Delgate
			UpdateManager.updateDelegate += Loop;
		}
		catch (NullReferenceException) { }
	}

	public void Activate() {
		// Activate MP Cost header
		MPCostHeader.SetActive(true);
		
		gameObject.SetActive(true);

		// Audio: Confirm
		AudioManager.S.PlaySFX(eSoundName.confirm);
	}

	public void Deactivate (bool playSound = false) {
		// Set Battle Turn Cursor sorting layer ABOVE UI
		BattleUI.S.turnCursorSRend.sortingLayerName = "Above UI";

		// Remove Listeners
		Utilities.S.RemoveListeners(spellsButtons);

		if (RPG.S.currentScene != "Battle") {
			// Buttons Interactable
			Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, true);
			// Set Selected Gameobject (Pause Screen: Spells Button)
			Utilities.S.SetSelectedGO(PauseScreen.S.buttonGO[2]);

			PauseMessage.S.DisplayText("Welcome to the Pause Screen!");

			PauseScreen.S.canUpdate = true;

			// Activate Cursor
			ScreenCursor.S.cursorGO[0].SetActive(true);
		} else {
			// If Player didn't use a Spell, go back to Player Turn
			if (mode != eSpellScreenMode.pickSpell) {
				if (Battle.S.battleMode == eBattleMode.itemOrSpellMenu) {
					Battle.S.PlayerTurn(false);
				}
			}

			// Deactivate screen cursors
			Utilities.S.SetActiveList(ScreenCursor.S.cursorGO, false);
		}

        if (playSound) {
			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);
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
		if (RPG.S.currentScene != "Battle") {
			if (mode == eSpellScreenMode.pickWhichSpellsToDisplay) {
				if (Input.GetButtonDown ("SNES B Button")) {
					Deactivate(true);
				}
			}
		} else {
			if (Input.GetButtonDown ("SNES B Button")) {
				// Audio: Deny
				AudioManager.S.PlaySFX(eSoundName.deny);
				ScreenOffPlayerTurn();
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
		case eSpellScreenMode.pickAllMembersToHeal:
			if (Input.GetButtonDown("SNES B Button")) {
				GoBackToPickSpellMode();
			}
		break;
		case eSpellScreenMode.pickWhereToWarp:
			if (canUpdate) {
				WarpManager.S.DisplayButtonDescriptions(spellsButtons, -160);
			}

			if (Input.GetButtonDown("SNES B Button")) {
				GoBackToPickSpellMode();

				// Activate MP Cost header
				MPCostHeader.SetActive(true);
				// Reset Slot Headers Text 
				nameHeaderText.text = "Name:";
			}
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

	void GoBackToPickSpellMode() {
		if (PauseMessage.S.dialogueFinished) {
			// Set animations to idle
			PlayerButtons.S.SetSelectedAnim("Idle");

			// Reset button colors
			PlayerButtons.S.SetButtonsColor(PlayerButtons.S.buttonsCS, new Color32(255, 255, 255, 200));

			// Deactivate screen cursors
			Utilities.S.SetActiveList(ScreenCursor.S.cursorGO, false);

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);

			LoadSpells(playerNdx); // Go Back
		}
	}
	
	public void LoadSpells(int playerNdx, bool playSound = false){
		PlayerButtons.S.SetSelectedAnim("Idle");

		// Buttons Interactable
		Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, false);
		Utilities.S.ButtonsInteractable(spellsButtons, true);

		this.playerNdx = playerNdx; // Now used to DisplaySpellsDescriptions

		if (RPG.S.currentScene == "Battle") {
			// Activate Spell Screen
			Activate();
		}

		// Empty Inventory
		if (Party.S.stats[playerNdx].spellNdx == 0) {
			PauseMessage.S.DisplayText(Party.S.stats[playerNdx].name + " knows no spells, fool!");

			canUpdate = true;
			// Switch ScreenMode
			mode = eSpellScreenMode.doesntKnowSpells;

			// Deactivate screen cursors
			Utilities.S.SetActiveList(ScreenCursor.S.cursorGO, false);

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);
		} else {
			canUpdate = true;
			// Switch ScreenMode 
			mode = eSpellScreenMode.pickSpell;

			// Set Selected GameObject 
			// If previousSelectedGameObject is enabled...
			if (previousSelectedSpellGO.activeInHierarchy && RPG.S.currentScene != "Battle") {
				// Select previousSelectedGameObject
				Utilities.S.SetSelectedGO(previousSelectedSpellGO);
			} else {
				// Set previously selected GameObject
				previousSelectedSpellGO = spellsButtons[0].gameObject;

				// Select first spell in the list
				Utilities.S.SetSelectedGO(spellsButtons[0].gameObject);
			}

			// Activate Cursor
			ScreenCursor.S.cursorGO[0].SetActive(true);

			if (playSound) {
				// Audio: Confirm
				AudioManager.S.PlaySFX(eSoundName.confirm);
			}
		}

		DeactivateUnusedSpellsSlots(playerNdx);
		AssignSpellsEffect(playerNdx);
		DisplaySpellsDescriptions(playerNdx);

		// Set the first and last button’s navigation 
		SetButtonNavigation();
	}

	public void DisplaySpellsDescriptions (int playerNdx) {
		for (int i = 0; i < Party.S.stats[playerNdx].spellNdx; i++) {
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == spellsButtons[i].gameObject) {
				PauseMessage.S.SetText(Party.S.stats[playerNdx].spells[i].description);

				// Cursor Position set to Selected Button
				Utilities.S.PositionCursor(spellsButtons[i].gameObject, -160, 0, 0);

				// Set selected button text color	
				spellsButtonNameText[i].color = new Color32(205, 208, 0, 255);
				spellsButtonMPCostText[i].color = new Color32(205, 208, 0, 255);

				// Audio: Selection (when a new gameObject is selected)
				Utilities.S.PlayButtonSelectedSFX(ref previousSelectedSpellGO);
			} else {
				// Set non-selected button text color
				spellsButtonNameText[i].color = new Color32(255, 255, 255, 255);
				spellsButtonMPCostText[i].color = new Color32(255, 255, 255, 255);
			}
		}
	}
	
	public void DeactivateUnusedSpellsSlots (int playerNdx) {
		for (int i = 0; i < spellsButtons.Count; i++) {
			if (i < Party.S.stats[playerNdx].spellNdx) {
				spellsButtons[i].gameObject.SetActive(true);
				spellsButtonMPCostText[i].gameObject.SetActive(true);
			} else {
				spellsButtons[i].gameObject.SetActive(false);
				spellsButtonMPCostText[i].gameObject.SetActive(false);
			} 
		}
	}

	public void AssignSpellsEffect(int playerNdx) { 
		Utilities.S.RemoveListeners(PlayerButtons.S.buttonsCS);
		Utilities.S.RemoveListeners(spellsButtons);

		for (int i = 0; i < Party.S.stats[playerNdx].spellNdx; i++) {
			// Add listener to Spell Button
			int copy = i;
			spellsButtons[copy].onClick.AddListener(delegate { UseSpell(Party.S.stats[playerNdx].spells[copy]); });

			// Assign Button Name Text
			AssignSpellsNames(playerNdx);
		}
	}

	public void AssignSpellsNames(int playerNdx) {
		for (int i = 0; i < Party.S.stats[playerNdx].spellNdx; i++) {
			// Assign Button Name Text
			string ndx = (i + 1).ToString();
			spellsButtonNameText[i].text = ndx + ") " + Party.S.stats[playerNdx].spells[i].name;
			spellsButtonMPCostText[i].text = Party.S.stats[playerNdx].spells[i].cost.ToString();
		}
	}

	// Set the first and last button’s navigation 
	public void SetButtonNavigation() {
		// Reset all button's navigation to automatic
		for (int i = 0; i < Party.S.stats[playerNdx].spellNdx; i++) {
			// Get the Navigation data
			Navigation navigation = spellsButtons[i].navigation;

			// Switch mode to Automatic
			navigation.mode = Navigation.Mode.Automatic;

			// Reassign the struct data to the button
			spellsButtons[i].navigation = navigation;
		}

		// Set button navigation if inventory is less than 10
		//if (Party.S.stats[playerNdx].spellNdx < spellsButtons.Count) {
		if (Party.S.stats[playerNdx].spellNdx > 1) {
			// Set first button navigation
			Utilities.S.SetVerticalButtonNavigation(
				spellsButtons[0],
				spellsButtons[1],
				spellsButtons[Party.S.stats[playerNdx].spellNdx - 1]);

			// Set last button navigation
			Utilities.S.SetVerticalButtonNavigation(
				spellsButtons[Party.S.stats[playerNdx].spellNdx - 1],
				spellsButtons[0],
				spellsButtons[Party.S.stats[playerNdx].spellNdx - 2]);
			//}
		}
	}

	public void ScreenOffPlayerTurn (){
		PauseMessage.S.gameObject.SetActive (false);

		Deactivate(true);

		if (Battle.S.battleMode == eBattleMode.itemOrSpellMenu) {
			Battle.S.PlayerTurn(false);
		}
	}

	public void ScreenOffEnemyDeath (int enemyNdx){
		Deactivate();

		BattleEnd.S.EnemyDeath(enemyNdx); 
	}

	// Inspired by ConsumeItem() in ItemScreen.cs
	public void UseSpell(Spell spell) {
		canUpdate = true;

		if (RPG.S.currentScene == "Battle") { // if Battle
			if (spell.name == "Heal") {
				BattleSpells.S.AddFunctionToButton(BattleSpells.S.AttemptHealSelectedPartyMember, "Heal which party member?", spell);
			} else if (spell.name == "Fireball") {
				BattleSpells.S.AddFunctionToButton(BattleSpells.S.AttackSelectedEnemies, "Attack which enemy?", spell);
			} else if (spell.name == "Fireblast") {
				BattleSpells.S.AddFunctionToButton(BattleSpells.S.AttackAllEnemies, "Attack all enemies?", spell);
			} else if (spell.name == "Heal All") {
				BattleSpells.S.AddFunctionToButton(BattleSpells.S.AttemptHealAll, "Heal all party members?", spell);
			} else if (spell.name == "Revive") {
				BattleSpells.S.AddFunctionToButton(BattleSpells.S.ReviveSelectedPartyMember, "Revive which party member?", spell);
			} else {
				SpellManager.S.CantUseSpell("Can't use this spell during battle!");
			}
		} else { // if Overworld
			if (spell.name == "Heal") {
				WorldSpells.S.AddFunctionToButton(WorldSpells.S.HealSelectedPartyMember, "Heal which party member?", spell);
			} else if (spell.name == "Warp") {
				WorldSpells.S.WarpSpell();
			} else if (spell.name == "Heal All") {
				WorldSpells.S.AddFunctionToButton(WorldSpells.S.HealAllPartyMembers, "Heal all party members?", spell);
			} else {
				SpellManager.S.CantUseSpell("You ain't battlin' no one, so ya can't use this spell!");
			}
		}
	}
}
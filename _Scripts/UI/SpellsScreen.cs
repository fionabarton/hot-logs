using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

// Not used yet
public enum eSpellScreenMode { pickWhichSpellsToDisplay, pickSpell, doesntKnowSpells, pickWhichMemberToHeal, usedSpell, cantUseSpell };

public class SpellsScreen : MonoBehaviour {
	// Overworld & Battle Spells: Antidote, Zing, Full Heal

	// Overworld Spells: Evac, Warp

	// Battle ONLY Spells: Buff (Strength, Agility), AOE (Attack, Heal), Status (Sleep, Poison, Confuse, Blind)

	[Header("Set in Inspector")]
	// Spells "Buttons"
	public List<Button> 	spellsButtons;
	public List<Text> 	  	spellsButtonNameTexts;

	[Header("Set Dynamically")]
	// Singleton
	private static SpellsScreen _S;
	public static SpellsScreen S { get { return _S; } set { _S = value; } }

	public int 				playerNdx = 0; // Used to set Player 1 or 2 for DisplaySpellsDescriptions(), set in LoadSpells()

	// For Input & Display MessageRemoveAllListeners
	public eSpellScreenMode	spellScreenMode = eSpellScreenMode.pickWhichSpellsToDisplay;

	public bool 			canUpdate;

	void Awake() {
		// Singleton
		S = this;

	}

	void OnEnable () {
		try{
		if (RPG.S.currentSceneName != "Battle") { 
			// Buttons Interactable
			Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, true);
			Utilities.S.ButtonsInteractable(spellsButtons, false);

			canUpdate = true;

			// Remove Listeners
			Utilities.S.RemoveListeners(PlayerButtons.S.buttonsCS);
			// Add Listeners
			PlayerButtons.S.buttonsCS [0].onClick.AddListener (delegate {LoadSpells (0);});
			PlayerButtons.S.buttonsCS [1].onClick.AddListener (delegate {LoadSpells (1);});

				// Activate PlayerButtons
				ScreenManager.S.playerButtonsGO.SetActive (true);

			// Set Selected
			Utilities.S.SetSelectedGO (PlayerButtons.S.buttonsCS [0].gameObject);

			// Display Text
			PauseMessage.S.DisplayText ("Use whose spells?!");

			// Switch ScreenMode 
			spellScreenMode = eSpellScreenMode.pickWhichSpellsToDisplay;

			// Activate Cursor
			ScreenCursor.S.cursorGO.SetActive (true);
            }
            else
            {
				// Set Turn Cursor sorting layer BELOW UI
				BattleUI.S.turnCursorSRend.sortingLayerName = "0";
			}
		}catch(NullReferenceException){}
	}

	void OnDisable () {
		if (!RPG.S.paused) {
			// Deactivate Cursor
			ScreenCursor.S.cursorGO.SetActive (false);
		} else {
			// Activate Cursor
			ScreenCursor.S.cursorGO.SetActive (true);
		}

		// If used Warp Spell... otherwise can't Pause after Warping
		//isActive = false;

		// Set Turn Cursor sorting layer ABOVE UI
		BattleUI.S.turnCursorSRend.sortingLayerName = "Above UI";
	}

	public void Loop () {
		// Reset canUpdate
		if (Input.GetAxisRaw ("Horizontal") != 0f || Input.GetAxisRaw ("Vertical") != 0f) { 
			canUpdate = true;
		}

		// Deactivate SpellScreen
		if (RPG.S.currentSceneName != "Battle") {
			if (SpellsScreen.S.spellScreenMode == eSpellScreenMode.pickWhichSpellsToDisplay) {
				if (Input.GetButtonDown ("SNES B Button")) { 
					ScreenManager.S.SpellsScreenOff (); 
				}
			}
		} else {
			if (Input.GetButtonDown ("SNES B Button")) { 
				SpellsScreen.S.ScreenOffPlayerTurn ();
			}
		}

		switch (spellScreenMode) {
		case eSpellScreenMode.pickWhichSpellsToDisplay:
			if (canUpdate) {
				PlayerButtons.S.PositionCursor ();
		
				// Display each Member's Spells
				for (int i = 0; i < PlayerButtons.S.buttonsCS.Count; i++) {
					if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == PlayerButtons.S.buttonsCS [i].gameObject) {
						DeactivateUnusedSpellsSlots (i);
						DisplaySpellsDescriptions (i);
					}
				}
					
				canUpdate = false; 
			}
			break;
		case eSpellScreenMode.pickSpell:
			if (canUpdate) {
				DisplaySpellsDescriptions (playerNdx);
				canUpdate = false; 
			}

			if (PauseMessage.S.dialogueFinished) {
				if (Input.GetButtonDown ("SNES B Button")) {
					OnEnable(); // Go Back
				}
			}
			break;
		case eSpellScreenMode.doesntKnowSpells:
			if (PauseMessage.S.dialogueFinished) {
				if (Input.GetButtonDown ("SNES A Button")) {
					OnEnable();
				}
			}
			break;
		case eSpellScreenMode.pickWhichMemberToHeal:
			if (canUpdate) {
				PlayerButtons.S.PositionCursor ();
				canUpdate = false; 
			}

			if (PauseMessage.S.dialogueFinished) {
				if (Input.GetButtonDown ("SNES B Button")) {
					LoadSpells(playerNdx); // Go Back
				}
			}
			break;
		case eSpellScreenMode.usedSpell:
			if (PauseMessage.S.dialogueFinished) {
				if (Input.GetButtonDown ("SNES A Button")) {
					LoadSpells(playerNdx);
				}
			}
			break;
		// During Battle... "Not Enough MP Message", then back to Player Turn w/ Button Press
		case eSpellScreenMode.cantUseSpell:
			if (PauseMessage.S.dialogueFinished) {
				if (Input.GetButtonDown ("SNES A Button")) {
					ScreenOffPlayerTurn ();
				}
			}
			break;
		}
	}
	
	public void LoadSpells(int ndx){
		// Buttons Interactable
		Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, false);
		Utilities.S.ButtonsInteractable(spellsButtons, true);

		playerNdx = ndx; // Now used to DisplaySpellsDescriptions

		if (RPG.S.currentSceneName == "Battle") {
			// Activate Spell Screen
			SpellsScreen.S.gameObject.SetActive (true);
		}

		DeactivateUnusedSpellsSlots (ndx);
		AssignSpellsEffect (ndx);
		DisplaySpellsDescriptions (ndx);

		// Empty Inventory
		if (Stats.S.spellNdx [ndx] == 0) {
			PauseMessage.S.DisplayText(Stats.S.playerName[ndx] + " knows no spells, fool!");

			canUpdate = true;
			// Switch ScreenMode
			spellScreenMode = eSpellScreenMode.doesntKnowSpells;

			// Deactivate Cursor
			ScreenCursor.S.cursorGO.SetActive (false);
		} else {
			canUpdate = true;
			// Switch ScreenMode 
			spellScreenMode = eSpellScreenMode.pickSpell;

			// Set Selected GameObject (Spells Screen: Spells Slot 1)
			Utilities.S.SetSelectedGO(spellsButtons[0].gameObject);

			// Activate Cursor
			ScreenCursor.S.cursorGO.SetActive (true);
		}
	}

	void DisplaySpellsDescriptions (int ndx) {
		for (int i = 0; i <= spellsButtons.Count - 1; i++) {
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == spellsButtons [i].gameObject) {
				switch (ndx){
				case 0: // Player 1
					switch (i) {
					case 0: PauseMessage.S.SetText("Replenishes at least 30 HP! What a heal spell!" + "\n Cost: 3 MP"); break;
					case 1: PauseMessage.S.SetText("Blasts Enemy for at least 8 HP! What a damage spell!" + "\n Cost: 2 MP"); break;
					case 2: PauseMessage.S.SetText("Warp to a previously visited land!" + "\n Cost: 3 MP"); break;
					case 3: PauseMessage.S.SetText("Blasts ALL enemies for at least 12 HP! Hot damn!" + "\n Cost: 3 MP"); break;
					}
					break;
				case 1: // Player 2
					switch (i) {
					case 0: PauseMessage.S.SetText("Replenishes at least 30 HP! What a heal spell!" + "\n Cost: 3 MP"); break;
					case 1: PauseMessage.S.SetText("Blasts Enemy for at least 8 HP! What a damage spell!" + "\n Cost: 2 MP"); break;
					case 2: PauseMessage.S.SetText("Warp to a previously visited land!" + "\n Cost: 3 MP"); break;
					case 3: PauseMessage.S.SetText("Blasts ALL enemies for at least 12 HP! Hot damn!" + "\n Cost: 3 MP"); break;
					}
					break;
				}

				// Cursor Position set to Selected Button
				float tPosX = spellsButtons [i].gameObject.GetComponent<RectTransform> ().anchoredPosition.x;
				float tPosY = spellsButtons [i].gameObject.GetComponent<RectTransform> ().anchoredPosition.y;

				float tParentX = spellsButtons [i].gameObject.transform.parent.GetComponent<RectTransform> ().anchoredPosition.x;
				float tParentY = spellsButtons [i].gameObject.transform.parent.GetComponent<RectTransform> ().anchoredPosition.y;

				ScreenCursor.S.rectTrans.anchoredPosition = new Vector2 ((tPosX + tParentX + 160), (tPosY + tParentY));
			}
		}
	}

	void DeactivateUnusedSpellsSlots (int ndx) {
		for (int i = 0; i <= spellsButtons.Count - 1; i++) {
			if (i < Stats.S.spellNdx [ndx]) {
				spellsButtons [i].gameObject.SetActive (true);
			} else {
				spellsButtons [i].gameObject.SetActive (false);
			} 
		}
	}

	void AssignSpellsEffect(int ndx) { // Would like to use a loop, but the Delegate's parameters == Error
		Utilities.S.RemoveListeners(PlayerButtons.S.buttonsCS);
		Utilities.S.RemoveListeners(spellsButtons);

		switch (ndx) {
		case 0: // Player 1
			spellsButtons [0].onClick.AddListener (delegate { HealSpell (); });
			spellsButtons [1].onClick.AddListener (delegate { FireballSpell (); });
			spellsButtons [2].onClick.AddListener (delegate { WarpSpell (); });
			spellsButtons [3].onClick.AddListener (delegate { FireblastSpell (); });

			// Assign Button Name Text
			spellsButtonNameTexts [0].text = "Heal";
			spellsButtonNameTexts [1].text = "Fireball";
			spellsButtonNameTexts [2].text = "Warp";
			spellsButtonNameTexts [3].text = "Fireblast";
			break;
		case 1: // Player 2
			spellsButtons [0].onClick.AddListener (delegate { HealSpell (); });
			spellsButtons [1].onClick.AddListener (delegate { FireballSpell (); });
			spellsButtons [2].onClick.AddListener (delegate { WarpSpell(); });
			spellsButtons [3].onClick.AddListener (delegate { FireblastSpell (); });

			// Assign Button Name Text
			spellsButtonNameTexts [0].text = "Heal";
			spellsButtonNameTexts [1].text = "Fireball";
			spellsButtonNameTexts [2].text = "Warp";
			spellsButtonNameTexts [3].text = "Fireblast";
			break;
		}
	}
	
	// Overworld & Battle Spells 
	public void HealSpell (){

		// if Battle
		if (RPG.S.currentSceneName == "Battle") {
			if (Stats.S.MP [Battle.S.PlayerNdx()] >= 3) {	
				ScreenManager.S.SpellsScreenOff ();
				BattleSpells.S.ClickedHealSpellButton ();
			} else {
				CantUseSpell ("Not enough MP to cast this spell!");
			}
			// if Overworld
		} else { ClickedHealSpellButton (); }
	}

	public void ClickedHealSpellButton(){ // Overworld
		if (Stats.S.MP [playerNdx] >= 3) {

			// Buttons Interactable
			Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, true);
			Utilities.S.ButtonsInteractable(spellsButtons, false);
			// Set Selected GameObject
			Utilities.S.SetSelectedGO (PlayerButtons.S.buttonsCS [0].gameObject);

			// Add Listeners
			PlayerButtons.S.buttonsCS [0].onClick.AddListener (delegate {HealSpell (0);});
			PlayerButtons.S.buttonsCS [1].onClick.AddListener (delegate {HealSpell (1);});

			// Display Text
			PauseMessage.S.DisplayText ("Heal which party member?");

			canUpdate = true;
			// Switch ScreenMode
			spellScreenMode = eSpellScreenMode.pickWhichMemberToHeal;
		} else {
			PauseMessage.S.DisplayText("Not enough MP to cast this spell!");
			SpellHelper ();
		}
	}
	public void HealSpell(int ndx){ // Overworld
		if (Stats.S.HP [ndx] < Stats.S.maxHP [ndx]) {
			// Subtract Spell cost from CASTING Player's MP 
			RPG.S.SubtractPlayerMP (playerNdx, 3);

			// Add 30-45 HP to TARGET Player's HP
			int randomValue = UnityEngine.Random.Range(30, 45);
			RPG.S.AddPlayerHP (ndx, randomValue);

			// Display Text
			if (Stats.S.HP[ndx] >= Stats.S.maxHP[ndx]) {
				PauseMessage.S.DisplayText("Used Heal Spell!\nHealed " + Stats.S.playerName [ndx] + " back to Max HP!");
			} else {
				PauseMessage.S.DisplayText("Used Heal Spell!\nHealed " + Stats.S.playerName [ndx] + " for " + randomValue + " HP!");
			}
		} else {
			// Display Text
			PauseMessage.S.DisplayText(Stats.S.playerName [ndx] + " already at full health...\n...no need to cast this spell!");
		}
		SpellHelper ();
	}
		
	public void SpellHelper(){
		// Buttons Interactable
		Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, false);
		Utilities.S.ButtonsInteractable(spellsButtons, false);

		// Update GUI
		PlayerButtons.S.UpdateGUI ();
		PauseScreen.S.UpdateGUI ();

		// Deactivate Cursor
		ScreenCursor.S.cursorGO.SetActive (false);

		canUpdate = true;
		// Switch ScreenMode 
		spellScreenMode = eSpellScreenMode.usedSpell;
	}

	// Battle ONLY Spells 
	public void FireballSpell (){
		// if Battle
		if (RPG.S.currentSceneName == "Battle") {
			if (Stats.S.MP [Battle.S.PlayerNdx()] >= 2) {	
				ScreenManager.S.SpellsScreenOff ();
				BattleSpells.S.ClickedFireSpellButton ();
			} else {
				CantUseSpell ("Not enough MP to cast this spell!");
			}
			// if Overworld
		} else { 
			PauseMessage.S.DisplayText("You ain't battlin' no one, so ya can't use dis spell!"); 

			SpellHelper ();
		}
	}

	public void FireblastSpell (){
		// if Battle
		if (RPG.S.currentSceneName == "Battle") {
			if (Stats.S.MP [Battle.S.PlayerNdx()] >= 3) {	
				ScreenManager.S.SpellsScreenOff ();
				BattleSpells.S.FireblastSpell ();
			} else {
				CantUseSpell ("Not enough MP to cast this spell!");
			}
			// if Overworld
		} else { 
			PauseMessage.S.DisplayText("You ain't battlin' no one, so ya can't use dis spell!"); 
		
			SpellHelper ();
		}
	}

	// Overworld ONLY Spells
	public void WarpSpell (){
		// if Battle
		if (RPG.S.currentSceneName == "Battle") {
			CantUseSpell ("Can't warp during battle!");
			// if Overworld
		} else {
			if (Stats.S.MP [0] >= 3) {	


				if (UnityEngine.Random.value > 0.5f) {
					StartCoroutine (WarpManager.S.Warp (Vector3.zero, true, "zCave"));
				} else {
					StartCoroutine (WarpManager.S.Warp (new Vector3(0,-1,0), true, "_AnCross"));
				}
			

			} else {
				CantUseSpell ("Not enough MP to cast this spell!");
			}
		}
	}
	
	public void CantUseSpell (string message) {
		PauseMessage.S.DisplayText(message);
	
		// if Battle
		if (RPG.S.currentSceneName == "Battle") {
			Utilities.S.RemoveListeners(spellsButtons);

			canUpdate = true;

			// Switch ScreenMode 
			spellScreenMode = eSpellScreenMode.cantUseSpell;
		}
	}

	public void ScreenOffPlayerTurn (){
		PauseMessage.S.gameObject.SetActive (false);

		ScreenManager.S.SpellsScreenOff ();
		if (Battle.S.battleMode == eBattleMode.itemOrSpellMenu) {
			Battle.S.PlayerTurn();
		}
	}

	public void ScreenOffEnemyDeath (int enemyNdx){
		ScreenManager.S.SpellsScreenOff ();

		BattleEnd.S.EnemyDeath (enemyNdx); 
	}
	
	// Buttons Interactable
	//public void ButtonsInteractable(bool isActive){
	//	for (int i = 0; i <= spellsButtons.Count - 1; i++) {
	//		spellsButtons [i].interactable = isActive;
	//	}
	//}
}
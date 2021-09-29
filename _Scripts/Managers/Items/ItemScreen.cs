using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public enum eItemScreenMode { pickItem, pickPartyMember, pickAllPartyMembers, usedItem, pickWhereToWarp }; 

public class ItemScreen : MonoBehaviour { 
	[Header("Set in Inspector")]
	// Item "Buttons"
	public List<Button> 	itemButtons;
	public List<Text> 	  	itemButtonsText;

	public Button			sortButton;

	[Header("Set Dynamically")]
	// Singleton
	private static ItemScreen _S;
	public static ItemScreen S { get { return _S; } set { _S = value; } }

	// For Input & Display Message
	public eItemScreenMode  mode;

	// Allows parts of Loop() to be called once rather than repeatedly every frame.
	public bool 			canUpdate;

	public GameObject		previousSelectedGameObject;

	void Awake() {
		S = this;
	}

	public void OnEnable () {
		// Ensures first slot is selected when screen enabled
		previousSelectedGameObject = itemButtons[0].gameObject;

		ItemScreen_PickItemMode.S.Setup(S);

		// Add Loop() to Update Delgate
		UpdateManager.updateDelegate += Loop;
	}

	public void Deactivate () {
		// Deactivate Cursors if in Battle Mode
		if (!RPG.S.paused) {
			Utilities.S.SetActiveList(ScreenCursor.S.cursorGO, false);
		}

		// Set Battle Turn Cursor sorting layer ABOVE UI
		BattleUI.S.turnCursorSRend.sortingLayerName = "Above UI";

		// Remove Listeners
		Utilities.S.RemoveListeners(itemButtons);

		if (RPG.S.currentSceneName != "Battle") {
			// Buttons Interactable
			Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, true);
			// Set Selected Gameobject (Pause Screen: Items Button)
			Utilities.S.SetSelectedGO(PauseScreen.S.buttonGO[0]);

			PauseMessage.S.DisplayText("Welcome to the Pause Screen!");

			PauseScreen.S.canUpdate = true;
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
		
		// Deactivate ItemScreen during Battle
		if (RPG.S.currentSceneName == "Battle") {
			if (Input.GetButtonDown ("SNES B Button")) {
				PauseMessage.S.gameObject.SetActive(false);
				Deactivate();
				Battle.S.PlayerTurn();
			}
		}

		switch (mode) {
			case eItemScreenMode.pickItem:
				ItemScreen_PickItemMode.S.Loop(S);
			break;
			case eItemScreenMode.pickPartyMember:
				ItemScreen_PickPartyMemberMode.S.Loop(S);
			break;
			case eItemScreenMode.pickAllPartyMembers:
				if (Input.GetButtonDown("SNES B Button")) {
					GoBackToPickItemMode();
				}
			break;
			case eItemScreenMode.pickWhereToWarp:
				if (canUpdate) {
					WarpManager.S.DisplayButtonDescriptions(itemButtons, -170);
				}

				if (Input.GetButtonDown("SNES B Button")) {
					GoBackToPickItemMode();
				}
				break;
			case eItemScreenMode.usedItem:
				ItemScreen_UsedItemMode.S.Loop(S);
			break;
		}

		// TEST: Sort Inventory
		if (Input.GetKeyDown (KeyCode.X)) {
			//_items = SortItems.S.SortByABC(_items);
			Inventory.S.items = SortItems.S.SortByValue(Inventory.S.items);
		}
	}

	void GoBackToPickItemMode() {
		if (PauseMessage.S.dialogueFinished) {
			// Set animations to idle
			PlayerButtons.S.SetSelectedAnim("Idle");

			// Reset button colors
			PlayerButtons.S.SetButtonsColor(PlayerButtons.S.buttonsCS, new Color32(255, 255, 255, 200));

			// Deactivate screen cursors
			Utilities.S.SetActiveList(ScreenCursor.S.cursorGO, false);

			// Go back to PickItem mode
			ItemScreen_PickItemMode.S.Setup(S);
		}
	}

	public void AssignItemEffect() { 
		Utilities.S.RemoveListeners(itemButtons);

		for (int i = 0; i < 30; i++) {
			int copy = i;
			itemButtons[copy].onClick.AddListener(delegate { ConsumeItem(Inventory.S.GetItemList()[copy]); });
		}
	}

	public void AssignItemNames () {
		for (int i = 0; i < Inventory.S.GetItemList().Count; i++) {
			itemButtonsText[i].text = Inventory.S.GetItemList()[i].name + "(" + Inventory.S.GetItemCount (Inventory.S.GetItemList()[i]) + ")";
		}
	}

	public void ConsumeItem(Item tItem){
		// Check if the item is in inventory
		if (Inventory.S.items.ContainsKey (tItem)) {
			canUpdate = true;

			if (RPG.S.currentSceneName == "Battle") { // if Battle
				if (tItem.name == "Health Potion") {
					BattleItems.S.AddFunctionToButton(BattleItems.S.HPPotion, "Use potion on which party member?", tItem);
				} else if (tItem.name == "Magic Potion") {
					BattleItems.S.AddFunctionToButton(BattleItems.S.MPPotion, "Use potion on which party member?", tItem);
				} else if (tItem.name == "Heal All Potion") {
					BattleItems.S.AddFunctionToButton(BattleItems.S.HealAllPotion, "Use potion to heal all party members?", tItem);
				} else {
					BattleItems.S.CantUseItem();
				}
			} else { // if Overworld
				if (tItem.name == "Health Potion") {
					WorldItems.S.AddFunctionToButton(WorldItems.S.HPPotion, "Heal which party member?", tItem);
				} else if (tItem.name == "Magic Potion") {
					WorldItems.S.AddFunctionToButton(WorldItems.S.MPPotion, "Use MP potion on which party member?", tItem);
				} else if (tItem.name == "Heal All Potion") {
					WorldItems.S.AddFunctionToButton(WorldItems.S.HealAllPotion, "Use potion to heal all party members?", tItem);
				} else if (tItem.name == "Warp Potion") {
					WorldItems.S.WarpPotion();
				} else {
					WorldItems.S.CantUseItem();
				}
			}
		}
	}
}
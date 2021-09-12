using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public enum eItemScreenMode { pickItem, pickPartyMember, usedItem }; 

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
	public eItemScreenMode  itemScreenMode;

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
		// Deactivate Cursor if in Battle or Sell Mode
		if (!RPG.S.paused) {
			ScreenCursor.S.cursorGO.SetActive(false);
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

		switch (itemScreenMode) {
			case eItemScreenMode.pickItem:
				ItemScreen_PickItemMode.S.Loop(S);
				break;
			case eItemScreenMode.pickPartyMember:
				ItemScreen_PickPartyMemberMode.S.Loop(S);
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

			switch (tItem.name) {
			case "Health Potion":
				// Switch ScreenMode
				itemScreenMode = eItemScreenMode.pickPartyMember;

				if (RPG.S.currentSceneName == "Battle") { // if Battle
					BattleItems.S.AddFunctionToButton(BattleItems.S.HPPotion, "Use potion on which party member?");
				} else { // if Overworld
					WorldItems.S.AddFunctionToButton(WorldItems.S.HPPotion, "Heal which party member?");
				}
			break;
			case "Magic Potion":
				// Switch ScreenMode
				itemScreenMode = eItemScreenMode.pickPartyMember;

				if (RPG.S.currentSceneName == "Battle") { // if Battle
					BattleItems.S.AddFunctionToButton(BattleItems.S.MPPotion, "Use potion on which party member?");
				} else { // if Overworld
					WorldItems.S.AddFunctionToButton(WorldItems.S.MPPotion, "Use MP potion on which party member?");
				}
			break;
			default: // Items that can't be used...
				if (RPG.S.currentSceneName == "Battle") { // if Battle
					BattleItems.S.CantUseItem();
				} else { // if Overworld
					WorldItems.S.CantUseItem();
				}
				break;
			}
		}
	}
}
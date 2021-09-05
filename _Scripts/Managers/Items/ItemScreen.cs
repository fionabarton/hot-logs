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
	public eItemScreenMode  itemScreenMode = eItemScreenMode.pickItem;

	// Allows parts of Loop() to be called once rather than repeatedly every frame.
	public bool 			canUpdate;

	void Awake() {
		S = this;
	}

	void OnEnable () {
		itemScreenMode = eItemScreenMode.pickItem;

		DeactivateUnusedItemSlots ();
		AssignItemNames (); 
		AssignItemEffect ();

		// Buttons Interactable
		Utilities.S.ButtonsInteractable(itemButtons, true);

		// Add Loop() to Update Delgate
		UpdateManager.updateDelegate += Loop;

		canUpdate = true;
		
		try {
			// Activate PlayerButtons
			PlayerButtons.S.gameObject.SetActive(true);
			Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, false);
			Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, false);

			// If Inventory Empty 
			if (Inventory.S.GetItemList().Count == 0) {
				PauseMessage.S.DisplayText("You have no items, fool!");

				// Deactivate Cursor
				ScreenCursor.S.cursorGO.SetActive (false);
			} else {
				// Set Selected GameObject (Item Screen: Item Slot 1)
				Utilities.S.SetSelectedGO(itemButtons[0].gameObject);

				// Activate Cursor
				ScreenCursor.S.cursorGO.SetActive (true);
			}

			// Set Battle Turn Cursor sorting layer BELOW UI
			BattleUI.S.turnCursorSRend.sortingLayerName = "0";
		}
		catch(NullReferenceException){}
			
		// Remove Listeners
		sortButton.onClick.RemoveAllListeners();
		// Assign Listener (Sort Button)
		sortButton.onClick.AddListener (delegate { Inventory.S.items = SortItems.S.SortByABC (Inventory.S.items);});
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
				if (Inventory.S.GetItemList().Count > 0) {
					if (canUpdate) {
						DisplayItemDescriptions();
						canUpdate = false;
					}
				}

				if (RPG.S.currentSceneName != "Battle") {
					if (Input.GetButtonDown("SNES B Button")) {
						Deactivate();
					}
				}

				break;
			case eItemScreenMode.pickPartyMember:
				if (canUpdate) {
					Utilities.S.PositionCursor(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject, 0, 60, 3);

					// Set animation to walk
					PlayerButtons.S.SetAnim("Walk");

					canUpdate = false;
				}
		
				if (PauseMessage.S.dialogueFinished) {
					if (Input.GetButtonDown("SNES B Button")) {
                        // Set animation to idle
                        PlayerButtons.S.SetAnim("Idle");

                        OnEnable(); // Go Back
					}
				}
				break;
			case eItemScreenMode.usedItem:
				if (PauseMessage.S.dialogueFinished) {
					if (Input.GetButtonDown("SNES A Button")) {
                        // Set animation to idle
                        PlayerButtons.S.SetAnim("Idle");

                        OnEnable();
					}
				}
				break;
		}

		// TEST: Sort Inventory
		if (Input.GetKeyDown (KeyCode.X)) {
			//_items = SortItems.S.SortByABC(_items);
			Inventory.S.items = SortItems.S.SortByValue(Inventory.S.items);
		}
	}

	void DeactivateUnusedItemSlots () {
		for (int i = 0; i <= itemButtons.Count - 1; i++) {
			if (i < Inventory.S.GetItemList().Count) {
				itemButtons [i].gameObject.SetActive (true);
			} else {
				itemButtons [i].gameObject.SetActive (false);
			} 
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

	public void DisplayItemDescriptions () {
		for (int i = 0; i < Inventory.S.GetItemList().Count; i++) {
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == itemButtons [i].gameObject) {
				PauseMessage.S.SetText (Inventory.S.GetItemList()[i].description);

				// Set Cursor Position set to Selected Button
				Utilities.S.PositionCursor(itemButtons[i].gameObject, -170, 0, 0);

				// Set selected button text color	
				itemButtons[i].gameObject.GetComponentInChildren<Text>().color = new Color32(205, 208, 0, 255);
			} else {
				// Set non-selected button text color
				itemButtons[i].gameObject.GetComponentInChildren<Text>().color = new Color32(255, 255, 255, 255);
			}
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
					OverworldItems.S.AddFunctionToButton(OverworldItems.S.HPPotion, "Heal which party member?");
				}
			break;
			case "Magic Potion":
				// Switch ScreenMode
				itemScreenMode = eItemScreenMode.pickPartyMember;

				if (RPG.S.currentSceneName == "Battle") { // if Battle
					BattleItems.S.AddFunctionToButton(BattleItems.S.MPPotion, "Use potion on which party member?");
				} else { // if Overworld
					OverworldItems.S.AddFunctionToButton(OverworldItems.S.MPPotion, "Use MP potion on which party member?");
				}
			break;
			default: // Items that can't be used...
				if (RPG.S.currentSceneName == "Battle") { // if Battle
					BattleItems.S.CantUseItem();
				} else { // if Overworld
					OverworldItems.S.CantUseItem();
				}
				break;
			}
		}
	}
}
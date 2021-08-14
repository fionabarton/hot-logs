using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public enum eUseOrSellMode { useMode, sellMode };

public enum eItemScreenMode { pickItem, pickPartyMember, usedItem, soldItem };

public class ItemScreen : MonoBehaviour { // hpPotion, mpPotion, key, fullHeal/Magic, partyFullHeal/Magic
	[Header("Set in Inspector")]
	// Item "Buttons"
	public List<Button> 	itemButtons;
	public List<Text> 	  	itemButtonNameTexts;

	public Button			sortButton;

	[Header("Set Dynamically")]
	// Singleton
	private static ItemScreen _S;
	public static ItemScreen S { get { return _S; } set { _S = value; } }

	// Sell Items
	public eUseOrSellMode	useOrSellMode = eUseOrSellMode.useMode; 

	// For Input & Display Message
	public eItemScreenMode  itemScreenMode = eItemScreenMode.pickItem;

	public bool 			canUpdate;

	void Awake() {
		// Singleton
		S = this;
	}

	void OnEnable () {
		itemScreenMode = eItemScreenMode.pickItem;

		DeactivateUnusedItemSlots ();
		AssignItemNames (); 
		AssignItemEffect ();

		// Buttons Interactable
		Utilities.S.ButtonsInteractable(itemButtons, true);

		canUpdate = true;

		try{
			// Activate PlayerButtons
			ScreenManager.S.playerButtonsGO.SetActive(true);
			Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, false);

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

		// SELL Mode
		if (useOrSellMode == eUseOrSellMode.sellMode) {
			// Activate PauseScreen
			PauseMessage.S.gameObject.SetActive (true);
		}

        // Set Battle Turn Cursor sorting layer BELOW UI
		//Battle.S.turnCursorSRend.sortingLayerName = "0";
	}

	void OnDisable () {
		// Deactivate Cursor if in Battle or Sell Mode
		if (!RPG.S.paused) {
			ScreenCursor.S.cursorGO.SetActive(false);
		}

        // SELL Mode
        if (useOrSellMode == eUseOrSellMode.sellMode) {
			// Deactivate PauseScreen
			PauseMessage.S.gameObject.SetActive (false);

			// Unfreeze Player
			Player.S.canMove = true;
		}

		// Reset useOrSellMode 
		useOrSellMode = eUseOrSellMode.useMode;

		// Set Battle Turn Cursor sorting layer ABOVE UI
		BattleUI.S.turnCursorSRend.sortingLayerName = "Above UI";
	}

	public void Loop () {
		// Reset canUpdate
		if (Input.GetAxisRaw ("Horizontal") != 0f || Input.GetAxisRaw ("Vertical") != 0f) { 
			canUpdate = true;
		}

		// Deactivate ItemScreen in Overworld and Battle
		if (RPG.S.currentSceneName != "Battle") { 
			if (itemScreenMode == eItemScreenMode.pickItem) {
				if (Input.GetButtonDown ("SNES B Button")) { 
					ScreenManager.S.ItemScreenOff (); 
				}
			}
		} else {
			if (Input.GetButtonDown ("SNES B Button")) {
				PauseMessage.S.gameObject.SetActive(false);
				ScreenManager.S.ItemScreenOff();
				if(Battle.S.battleMode == eBattleMode.itemOrSpellMenu) {
					Debug.Log("Called in ItemScreen");
					Battle.S.PlayerTurn();
				}
			}
		}

		switch (useOrSellMode) { 
		// Use Mode Input
		case eUseOrSellMode.useMode: 
			switch (itemScreenMode) {
			case eItemScreenMode.pickItem:
				if (Inventory.S.GetItemList ().Count > 0) {
					if (canUpdate) {
						DisplayItemDescriptions ();
						canUpdate = false; 
					}
				}

			break;
			case eItemScreenMode.pickPartyMember:
				if (canUpdate) {
					PlayerButtons.S.PositionCursor ();
					canUpdate = false; 
				}

				if (PauseMessage.S.dialogueFinished) {
					if (Input.GetButtonDown ("SNES B Button")) {
						OnEnable (); // Go Back
					}
				}
			break;
			case eItemScreenMode.usedItem:
				if (PauseMessage.S.dialogueFinished) {
					if (Input.GetButtonDown ("SNES A Button")) {
						OnEnable ();
					}
				}
			break;
			}
		break;

		// Sell Mode Input
		case eUseOrSellMode.sellMode: 
			switch (itemScreenMode) {
			case eItemScreenMode.pickItem:
				if (Inventory.S.GetItemList ().Count > 0) {
					if (canUpdate) {
						DisplayItemDescriptions ();
						canUpdate = false; 
					}
				}
			break;
			case eItemScreenMode.soldItem:
				if (PauseMessage.S.dialogueFinished) {
					if (Input.GetButtonDown ("SNES A Button")) {
						OnEnable ();
					}
				}
			break;
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

		switch (useOrSellMode) {
		case eUseOrSellMode.useMode:
			for(int i = 0; i < 30; i++) {
				int copy = i;
				itemButtons[copy].onClick.AddListener(delegate { ConsumeItem(Inventory.S.GetItemList()[copy]); });
			}
		break;
		case eUseOrSellMode.sellMode:
			for (int i = 0; i < 30; i++) {
				int copy = i;
				itemButtons[copy].onClick.AddListener(delegate { SellItem(Inventory.S.GetItemList()[copy]); });
			}
		break;
		}
	}

	public void AssignItemNames () {
		for (int i = 0; i <= Inventory.S.GetItemList().Count - 1; i++) {
			itemButtonNameTexts [i].text = Inventory.S.GetItemList()[i].name + "(" + Inventory.S.GetItemCount (Inventory.S.GetItemList()[i]) + ")";
		}
	}

	public void DisplayItemDescriptions () {
		for (int i = 0; i <= Inventory.S.GetItemList().Count - 1; i++) {
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == itemButtons [i].gameObject) {
				PauseMessage.S.SetText (Inventory.S.GetItemList()[i].description);

				// Set Cursor Position set to Selected Button
				float tPosX = itemButtons [i].gameObject.GetComponent<RectTransform> ().anchoredPosition.x;
				float tPosY = itemButtons [i].gameObject.GetComponent<RectTransform> ().anchoredPosition.y;
				float tParentX = itemButtons [i].gameObject.transform.parent.GetComponent<RectTransform> ().anchoredPosition.x;
				float tParentY = itemButtons [i].gameObject.transform.parent.GetComponent<RectTransform> ().anchoredPosition.y;
				ScreenCursor.S.rectTrans.anchoredPosition = new Vector2 ((tPosX + tParentX + 160), (tPosY + tParentY));
			}
		}
	}

	// Sell an item in the party's inventory
	void SellItem (Item tItem) {
		// Buttons Interactable
		Utilities.S.ButtonsInteractable(itemButtons, false);

		canUpdate = true;

		itemScreenMode = eItemScreenMode.soldItem;

		// Dialogue
		PauseMessage.S.DisplayText("Sold!" + " For " + tItem.value + " gold!" + " Cha - CHING!");

		// Add item price to Player's Gold
		Stats.S.Gold += tItem.value;

		// Update Gold 
		PlayerButtons.S.goldValue.text = Stats.S.Gold.ToString();

		// Deactivate Cursor, Button, & Remove Item from Inventory, & Update GUI
		Inventory.S.RemoveItemFromInventory(tItem);
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
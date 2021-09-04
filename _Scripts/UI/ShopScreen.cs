using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public enum eShopScreenMode { pickItem, selectQuantity, itemPurchased, endOfTransaction };

public class ShopScreen : MonoBehaviour {
	[Header("Set in Inspector")]
	// Inventory Buttons
	public List<Button> 	inventoryButtons;
	public List<Text> 		inventoryButtonsText;

	[Header("Set Dynamically")]
	// Singleton
	private static ShopScreen _S;
	public static ShopScreen S { get { return _S; } set { _S = value; } }

	public List<Item> 		inventory = new List<Item>();

	// For Input & Display Message
	public eShopScreenMode  shopScreenMode = eShopScreenMode.pickItem;

	public bool				buyOrSellMode;

	// Allows parts of Loop() to be called once rather than repeatedly every frame.
	public bool 			canUpdate;

	void Awake() {
		S = this;
	}

	void OnEnable () {
		// Set ScreenMode
		shopScreenMode = eShopScreenMode.pickItem;

		// Reimport inventory if an item was sold
        if (!buyOrSellMode) {
			ImportInventory(Inventory.S.GetItemList());
		}

		DeactivateUnusedItemSlots ();
		AssignItemNames ();
		AssignItemEffect();

		canUpdate = true;

		// Set Selected GameObject (Save Screen: Shop Slot 1)
		Utilities.S.SetSelectedGO(inventoryButtons[0].gameObject);

		// Freeze Player
		RPG.S.paused = true;
		Player.S.mode = eRPGMode.idle;

		// Add Loop() to Update Delgate
		UpdateManager.updateDelegate += Loop;

		try {
			// Activate PlayerButtons
			PlayerButtons.S.gameObject.SetActive(true);
			Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, false);

			// If Inventory Empty... 
			if (Inventory.S.GetItemList().Count == 0) {
				PauseMessage.S.DisplayText("You have nothing to sell, fool!");

				// Deactivate Cursor
				ScreenCursor.S.cursorGO.SetActive(false);
			} else {
				// Set Selected GameObject (Shop Screen: Item Slot 1)
				Utilities.S.SetSelectedGO(inventoryButtons[0].gameObject);

				// Activate Cursor
				ScreenCursor.S.cursorGO.SetActive(true);
			}

			// Activate PauseScreen
			PauseMessage.S.gameObject.SetActive (true);
		}catch(NullReferenceException){}
	}

	void OnDisable () {
		// Deactivate Cursor
		ScreenCursor.S.cursorGO.SetActive (false);

		// Unpause
		RPG.S.paused = false;

		// Deactivate PauseMessage and PlayerButtons
		PauseMessage.S.gameObject.SetActive(false);
		PlayerButtons.S.gameObject.SetActive(false);

		// Remove Loop() from Update Delgate
		UpdateManager.updateDelegate -= Loop;

		// Set Camera to Player gameObject
		CamManager.S.ChangeTarget(Player.S.gameObject, true);

		// Broadcast event
		EventManager.ShopScreenDeactivated();
	}

	public void Loop () {
		// Reset canUpdate
		if (Input.GetAxisRaw ("Horizontal") != 0f || Input.GetAxisRaw ("Vertical") != 0f) { 
			canUpdate = true;
		}

		// Deactivate EquipScreen
		if(Input.GetButtonDown ("SNES B Button")) {
			gameObject.SetActive(false);
		}

		switch (shopScreenMode) {
		case eShopScreenMode.pickItem:
			if (canUpdate) {
				DisplayItemDescriptions ();
				canUpdate = false; 
			}
			break;
		case eShopScreenMode.selectQuantity:
			break;
		case eShopScreenMode.itemPurchased:
			if (PauseMessage.S.dialogueFinished) {
				if (Input.GetButtonDown ("SNES A Button")) {
					OnEnable();
				}
			}
			break;
		case eShopScreenMode.endOfTransaction:
			break;
		}
	}

	void DeactivateUnusedItemSlots () {
		for (int i = 0; i < inventoryButtons.Count; i++) {
			if (i < inventory.Count) {
				inventoryButtons [i].gameObject.SetActive (true);
			} else {
				inventoryButtons [i].gameObject.SetActive (false);
			} 
		}
	}

	public void AssignItemEffect() {
		for (int i = 0; i < inventoryButtons.Count; i++) {
			int copy = i;
			inventoryButtons[i].onClick.RemoveAllListeners();

			if (buyOrSellMode) {
				inventoryButtons[copy].onClick.AddListener(delegate { PurchaseItem(inventory[copy]); });
			} else {
				inventoryButtons[copy].onClick.AddListener(delegate { SellItem(inventory[copy]); });
			}
		}
	}

	void AssignItemNames () {
		for (int i = 0; i <= inventory.Count - 1; i++) {
			inventoryButtonsText[i].text = inventory[i].name;
		}
	}

	public void DisplayItemDescriptions () {
		for (int i = 0; i < inventory.Count; i++) {
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == inventoryButtons[i].gameObject) {
				PauseMessage.S.SetText(inventory[i].description);

				// Cursor Position set to Selected Button
				Utilities.S.PositionCursor(inventoryButtons[i].gameObject, 160);
			}
		}
	}

	void PurchaseItem (Item item) {
		canUpdate = true;

		// Switch ScreenMode 
		shopScreenMode = eShopScreenMode.itemPurchased;

		if (PartyStats.S.Gold >= item.value) {
			// Added to Player Inventory
			Inventory.S.AddItemToInventory(item);

			// Dialogue
			PauseMessage.S.DisplayText("Purchased!" + " For " + item.value + " gold!");

			// Subtract item price from Player's Gold
			PartyStats.S.Gold -= item.value;

			// Update Gold 
			PlayerButtons.S.goldValue.text = PartyStats.S.Gold.ToString();
		} else {
			// Dialogue
			PauseMessage.S.DisplayText("Not enough money!");
		}

		// Remove Listeners
		Utilities.S.RemoveListeners(inventoryButtons);

		// Deactivate Cursor
		ScreenCursor.S.cursorGO.SetActive (false);
	}

    void SellItem(Item item) {
		canUpdate = true;

		// Switch ScreenMode 
		shopScreenMode = eShopScreenMode.itemPurchased;

		//Remove item from Inventory
		Inventory.S.RemoveItemFromInventory(item);

		// Dialogue
		PauseMessage.S.DisplayText("Sold!" + " For " + item.value + " gold!" + " Cha - CHING!");

		// Subtract item price from Player's Gold
		PartyStats.S.Gold += item.value;

		// Update Gold 
		PlayerButtons.S.goldValue.text = PartyStats.S.Gold.ToString();

		// Remove Listeners
		Utilities.S.RemoveListeners(inventoryButtons);

		// Deactivate Cursor
		ScreenCursor.S.cursorGO.SetActive(false);
    }

	// Import inventory from shopkeeper or party 
	public void ImportInventory(List<Item> inventoryToImport) {
		// Clear Inventory
		inventory.Clear();

		// Import Inventory
		for (int i = 0; i < inventoryToImport.Count; i++) {
			inventory.Add(inventoryToImport[i]);
		}
	}
}
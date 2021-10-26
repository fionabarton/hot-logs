using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public enum eShopScreenMode { pickItem, itemPurchasedOrSold };

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

	public GameObject		previousSelectedGameObject;
	public int				previousSelectedNdx;

	// Caches what index of the inventory is currently stored in the first item slot
	public int				firstSlotNdx;

	// Prevents instantly registering input when the first or last slot is selected
	private bool			verticalAxisIsInUse;
	private bool			firstOrLastSlotSelected;

	void Awake() {
		S = this;
	}

	void OnEnable () {
		// Ensures first slot is selected when screen enabled
		previousSelectedGameObject = inventoryButtons[0].gameObject;

		firstSlotNdx = 0;

		ShopScreen_PickItemMode.S.Setup(S);

		// Add Loop() to Update Delgate
		UpdateManager.updateDelegate += Loop;
	}

	public void Deactivate() {
		// Deactivate screen cursors
		Utilities.S.SetActiveList(ScreenCursor.S.cursorGO, false);

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

		// Deactivate this gameObject
		gameObject.SetActive(false);
	}

	public void Loop () {
		// Reset canUpdate
		if (Input.GetAxisRaw ("Horizontal") != 0f || Input.GetAxisRaw ("Vertical") != 0f) { 
			canUpdate = true;
		}

		// Deactivate ShopScreen
		if(Input.GetButtonDown ("SNES B Button")) {
			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);
			Deactivate();
		}

		switch (shopScreenMode) {
		case eShopScreenMode.pickItem:
			// On vertical input, scroll the item list when the first or last slot is selected
			ScrollItemList();

			ShopScreen_PickItemMode.S.Loop(S);
			break;
		case eShopScreenMode.itemPurchasedOrSold:
			ShopScreen_ItemPurchasedOrSoldMode.S.Loop(S);
			break;
		}
	}

	// On vertical input, scroll the item list when the first or last slot is selected
	void ScrollItemList() {
		if (inventory.Count > 1) {
			// If first or last slot selected...
			if (firstOrLastSlotSelected) {
				if (Input.GetAxisRaw("Vertical") == 0) {
					verticalAxisIsInUse = false;
				} else {
					if (!verticalAxisIsInUse) {
						if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == inventoryButtons[0].gameObject) {
							if (Input.GetAxisRaw("Vertical") > 0) {
								if (firstSlotNdx == 0) {
									firstSlotNdx = inventory.Count - inventoryButtons.Count;

									// Set  selected GameObject
									Utilities.S.SetSelectedGO(inventoryButtons[9].gameObject);
								} else {
									firstSlotNdx -= 1;
								}
							}
						} else if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == inventoryButtons[9].gameObject) {
							if (Input.GetAxisRaw("Vertical") < 0) {
								if (firstSlotNdx + inventoryButtons.Count == inventory.Count) {
									firstSlotNdx = 0;

									// Set  selected GameObject
									Utilities.S.SetSelectedGO(inventoryButtons[0].gameObject);
								} else {
									firstSlotNdx += 1;
								}
							}
						}

						ShopScreen_PickItemMode.S.AssignItemEffect(this);
						ShopScreen_PickItemMode.S.AssignItemNames(this);

						// Audio: Selection
						AudioManager.S.PlaySFX(eSoundName.selection);

						verticalAxisIsInUse = true;
					}
				}
			}

			// Check if first or last slot is selected
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == inventoryButtons[0].gameObject
			 || UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == inventoryButtons[9].gameObject) {
				firstOrLastSlotSelected = true;
			} else {
				firstOrLastSlotSelected = false;
			}
		}
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
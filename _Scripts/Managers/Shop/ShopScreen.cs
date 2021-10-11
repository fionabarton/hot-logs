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

	void Awake() {
		S = this;
	}

	void OnEnable () {
		// Ensures first slot is selected when screen enabled
		previousSelectedGameObject = inventoryButtons[0].gameObject;

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
			ShopScreen_PickItemMode.S.Loop(S);
			break;
		case eShopScreenMode.itemPurchasedOrSold:
			ShopScreen_ItemPurchasedOrSoldMode.S.Loop(S);
			break;
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
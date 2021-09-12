using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopScreen_ItemPurchasedOrSoldMode : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static ShopScreen_ItemPurchasedOrSoldMode _S;
	public static ShopScreen_ItemPurchasedOrSoldMode S { get { return _S; } set { _S = value; } }

	void Awake() {
		S = this;
	}

	public void Loop(ShopScreen shopScreen) {
		if (PauseMessage.S.dialogueFinished) {
			if (Input.GetButtonDown("SNES A Button")) {
				ShopScreen_PickItemMode.S.Setup(shopScreen);
			}
		}
	}

	public void PurchaseItem(Item item, ShopScreen shopScreen) {
		shopScreen.canUpdate = true;

		// Switch ScreenMode 
		shopScreen.shopScreenMode = eShopScreenMode.itemPurchasedOrSold;

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
		Utilities.S.RemoveListeners(shopScreen.inventoryButtons);

		// Deactivate Cursor
		ScreenCursor.S.cursorGO.SetActive(false);
	}

	public void SellItem(Item item, ShopScreen shopScreen) {
		shopScreen.canUpdate = true;

		// Switch ScreenMode 
		shopScreen.shopScreenMode = eShopScreenMode.itemPurchasedOrSold;

		//Remove item from Inventory
		Inventory.S.RemoveItemFromInventory(item);

		// Dialogue
		PauseMessage.S.DisplayText("Sold!" + " For " + item.value + " gold!" + " Cha - CHING!");

		// Subtract item price from Player's Gold
		PartyStats.S.Gold += item.value;

		// Update Gold 
		PlayerButtons.S.goldValue.text = PartyStats.S.Gold.ToString();

		// Remove Listeners
		Utilities.S.RemoveListeners(shopScreen.inventoryButtons);

		// Deactivate Cursor
		ScreenCursor.S.cursorGO.SetActive(false);
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ShopScreen Mode/Step 2: ItemPurchasedOrSold
/// - Buy or sell an item
/// </summary>
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

		if (Party.S.gold >= item.value) {
			// Added to Player Inventory
			Inventory.S.AddItemToInventory(item);

			// Dialogue
			PauseMessage.S.DisplayText("Purchased!" + " For " + item.value + " gold!");

			// Subtract item price from Player's Gold
			Party.S.gold -= item.value;

			// Update Gold 
			PlayerButtons.S.goldValue.text = Party.S.gold.ToString();

			// Audio: Buff 1
			AudioManager.S.PlaySFX(eSoundName.buff1);
		} else {
			// Dialogue
			PauseMessage.S.DisplayText("Not enough money!");

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);
		}

		// Remove Listeners
		Utilities.S.RemoveListeners(shopScreen.inventoryButtons);

		// Deactivate screen cursors
		Utilities.S.SetActiveList(ScreenCursor.S.cursorGO, false);
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
		Party.S.gold += item.value;

		// Update Gold 
		PlayerButtons.S.goldValue.text = Party.S.gold.ToString();

		// Remove Listeners
		Utilities.S.RemoveListeners(shopScreen.inventoryButtons);

		// Deactivate screen cursors
		Utilities.S.SetActiveList(ScreenCursor.S.cursorGO, false);

		if(shopScreen.firstSlotNdx != 0) {
			shopScreen.firstSlotNdx -= 1;
		}

		// Audio: Buff 1
		AudioManager.S.PlaySFX(eSoundName.buff1);
	}
}
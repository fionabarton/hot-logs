using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// ShopScreen Mode/Step 1: PickItem
/// - Select an item to buy or sell
/// </summary>
public class ShopScreen_PickItemMode : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static ShopScreen_PickItemMode _S;
	public static ShopScreen_PickItemMode S { get { return _S; } set { _S = value; } }

	void Awake() {
		S = this;
	}

	public void Setup(ShopScreen shopScreen) {
		try {
			// Set ScreenMode
			shopScreen.shopScreenMode = eShopScreenMode.pickItem;

			// Reimport inventory if an item was sold
			if (!shopScreen.buyOrSellMode) {
				shopScreen.ImportInventory(Inventory.S.GetItemList());
			}

			DeactivateUnusedItemSlots(shopScreen);
			AssignItemNames(shopScreen);
			AssignItemEffect(shopScreen);

			shopScreen.canUpdate = true;

			// Freeze Player
			RPG.S.paused = true;
			Player.S.mode = eRPGMode.idle;

			// Activate PlayerButtons
			PlayerButtons.S.gameObject.SetActive(true);
			Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, false);

			// If Inventory Empty... 
			if (Inventory.S.GetItemList().Count == 0 && !shopScreen.buyOrSellMode) {
				PauseMessage.S.DisplayText("You have nothing to sell, fool!");

				// Deactivate screen cursors
				Utilities.S.SetActiveList(ScreenCursor.S.cursorGO, false);

				// Audio: Deny
				AudioManager.S.PlaySFX(eSoundName.deny);
			} else {
				// If previousSelectedGameObject is enabled...
				if (shopScreen.previousSelectedGameObject.activeInHierarchy) {
					// Select previousSelectedGameObject
					Utilities.S.SetSelectedGO(shopScreen.previousSelectedGameObject);
				} else {
					// Select previous inventoryButton in the list
					Utilities.S.SetSelectedGO(shopScreen.inventoryButtons[shopScreen.previousSelectedNdx - 1].gameObject);
				}

				// Set the first and last button’s navigation 
				SetButtonNavigation(shopScreen);

				// Activate Cursor
				ScreenCursor.S.cursorGO[0].SetActive(true);
			}

			// Activate PauseMessage
			PauseMessage.S.gameObject.SetActive(true);
		}
		catch (NullReferenceException) { }
	}

	public void Loop(ShopScreen shopScreen) {
		if (shopScreen.canUpdate) {
			if(shopScreen.inventory.Count > 0) {
				DisplayItemDescriptions(shopScreen);
			}
			shopScreen.canUpdate = false;
		}
	}

	public void DeactivateUnusedItemSlots(ShopScreen shopScreen) {
		for (int i = 0; i < shopScreen.inventoryButtons.Count; i++) {
			if (i < shopScreen.inventory.Count) {
				shopScreen.inventoryButtons[i].gameObject.SetActive(true);
			} else {
				shopScreen.inventoryButtons[i].gameObject.SetActive(false);
			}
		}
	}

	public void AssignItemEffect(ShopScreen shopScreen) {
		for (int i = 0; i < shopScreen.inventoryButtons.Count; i++) {
			int copy = i;
			shopScreen.inventoryButtons[i].onClick.RemoveAllListeners();

			if (shopScreen.buyOrSellMode) {
				shopScreen.inventoryButtons[copy].onClick.AddListener(delegate { ShopScreen_ItemPurchasedOrSoldMode.S.PurchaseItem(shopScreen.inventory[shopScreen.firstSlotNdx + copy], shopScreen); });
			} else {
				shopScreen.inventoryButtons[copy].onClick.AddListener(delegate { ShopScreen_ItemPurchasedOrSoldMode.S.SellItem(shopScreen.inventory[shopScreen.firstSlotNdx + copy], shopScreen); });
			}
		}
	}

	public void AssignItemNames(ShopScreen shopScreen) {
		for (int i = 0; i < shopScreen.inventoryButtonsText.Count; i++) {
			//if(shopScreen.inventory.Count <= i) {
			//	return;
			//         }

			//// Assign Button Name Text
			//string ndx = (i + 1).ToString();
			//shopScreen.inventoryButtonsText[i].text = ndx + ") " + shopScreen.inventory[i].name;

			if (shopScreen.firstSlotNdx + i < shopScreen.inventory.Count) {
				string inventoryNdx = (shopScreen.firstSlotNdx + i + 1).ToString();

				shopScreen.inventoryButtonsText[i].text = inventoryNdx + ") " + shopScreen.inventory[shopScreen.firstSlotNdx + i].name;
			}
		}
	}

	public void DisplayItemDescriptions(ShopScreen shopScreen) {
		for (int i = 0; i < shopScreen.inventoryButtonsText.Count; i++) {
			//if (shopScreen.inventory.Count <= i) {
			//	return;
			//}

			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == shopScreen.inventoryButtons[i].gameObject) {
				PauseMessage.S.SetText(shopScreen.inventory[shopScreen.firstSlotNdx + i].description);

				// Cursor Position set to Selected Button
				Utilities.S.PositionCursor(shopScreen.inventoryButtons[i].gameObject, -160, 0, 0);

				// Set selected button text color	
				shopScreen.inventoryButtons[i].gameObject.GetComponentInChildren<Text>().color = new Color32(205, 208, 0, 255);

				// Audio: Selection (when a new gameObject is selected)
				Utilities.S.PlayButtonSelectedSFX(ref shopScreen.previousSelectedGameObject);
				// Cache Selected Gameobject's index 
				shopScreen.previousSelectedNdx = i;
			} else {
				// Set non-selected button text color
				shopScreen.inventoryButtons[i].gameObject.GetComponentInChildren<Text>().color = new Color32(255, 255, 255, 255);
			}
		}
	}

	// Set the first and last button’s navigation 
	public void SetButtonNavigation(ShopScreen shopScreen) {
		// Reset all button's navigation to automatic
		for (int i = 0; i < shopScreen.inventoryButtons.Count; i++) {
			// Get the Navigation data
			Navigation navigation = shopScreen.inventoryButtons[i].navigation;

			// Switch mode to Automatic
			navigation.mode = Navigation.Mode.Automatic;

			// Reassign the struct data to the button
			shopScreen.inventoryButtons[i].navigation = navigation;
		}

		// Set button navigation if inventory is less than 10
		if (shopScreen.inventory.Count < shopScreen.inventoryButtons.Count) {
			if (shopScreen.inventory.Count > 1) {
				// Set first button navigation
				Utilities.S.SetButtonNavigation(
					shopScreen.inventoryButtons[0],
					shopScreen.inventoryButtons[1],
					shopScreen.inventoryButtons[shopScreen.inventory.Count - 1]);

				// Set last button navigation
				Utilities.S.SetButtonNavigation(
					shopScreen.inventoryButtons[shopScreen.inventory.Count - 1],
					shopScreen.inventoryButtons[0],
					shopScreen.inventoryButtons[shopScreen.inventory.Count - 2]);
			}
		}
	}
}
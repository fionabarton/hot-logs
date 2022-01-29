using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// ItemScreen Mode/Step 1: PickItem
/// - Select an item to use
/// </summary>
public class ItemScreen_PickItemMode : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static ItemScreen_PickItemMode _S;
	public static ItemScreen_PickItemMode S { get { return _S; } set { _S = value; } }

	public int previousSelectedNdx;

	void Awake() {
		S = this;
	}

	public void Setup(ItemScreen itemScreen) {
		try {
			itemScreen.mode = eItemScreenMode.pickItem;

			DeactivateUnusedItemSlots(itemScreen);
			itemScreen.AssignItemNames();
			itemScreen.AssignItemEffect();

			// Buttons Interactable
			Utilities.S.ButtonsInteractable(itemScreen.itemButtons, true);
			Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, false);
			Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, false);

			itemScreen.canUpdate = true;

			if (RPG.S.currentScene != "Battle") {
				// Activate PlayerButtons
				PlayerButtons.S.gameObject.SetActive(true);
			}

			// Activate Slot Headers 
			itemScreen.nameHeaderText.text = "Name:";
			itemScreen.valueHeader.SetActive(true);
			itemScreen.QTYOwnedHeader.SetActive(true);

			// If Inventory Empty 
			if (Inventory.S.GetItemList().Count == 0) {
				PauseMessage.S.DisplayText("You have no items, fool!");

				// Deactivate screen cursors
				Utilities.S.SetActiveList(ScreenCursor.S.cursorGO, false);
			} else {
				// If previousSelectedGameObject is enabled...
				if (ItemScreen.S.previousSelectedGameObject.activeInHierarchy) {
					// Select previousSelectedGameObject
					Utilities.S.SetSelectedGO(ItemScreen.S.previousSelectedGameObject);

					// Set previously selected GameObject
					Battle.S.previousSelectedForAudio = ItemScreen.S.previousSelectedGameObject;
				} else {
					// Select previous itemButton in the list
					Utilities.S.SetSelectedGO(itemScreen.itemButtons[previousSelectedNdx - 1].gameObject);
				}

				// Set button navigation if inventory is less than 10
				SetButtonNavigation(itemScreen);

				// Activate Cursor
				ScreenCursor.S.cursorGO[0].SetActive(true);
			}

			// Set Battle Turn Cursor sorting layer BELOW UI
			BattleUI.S.turnCursorSRend.sortingLayerName = "0";
		}
		catch (NullReferenceException) { }

		// Remove Listeners
		itemScreen.sortButton.onClick.RemoveAllListeners();
		// Assign Listener (Sort Button)
		itemScreen.sortButton.onClick.AddListener(delegate { Inventory.S.items = SortItems.S.SortByABC(Inventory.S.items); });
	}

	// Set the first and last button’s navigation if the player’s inventory is less than 10
	public void SetButtonNavigation(ItemScreen itemScreen) {
		// Reset all button's navigation to automatic
		for (int i = 0; i < itemScreen.itemButtons.Count; i++) {
			// Get the Navigation data
			Navigation navigation = itemScreen.itemButtons[i].navigation;

			// Switch mode to Automatic
			navigation.mode = Navigation.Mode.Automatic;

			// Reassign the struct data to the button
			itemScreen.itemButtons[i].navigation = navigation;
		}

		// Set button navigation if inventory is less than 10
		if (Inventory.S.GetItemList().Count < itemScreen.itemButtons.Count) {
			if (Inventory.S.GetItemList().Count > 1) {
				// Set first button navigation
				Utilities.S.SetButtonNavigation(
					itemScreen.itemButtons[0],
					itemScreen.itemButtons[Inventory.S.GetItemList().Count - 1],
					itemScreen.itemButtons[1]);

				// Set last button navigation
				Utilities.S.SetButtonNavigation(
					itemScreen.itemButtons[Inventory.S.GetItemList().Count - 1],
					itemScreen.itemButtons[Inventory.S.GetItemList().Count - 2],
					itemScreen.itemButtons[0]);
			}
		}
	}

	public void Loop(ItemScreen itemScreen) {
		if (Inventory.S.GetItemList().Count > 0) {
			if (itemScreen.canUpdate) {
				DisplayItemDescriptions(itemScreen);
				itemScreen.canUpdate = false;
			}
		}

		if (RPG.S.currentScene != "Battle") {
			if (Input.GetButtonDown("SNES Y Button")) {
				itemScreen.Deactivate(true);
			}
		}
	}

	public void DisplayItemDescriptions(ItemScreen itemScreen) {
		for (int i = 0; i < itemScreen.itemButtons.Count; i++) {
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == itemScreen.itemButtons[i].gameObject) {
				PauseMessage.S.SetText(Inventory.S.GetItemList()[i + itemScreen.firstSlotNdx].description);

				// Set Cursor Position set to Selected Button
				Utilities.S.PositionCursor(itemScreen.itemButtons[i].gameObject, -170, 0, 0);

				// Set selected button text color	
				itemScreen.itemButtonsNameText[i].color = new Color32(205, 208, 0, 255);
				itemScreen.itemButtonsValueText[i].color = new Color32(205, 208, 0, 255);
				itemScreen.itemButtonsQTYOwnedText[i].color = new Color32(205, 208, 0, 255);

				// Audio: Selection (when a new gameObject is selected)
				Utilities.S.PlayButtonSelectedSFX(ref ItemScreen.S.previousSelectedGameObject);
				// Cache Selected Gameobject's index 
				previousSelectedNdx = i;
			} else {
				// Set non-selected button text color
				itemScreen.itemButtonsNameText[i].color = new Color32(255, 255, 255, 255);
				itemScreen.itemButtonsValueText[i].color = new Color32(255, 255, 255, 255);
				itemScreen.itemButtonsQTYOwnedText[i].color = new Color32(255, 255, 255, 255);
			}
		}
	}

	void DeactivateUnusedItemSlots(ItemScreen itemScreen) {
		for (int i = 0; i < itemScreen.itemButtons.Count; i++) {
			if (i < Inventory.S.GetItemList().Count) {
				itemScreen.itemButtons[i].gameObject.SetActive(true);
				itemScreen.itemButtonsValueText[i].gameObject.SetActive(true);
				itemScreen.itemButtonsQTYOwnedText[i].gameObject.SetActive(true);
			} else {
				itemScreen.itemButtons[i].gameObject.SetActive(false);
				itemScreen.itemButtonsValueText[i].gameObject.SetActive(false);
				itemScreen.itemButtonsQTYOwnedText[i].gameObject.SetActive(false);
			}
		}
	}
}
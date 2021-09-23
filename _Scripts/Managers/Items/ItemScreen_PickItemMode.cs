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
		itemScreen.itemScreenMode = eItemScreenMode.pickItem;

		DeactivateUnusedItemSlots(itemScreen);
		itemScreen.AssignItemNames();
		itemScreen.AssignItemEffect();

		// Buttons Interactable
		Utilities.S.ButtonsInteractable(itemScreen.itemButtons, true);

		itemScreen.canUpdate = true;

		try {
			// Activate PlayerButtons
			PlayerButtons.S.gameObject.SetActive(true);
			Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, false);
			Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, false);

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
				} else {
					// Select previous itemButton in the list
					Utilities.S.SetSelectedGO(itemScreen.itemButtons[previousSelectedNdx - 1].gameObject);
				}

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

	public void Loop(ItemScreen itemScreen) {
		if (Inventory.S.GetItemList().Count > 0) {
			if (itemScreen.canUpdate) {
				DisplayItemDescriptions(itemScreen);
				itemScreen.canUpdate = false;
			}
		}

		if (RPG.S.currentSceneName != "Battle") {
			if (Input.GetButtonDown("SNES B Button")) {
				itemScreen.Deactivate();
			}
		}
	}

	public void DisplayItemDescriptions(ItemScreen itemScreen) {
		for (int i = 0; i < Inventory.S.GetItemList().Count; i++) {
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == itemScreen.itemButtons[i].gameObject) {
				PauseMessage.S.SetText(Inventory.S.GetItemList()[i].description);

				// Set Cursor Position set to Selected Button
				Utilities.S.PositionCursor(itemScreen.itemButtons[i].gameObject, -170, 0, 0);

				// Set selected button text color	
				itemScreen.itemButtons[i].gameObject.GetComponentInChildren<Text>().color = new Color32(205, 208, 0, 255);

				// Cache Selected Gameobject 
				ItemScreen.S.previousSelectedGameObject = itemScreen.itemButtons[i].gameObject;
				// Cache Selected Gameobject's index 
				previousSelectedNdx = i;
			} else {
				// Set non-selected button text color
				itemScreen.itemButtons[i].gameObject.GetComponentInChildren<Text>().color = new Color32(255, 255, 255, 255);
			}
		}
	}

	void DeactivateUnusedItemSlots(ItemScreen itemScreen) {
		for (int i = 0; i <= itemScreen.itemButtons.Count - 1; i++) {
			if (i < Inventory.S.GetItemList().Count) {
				itemScreen.itemButtons[i].gameObject.SetActive(true);
			} else {
				itemScreen.itemButtons[i].gameObject.SetActive(false);
			}
		}
	}
}
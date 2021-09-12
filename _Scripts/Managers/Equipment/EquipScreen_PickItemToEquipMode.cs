using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// EquipScreen Mode/Step 3: PickItemToEquip
/// - Select which item of a certain type to equip 
/// - Ex. Weapons: Wooden Club, Steel Sword, etc.
/// </summary>
public class EquipScreen_PickItemToEquipMode : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static EquipScreen_PickItemToEquipMode _S;
	public static EquipScreen_PickItemToEquipMode S { get { return _S; } set { _S = value; } }

	void Awake() {
		S = this;
	}

	public void SetUp(eItemType itemType, EquipScreen equipScreen) {
		// Buttons Interactable
		Utilities.S.ButtonsInteractable(equipScreen.inventoryButtons, true);
		Utilities.S.ButtonsInteractable(equipScreen.equippedButtons, false);

		// No items of this type
		if (SortItems.S.tItems.Count <= 0) {
			// Switch Mode 
			equipScreen.equipScreenMode = eEquipScreenMode.noInventory;

			ScreenCursor.S.cursorGO.SetActive(false);

			PauseMessage.S.DisplayText("You don't have any items of this type to equip!");
		}

		// Add Listeners
		AddListenersToInventoryButtons(equipScreen.playerNdx, equipScreen);

		// Switch mode
		equipScreen.SwitchMode(eEquipScreenMode.pickItemToEquip, equipScreen.inventoryButtons[0].gameObject, false);
	}

	public void Loop(EquipScreen equipScreen) {
		if (equipScreen.canUpdate) {
			DisplayInventoryDescriptions(equipScreen.playerNdx, equipScreen);
			equipScreen.canUpdate = false;
		}

		// Go back to pickTypeToEquip
		equipScreen.GoBackToPickTypeToEquipMode("SNES B Button");
	}

	// Add listeners to inventory buttons
	public void AddListenersToInventoryButtons(int playerNdx, EquipScreen equipScreen) {
		// Remove and add listeners
		for (int i = 0; i < equipScreen.inventoryButtons.Count; i++) {
			int tInt = i;
			equipScreen.inventoryButtons[tInt].onClick.RemoveAllListeners();
			equipScreen.inventoryButtons[tInt].onClick.AddListener(delegate { equipScreen.EquipItem(playerNdx, SortItems.S.tItems[tInt]); });
		}
	}

	// Display description of item to be potentially equipped
	public void DisplayInventoryDescriptions(int playerNdx, EquipScreen equipScreen) {
		if (SortItems.S.tItems != null) {
			for (int i = 0; i < SortItems.S.tItems.Count; i++) {
				if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == equipScreen.inventoryButtons[i].gameObject) {
					// Display item's description
					PauseMessage.S.SetText(SortItems.S.tItems[i].description);

					// Set cursor position to currently selected button
					Utilities.S.PositionCursor(equipScreen.inventoryButtons[i].gameObject, -160, 0, 0);

					// Set selected button text color	
					equipScreen.inventoryButtons[i].gameObject.GetComponentInChildren<Text>().color = new Color32(205, 208, 0, 255);

					// Calculate and display potential stats
					EquipStatsEffect.S.DisplayPotentialStats(playerNdx, SortItems.S.tItems[i], equipScreen.playerEquipment);
                } else {
					// Set non-selected button text color
					equipScreen.inventoryButtons[i].gameObject.GetComponentInChildren<Text>().color = new Color32(39, 201, 255, 255);
				}
			}
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// EquipScreen Mode/Step 2: PickTypeToEquip
/// - Select which type of item to equip (weapon, armor, etc.)
/// </summary>
public class EquipScreen_PickTypeToEquipMode : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static EquipScreen_PickTypeToEquipMode _S;
	public static EquipScreen_PickTypeToEquipMode S { get { return _S; } set { _S = value; } }

	void Awake() {
		S = this;
	}

	public void SetUp(int ndx, EquipScreen equipScreen) {
		// Set anims
		equipScreen.playerAnim.CrossFade("Walk", 0);
		PlayerButtons.S.anim[ndx].CrossFade("Idle", 0);

		// Buttons Interactable
		Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, false);
		Utilities.S.ButtonsInteractable(equipScreen.equippedButtons, true);

		equipScreen.playerNdx = ndx;

		// Add Listeners
		AddListenersToEquippedButtons(equipScreen);

		// Switch mode
		equipScreen.SwitchMode(eEquipScreenMode.pickTypeToEquip, equipScreen.equippedButtons[0].gameObject, false);

		// Buttons Interactable
		Utilities.S.ButtonsInteractable(equipScreen.inventoryButtons, false);
	}

	public void Loop(EquipScreen equipScreen) {
		if (equipScreen.canUpdate) {
			DisplayCurrentEquipmentDescriptions(equipScreen.playerNdx, equipScreen);
			// Cache Selected Gameobject 
			EquipScreen.S.previousSelectedGameObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
			equipScreen.canUpdate = false;

			// Display items in the inventory of the currently selected equipment type
			for (int i = 0; i < equipScreen.equippedButtons.Count; i++) {
				if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == equipScreen.equippedButtons[i].gameObject) {
					// Sort Items by ItemType
					SortItems.S.SortByItemType(Inventory.S.items, equipScreen.playerEquipment[equipScreen.playerNdx][i].type);

					// Set selected button text color	
					equipScreen.equippedButtons[i].gameObject.GetComponentInChildren<Text>().color = new Color32(205, 208, 0, 255);

					// Deactivate Unused Inventory Buttons Slots
					for (int j = 0; j < equipScreen.inventoryButtons.Count; j++) {
						if (j < SortItems.S.tItems.Count) {
							equipScreen.inventoryButtons[j].gameObject.SetActive(true);
						} else {
							equipScreen.inventoryButtons[j].gameObject.SetActive(false);
						}
					}

					// Assign item names to buttons
					for (int j = 0; j < SortItems.S.tItems.Count; j++) {
						equipScreen.inventoryButtonsTxt[j].text = SortItems.S.tItems[j].name;
					}
				} else {
					// Set non-selected button text color
					equipScreen.equippedButtons[i].gameObject.GetComponentInChildren<Text>().color = new Color32(39, 201, 255, 255);
				}
			}
		}

		// Go back to pickPartyMember mode
		if (PauseMessage.S.dialogueFinished) {
			if (Input.GetButtonDown("SNES B Button")) {
				EquipScreen_PickPartyMemberMode.S.SetUp(equipScreen);

				// Reset equippedButtons text color
				Utilities.S.SetTextColor(equipScreen.equippedButtons, new Color32(39, 201, 255, 255));
			}
		}
	}

	// Add listeners to equipped buttons
	public void AddListenersToEquippedButtons(EquipScreen equipScreen) {
		// Remove and add listeners
		for (int i = 0; i < equipScreen.equippedButtons.Count; i++) {
			int tInt = i;
			equipScreen.equippedButtons[tInt].onClick.RemoveAllListeners();
			equipScreen.equippedButtons[tInt].onClick.AddListener(delegate { EquipScreen_PickItemToEquipMode.S.SetUp((eItemType)tInt, equipScreen); });
		}
	}

	// Display descriptions of party member's current equipment
	public void DisplayCurrentEquipmentDescriptions(int playerNdx, EquipScreen equipScreen) {
		for (int i = 0; i <= equipScreen.equippedButtons.Count - 1; i++) {
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == equipScreen.equippedButtons[i].gameObject) {
				// Display item's description
				PauseMessage.S.SetText(equipScreen.playerEquipment[playerNdx][i].description);

				// Set cursor position to currently selected button
				Utilities.S.PositionCursor(equipScreen.equippedButtons[i].gameObject, -160, 0, 0);
			}
		}
	}
}
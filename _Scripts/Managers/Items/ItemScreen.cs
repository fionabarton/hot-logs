using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ItemScreen : MonoBehaviour { 
	[Header("Set in Inspector")]
	// Item "Buttons"
	public List<Button> 	itemButtons;
	public List<Text> 	  	itemButtonsNameText;
	public List<Text>		itemButtonsValueText;
	public List<Text>		itemButtonsQTYOwnedText;

	public Text				nameHeaderText;
	public GameObject		valueHeader;
	public GameObject		QTYOwnedHeader;

	public Button			sortButton;

	[Header("Set Dynamically")]
	// Singleton
	private static ItemScreen _S;
	public static ItemScreen S { get { return _S; } set { _S = value; } }

	// For Input & Display Message
	public eItemScreenMode  mode;

	// Allows parts of Loop() to be called once rather than repeatedly every frame.
	public bool 			canUpdate;

	public GameObject		previousSelectedGameObject;

	// Caches what index of the inventory is currently stored in the first item slot
	public int				firstSlotNdx;

	// Prevents instantly registering input when the first or last slot is selected
	private bool			verticalAxisIsInUse;
	private bool			firstOrLastSlotSelected;

	void Awake() {
		S = this;
	}

	public void OnEnable () {
		// Ensures first slot is selected when screen enabled
		previousSelectedGameObject = itemButtons[0].gameObject;

		firstSlotNdx = 0;

		ItemScreen_PickItemMode.S.Setup(S);

		// Add Loop() to Update Delgate
		UpdateManager.updateDelegate += Loop;
	}

	public void Activate() {
		gameObject.SetActive(true);

		// Audio: Confirm
		AudioManager.S.PlaySFX(eSoundName.confirm);
	}

	public void Deactivate(bool playSound = false) {
		// Deactivate Cursors if in Battle Mode
		if (!RPG.S.paused) {
			Utilities.S.SetActiveList(ScreenCursor.S.cursorGO, false);
		}

		// Set Battle Turn Cursor sorting layer ABOVE UI
		BattleUI.S.turnCursorSRend.sortingLayerName = "Above UI";

		// Remove Listeners
		Utilities.S.RemoveListeners(itemButtons);

		if (RPG.S.currentScene != "Battle") {
			// Buttons Interactable
			Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, true);

			// Set Selected Gameobject (Pause Screen: Items Button)
			Utilities.S.SetSelectedGO(PauseScreen.S.buttonGO[0]);

			PauseMessage.S.DisplayText("Welcome to the Pause Screen!");

			PauseScreen.S.canUpdate = true;
		}

		if (playSound) {
			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);
		}

		// Deactivate PlayerButtons
		PlayerButtons.S.gameObject.SetActive(false);

		// Remove Loop() from Update Delgate
		UpdateManager.updateDelegate -= Loop;

		// Deactivate this gameObject
		gameObject.SetActive(false);
	}
	
	public void Loop () {
		// Reset canUpdate
		if (Input.GetAxisRaw ("Horizontal") != 0f || Input.GetAxisRaw ("Vertical") != 0f) { 
			canUpdate = true;
		}
		
		// Deactivate ItemScreen during Battle
		if (RPG.S.currentScene == "Battle") {
			if (Input.GetButtonDown ("SNES Y Button")) {
				PauseMessage.S.gameObject.SetActive(false);
				Deactivate(true);
				Battle.S.PlayerTurn(true, false);
			}
		}

		switch (mode) {
			case eItemScreenMode.pickItem:
				// On vertical input, scroll the item list when the first or last slot is selected
				if (Inventory.S.GetItemList().Count > itemButtons.Count) {
					ScrollItemList();
				}

				ItemScreen_PickItemMode.S.Loop(S);
				break;
			case eItemScreenMode.pickPartyMember:
				ItemScreen_PickPartyMemberMode.S.Loop(S);
			break;
			case eItemScreenMode.pickAllPartyMembers:
				if (Input.GetButtonDown("SNES Y Button")) {
					GoBackToPickItemMode();
				}
			break;
			case eItemScreenMode.pickWhereToWarp:
				if (canUpdate) {
					WarpManager.S.DisplayButtonDescriptions(itemButtons, -170);
				}

				if (Input.GetButtonDown("SNES Y Button")) {
					GoBackToPickItemMode();
				}
				break;
			case eItemScreenMode.usedItem:
				ItemScreen_UsedItemMode.S.Loop(S);
			break;
		}

		// TEST: Sort Inventory
		if (Input.GetKeyDown (KeyCode.X)) {
			//_items = SortItems.S.SortByABC(_items);
			Inventory.S.items = SortItems.S.SortByValue(Inventory.S.items);
		}
	}

	// On vertical input, scroll the item list when the first or last slot is selected
	void ScrollItemList() {
		if (Inventory.S.GetItemList().Count > 1) {
			// If first or last slot selected...
			if (firstOrLastSlotSelected) {
				if (Input.GetAxisRaw("Vertical") == 0) {
					verticalAxisIsInUse = false;
				} else {
					if (!verticalAxisIsInUse) {
						if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == itemButtons[0].gameObject) {
							if (Input.GetAxisRaw("Vertical") > 0) {
								if (firstSlotNdx == 0) {
									firstSlotNdx = Inventory.S.GetItemList().Count - itemButtons.Count;

									// Set  selected GameObject
									Utilities.S.SetSelectedGO(itemButtons[9].gameObject);
								} else {
									firstSlotNdx -= 1;
								}
							}
						} else if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == itemButtons[9].gameObject) {
							if (Input.GetAxisRaw("Vertical") < 0) {
								if (firstSlotNdx + itemButtons.Count == Inventory.S.GetItemList().Count) {
									firstSlotNdx = 0;

									// Set  selected GameObject
									Utilities.S.SetSelectedGO(itemButtons[0].gameObject);
								} else {
									firstSlotNdx += 1;
								}
							}
						}

						AssignItemEffect();
						AssignItemNames();

						// Audio: Selection
						AudioManager.S.PlaySFX(eSoundName.selection);

						verticalAxisIsInUse = true;

						// Allows scrolling when the vertical axis is held down in 0.2 seconds
						Invoke("VerticalAxisScrollDelay", 0.2f);
					}
				}
			}

			// Check if first or last slot is selected
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == itemButtons[0].gameObject
			 || UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == itemButtons[9].gameObject) {
				firstOrLastSlotSelected = true;
			} else {
				firstOrLastSlotSelected = false;
			}
		}
	}

	// Allows scrolling when the vertical axis is held down 
	void VerticalAxisScrollDelay() {
		verticalAxisIsInUse = false;
	}

	void GoBackToPickItemMode() {
		if (PauseMessage.S.dialogueFinished) {
			// Set animations to idle
			PlayerButtons.S.SetSelectedAnim("Idle");

			// Reset button colors
			PlayerButtons.S.SetButtonsColor(PlayerButtons.S.buttonsCS, new Color32(255, 255, 255, 200));

			// Deactivate screen cursors
			Utilities.S.SetActiveList(ScreenCursor.S.cursorGO, false);

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);

			// Go back to PickItem mode
			ItemScreen_PickItemMode.S.Setup(S);
		}
	}

	public void AssignItemEffect() { 
		Utilities.S.RemoveListeners(itemButtons);

		for (int i = 0; i < itemButtons.Count; i++) {
			int copy = i;
			itemButtons[copy].onClick.AddListener(delegate { ConsumeItem(Inventory.S.GetItemList()[firstSlotNdx + copy]); });
		}
	}

	public void AssignItemNames () {
		for (int i = 0; i < itemButtons.Count; i++) {
			if(firstSlotNdx + i < Inventory.S.GetItemList().Count) {
				string inventoryNdx = (firstSlotNdx + i + 1).ToString();

				itemButtonsNameText[i].text = inventoryNdx + ") " + Inventory.S.GetItemList()[firstSlotNdx + i].name;
				itemButtonsValueText[i].text = Inventory.S.GetItemList()[firstSlotNdx + i].value.ToString();
				itemButtonsQTYOwnedText[i].text = Inventory.S.GetItemCount(Inventory.S.GetItemList()[firstSlotNdx + i]).ToString();
			}	
		}
	}

	public void ConsumeItem(Item item){
		// Check if the item is in inventory
		if (Inventory.S.items.ContainsKey (item)) {
			canUpdate = true;

			if (RPG.S.currentScene == "Battle") { // if Battle
				if (item.name == "Health Potion") {
					BattleItems.S.AddFunctionToButton(BattleItems.S.HPPotion, "Use potion on which party member?", item);
				} else if (item.name == "Magic Potion") {
					BattleItems.S.AddFunctionToButton(BattleItems.S.MPPotion, "Use potion on which party member?", item);
				} else if (item.name == "Heal All Potion") {
					BattleItems.S.AddFunctionToButton(BattleItems.S.HealAllPotion, "Use potion to heal all party members?", item);
				} else if (item.name == "Revive Potion") {
					BattleItems.S.AddFunctionToButton(BattleItems.S.RevivePotion, "Use potion to revive which party member?", item);
				} else {
					BattleItems.S.CantUseItem();
				}
			} else { // if Overworld
				if (item.name == "Health Potion") {
					WorldItems.S.AddFunctionToButton(WorldItems.S.HPPotion, "Heal which party member?", item);
				} else if (item.name == "Magic Potion") {
					WorldItems.S.AddFunctionToButton(WorldItems.S.MPPotion, "Use MP potion on which party member?", item);
				} else if (item.name == "Heal All Potion") {
					WorldItems.S.AddFunctionToButton(WorldItems.S.HealAllPotion, "Use potion to heal all party members?", item);
				} else if (item.name == "Warp Potion") {
					WorldItems.S.WarpPotion();
				} else {
					WorldItems.S.CantUseItem();
				}
			}
		}
	}
}
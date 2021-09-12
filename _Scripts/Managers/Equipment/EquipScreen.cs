﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum eEquipScreenMode { pickPartyMember, pickTypeToEquip, noInventory, pickItemToEquip, equippedItem };

public class EquipScreen : MonoBehaviour {
	[Header("Set in Inspector")]
	public Text				playerName;
	public Text 			currentStats;
	public GameObject		equippedItemTypeNames;

	public Animator			playerAnim;

	// Equipped Buttons (the currently selected party member's equipment)
	public List <Button>  	equippedButtons;
	public List <Text>  	equippedButtonsTxt;

	// Inventory Buttons (dynamic list of different types of items to be equipped (list of either weapon, armor, etc.))
	public List <Button>  	inventoryButtons;
	public List <Text>  	inventoryButtonsTxt;

	[Header("Set Dynamically")]
	// Singleton
	private static EquipScreen _S;
	public static EquipScreen S { get { return _S; } set { _S = value; } }

	public int 				playerNdx = 0;

	// Each party member's current equipment ([playerNdx][Weapon, Armor, Helmet, Other])
	public List<List<Item>> playerEquipment = new List<List<Item>>();
		
	public eEquipScreenMode equipScreenMode = eEquipScreenMode.pickPartyMember;

	// Allows parts of Loop() to be called once rather than repeatedly every frame.
	public bool				canUpdate;

	public GameObject		previousSelectedGameObject;

	void Awake() {
		S = this;
	}

    private void Start() {
        // Intialize the party's equipment 
        playerEquipment.Add(new List<Item> { 
			ItemManager.S.items[18], ItemManager.S.items[19], ItemManager.S.items[20], ItemManager.S.items[21] 
		});

        playerEquipment.Add(new List<Item> { 
			ItemManager.S.items[18], ItemManager.S.items[19], ItemManager.S.items[20], ItemManager.S.items[21] 
		});
	}

    void OnEnable () {
		playerNdx = 0;
		previousSelectedGameObject = null;

		EquipScreen_PickPartyMemberMode.S.SetUp(S);

		// Add Loop() to Update Delgate
		UpdateManager.updateDelegate += Loop;

		PlayerButtons.S.buttonsCS[0].Select();
		PlayerButtons.S.buttonsCS[0].OnSelect(null);
		PlayerButtons.S.anim[0].CrossFade("Walk", 0);
	}

	public void Deactivate() {
		// Activate Cursor
		ScreenCursor.S.cursorGO.SetActive(true);

		// Go back to Pause Screen
		if (RPG.S.currentSceneName != "Battle") {
			// Pause Buttons Interactable
			Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, true);

			// Set Selected Gameobject (Pause Screen: Equip Button)
			Utilities.S.SetSelectedGO(PauseScreen.S.buttonGO[1]);

			PauseMessage.S.DisplayText("Welcome to the Pause Screen!");

			PauseScreen.S.canUpdate = true;
		}

		// Deactivate PlayerButtons
		PlayerButtons.S.gameObject.SetActive(false);
		
		// Remove Loop() from Update Delgate
		UpdateManager.updateDelegate -= Loop;

		// Deactivate this gameObject
		gameObject.SetActive(false);
	}

	public void Loop(){
		// Reset canUpdate
		if (Input.GetAxisRaw ("Horizontal") != 0f || Input.GetAxisRaw ("Vertical") != 0f) { 
			canUpdate = true;
        } 

		switch (equipScreenMode) {
		case eEquipScreenMode.pickPartyMember:
			EquipScreen_PickPartyMemberMode.S.Loop(S);
			break;
		case eEquipScreenMode.pickTypeToEquip:
			EquipScreen_PickTypeToEquipMode.S.Loop(S);
			break;
		case eEquipScreenMode.pickItemToEquip:
			EquipScreen_PickItemToEquipMode.S.Loop(S);
			break;
		case eEquipScreenMode.noInventory:
		case eEquipScreenMode.equippedItem:
			// Go back to pickTypeToEquip mode 
			GoBackToPickTypeToEquipMode("SNES A Button");
			break;
		}
	}

	public void GoBackToPickTypeToEquipMode(string inputName) {
		if (PauseMessage.S.dialogueFinished) {
			if (Input.GetButtonDown(inputName)) {
				// Deactivate Buttons
				for (int i = 0; i < inventoryButtons.Count; i++) {
					inventoryButtons[i].gameObject.SetActive(false);
				}

				// Set Up pickTypeToEquip mode
				EquipScreen_PickTypeToEquipMode.S.SetUp(playerNdx, S);

				// Set Selected Gameobject 
				Utilities.S.SetSelectedGO(previousSelectedGameObject);

				// Activate Cursor
				ScreenCursor.S.cursorGO.SetActive(true);

				// Reset inventoryButtons text color
				Utilities.S.SetTextColor(inventoryButtons, new Color32(39, 201, 255, 255));
			}
		}
	}
	
	// Display member's name and current stats
	public void DisplayCurrentStats(int playerNdx){
		playerName.text = Party.stats[playerNdx].name;
		currentStats.text = Party.stats[playerNdx].LVL + "\n" + Party.stats[playerNdx].HP + "/" + Party.stats[playerNdx].maxHP + "\n" + Party.stats[playerNdx].MP + "/" + Party.stats[playerNdx].maxMP;
		EquipStatsEffect.S.currentAttributeAmounts.text = Party.stats[playerNdx].STR + "\n" + Party.stats[playerNdx].DEF + "\n" + Party.stats[playerNdx].WIS + "\n" + Party.stats[playerNdx].AGI;
		EquipStatsEffect.S.potentialStats.text = "";
	}

	// Display the names of the member's current equipment
	public void DisplayCurrentEquipmentNames(int playerNdx){ 
		// Set Button Text
		for(int i = 0; i < equippedButtonsTxt.Count; i++) {
			equippedButtonsTxt[i].text = playerEquipment[playerNdx][i].name;
		}
	}

	// eEquipScreenMode.equippedItem /////////////////////////////////////////////////////////////
	
	// Remove equipped item and equip new item
	public void EquipItem(int playerNdx, Item item) {
		// Remove Listeners
		Utilities.S.RemoveListeners(inventoryButtons);

		// Switch mode
		SwitchMode(eEquipScreenMode.equippedItem, null, false);

		// Buttons Interactable
		Utilities.S.ButtonsInteractable(inventoryButtons, false);

		// Deactivate Cursor
		ScreenCursor.S.cursorGO.SetActive(false);

		// Add old item back to inventory
		Inventory.S.AddItemToInventory(playerEquipment[playerNdx][(int)item.type]);
		// Remove new item from inventory
		Inventory.S.RemoveItemFromInventory(item);

		// Subtract old item stat effect 
		EquipStatsEffect.S.RemoveItemEffect(playerNdx, playerEquipment[playerNdx][(int)item.type]);

		// Equip new item
		playerEquipment[playerNdx][(int)item.type] = item;

		//PauseMessage.S.DisplayText(Stats.S.playerName[playerNdx] + " equipped " + item.name + "!");
		PauseMessage.S.DisplayText(Party.stats[playerNdx].name + " equipped " + item.name + "!");

		// Add Item StatEffect
		EquipStatsEffect.S.AddItemEffect(playerNdx, item);

		// Update GUI
		DisplayCurrentStats(playerNdx);
		DisplayCurrentEquipmentNames(playerNdx);

		playerAnim.CrossFade("Success", 0);
	}

	public void SwitchMode(eEquipScreenMode mode, GameObject selectedGO, bool potentialStats) {
		canUpdate = true;

		// Switch ScreenMode
		equipScreenMode = mode;

		// Activate Potential Stat
		EquipStatsEffect.S.potentialStatHolder.SetActive(potentialStats);

		// Set Selected GameObject 
		Utilities.S.SetSelectedGO(selectedGO);
	}
}
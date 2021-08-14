using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using System;

public enum eEquipScreenMode { pickPartyMember, pickTypeToEquip, noInventory, pickItemToEquip, equippedItem };

public class EquipScreen : MonoBehaviour {
	[Header("Set in Inspector")]
	public Text				playerName;
	public Text 			currentStats;
	public GameObject		equippedItemTypeNames;
	
	public Image			playerImage;
	public List<Sprite>		playerSprites;
	public Animator			playerAnim;

	// Equipped Buttons (what the party member is currently equipped with)
	public List <Button>  	equippedButtons;
	public List <Text>  	equippedButtonsTxt;

	// Inventory Buttons (dynamic list of different types of items to be equipped (list of either weapon, armor, etc.))
	public List <Button>  	inventoryButtons;
	public List <Text>  	inventoryButtonsTxt;

	// Potential Stats
	public GameObject		potentialStatHolder;
	public Text				currentAttributeAmounts; // STR, DEF, WIS, AGI
	public Text 			potentialStats;
	public List<GameObject> arrowGO;
	public List<Animator>	arrowAnim;

	[Header("Set Dynamically")]
	// Singleton
	private static EquipScreen _S;
	public static EquipScreen S { get { return _S; } set { _S = value; } }

	public int 				playerNdx = 0;
	GameObject				previousSelectedGameObject;

	// Each party member's current equipment ([playerNdx][Weapon, Armor, Helmet, Other])
	public List<List<Item>>	playerEquipment = new List<List<Item>>();

	// For Input & Display Message
	public eEquipScreenMode equipScreenMode = eEquipScreenMode.pickPartyMember;

	// Allows parts of Loop() to not be called once rather than repeatedly every frame.
	// If there is directional input, it calls them just once again.
	bool 					canUpdate;

	void Awake() {
		// Singleton
		S = this;
	}

    private void Start() {
		// Intialize the party's equipment to nothing
		playerEquipment.Add(new List<Item> { ItemManager.S.items[18], ItemManager.S.items[19], ItemManager.S.items[20], ItemManager.S.items[21] });
		playerEquipment.Add(new List<Item> { ItemManager.S.items[18], ItemManager.S.items[19], ItemManager.S.items[20], ItemManager.S.items[21] });
	}

    void OnEnable () {
		playerNdx = 0;
		previousSelectedGameObject = null;

		PickPartyMemberMode();

		// Update Delgate
		UpdateManager.updateDelegate += Loop;

		PlayerButtons.S.buttonsCS[0].Select();
		PlayerButtons.S.buttonsCS[0].OnSelect(null);
		PlayerButtons.S.anim[0].CrossFade("Walk", 0);
	}

	void OnDisable () {
		// Activate Cursor
		ScreenCursor.S.cursorGO.SetActive(true);

		// Deactivate Arrow Sprites
		for (int i = 0; i <= arrowGO.Count - 1; i++) {
			arrowGO[i].SetActive (false);
		}

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
		ScreenManager.S.playerButtonsGO.SetActive(false);

		// Update Delegate
		UpdateManager.updateDelegate -= Loop;
	}

	public void Loop(){
		// Reset canUpdate
		if (Input.GetAxisRaw ("Horizontal") != 0f || Input.GetAxisRaw ("Vertical") != 0f) { 
			canUpdate = true;
        } 

		switch (equipScreenMode) {
		case eEquipScreenMode.pickPartyMember:
			if(previousSelectedGameObject != UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject) {
				// Position Cursor
				PlayerButtons.S.PositionCursor();

				// Display currently selected Member's Stats/Equipment 
				for (int i = 0; i < PlayerButtons.S.buttonsCS.Count; i++) {
					PlayerButtons.S.anim[i].CrossFade("Idle", 0);

					if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == PlayerButtons.S.buttonsCS[i].gameObject) {
						// Cache Selected Gameobject 
						previousSelectedGameObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

						DisplayCurrentStats(i);
						DisplayCurrentEquipmentNames(i);

						playerAnim.runtimeAnimatorController = PlayerButtons.S.anim[i].runtimeAnimatorController;
						PlayerButtons.S.anim[i].CrossFade("Walk", 0);
					}
				}
			}

			// Deactivate EquipScreen
			if (Input.GetButtonDown("SNES B Button")) {
				gameObject.SetActive(false);
			}
			break;
		case eEquipScreenMode.pickTypeToEquip:
			if (canUpdate) {
				DisplayCurrentEquipmentDescriptions (playerNdx);
				// Cache Selected Gameobject 
				previousSelectedGameObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
				canUpdate = false; 
			}

			// Go back to pickPartyMember
			if (PauseMessage.S.dialogueFinished) {
				if (Input.GetButtonDown ("SNES B Button")) {
					PickPartyMemberMode();
				}
			}
			break;
		case eEquipScreenMode.pickItemToEquip:
			if (canUpdate) {
				DisplayInventoryDescriptions (playerNdx);
				canUpdate = false; 
			}

			// Go back to pickTypeToEquip
			GoBackToPickTypeToEquipMode("SNES B Button");
			break;
		case eEquipScreenMode.noInventory:
		case eEquipScreenMode.equippedItem:
			// Go back to pickTypeToEquip
			GoBackToPickTypeToEquipMode("SNES A Button");
			break;
		}
	}

	// Set up and go to eEquipScreenMode.pickPartyMember
	void PickPartyMemberMode() {
		playerAnim.CrossFade("Idle", 0);

		// Switch mode
		SwitchMode(eEquipScreenMode.pickPartyMember, PlayerButtons.S.buttonsCS[playerNdx].gameObject, false);

		// Buttons Interactable
		Utilities.S.ButtonsInteractable(equippedButtons, false);
		Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, true);
		Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, false);

		// Remove & Add Listeners
		Utilities.S.RemoveListeners(PlayerButtons.S.buttonsCS);
		PlayerButtons.S.buttonsCS[0].onClick.AddListener(delegate { PickTypeToEquipMode(0); });
		PlayerButtons.S.buttonsCS[1].onClick.AddListener(delegate { PickTypeToEquipMode(1); });

		// Activate PlayerButtons
		PlayerButtons.S.gameObject.SetActive(true);

		// Display Text
		PauseMessage.S.DisplayText("Assign whose equipment?!");

		// Activate Cursor
		ScreenCursor.S.cursorGO.SetActive(true);
	}

	void GoBackToPickTypeToEquipMode(string inputName) {
		if (PauseMessage.S.dialogueFinished) {
			if (Input.GetButtonDown(inputName)) {
				// Deactivate Buttons
				for (int i = 0; i < inventoryButtons.Count; i++) {
					inventoryButtons[i].gameObject.SetActive(false);
				}

				PickTypeToEquipMode(playerNdx);

				// Set Selected Gameobject 
				Utilities.S.SetSelectedGO(previousSelectedGameObject);

				// Activate Cursor
				ScreenCursor.S.cursorGO.SetActive(true);
			}
		}
	}

	// eEquipScreenMode.pickPartyMember /////////////////////////////////////////////////////////////
	
	// Display member's name and current stats
	public void DisplayCurrentStats(int playerNdx){
		playerName.text = Stats.S.playerName [playerNdx];
		currentStats.text = Stats.S.LVL [playerNdx] + "\n" + Stats.S.HP [playerNdx] + "/" + Stats.S.maxHP[playerNdx] + "\n" + Stats.S.MP [playerNdx] + "/" + Stats.S.maxMP[playerNdx];
		currentAttributeAmounts.text = Stats.S.STR[playerNdx] + "\n" + Stats.S.DEF[playerNdx] + "\n" + Stats.S.WIS[playerNdx] + "\n" + Stats.S.AGI[playerNdx];
		potentialStats.text = "";
		playerImage.sprite = playerSprites[playerNdx];
	}

	// Display the names of the member's current equipment
	public void DisplayCurrentEquipmentNames(int playerNdx){ 
		// Set Button Text
		for(int i = 0; i < equippedButtonsTxt.Count; i++) {
			equippedButtonsTxt[i].text = playerEquipment[playerNdx][i].name;
		}
	}

	// eEquipScreenMode.pickTypeToEquip /////////////////////////////////////////////////////////////
	
	// Set up and go to eEquipScreenMode.pickTypeToEquip
	void PickTypeToEquipMode(int ndx){
		// Set anims
		playerAnim.CrossFade("Walk", 0);
		PlayerButtons.S.anim[ndx].CrossFade("Idle", 0);

		// Switch mode
		SwitchMode(eEquipScreenMode.pickTypeToEquip, equippedButtons[0].gameObject, false);

		// Buttons Interactable
		Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, false);
		Utilities.S.ButtonsInteractable(equippedButtons, true);

		playerNdx = ndx; 

		// Add Listeners
		AddListenersToEquippedButtons ();
	}

	// Add listeners to equipped buttons
	public void AddListenersToEquippedButtons(){
		// Remove and add listeners
		for (int i = 0; i < equippedButtons.Count; i++) {
			int tInt = i;
			equippedButtons[tInt].onClick.RemoveAllListeners();
			equippedButtons[tInt].onClick.AddListener(delegate { PickItemToEquipMode((eItemType)tInt); });
		}
	}

	// Display descriptions of party member's current equipment
	public void DisplayCurrentEquipmentDescriptions (int playerNdx) {
		for (int i = 0; i <= equippedButtons.Count - 1; i++) {
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == equippedButtons [i].gameObject) {
				// Display item's description
                PauseMessage.S.SetText(playerEquipment[playerNdx][i].description);

                // Set cursor position to currently selected button
                PositionCursor(equippedButtons[i].gameObject);
			}
		} 
	}

	// eEquipScreenMode.pickItemToEquip /////////////////////////////////////////////////////////////
	
	// Set up and go to eEquipScreenMode.pickItemToEquip
	public void PickItemToEquipMode (eItemType itemType){
		// Activate InventoryButtons 
		for (int i = 0; i < inventoryButtons.Count; i++) {
            inventoryButtons[i].gameObject.SetActive(true);
        }

        // Switch mode
        SwitchMode(eEquipScreenMode.pickItemToEquip, inventoryButtons[0].gameObject, false);

		// Buttons Interactable
		Utilities.S.ButtonsInteractable(inventoryButtons, true);
		Utilities.S.ButtonsInteractable(equippedButtons, false);

		// Sort Items by ItemType
		SortItems.S.SortByItemType (Inventory.S.items, itemType);

		// No items of this type
		if (SortItems.S.tItems.Count <= 0) {
			// Switch Mode 
			equipScreenMode = eEquipScreenMode.noInventory;

			ScreenCursor.S.cursorGO.SetActive (false);

			PauseMessage.S.DisplayText("You don't have any items of this type to equip!");
		}

		// Deactivate Unused Inventory Buttons Slots
		for (int i = 0; i <= inventoryButtons.Count - 1; i++) {
			if (i < SortItems.S.tItems.Count) {
				inventoryButtons [i].gameObject.SetActive (true);
			} else {
				inventoryButtons [i].gameObject.SetActive (false);
			} 
		}

		// Assign item names to buttons
		for (int i = 0; i <= SortItems.S.tItems.Count - 1; i++) {
			inventoryButtonsTxt [i].text = SortItems.S.tItems [i].name;
		}

		// Add Listeners
		AddListenersToInventoryButtons(playerNdx);
	}

	// Add listeners to inventory buttons
	public void AddListenersToInventoryButtons(int playerNdx){
		// Remove and add listeners
		for (int i = 0; i < inventoryButtons.Count; i++) {
			int tInt = i;
			inventoryButtons[tInt].onClick.RemoveAllListeners();
			inventoryButtons[tInt].onClick.AddListener(delegate { EquipItem(playerNdx, SortItems.S.tItems[tInt]); });
		}
	}

	// Display description of item to be potentially equipped
	public void DisplayInventoryDescriptions (int playerNdx) {
			if (SortItems.S.tItems != null) {
				for (int i = 0; i <= SortItems.S.tItems.Count - 1; i++) {
				if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == inventoryButtons [i].gameObject) {
					// Display item's description
					PauseMessage.S.SetText(SortItems.S.tItems[i].description);

					// Set cursor position to currently selected button
					PositionCursor(inventoryButtons[i].gameObject);

					// Calculate and display potential stats
					DisplayPotentialStats (playerNdx, SortItems.S.tItems [i]);
				}
			} 
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

		// Subtract old item stat effect 
		RemoveItemStatEffect(playerNdx, playerEquipment[playerNdx][(int)item.type]);
		
		// Equip new item
		playerEquipment[playerNdx][(int)item.type] = item;

		PauseMessage.S.DisplayText(Stats.S.playerName[playerNdx] + " equipped " + item.name + "!");

		// Add Item StatEffect
		AddItemStatEffect(playerNdx, item);

		// Update GUI
		DisplayCurrentStats(playerNdx);
		DisplayCurrentEquipmentNames(playerNdx);
	}

	// Add item's stat effect to party member's stats
	public void AddItemStatEffect(int playerNdx, Item item) {
		switch (item.statEffect) {
			case eItemStatEffect.AGI: Stats.S.AGI[playerNdx] += item.statEffectValue; break;
			case eItemStatEffect.DEF: Stats.S.DEF[playerNdx] += item.statEffectValue; break;
			case eItemStatEffect.HP: Stats.S.HP[playerNdx] += item.statEffectValue; break;
			case eItemStatEffect.MP: Stats.S.MP[playerNdx] += item.statEffectValue; break;
			case eItemStatEffect.STR: Stats.S.STR[playerNdx] += item.statEffectValue; break;
			case eItemStatEffect.WIS: Stats.S.WIS[playerNdx] += item.statEffectValue; break;
		}
	}

	// Remove item's stat effect from party member's stats
	void RemoveItemStatEffect(int playerNdx, Item item) {
		// Subtract Item Effect
		switch (item.statEffect) {
			case eItemStatEffect.AGI: Stats.S.AGI[playerNdx] -= item.statEffectValue; break;
			case eItemStatEffect.DEF: Stats.S.DEF[playerNdx] -= item.statEffectValue; break;
			case eItemStatEffect.HP: Stats.S.HP[playerNdx] -= item.statEffectValue; break;
			case eItemStatEffect.MP: Stats.S.MP[playerNdx] -= item.statEffectValue; break;
			case eItemStatEffect.STR: Stats.S.STR[playerNdx] -= item.statEffectValue; break;
			case eItemStatEffect.WIS: Stats.S.WIS[playerNdx] -= item.statEffectValue; break;
		}
	}

	// Display party member's stats if they equipped this item
	void DisplayPotentialStats (int playerNdx, Item tItem){
		// Deactivate Arrow GameObjects
		for (int i = 0; i <= arrowGO.Count - 1; i++) {
			arrowGO[i].SetActive (false);
		}

        // Get Current Stats
        List<int> potential = new List<int>() { Stats.S.STR[playerNdx], Stats.S.DEF[playerNdx], Stats.S.WIS[playerNdx], Stats.S.AGI[playerNdx] };

        // Subtract stats of currently equipped item 
		switch (playerEquipment[playerNdx][(int)tItem.type].statEffect) {
			case eItemStatEffect.STR: potential[0] -= playerEquipment[playerNdx][(int)tItem.type].statEffectValue; break;
			case eItemStatEffect.DEF: potential[1] -= playerEquipment[playerNdx][(int)tItem.type].statEffectValue; break;
			case eItemStatEffect.WIS: potential[2] -= playerEquipment[playerNdx][(int)tItem.type].statEffectValue; break;
			case eItemStatEffect.AGI: potential[3] -= playerEquipment[playerNdx][(int)tItem.type].statEffectValue; break;
		}

		// Add stats of item to be potentially equipped
		switch (tItem.statEffect) {
			case eItemStatEffect.STR: potential[0] += tItem.statEffectValue; break;
			case eItemStatEffect.DEF: potential[1] += tItem.statEffectValue; break;
			case eItemStatEffect.WIS: potential[2] += tItem.statEffectValue; break;
			case eItemStatEffect.AGI: potential[3] += tItem.statEffectValue; break;
		}

		// Find difference between current & potential Stats
		List<int> statDifference = new List<int>() { potential[0] - Stats.S.STR[playerNdx], potential[1] - Stats.S.DEF[playerNdx], potential[2] - Stats.S.WIS [playerNdx], potential[3] - Stats.S.AGI[playerNdx] };

		// If Current Stats != Potential Stats, activate potential stats & arrows
		if (potential[0] != Stats.S.STR [playerNdx]) {
			ActivatePotentialStatsAndArrow(0, statDifference[0]);
		}
		if (potential[1] != Stats.S.DEF [playerNdx]) {
			ActivatePotentialStatsAndArrow(1, statDifference[1]);
		}
		if (potential[2] != Stats.S.WIS [playerNdx]) {
			ActivatePotentialStatsAndArrow(2, statDifference[2]);
		}
		if (potential[3] != Stats.S.AGI [playerNdx]) {
			ActivatePotentialStatsAndArrow(3, statDifference[3]);
		}

		// Update GUI
		potentialStats.text = potential[0] + "\n" + potential[1] + "\n" + potential[2] + "\n" + potential[3];
	}

	/////////////////////////////////////////////////////////////
	// Set cursor position to currently selected button
	void PositionCursor(GameObject selectedGO) {
		float tPosX = selectedGO.GetComponent<RectTransform>().anchoredPosition.x;
		float tPosY = selectedGO.GetComponent<RectTransform>().anchoredPosition.y;

		float tParentX = selectedGO.transform.parent.GetComponent<RectTransform>().anchoredPosition.x;
		float tParentY = selectedGO.transform.parent.GetComponent<RectTransform>().anchoredPosition.y;

		ScreenCursor.S.rectTrans.anchoredPosition = new Vector2((tPosX + tParentX + 160), (tPosY + tParentY));
	}

	void ActivatePotentialStatsAndArrow(int ndx, int amount) {
		// Activate Potential Stat
		potentialStatHolder.SetActive(true);

		// Activate Arrow GameObject
		arrowGO[ndx].SetActive(true);

		// Set Arrow Sprite
		if (amount > 0) {
			arrowAnim[ndx].CrossFade("Arrow_Up", 0);
		} else {
			arrowAnim[ndx].CrossFade("Arrow_Down", 0);
		}
	}

	void SwitchMode(eEquipScreenMode mode, GameObject selectedGO, bool potentialStats) {
		canUpdate = true;

		// Switch ScreenMode
		equipScreenMode = mode;

		// Set Selected GameObject 
		Utilities.S.SetSelectedGO(selectedGO);

		// Activate Potential Stat
		potentialStatHolder.SetActive(potentialStats);
	}
}
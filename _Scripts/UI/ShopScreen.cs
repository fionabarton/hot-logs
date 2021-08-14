using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public enum eShopScreenMode { pickItem, selectQuantity, itemPurchased, endOfTransaction };

public class ShopScreen : MonoBehaviour {

	[Header("Set in Inspector")]
	// Item "Buttons"	
	public List<Button> 	itemButtons;
	public List<Text> 		itemButtonNameTexts;

	[Header("Set Dynamically")]
	// Singleton
	private static ShopScreen _S;
	public static ShopScreen S { get { return _S; } set { _S = value; } }

	// Set in RPGEventTrigger
	public List<Item> 		inventory = new List<Item>();

	// For Input & Display Message
	public eShopScreenMode  shopScreenMode = eShopScreenMode.pickItem;

	public bool 			canUpdate;

	void Awake() {
		// Singleton
		S = this;
	}

	void OnEnable () {
		// Switch ScreenMode
		shopScreenMode = eShopScreenMode.pickItem;

		DeactivateUnusedItemSlots ();
		AssignItemNames (); // In case other than final item was used: assigns proper Button Name
		StartCoroutine("AssignItemEffect");

		canUpdate = true;

		try{
			// Activate PlayerButtons
			ScreenManager.S.playerButtonsGO.SetActive(true);

			Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, false);

			// Activate PauseScreen
			PauseMessage.S.gameObject.SetActive (true);
		
			// Activate Cursor
			ScreenCursor.S.cursorGO.SetActive (true);
		}catch(NullReferenceException){}
	}

	void OnDisable () {
		// Deactivate Cursor
		ScreenCursor.S.cursorGO.SetActive (false);
	}

	public void Loop () {
		// Reset canUpdate
		if (Input.GetAxisRaw ("Horizontal") != 0f || Input.GetAxisRaw ("Vertical") != 0f) { 
			canUpdate = true;
		}

		// Deactivate EquipScreen
		if(Input.GetButtonDown ("SNES B Button")) { 
			ScreenManager.S.ShopScreenOff(); 
		}

		switch (shopScreenMode) {
		case eShopScreenMode.pickItem:
			if (canUpdate) {
				DisplayItemDescriptions ();
				canUpdate = false; 
			}
			break;
		case eShopScreenMode.selectQuantity:
			break;
		case eShopScreenMode.itemPurchased:
			if (PauseMessage.S.dialogueFinished) {
				if (Input.GetButtonDown ("SNES A Button")) {
					OnEnable();
				}
			}
			break;
		case eShopScreenMode.endOfTransaction:
			break;
		}
	}

	void DeactivateUnusedItemSlots () {
		for (int i = 0; i <= itemButtons.Count - 1; i++) {
			if (i < inventory.Count) {
				itemButtons [i].gameObject.SetActive (true);
			} else {
				itemButtons [i].gameObject.SetActive (false);
			} 
		}
	}

	// This is a coroutine so PurchaseItem(inventory[0]) isn't called when Listener is Added
	public IEnumerator AssignItemEffect(){

		for (int i = 0; i <= itemButtons.Count - 1; i++) {
			int copy = i;
			itemButtons [i].onClick.RemoveAllListeners ();

			itemButtons[copy].onClick.AddListener(delegate { PurchaseItem(inventory[copy]); });
		}

		yield return new WaitForEndOfFrame ();
	}

	void AssignItemNames () {
		for (int i = 0; i <= inventory.Count - 1; i++) {
			itemButtonNameTexts [i].text = inventory[i].name + " " + inventory[i].value + " Gold";
		}
	}

	public void DisplayItemDescriptions () {
		for (int i = 0; i <= inventory.Count - 1; i++) {
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == itemButtons [i].gameObject) {

				PauseMessage.S.SetText(inventory[i].description);

				// Cursor Position set to Selected Button
				float tPosX = itemButtons [i].gameObject.GetComponent<RectTransform> ().anchoredPosition.x;
				float tPosY = itemButtons [i].gameObject.GetComponent<RectTransform> ().anchoredPosition.y;

				float tParentX = itemButtons [i].gameObject.transform.parent.GetComponent<RectTransform> ().anchoredPosition.x;
				float tParentY = itemButtons [i].gameObject.transform.parent.GetComponent<RectTransform> ().anchoredPosition.y;

				ScreenCursor.S.rectTrans.anchoredPosition = new Vector2 ((tPosX + tParentX + 160), (tPosY + tParentY));
			}
		}
	}

	void PurchaseItem (Item tItem) {
		canUpdate = true;

		// Switch ScreenMode 
		shopScreenMode = eShopScreenMode.itemPurchased;

		if (Stats.S.Gold >= tItem.value) {
			// Added to Player Inventory
			Inventory.S.AddItemToInventory(tItem);

			// Dialogue
			PauseMessage.S.DisplayText("Purchased!" + " For " + tItem.value + " gold!");

			// Subtract item price from Player's Gold
			Stats.S.Gold -= tItem.value;

			// Update Gold 
			PlayerButtons.S.goldValue.text = Stats.S.Gold.ToString();
		} else {
			// Dialogue
			PauseMessage.S.DisplayText("Not enough money!");
		}

		// Remove Listeners
		Utilities.S.RemoveListeners(itemButtons);

		// Deactivate Cursor
		ScreenCursor.S.cursorGO.SetActive (false);
	}

	// Called in ShopkeeperTrigger
	public void ImportInventory(List<eItem> sellerInventory){
		// Clear Inventory
		inventory.Clear ();

		// Import Inventory
		for (int i = 0; i < sellerInventory.Count; i++) {
			inventory.Add (ItemManager.S.GetItem(sellerInventory [i]));
		}
	}
}
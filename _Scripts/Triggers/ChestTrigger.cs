using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestTrigger : ActivateOnButtonPress {
	[Header ("Set in Inspector")]
	public eItem			item;

	// Gives the index of which chest has already been opened to ChestManager.cs
	public int				ndx;

	public Sprite			openChestTop, closedChestTop;

	public SpriteRenderer	sRendTop;

	[Header("Set Dynamically")]
	public bool				chestIsOpen;

	protected override void Action() {
		// Set Camera to Chest gameObject
		CamManager.S.ChangeTarget(gameObject, true);

		if (!chestIsOpen) {
			OpenChest();
		} else {
			// Display Dialogue
			DialogueManager.S.DisplayText("You've already looted this chest. It's empty, you greedy pig.");
		}
	}

	void OpenChest(){
		// Change Sprite
		sRendTop.sprite = openChestTop;

		// Add Item to Inventory
		Item tItem = ItemManager.S.GetItem (item);
		Inventory.S.AddItemToInventory(tItem);

		// Display Dialogue 
		DialogueManager.S.DisplayText ("Wow, a " + tItem.name + "! The party adds it to their inventory!");

		chestIsOpen = true;

		// Chest Manager
		ChestManager.S.isOpen[ndx] = true;
	}
}

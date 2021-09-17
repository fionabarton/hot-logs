using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Outside of battle, handles what happens when an item button is clicked
/// </summary>
public class WorldItems : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static WorldItems _S;
	public static WorldItems S { get { return _S; } set { _S = value; } }

	void Awake() {
		S = this;
	}

	public void AddFunctionToButton(Action<int> functionToPass, string messageToDisplay) { 
		// Buttons Interactable
		Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, true);
		Utilities.S.ButtonsInteractable(ItemScreen.S.itemButtons, false);

		// Set Selected GameObject
		Utilities.S.SetSelectedGO(PlayerButtons.S.buttonsCS[0].gameObject);

		// Remove Listeners
		Utilities.S.RemoveListeners(PlayerButtons.S.buttonsCS);

		// Display Text
		PauseMessage.S.DisplayText(messageToDisplay);

		// Add Listeners
		PlayerButtons.S.buttonsCS[0].onClick.AddListener(delegate { functionToPass(0); });
		PlayerButtons.S.buttonsCS[1].onClick.AddListener(delegate { functionToPass(1); });
	}

	public void HPPotion(int ndx) {
		if (Party.stats[ndx].HP < Party.stats[ndx].maxHP) {
			// Remove from Inventory
			Inventory.S.RemoveItemFromInventory(ItemManager.S.items[0]);

			// Add 30-45 HP to TARGET Player's HP
			int randomValue = UnityEngine.Random.Range(30, 45);
			RPG.S.AddPlayerHP(ndx, randomValue);

			// Display Text
			if (Party.stats[ndx].HP >= Party.stats[ndx].maxHP) {
				PauseMessage.S.DisplayText("Used Heal Potion!\nHealed " + Party.stats[ndx].name + " back to Max HP!");
			} else {
				PauseMessage.S.DisplayText("Used Heal Potion!\nHealed " + Party.stats[ndx].name + " for " + randomValue + " HP!");
			}

			// Set animation to success
			PlayerButtons.S.anim[ndx].CrossFade("Success", 0);
		} else {
			// Display Text
			PauseMessage.S.DisplayText(Party.stats[ndx].name + " already at full health...\n...no need to use this potion!");
		}
		ClickedButtonHelper();
	}

	public void MPPotion(int ndx) {
		if (Party.stats[ndx].MP < Party.stats[ndx].maxMP) {
			// Remove from Inventory
			Inventory.S.RemoveItemFromInventory(ItemManager.S.items[1]);

			// Add 30-45 MP to TARGET Player's MP
			int randomValue = UnityEngine.Random.Range(30, 45);
			RPG.S.AddPlayerMP(ndx, randomValue);

			// Display Text
			if (Party.stats[ndx].MP >= Party.stats[ndx].maxMP) {
				PauseMessage.S.DisplayText("Used Magic Potion!\n" + Party.stats[ndx].name + " back to Max MP!");
			} else {
				PauseMessage.S.DisplayText("Used Magic Potion!\n" + Party.stats[ndx].name + " gained " + randomValue + " MP!");
			}

			// Set animation to success
			PlayerButtons.S.anim[ndx].CrossFade("Success", 0);
		} else {
			// Display Text
			PauseMessage.S.DisplayText(Party.stats[ndx].name + " already at full magic...\n...no need to use this potion!");
		}
		ClickedButtonHelper();
	}

	public void ClickedButtonHelper() {
		// Buttons Interactable
		Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, false);
		Utilities.S.ButtonsInteractable(ItemScreen.S.itemButtons, true);

		// Update GUI
		PlayerButtons.S.UpdateGUI();
		PauseScreen.S.UpdateGUI();

		// Deactivate Cursor
		ScreenCursor.S.cursorGO.SetActive(false);

		ItemScreen.S.canUpdate = true;

		// Switch ScreenMode 
		ItemScreen.S.itemScreenMode = eItemScreenMode.usedItem;
	}

	public void CantUseItem() {
		Utilities.S.ButtonsInteractable(ItemScreen.S.itemButtons, false);
		ItemScreen.S.itemScreenMode = eItemScreenMode.usedItem;
		PauseMessage.S.DisplayText("This item is not usable... sorry!");
		// Deactivate Cursor
		ScreenCursor.S.cursorGO.SetActive(false);
	}
}
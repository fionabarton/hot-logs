﻿using System.Collections;
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

	public void AddFunctionToButton(Action<int> functionToPass, string messageToDisplay, Item item) { 
		// Buttons Interactable
		Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, true);
		Utilities.S.ButtonsInteractable(ItemScreen.S.itemButtons, false);

		// Set Selected GameObject
		Utilities.S.SetSelectedGO(PlayerButtons.S.buttonsCS[0].gameObject);

		// Set previously selected GameObject
		ItemScreen_PickPartyMemberMode.S.previousSelectedPlayerGO = PlayerButtons.S.buttonsCS[0].gameObject;

		// Remove Listeners
		Utilities.S.RemoveListeners(PlayerButtons.S.buttonsCS);

		// Display Text
		PauseMessage.S.DisplayText(messageToDisplay);

		// Audio: Confirm
		AudioManager.S.PlaySFX(eSoundName.confirm);

		// Add Listeners
		PlayerButtons.S.buttonsCS[0].onClick.AddListener(delegate { functionToPass(0); });
		PlayerButtons.S.buttonsCS[1].onClick.AddListener(delegate { functionToPass(1); });
		PlayerButtons.S.buttonsCS[2].onClick.AddListener(delegate { functionToPass(2); });

		// If multiple targets
		if (!item.multipleTargets) {
			// Set animation to idle
			PlayerButtons.S.SetSelectedAnim("Idle");

			ItemScreen.S.mode = eItemScreenMode.pickPartyMember;
		} else {
			for (int i = 0; i <= Party.S.partyNdx; i++) {
				// Set cursor positions
				Utilities.S.PositionCursor(PlayerButtons.S.buttonsCS[i].gameObject, 0, 60, 3, i);

				// Set animations to walk
				PlayerButtons.S.anim[i].CrossFade("Walk", 0);

				// Activate cursors
				ScreenCursor.S.cursorGO[i].SetActive(true);
			}

			// Set button colors
			PlayerButtons.S.SetButtonsColor(PlayerButtons.S.buttonsCS, new Color32(253, 255, 116, 255));

			ItemScreen.S.mode = eItemScreenMode.pickAllPartyMembers;
		}
	}

	public void HPPotion(int ndx) {
		if (Party.S.stats[ndx].HP < Party.S.stats[ndx].maxHP) {
			// Remove from Inventory
			Inventory.S.RemoveItemFromInventory(ItemManager.S.items[0]);

			// Add 30-45 HP to TARGET Player's HP
			int randomValue = UnityEngine.Random.Range(30, 45);
			RPG.S.AddPlayerHP(ndx, randomValue);

			// Display Text
			if (Party.S.stats[ndx].HP >= Party.S.stats[ndx].maxHP) {
				PauseMessage.S.DisplayText("Used Heal Potion!\nHealed " + Party.S.stats[ndx].name + " back to Max HP!");
			} else {
				PauseMessage.S.DisplayText("Used Heal Potion!\nHealed " + Party.S.stats[ndx].name + " for " + randomValue + " HP!");
			}

			// Set animation to success
			PlayerButtons.S.anim[ndx].CrossFade("Success", 0);

			// Audio: Buff 1
			AudioManager.S.PlaySFX(eSoundName.buff1);
		} else {
			// Display Text
			PauseMessage.S.DisplayText(Party.S.stats[ndx].name + " already at full health...\n...no need to use this potion!");

			// Set animation to idle
			PlayerButtons.S.anim[ndx].CrossFade("Idle", 0);

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);
		}
		ClickedButtonHelper();
	}

	public void MPPotion(int ndx) {
		if (Party.S.stats[ndx].MP < Party.S.stats[ndx].maxMP) {
			// Remove from Inventory
			Inventory.S.RemoveItemFromInventory(ItemManager.S.items[1]);

			// Add 30-45 MP to TARGET Player's MP
			int randomValue = UnityEngine.Random.Range(30, 45);
			RPG.S.AddPlayerMP(ndx, randomValue);

			// Display Text
			if (Party.S.stats[ndx].MP >= Party.S.stats[ndx].maxMP) {
				PauseMessage.S.DisplayText("Used Magic Potion!\n" + Party.S.stats[ndx].name + " back to Max MP!");
			} else {
				PauseMessage.S.DisplayText("Used Magic Potion!\n" + Party.S.stats[ndx].name + " gained " + randomValue + " MP!");
			}

			// Set animation to success
			PlayerButtons.S.anim[ndx].CrossFade("Success", 0);

			// Audio: Buff 1
			AudioManager.S.PlaySFX(eSoundName.buff1);
		} else {
			// Display Text
			PauseMessage.S.DisplayText(Party.S.stats[ndx].name + " already at full magic...\n...no need to use this potion!");

			// Set animation to idle
			PlayerButtons.S.anim[ndx].CrossFade("Idle", 0);

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);
		}
		ClickedButtonHelper();
	}

	public void HealAllPotion(int unusedIntBecauseOfAddFunctionToButtonParameter = 0) {
		int totalAmountToHeal = 0;

		if (Party.S.stats[0].HP < Party.S.stats[0].maxHP ||
			Party.S.stats[1].HP < Party.S.stats[1].maxHP ||
			Party.S.stats[2].HP < Party.S.stats[2].maxHP) {
			for (int i = 0; i < Party.S.stats.Count; i++) {
				// Get amount and max amount to heal
				int amountToHeal = UnityEngine.Random.Range(12, 20);
				int maxAmountToHeal = Party.S.stats[i].maxHP - Party.S.stats[i].HP;
				// Add Player's WIS to Heal Amount
				amountToHeal += Party.S.stats[i].WIS;

				// Add 12-20 HP to TARGET Player's HP
				RPG.S.AddPlayerHP(i, amountToHeal);

				// Cap amountToHeal to maxAmountToHeal
				if (amountToHeal >= maxAmountToHeal) {
					amountToHeal = maxAmountToHeal;
				}

				totalAmountToHeal += amountToHeal;
			}

			// Remove from Inventory
			Inventory.S.RemoveItemFromInventory(ItemManager.S.items[22]);

			// Display Text
			PauseMessage.S.DisplayText("Used Heal All Potion!\nHealed ALL party members for an average of "
				+ Utilities.S.CalculateAverage(totalAmountToHeal, Party.S.stats.Count) + " HP!");

			// Set animations to success
			for (int i = 0; i <= Party.S.partyNdx; i++) {
				PlayerButtons.S.anim[i].CrossFade("Success", 0);
			}

			// Audio: Buff 1
			AudioManager.S.PlaySFX(eSoundName.buff1);
		} else {
			// Display Text
			PauseMessage.S.DisplayText("The party is already at full health...\n...no need to use this potion!");

			// Set animations to idle
			for (int i = 0; i <= Party.S.partyNdx; i++) {
				PlayerButtons.S.anim[i].CrossFade("Idle", 0);
			}

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);
		}

		// Reset button colors
		PlayerButtons.S.SetButtonsColor(PlayerButtons.S.buttonsCS, new Color32(255, 255, 255, 200));

		// Deactivate screen cursors
		Utilities.S.SetActiveList(ScreenCursor.S.cursorGO, false);

		ClickedButtonHelper();
	}

	public void WarpPotion() {
		ItemScreen.S.mode = eItemScreenMode.pickWhereToWarp;

		// Set Selected GameObject
		Utilities.S.SetSelectedGO(ItemScreen.S.itemButtons[0].gameObject);

		// Set previously selected GameObject
		WarpManager.S.previousSelectedLocationGO = ItemScreen.S.itemButtons[0].gameObject;

		// Use ItemScreen's buttons to select/display warp locations
		WarpManager.S.DeactivateUnusedButtonSlots(ItemScreen.S.itemButtons);
		WarpManager.S.AssignButtonEffect(ItemScreen.S.itemButtons);
		WarpManager.S.AssignButtonNames(ItemScreen.S.itemButtonsNameText);
		WarpManager.S.SetButtonNavigation(ItemScreen.S.itemButtons);

		// Audio: Confirm
		AudioManager.S.PlaySFX(eSoundName.confirm);
	}

	public void ClickedButtonHelper() {
		// Buttons Interactable
		Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, false);
		Utilities.S.ButtonsInteractable(ItemScreen.S.itemButtons, true);

		// Update GUI
		PlayerButtons.S.UpdateGUI();
		PauseScreen.S.UpdateGUI();

		// Deactivate screen cursors
		Utilities.S.SetActiveList(ScreenCursor.S.cursorGO, false);

		ItemScreen.S.canUpdate = true;

		// Switch ScreenMode 
		ItemScreen.S.mode = eItemScreenMode.usedItem;
	}

	public void CantUseItem() {
		Utilities.S.ButtonsInteractable(ItemScreen.S.itemButtons, false);
		ItemScreen.S.mode = eItemScreenMode.usedItem;
		PauseMessage.S.DisplayText("This item is not usable... sorry!");
		
		// Deactivate screen cursors
		Utilities.S.SetActiveList(ScreenCursor.S.cursorGO, false);

		// Audio: Deny
		AudioManager.S.PlaySFX(eSoundName.deny);
	}
}
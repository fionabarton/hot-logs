﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// During battle, handles what happens when an item button is clicked
/// </summary>
public class BattleItems : MonoBehaviour {
	[Header ("Set Dynamically")]
	int amountToHeal;
	int maxAmountToHeal;

	private static BattleItems _S;
	public static BattleItems S { get { return _S; } set { _S = value; } }

	private Battle _;

	void Awake() {
		S = this;
	}

	void Start () {
		_ = Battle.S;
	}

	public void AddFunctionToButton(Action<int, Item> functionToPass, string messageToDisplay, Item item) {
		ItemScreen.S.Deactivate();

		// Deactivate PauseMessage
		PauseMessage.S.gameObject.SetActive (false);

		BattlePlayerActions.S.ButtonsInteractable (false, false, false, false, false, false, false, false, true, true);

		// Set a Player Button as Selected GameObject
		Utilities.S.SetSelectedGO(BattlePlayerActions.S.playerButtonGO[_.PlayerNdx()].gameObject);

		// Set previously selected GameObject
		_.previousSelectedForAudio = BattlePlayerActions.S.playerButtonGO[_.PlayerNdx()].gameObject;

		BattleDialogue.S.DisplayText (messageToDisplay);

		// Audio: Confirm
		AudioManager.S.PlaySFX(eSoundName.confirm);

		// Add Item Listeners to Player Buttons
		BattlePlayerActions.S.playerButtonCS[0].onClick.AddListener(delegate { functionToPass(0, item); });
		BattlePlayerActions.S.playerButtonCS[1].onClick.AddListener(delegate { functionToPass(1, item); });
		BattlePlayerActions.S.playerButtonCS[2].onClick.AddListener(delegate { functionToPass(2, item); });

		// If multiple targets
		if (!item.multipleTargets) {
			_.battleMode = eBattleMode.canGoBackToFightButton;
		} else {
			_.battleMode = eBattleMode.canGoBackToFightButtonMultipleTargets;
			BattleUI.S.TargetAllPartyMembers();
		}
	}

	public void Heal(int ndx, Item item, int min, int max) {
		// Get amount and max amount to heal
		amountToHeal = UnityEngine.Random.Range(min, max);
		maxAmountToHeal = Party.S.stats[ndx].maxHP - Party.S.stats[ndx].HP;

		// Cap amountToHeal to maxAmountToHeal
		if (amountToHeal >= maxAmountToHeal) {
			amountToHeal = maxAmountToHeal;
		}

		// Add to TARGET Player's HP
		RPG.S.AddPlayerHP(ndx, amountToHeal);

		CurePlayerAnimation(ndx, true, amountToHeal);
	}

	public void HPPotion(int ndx, Item item) {
        if (_.playerDead[ndx]) {
			ItemIsNotUseful(Party.S.stats[ndx].name + " is dead...\n...and dead folk can't be healed, dummy!");
			return;
		}

        // If HP is less than maxHP
        if (Party.S.stats[ndx].HP < Party.S.stats[ndx].maxHP) {
			Heal(ndx, item, item.statEffectMinValue, item.statEffectMaxValue);

			// Display Text
			if (amountToHeal >= maxAmountToHeal) {
                BattleDialogue.S.DisplayText("Used " + item.name + "!\nHealed " + Party.S.stats[ndx].name + " back to Max HP!");
            } else {
                BattleDialogue.S.DisplayText("Used " + item.name + "!\nHealed " + Party.S.stats[ndx].name + " for " + amountToHeal + " HP!");
            }

			ItemIsUseful(item);
		} else {
			ItemIsNotUseful(Party.S.stats[ndx].name + " already at full health...\n...no need to use this potion!");
		}
    }

	public void MPPotion(int ndx, Item item) {
		if (_.playerDead[ndx]) {
			ItemIsNotUseful(Party.S.stats[ndx].name + " is dead...\n...and dead folk can't be healed, dummy!");
			return;
		}

		// If MP is less than maxMP
		if (Party.S.stats[ndx].MP < Party.S.stats[ndx].maxMP) {
			// Get amount and max amount to heal
			amountToHeal = UnityEngine.Random.Range(item.statEffectMinValue, item.statEffectMaxValue);
			maxAmountToHeal = Party.S.stats[ndx].maxMP - Party.S.stats[ndx].MP;

			// Add 12-20 MP to TARGET Player's MP
			RPG.S.AddPlayerMP(ndx, amountToHeal);

			// Display Text
			if (amountToHeal >= maxAmountToHeal) {
				BattleDialogue.S.DisplayText("Used " + item.name + "!\n" + Party.S.stats[ndx].name + " back to Max MP!");

				// Prevents Floating Score being higher than the acutal amount healed
				amountToHeal = maxAmountToHeal;
			} else {
				BattleDialogue.S.DisplayText("Used " + item.name + "!\n" + Party.S.stats[ndx].name + " gained " + amountToHeal + " MP!");
			}

			CurePlayerAnimation(ndx, true, amountToHeal, false);

			ItemIsUseful(item);
		} else {
			ItemIsNotUseful(Party.S.stats[ndx].name + " already at full magic...\n...no need to use this potion!");
		}
	}

	public void HealAllPotion(int unusedIntBecauseOfAddFunctionToButtonParameter, Item item) {
		int totalAmountToHeal = 0;

		if (Party.S.stats[0].HP < Party.S.stats[0].maxHP ||
			Party.S.stats[1].HP < Party.S.stats[1].maxHP ||
			Party.S.stats[2].HP < Party.S.stats[2].maxHP) {
			for (int i = 0; i < _.playerDead.Count; i++) {
				if (!_.playerDead[i]) {
					Heal(i, item, item.statEffectMinValue, item.statEffectMaxValue);

					totalAmountToHeal += amountToHeal;
				}
			}

			// Display Text
			BattleDialogue.S.DisplayText("Used " + item.name + "!\nHealed ALL party members for an average of "
				+ Utilities.S.CalculateAverage(totalAmountToHeal, _.playerDead.Count) + " HP!");

			ItemIsUseful(item);
		} else {
			ItemIsNotUseful("The party is already at full health...\n...no need to use this potion!");
		}
	}

	public void RevivePotion(int ndx, Item item) {
		if (_.playerDead[ndx]) {
			_.playerDead[ndx] = false;

			// Add to PartyQty 
			_.partyQty += 1;

			// Add Player to Turn Order
			 _.turnOrder.Add(Party.S.stats[ndx].name);

			// Get 6-10% of max HP
			float lowEnd = Party.S.stats[ndx].maxHP * 0.06f;
			float highEnd = Party.S.stats[ndx].maxHP * 0.10f;
			Heal(ndx, item, (int)lowEnd, (int)highEnd);

			// Display Text
			if (amountToHeal >= maxAmountToHeal) {
				BattleDialogue.S.DisplayText("Used " + item.name + "!\nHealed " + Party.S.stats[ndx].name + " back to Max HP!");
			} else {
				BattleDialogue.S.DisplayText("Used " + item.name + "!\nHealed " + Party.S.stats[ndx].name + " for " + amountToHeal + " HP!");
			}

			ItemIsUseful(item);
		} else {
			ItemIsNotUseful(Party.S.stats[ndx].name + " ain't dead...\n...and dead folk don't need to be revived, dummy!");
		}
	}

	public void DetoxifyPotion(int ndx, Item item) {
		if (_.playerDead[ndx]) {
			ItemIsNotUseful(Party.S.stats[ndx].name + " is dead...\n...and dead folk don't need to be detoxified, dummy!");
			return;
		}

		if (BattleStatusEffects.S.CheckIfPoisoned(Party.S.stats[ndx].name)) {
			// Remove poison
			BattleStatusEffects.S.RemovePoisoned(Party.S.stats[ndx].name);

			// Deactivate status ailment icon
			BattleStatusEffects.S.playerPoisonedIcons[ndx].SetActive(false);

			// Display Text
			BattleDialogue.S.DisplayText("Used " + item.name + "!\n" + Party.S.stats[ndx].name + " is no longer poisoned!");

			CurePlayerAnimation(ndx);

			ItemIsUseful(item);
		} else {
			ItemIsNotUseful(Party.S.stats[ndx].name + " is not suffering from the effects of poison...\n...no need to use this potion!");
		}
	}

	public void DisableButtonsAndRemoveListeners() {
		BattlePlayerActions.S.ButtonsDisableAll();
		Utilities.S.RemoveListeners(BattlePlayerActions.S.playerButtonCS);
	}

	public void ItemIsUseful(Item item) {
		// Remove from Inventory
		Inventory.S.RemoveItemFromInventory(item);

		// Audio: Buff 1
		AudioManager.S.PlaySFX(eSoundName.buff1);

		_.NextTurn();

		DisableButtonsAndRemoveListeners();
	}

	public void ItemIsNotUseful(string message) {
		// Display Text
		BattleDialogue.S.DisplayText(message);

		// Deactivate Cursors
		Utilities.S.SetActiveList(BattleUI.S.targetCursors, false);

		// Audio: Deny
		AudioManager.S.PlaySFX(eSoundName.deny);

        // Switch Mode
        if (BattleStatusEffects.S.HasStatusAilment(Party.S.stats[_.PlayerNdx()].name)) {
			_.battleMode = eBattleMode.statusAilment;
		} else {
			_.battleMode = eBattleMode.playerTurn;
		}

		DisableButtonsAndRemoveListeners();
	}

	public void CantUseItemInBattle() {
		ItemScreen.S.Deactivate();

		// Deactivate PauseMessage
		PauseMessage.S.gameObject.SetActive(false);

		BattlePlayerActions.S.ButtonsInteractable(false, false, false, false, false, false, false, false, true, true);

		BattleDialogue.S.DisplayText("This item ain't usable in battle... sorry!");

		// Audio: Deny
		AudioManager.S.PlaySFX(eSoundName.deny);

		// Switch Mode
		_.battleMode = eBattleMode.playerTurn;
	}

	public void CurePlayerAnimation(int ndx, bool displayFloatingScore = false, int scoreAmount = 0, bool greenOrBlue = true) {
		// Get and position Poof game object
		GameObject poof = ObjectPool.S.GetPooledObject("Poof");
		ObjectPool.S.PosAndEnableObj(poof, _.playerSprite[ndx]);

		// Display Floating Score
		if (displayFloatingScore) {
            if (greenOrBlue) {
				RPG.S.InstantiateFloatingScore(_.playerSprite[ndx], scoreAmount, Color.green);
			} else {
				RPG.S.InstantiateFloatingScore(_.playerSprite[ndx], scoreAmount, new Color32(39, 201, 255, 255));
			}
		}

		// Set anim
		_.playerAnimator[ndx].CrossFade("Win_Battle", 0);
	}
}
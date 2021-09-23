using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// During battle, handles what happens when an item button is clicked
/// </summary>
public class BattleItems : MonoBehaviour {
	[Header ("Set Dynamically")]
	// Singleton
	private static BattleItems _S;
	public static BattleItems S { get { return _S; } set { _S = value; } }

	private Battle _;

	void Awake() {
		S = this;
	}

	void Start () {
		_ = Battle.S;
	}

	public void AddFunctionToButton(Action<int> functionToPass, string messageToDisplay, Item item) {
		ItemScreen.S.Deactivate();

		// Deactivate PauseMessage
		PauseMessage.S.gameObject.SetActive (false);

		BattlePlayerActions.S.ButtonsInteractable (false, false, false, false, false, false, false, false, true, true);

		// Set a Player Button as Selected GameObject
		Utilities.S.SetSelectedGO(BattlePlayerActions.S.playerButtonGO[_.PlayerNdx()].gameObject);

		BattleDialogue.S.DisplayText (messageToDisplay);

		// Add Item Listeners to Player Buttons
		BattlePlayerActions.S.playerButtonCS[0].onClick.AddListener(delegate { functionToPass(0); });
		BattlePlayerActions.S.playerButtonCS[1].onClick.AddListener(delegate { functionToPass(1); });

		// If multiple targets
		if (!item.multipleTargets) {
			_.battleMode = eBattleMode.canGoBackToFightButton;
		} else {
			_.battleMode = eBattleMode.canGoBackToFightButtonMultipleTargets;
			BattleUI.S.TargetAllPartyMembers();
		}
	}

    public void HPPotion(int ndx) {
        if (_.playerDead[ndx]) {
            // Display Text
            BattleDialogue.S.DisplayText(Party.stats[ndx].name + " is dead...\n...and dead folk can't be healed, dummy!");

            // Switch Mode
            _.battleMode = eBattleMode.playerTurn;

            DisableButtonsAndRemoveListeners();

            return;
        }

        // If HP is less than maxHP
        if (Party.stats[ndx].HP < Party.stats[ndx].maxHP) {
            // Get amount and max amount to heal
            int amountToHeal = UnityEngine.Random.Range(30, 45);
            int maxAmountToHeal = Party.stats[ndx].maxHP - Party.stats[ndx].HP;

            // Add 30-45 HP to TARGET Player's HP
            RPG.S.AddPlayerHP(ndx, amountToHeal);

            // Remove from Inventory
            Inventory.S.RemoveItemFromInventory(ItemManager.S.items[0]);

            // Display Text
            if (amountToHeal >= maxAmountToHeal) {
                BattleDialogue.S.DisplayText("Used Health Potion!\nHealed " + Party.stats[ndx].name + " back to Max HP!");

                // Prevents Floating Score being higher than the acutal amount healed
                amountToHeal = maxAmountToHeal;
            } else {
                BattleDialogue.S.DisplayText("Used Health Potion!\nHealed " + Party.stats[ndx].name + " for " + amountToHeal + " HP!");
            }

            // Get and position Poof game object
            GameObject poof = ObjectPool.S.GetPooledObject("Poof");
            ObjectPool.S.PosAndEnableObj(poof, _.playerSprite[ndx]);

            // Display Floating Score
            RPG.S.InstantiateFloatingScore(_.playerSprite[ndx], amountToHeal, Color.green);

            _.NextTurn();
        } else {
            BattleDialogue.S.DisplayText(Party.stats[ndx].name + " already at full health...\n...no need to use this potion!");

            // Switch Mode
            _.battleMode = eBattleMode.playerTurn;
        }
        DisableButtonsAndRemoveListeners();
    }

    public void MPPotion(int ndx){
		if (_.playerDead[ndx]) {
			// Display Text
			BattleDialogue.S.DisplayText(Party.stats[ndx].name + " is dead...\n...and dead folk can't be healed, dummy!");

			// Switch Mode
			_.battleMode = eBattleMode.playerTurn;

			DisableButtonsAndRemoveListeners();

			return;
		}

		// If MP is less than maxMP
		if (Party.stats[ndx].MP < Party.stats[ndx].maxMP) {
			// Get amount and max amount to heal
			int amountToHeal = UnityEngine.Random.Range(30, 45);
			//int maxAmountToHeal = Stats.S.maxMP[ndx] - Stats.S.MP[ndx];
			int maxAmountToHeal = Party.stats[ndx].maxMP - Party.stats[ndx].MP;

			// Add 30-45 MP to TARGET Player's MP
			RPG.S.AddPlayerMP(ndx, amountToHeal);

			// Remove from Inventory
			Inventory.S.RemoveItemFromInventory(ItemManager.S.items[1]);

			// Display Text
			if (amountToHeal >= maxAmountToHeal) {
				BattleDialogue.S.DisplayText("Used Magic Potion!\n" + Party.stats[ndx].name + " back to Max MP!");

				// Prevents Floating Score being higher than the acutal amount healed
				amountToHeal = maxAmountToHeal;
			} else {
				BattleDialogue.S.DisplayText("Used Magic Potion!\n" + Party.stats[ndx].name + " gained " + amountToHeal + " MP!");
			}

			// Get and position Poof game object
			GameObject poof = ObjectPool.S.GetPooledObject("Poof");
			ObjectPool.S.PosAndEnableObj(poof, _.playerSprite[ndx]);

			// Display Floating Score
			RPG.S.InstantiateFloatingScore(_.playerSprite[ndx], amountToHeal, Color.green);

			_.NextTurn ();
		} else {
			BattleDialogue.S.DisplayText(Party.stats[ndx].name + " already at full magic...\n...no need to use this potion!");

			// Switch Mode
			_.battleMode = eBattleMode.playerTurn;
		}
		DisableButtonsAndRemoveListeners();
	}

	public void HealAllPotion(int unusedIntBecauseOfAddFunctionToButtonParameter = 0) {
		int totalAmountToHeal = 0;

		if (Party.stats[0].HP < Party.stats[0].maxHP ||
			Party.stats[1].HP < Party.stats[1].maxHP) {
			for (int i = 0; i < _.playerDead.Count; i++) {
				if (!_.playerDead[i]) {

					// Get amount and max amount to heal
					int amountToHeal = UnityEngine.Random.Range(12, 20);
					int maxAmountToHeal = Party.stats[i].maxHP - Party.stats[i].HP;
					// Add Player's WIS to Heal Amount
					amountToHeal += Party.stats[i].WIS;

					// Add 12-20 HP to TARGET Player's HP
					RPG.S.AddPlayerHP(i, amountToHeal);

					// Cap amountToHeal to maxAmountToHeal
					if (amountToHeal >= maxAmountToHeal) {
						amountToHeal = maxAmountToHeal;
					}

					totalAmountToHeal += amountToHeal;

					// Get and position Poof game object
					GameObject poof = ObjectPool.S.GetPooledObject("Poof");
					ObjectPool.S.PosAndEnableObj(poof, _.playerSprite[i]);

					// Display Floating Score
					RPG.S.InstantiateFloatingScore(_.playerSprite[i], amountToHeal, Color.green);
				}
			}

			// Remove from Inventory
			Inventory.S.RemoveItemFromInventory(ItemManager.S.items[22]);

			// Display Text
			BattleDialogue.S.DisplayText("Used Heal All Potion!\nHealed ALL party members for an average of "
				+ Utilities.S.CalculateAverage(totalAmountToHeal, _.playerDead.Count) + " HP!");

			_.NextTurn();
		} else {
			// Display Text
			BattleDialogue.S.DisplayText("The party is already at full health...\n...no need to use this potion!");

			// Deactivate Cursors
			Utilities.S.SetActiveList(BattleUI.S.targetCursors, false);

			// Switch Mode
			_.battleMode = eBattleMode.playerTurn;
		}
		DisableButtonsAndRemoveListeners();
	}

	public void DisableButtonsAndRemoveListeners() {
		BattlePlayerActions.S.ButtonsDisableAll();
		Utilities.S.RemoveListeners(BattlePlayerActions.S.playerButtonCS);
	}

	public void CantUseItem() {
		ItemScreen.S.Deactivate();

		// Deactivate PauseMessage
		PauseMessage.S.gameObject.SetActive(false);

		BattlePlayerActions.S.ButtonsInteractable(false, false, false, false, false, false, false, false, true, true);

		BattleDialogue.S.DisplayText("This item ain't usable in battle... sorry!");

		// Switch Mode
		_.battleMode = eBattleMode.playerTurn;
	}
}
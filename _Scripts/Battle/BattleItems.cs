using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

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

	public void AddFunctionToButton(Action<int> functionToPass, string messageToDisplay) {
		ItemScreen.S.gameObject.SetActive(false);

		// Deactivate PauseMessage
		PauseMessage.S.gameObject.SetActive (false);

		BattlePlayerActions.S.ButtonsInteractable (false, false, false, false, false, false, false, false, true, true);

		// Set a Player Button as Selected GameObject
		BattlePlayerActions.S.SetSelectedPlayerButton ();

		BattleDialogue.S.DisplayText (messageToDisplay);

		// Switch Mode
		_.battleMode = eBattleMode.canGoBackToFightButton;

		// Add Item Listeners to Player Buttons
		BattlePlayerActions.S.playerButtonCS[0].onClick.AddListener(delegate { functionToPass(0); });
		BattlePlayerActions.S.playerButtonCS[1].onClick.AddListener(delegate { functionToPass(1); });
	}

	public void HPPotion(int ndx){
		if (_.playerDead[ndx]) {
			// Display Text
			BattleDialogue.S.DisplayText(PartyStats.S.playerName[ndx] + " is dead...\n...and dead folk can't be healed, dummy!");

			// Switch Mode
			_.battleMode = eBattleMode.playerTurn;

			ClickedButtonHelper();

			return;
		}

		// If HP is less than maxHP
		if (PartyStats.S.HP [ndx] < PartyStats.S.maxHP [ndx]) {
			// Get amount and max amount to heal
			int amountToHeal = UnityEngine.Random.Range(30, 45);
			int maxAmountToHeal = PartyStats.S.maxHP[ndx] - PartyStats.S.HP[ndx];

			// Add 30-45 HP to TARGET Player's HP
			RPG.S.AddPlayerHP (ndx, amountToHeal);

			// Remove from Inventory
			Inventory.S.RemoveItemFromInventory(ItemManager.S.items[0]);

			// Display Text
			if (amountToHeal >= maxAmountToHeal) {
				BattleDialogue.S.DisplayText ("Used Health Potion!\nHealed " + PartyStats.S.playerName [ndx] + " back to Max HP!");

				// Prevents Floating Score being higher than the acutal amount healed
				amountToHeal = maxAmountToHeal;
			} else {
				BattleDialogue.S.DisplayText ("Used Health Potion!\nHealed " + PartyStats.S.playerName [ndx] + " for " + amountToHeal + " HP!");
			}

			// Get and position Poof game object
			GameObject poof = ObjectPool.S.GetPooledObject("Poof");
			ObjectPool.S.PosAndEnableObj(poof, _.playerSprite[ndx]);

			// Display Floating Score
			RPG.S.InstantiateFloatingScore(_.playerSprite[ndx], amountToHeal, Color.green);

			_.NextTurn ();
		} else {
			BattleDialogue.S.DisplayText (PartyStats.S.playerName [ndx] + " already at full health...\n...no need to use this potion!");

			// Switch Mode
			_.battleMode = eBattleMode.playerTurn;
		}
		ClickedButtonHelper();
	}

	public void MPPotion(int ndx){
		if (_.playerDead[ndx]) {
			// Display Text
			BattleDialogue.S.DisplayText(PartyStats.S.playerName[ndx] + " is dead...\n...and dead folk can't be healed, dummy!");

			// Switch Mode
			_.battleMode = eBattleMode.playerTurn;

			ClickedButtonHelper();

			return;
		}

		// If MP is less than maxMP
		if (PartyStats.S.MP [ndx] < PartyStats.S.maxMP [ndx]) {
			// Get amount and max amount to heal
			int amountToHeal = UnityEngine.Random.Range(30, 45);
			int maxAmountToHeal = PartyStats.S.maxMP[ndx] - PartyStats.S.MP[ndx];

			// Add 30-45 MP to TARGET Player's MP
			RPG.S.AddPlayerMP(ndx, amountToHeal);

			// Remove from Inventory
			Inventory.S.RemoveItemFromInventory(ItemManager.S.items[1]);

			// Display Text
			if (amountToHeal >= maxAmountToHeal) {
				BattleDialogue.S.DisplayText ("Used Magic Potion!\n" + PartyStats.S.playerName [ndx] + " back to Max MP!");

				// Prevents Floating Score being higher than the acutal amount healed
				amountToHeal = maxAmountToHeal;
			} else {
				BattleDialogue.S.DisplayText ("Used Magic Potion!\n" + PartyStats.S.playerName [ndx] + " gained " + amountToHeal + " MP!");
			}

			// Get and position Poof game object
			GameObject poof = ObjectPool.S.GetPooledObject("Poof");
			ObjectPool.S.PosAndEnableObj(poof, _.playerSprite[ndx]);

			// Display Floating Score
			RPG.S.InstantiateFloatingScore(_.playerSprite[ndx], amountToHeal, Color.green);

			_.NextTurn ();
		} else {
			BattleDialogue.S.DisplayText (PartyStats.S.playerName [ndx] + " already at full magic...\n...no need to use this potion!");

			// Switch Mode
			_.battleMode = eBattleMode.playerTurn;
		}
		ClickedButtonHelper();
	}

	public void ClickedButtonHelper() {
		BattlePlayerActions.S.ButtonsDisableAll();
		Utilities.S.RemoveListeners(BattlePlayerActions.S.playerButtonCS);

		// Set animation to idle
		PlayerButtons.S.SetAnim("Idle");
	}

	public void CantUseItem() {
		ItemScreen.S.gameObject.SetActive(false);

		// Deactivate PauseMessage
		PauseMessage.S.gameObject.SetActive(false);

		BattlePlayerActions.S.ButtonsInteractable(false, false, false, false, false, false, false, false, true, true);

		BattleDialogue.S.DisplayText("This item ain't usable in battle... sorry!");

		// Switch Mode
		_.battleMode = eBattleMode.playerTurn;
	}
}
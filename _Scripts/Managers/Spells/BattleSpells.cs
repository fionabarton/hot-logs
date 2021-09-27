﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// During battle, handles what happens when a spell button is clicked
/// </summary>
public class BattleSpells : MonoBehaviour {
	[Header ("Set Dynamically")]
	// Singleton
	private static BattleSpells _S;
	public static BattleSpells S { get { return _S; } set { _S = value; } }

	private Battle _;

	void Awake() {
		S = this;
	}

	void Start () {
		_ = Battle.S;
	}

    public void AddFunctionToButton(Action<int> functionToPass, string messageToDisplay, Spell spell) {
		if (Party.stats[Battle.S.PlayerNdx()].MP >= spell.cost) {
			if (spell.type == eSpellType.healing) {
				BattlePlayerActions.S.ButtonsInteractable(false, false, false, false, false, false, false, false, true, true);

				// Set a Player Button as Selected GameObject
				Utilities.S.SetSelectedGO(BattlePlayerActions.S.playerButtonGO[_.PlayerNdx()].gameObject);

				// Add Item Listeners to Player Buttons
				BattlePlayerActions.S.playerButtonCS[0].onClick.AddListener(delegate { functionToPass(0); });
				BattlePlayerActions.S.playerButtonCS[1].onClick.AddListener(delegate { functionToPass(1); });

				// If multiple targets
				if (spell.multipleTargets) {
					BattleUI.S.TargetAllPartyMembers();
				}
			} else if (spell.type == eSpellType.offensive) {
				BattlePlayerActions.S.ButtonsInteractable(false, false, false, false, false, true, true, true, false, false);

				// Set an Enemy Button as Selected GameObject
				BattlePlayerActions.S.SetSelectedEnemyButton();

				// Add Item Listeners to Enemy Buttons
				BattlePlayerActions.S.enemyButtonCS[0].onClick.AddListener(delegate { functionToPass(0); });
				BattlePlayerActions.S.enemyButtonCS[1].onClick.AddListener(delegate { functionToPass(1); });
				BattlePlayerActions.S.enemyButtonCS[2].onClick.AddListener(delegate { functionToPass(2); });

                // If multiple targets
                if (spell.multipleTargets) {
					BattleUI.S.TargetAllEnemies();
				}
			}
		} else {
			SpellManager.S.CantUseSpell("Not enough MP to cast this spell!");
			return;
		}

		// Deactivate SpellScreen and PauseMessage
		SpellScreen.S.Deactivate();
		PauseMessage.S.gameObject.SetActive(false);

		BattleDialogue.S.DisplayText(messageToDisplay);

		// If multiple targets
		if (!spell.multipleTargets) {
			_.battleMode = eBattleMode.canGoBackToFightButton;
		} else {
			_.battleMode = eBattleMode.canGoBackToFightButtonMultipleTargets;
		}
    }

    public void SpellHelper(){
		BattlePlayerActions.S.ButtonsDisableAll();

		Utilities.S.RemoveListeners(BattlePlayerActions.S.playerButtonCS);
		Utilities.S.RemoveListeners(BattlePlayerActions.S.enemyButtonCS);
	}

	//////////////////////////////////////////////////////////
	/// Heal - Heal the selected party member 
	//////////////////////////////////////////////////////////
	public void HealSelectedPartyMember(int ndx){
        if (_.playerDead[ndx]) {
			// Display Text
			BattleDialogue.S.DisplayText(Party.stats[ndx].name + " is dead...\n...and dead folk can't be healed, dummy!");

			// Switch Mode
			_.battleMode = eBattleMode.playerTurn;

			SpellHelper();

			return;
		}

		if (Party.stats[ndx].HP < Party.stats[ndx].maxHP) {
			// Subtract Spell cost from Player's MP
			RPG.S.SubtractPlayerMP (_.PlayerNdx(), 3);
			
			// Get amount and max amount to heal
			int amountToHeal = UnityEngine.Random.Range(30, 45);
			int maxAmountToHeal = Party.stats[ndx].maxHP - Party.stats[ndx].HP;
			// Add Player's WIS to Heal Amount
			amountToHeal += Party.stats[ndx].WIS;

			// Add 30-45 HP to TARGET Player's HP
			RPG.S.AddPlayerHP (ndx, amountToHeal);

			// Display Text
			if (amountToHeal >= maxAmountToHeal) {
				BattleDialogue.S.DisplayText("Used Heal Spell!\nHealed " + Party.stats[ndx].name + " back to Max HP!");

				// Prevents Floating Score being higher than the acutal amount healed
				amountToHeal = maxAmountToHeal;
			} else {
				BattleDialogue.S.DisplayText("Used Heal Spell!\nHealed " + Party.stats[ndx].name + " for " + amountToHeal + " HP!");
			}

			// Get and position Poof game object
			GameObject poof = ObjectPool.S.GetPooledObject("Poof");
			ObjectPool.S.PosAndEnableObj(poof, _.playerSprite[ndx]);

			// Display Floating Score
			RPG.S.InstantiateFloatingScore(_.playerSprite[ndx], amountToHeal, Color.green);

			// Set anim
			_.playerAnimator[ndx].CrossFade("Win_Battle", 0);

			_.NextTurn ();
		} else {
			// Display Text
			BattleDialogue.S.DisplayText(Party.stats[ndx].name + " already at full health...\n...no need to cast this spell!");

			// Switch Mode
			_.battleMode = eBattleMode.playerTurn;
		}
		SpellHelper();
	}

	//////////////////////////////////////////////////////////
	/// Fireball - Attack the selected enemy
	////////////////////////////////////////////////////////// 
	public void AttackSelectedEnemies(int ndx){
		// Subtract Spell cost from Player's MP
		RPG.S.SubtractPlayerMP (_.PlayerNdx(), 2);

		// Miss/Dodge
		// 5% chance to Miss/Dodge...
		// ...but 25% chance if Defender WIS is more than Attacker's 
		if (UnityEngine.Random.value <= 0.05f || (_.enemyStats [ndx].WIS > Party.stats[_.PlayerNdx()].WIS && UnityEngine.Random.value < 0.25f)) {
			if (UnityEngine.Random.value <= 0.5f) {
				BattleDialogue.S.DisplayText(Party.stats[_.PlayerNdx()].name + " attempted the spell... but missed " + _.enemyStats[ndx].name + " completely!");
			} else {
				BattleDialogue.S.DisplayText(Party.stats[_.PlayerNdx()].name + " cast the spell, but " + _.enemyStats[ndx].name + " deftly dodged out of the way!");
			}
			_.NextTurn ();
		} else {
			// Subtract 8-12 HP
			_.attackDamage = UnityEngine.Random.Range (8, 12);
			// Add Player's WIS to Damage
			_.attackDamage += Party.stats[_.PlayerNdx()].WIS;
			_.attackDamage += Party.stats[_.PlayerNdx()].WIS;
			// Subtract Enemy's DEF from Damage
			_.attackDamage -= _.enemyStats[ndx].DEF;

			// If DEFENDING, cut AttackDamage in HALF
			_.CheckIfDefending (Battle.S.enemyStats [ndx].name);

			if (_.attackDamage < 0) {
				_.attackDamage = 0;
			}
			
			// Animation: Shake Enemy Anim 
			_.enemyAnimator[ndx].CrossFade("Damage", 0);

			// Get and position Explosion game object
			GameObject explosion = ObjectPool.S.GetPooledObject("Explosion");
			ObjectPool.S.PosAndEnableObj (explosion, _.enemySprite[ndx].gameObject);

			// Display Floating Score
			RPG.S.InstantiateFloatingScore(_.enemySprite[ndx].gameObject, _.attackDamage, Color.red);

			// Subtract Enemy Health
			RPG.S.SubtractEnemyHP (ndx, _.attackDamage);

			if (_.enemyStats[ndx].HP < 1) {
				// Deactivate Spells Screen then Enemy Death
				SpellScreen.S.ScreenOffEnemyDeath (ndx);
			} else {
				// Deactivate Spells Screen then Enemy Turn
				BattleDialogue.S.DisplayText ("Used Fire Spell!\nHit " + _.enemyStats [ndx].name + " for " + _.attackDamage + " HP!");
				_.NextTurn ();
			}
		}
		SpellHelper ();
	}

	//////////////////////////////////////////////////////////
	/// Fireblast
	//////////////////////////////////////////////////////////
	public void AttackAllEnemies (int unusedIntBecauseOfAddFunctionToButtonParameter = 0) {
		// Subtract Spell cost from Player's MP
		RPG.S.SubtractPlayerMP (_.PlayerNdx(), 3);

		// *** TBR: Reference the Defender (still living) with the highest WIS ***
	
		// Miss/Dodge
		// 5% chance to Miss/Dodge...
		// ...but 25% chance if Defender WIS is more than Attacker's 
		if (UnityEngine.Random.value <= 0.05f || (_.enemyStats[0].WIS > Party.stats[_.PlayerNdx()].WIS && UnityEngine.Random.value < 0.25f)) {
			if (UnityEngine.Random.value <= 0.5f) {
				BattleDialogue.S.DisplayText(Party.stats[_.PlayerNdx()].name + " attempted the spell... but missed those goons completely!");
			} else {
				BattleDialogue.S.DisplayText(Party.stats[_.PlayerNdx()].name + " cast the spell, but these dummies you're fighting deftly dodged out of the way!");
			}
			_.NextTurn ();
		} else {
			List<int> deadEnemies = new List<int>();

			// Subtract 12-20 HP
			_.attackDamage = UnityEngine.Random.Range (12, 20);
			// Add Player's WIS to Damage
			_.attackDamage += Party.stats[_.PlayerNdx()].WIS;

			// Cache AttackDamage. When more than one Defender, prevents splitting it in 1/2 more than once.
			int tAttackDamage = _.attackDamage;

			// Used to Calculate AVERAGE Damage
			int totalAttackDamage = 0;

			// Loop through enemies
			for (int i = 0; i < _.enemyStats.Count; i++) {
				// Subtract Enemy's DEF from Damage
				_.attackDamage -= Battle.S.enemyStats[i].DEF;

				// If DEFENDING, cut AttackDamage in HALF
				_.CheckIfDefending (Battle.S.enemyStats[i].name);

				if (_.attackDamage < 0) {
					_.attackDamage = 0;
				} 

				// Subtract Enemy Heath
				RPG.S.SubtractEnemyHP (i, _.attackDamage);

				// Add to to TotalAttackDamage (Used to Calculate AVERAGE Damage)
				totalAttackDamage += _.attackDamage;

				// Shake Enemy 1, 2, & 3's Anim
				if (!_.enemyStats[i].isDead) {
					// Shake Enemy Anim 
					_.enemyAnimator[i].CrossFade("Damage", 0);

					// Get and position Explosion game object
					GameObject explosion = ObjectPool.S.GetPooledObject("Explosion");
					ObjectPool.S.PosAndEnableObj (explosion, _.enemySprite[i].gameObject);

					// Display Floating Score
					RPG.S.InstantiateFloatingScore(_.enemySprite[i].gameObject, _.attackDamage, Color.red);
				}

				// If DEFENDING, Reset AttackDamage for next Enemy
				_.attackDamage = tAttackDamage;

				// If Enemy HP < 0... DEAD!
				if (_.enemyStats[i].HP < 1 && !_.enemyStats [i].isDead) {
					deadEnemies.Add(i);
				}
			}

			// If no one is killed...
			if (deadEnemies.Count <= 0) {
				BattleDialogue.S.DisplayText ("Used Fire BLAST Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, _.enemyStats.Count) + " HP!");
				_.NextTurn (); 
			} else {
				EnemiesDeath(deadEnemies, totalAttackDamage);
			}
		}
		SpellHelper ();
	}

	// Handle enemy deaths
	public void EnemiesDeath(List<int> deadEnemies, int totalAttackDamage) {
		// Subtract from EnemyAmount 
		_.enemyAmount -= deadEnemies.Count;

		switch (deadEnemies.Count) {
			case 1: BattleDialogue.S.DisplayText("Used Fire BLAST Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, _.enemyStats.Count) + " HP!" + "\nOne enemy has been felled!"); break;
			case 2: BattleDialogue.S.DisplayText("Used Fire BLAST Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, _.enemyStats.Count) + " HP!" + "\nTwo enemies have been felled!"); break;
			case 3: BattleDialogue.S.DisplayText("Used Fire BLAST Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, _.enemyStats.Count) + " HP!" + "\nThree enemies have been felled!"); break;
		}

		for(int i = 0; i < deadEnemies.Count; i++) {
			EnemiesDeathHelper(deadEnemies[i], Battle.S.enemyStats[deadEnemies[i]].name);
		}

		// Add Exp & Gold or Next Turn
		if (_.enemyAmount <= 0) {
			// DropItem or AddExpAndGold
			if (_.droppedItems.Count >= 1) {
				// Switch Mode
				_.battleMode = eBattleMode.dropItem;
			} else {
				// Switch Mode
				_.battleMode = eBattleMode.addExpAndGold;
			}
		} else { _.NextTurn(); }
	}

	public void EnemiesDeathHelper(int enemyNdx, string enemyTurnOrder){
		// Drop Enemy Anim
		_.enemyAnimator[enemyNdx].CrossFade("Death", 0);

		// Deactivate Enemy Shield
		_.enemyShields[enemyNdx].SetActive(false);

		// Deactivate Cursors
		BattleUI.S.turnCursor.SetActive(false);
		//BattleUI.S.targetCursor.SetActive(false);
		Utilities.S.SetActiveList(BattleUI.S.targetCursors, false);

		// Deactivate Enemy Button, Stats
		BattlePlayerActions.S.EnemyButtonSetActive(enemyNdx, false);
		// Set Selected GameObject (Fight Button)
		_.enemyStats[enemyNdx].isDead = true;
		// Remove Enemy from Turn Order
		_.turnOrder.Remove (enemyTurnOrder);
		// Randomly select DropItem
		BattleEnd.S.AddDroppedItems(enemyNdx);
	}

	//////////////////////////////////////////////////////////
	/// Heal All - Heal all party members 
	//////////////////////////////////////////////////////////
	public void HealAllPartyMembers(int unusedIntBecauseOfAddFunctionToButtonParameter = 0) {
		int totalAmountToHeal = 0;

		if (Party.stats[0].HP < Party.stats[0].maxHP ||
			Party.stats[1].HP < Party.stats[1].maxHP) {
			// Subtract Spell cost from Player's MP
			RPG.S.SubtractPlayerMP(_.PlayerNdx(), 6);

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

					// Set anim
					_.playerAnimator[i].CrossFade("Win_Battle", 0);
				}
			}

			// Display Text
			BattleDialogue.S.DisplayText("Used Heal All Spell!\nHealed ALL party members for an average of " 
				+ Utilities.S.CalculateAverage(totalAmountToHeal, _.playerDead.Count) + " HP!");

			_.NextTurn();
		} else {
			// Display Text
			BattleDialogue.S.DisplayText("The party is already at full health...\n...no need to cast this spell!");

			// Deactivate Cursors
			Utilities.S.SetActiveList(BattleUI.S.targetCursors, false);

			// Switch Mode
			_.battleMode = eBattleMode.playerTurn;
		}
		SpellHelper();
	}
}
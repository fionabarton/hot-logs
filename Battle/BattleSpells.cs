using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// During battle, handles what happens when a spell button is clicked
/// </summary>
public class BattleSpells : MonoBehaviour {
	[Header("Set Dynamically")]
	int amountToHeal;
	int maxAmountToHeal;

	private Battle _;

	void Start() {
		_ = Battle.S;
	}

	public void AddFunctionToButton(Action<int, Spell> functionToPass, string messageToDisplay, Spell spell) {
		if (Party.S.stats[Battle.S.PlayerNdx()].MP >= spell.cost) {
			// Audio: Confirm
			AudioManager.S.PlaySFX(eSoundName.confirm);

			// Remove all listeners
			Utilities.S.RemoveListeners(_.playerActions.playerButtonCS);

			if (spell.type == eSpellType.Healing) {
				_.playerActions.ButtonsInteractable(false, false, false, false, false, false, false, false, false, false, true, true);

				// Set a Player Button as Selected GameObject
				Utilities.S.SetSelectedGO(_.playerActions.playerButtonGO[_.PlayerNdx()].gameObject);

				// Set previously selected GameObject
				_.previousSelectedForAudio = _.playerActions.playerButtonGO[_.PlayerNdx()].gameObject;

				// Add Item Listeners to Player Buttons
				_.playerActions.playerButtonCS[0].onClick.AddListener(delegate { functionToPass(0, spell); });
				_.playerActions.playerButtonCS[1].onClick.AddListener(delegate { functionToPass(1, spell); });
				_.playerActions.playerButtonCS[2].onClick.AddListener(delegate { functionToPass(2, spell); });

				// If multiple targets
				if (spell.multipleTargets) {
					_.UI.TargetAllPartyMembers();
				}
			} else if (spell.type == eSpellType.Offensive || spell.type == eSpellType.Thievery) {
				_.playerActions.ButtonsInteractable(false, false, false, false, false, true, true, true, true, true, false, false);

				// Set an Enemy Button as Selected GameObject
				_.playerActions.SetSelectedEnemyButton();

				// Add Item Listeners to Enemy Buttons
				_.playerActions.enemyButtonCS[0].onClick.AddListener(delegate { functionToPass(0, spell); });
				_.playerActions.enemyButtonCS[1].onClick.AddListener(delegate { functionToPass(1, spell); });
				_.playerActions.enemyButtonCS[2].onClick.AddListener(delegate { functionToPass(2, spell); });
				_.playerActions.enemyButtonCS[3].onClick.AddListener(delegate { functionToPass(3, spell); });
				_.playerActions.enemyButtonCS[4].onClick.AddListener(delegate { functionToPass(4, spell); });

				// If multiple targets
				if (spell.multipleTargets) {
					_.UI.TargetAllEnemies();
				}
			}
		} else {
			Spells.S.CantUseSpell("Not enough MP to cast this spell!");
			return;
		}

		// Deactivate SpellScreen and PauseMessage
		Spells.S.menu.Deactivate();
		PauseMessage.S.gameObject.SetActive(false);

		_.dialogue.DisplayText(messageToDisplay);

		// If multiple targets
		if (!spell.multipleTargets) {
			_.mode = eBattleMode.canGoBackToFightButton;
		} else {
			_.mode = eBattleMode.canGoBackToFightButtonMultipleTargets;
		}
	}

	public void SpellHelper() {
		_.playerActions.ButtonsDisableAll();

		Utilities.S.RemoveListeners(_.playerActions.playerButtonCS);
		Utilities.S.RemoveListeners(_.playerActions.enemyButtonCS);
	}

	////////////////////////////////////////////////////////////////////////////////////////
	// Heal party members
	////////////////////////////////////////////////////////////////////////////////////////

	public void Heal(int ndx, int min, int max) {
		// Get amount and max amount to heal
		amountToHeal = UnityEngine.Random.Range(min, max);
		maxAmountToHeal = Party.S.stats[ndx].maxHP - Party.S.stats[ndx].HP;

		// Add Player's WIS to Heal Amount
		amountToHeal += Party.S.stats[ndx].WIS;

		// Cap amountToHeal to maxAmountToHeal
		if (amountToHeal >= maxAmountToHeal) {
			amountToHeal = maxAmountToHeal;
		}

		// Add to TARGET Player's HP
		GameManager.S.AddPlayerHP(ndx, amountToHeal);

		CurePlayerAnimation(ndx, true, amountToHeal);
	}

	// Heal - Heal a single party member 
	public void AttemptHealSinglePartyMember(int ndx, Spell spell) {
		SpellHelper();

		if (_.playerDead[ndx]) {
			SpellIsNotUseful(Party.S.stats[ndx].name + " is dead...\n...and dead folk can't be healed, dummy!");
			return;
		}

		if (Party.S.stats[ndx].HP < Party.S.stats[ndx].maxHP) {
			ColorScreen.S.PlayClip("Swell", 0);
			ColorScreen.S.targetNdx = ndx;
			ColorScreen.S.spell = spell;
		} else {
			SpellIsNotUseful(Party.S.stats[ndx].name + " already at full health...\n...no need to cast this spell!");
		}
	}

	public void HealSinglePartyMember(int ndx, Spell spell) {
		// Subtract Spell cost from Player's MP
		GameManager.S.SubtractPlayerMP(_.PlayerNdx(), spell.cost);

		Heal(ndx, spell.statEffectMinValue, spell.statEffectMaxValue);

		// Display Text
		if (amountToHeal >= maxAmountToHeal) {
			_.dialogue.DisplayText("Used " + spell.name + " Spell!\nHealed " + Party.S.stats[ndx].name + " back to Max HP!");
		} else {
			_.dialogue.DisplayText("Used " + spell.name + " Spell!\nHealed " + Party.S.stats[ndx].name + " for " + amountToHeal + " HP!");
		}

		// Audio: Buff 1
		AudioManager.S.PlaySFX(eSoundName.buff1);

		_.NextTurn();
	}

	// Heal All - Heal all party members 
	public void AttemptHealAll(int unusedIntBecauseOfAddFunctionToButtonParameter, Spell spell) {
		SpellHelper();

		if (Party.S.stats[0].HP < Party.S.stats[0].maxHP ||
			Party.S.stats[1].HP < Party.S.stats[1].maxHP ||
			Party.S.stats[2].HP < Party.S.stats[2].maxHP) {
			ColorScreen.S.PlayClip("Swell", 2);
			ColorScreen.S.spell = spell;
		} else {
			SpellIsNotUseful("The party is already at full health...\n...no need to cast this spell!");
		}
	}

	public void HealAll(int unusedIntBecauseOfAddFunctionToButtonParameter, Spell spell) {
		int totalAmountToHeal = 0;

		// Subtract Spell cost from Player's MP
		GameManager.S.SubtractPlayerMP(_.PlayerNdx(), spell.cost);

		for (int i = 0; i < _.playerDead.Count; i++) {
			if (!_.playerDead[i]) {
				Heal(i, spell.statEffectMinValue, spell.statEffectMaxValue);

				totalAmountToHeal += amountToHeal;
			}
		}

		// Display Text
		_.dialogue.DisplayText("Used " + spell.name + " Spell!\nHealed ALL party members for an average of "
			+ Utilities.S.CalculateAverage(totalAmountToHeal, _.playerDead.Count) + " HP!");

		// Audio: Buff 1
		AudioManager.S.PlaySFX(eSoundName.buff1);

		_.NextTurn();
	}

	// Revive - Revive a single party member
	public void AttemptReviveSelectedPartyMember(int ndx, Spell spell) {
		SpellHelper();

		if (_.playerDead[ndx]) {
			ColorScreen.S.PlayClip("Swell", 3);
			ColorScreen.S.targetNdx = ndx;
			ColorScreen.S.spell = spell;
		} else {
			SpellIsNotUseful(Party.S.stats[ndx].name + " ain't dead...\n...and dead folk don't need to be revived, dummy!");
		}
	}

	public void ReviveSelectedPartyMember(int ndx, Spell spell) {
		_.playerDead[ndx] = false;

		// Add to PartyQty 
		_.partyQty += 1;

		// Add Player to Turn Order
		_.turnOrder.Add(Party.S.stats[ndx].name);

		// Get 6-10% of max HP
		float lowEnd = Mathf.Max(1, Party.S.stats[ndx].maxHP * 0.06f);
		float highEnd = Mathf.Max(1, Party.S.stats[ndx].maxHP * 0.10f);
		Heal(ndx, (int)lowEnd, (int)highEnd);

		// Display Text
		if (amountToHeal >= maxAmountToHeal) {
			_.dialogue.DisplayText("Used " + spell.name + " Spell!\nHealed " + Party.S.stats[ndx].name + " back to Max HP!");
		} else {
			_.dialogue.DisplayText("Used " + spell.name + " Spell!\nHealed " + Party.S.stats[ndx].name + " for " + amountToHeal + " HP!");
		}

		// Audio: Buff 1
		AudioManager.S.PlaySFX(eSoundName.buff1);

		_.NextTurn();
	}

	////////////////////////////////////////////////////////////////////////////////////////
	// Damage enemies
	////////////////////////////////////////////////////////////////////////////////////////

	// Fireball - Attack the selected enemy
	public void AttemptAttackSelectedEnemy(int ndx, Spell spell) {
		SpellHelper();
		ColorScreen.S.PlayClip("Flicker", 0);
		ColorScreen.S.targetNdx = ndx;
		ColorScreen.S.spell = spell;
	}

	public void AttackSelectedEnemy(int ndx, Spell spell) {
		// Subtract Spell cost from Player's MP
		GameManager.S.SubtractPlayerMP(_.PlayerNdx(), spell.cost);

		// 5% chance to Miss/Dodge...
		// ...but 10% chance if Defender WIS is more than Attacker's 
		if (UnityEngine.Random.value <= 0.05f || (_.enemyStats[ndx].WIS > Party.S.stats[_.PlayerNdx()].WIS && UnityEngine.Random.value < 0.10f)) {
			// Set anim
			_.playerAnimator[_.PlayerNdx()].CrossFade("Fail_Spell", 0);

			if (UnityEngine.Random.value <= 0.5f) {
				_.dialogue.DisplayText(Party.S.stats[_.PlayerNdx()].name + " attempted the spell... but missed " + _.enemyStats[ndx].name + " completely!");
			} else {
				_.dialogue.DisplayText(Party.S.stats[_.PlayerNdx()].name + " cast the spell, but " + _.enemyStats[ndx].name + " deftly dodged out of the way!");
			}

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);

			_.NextTurn();
		} else {
			// Subtract 8-12 HP
			_.attackDamage = UnityEngine.Random.Range(spell.statEffectMinValue, spell.statEffectMaxValue);
			// Add Player's WIS to Damage
			_.attackDamage += Party.S.stats[_.PlayerNdx()].WIS;
			// Subtract Enemy's DEF from Damage
			_.attackDamage -= _.enemyStats[ndx].DEF;

			// If DEFENDING, cut AttackDamage in HALF
			StatusEffects.S.CheckIfDefending(false, ndx);

			if (_.attackDamage < 0) {
				_.attackDamage = 0;
			}

			DamageEnemyAnimation(ndx, true);

			// Subtract Enemy Health
			GameManager.S.SubtractEnemyHP(ndx, _.attackDamage);

			if (_.enemyStats[ndx].HP < 1) {
				// Deactivate Spells Screen then Enemy Death
				Spells.S.menu.ScreenOffEnemyDeath(ndx);
			} else {
				// Deactivate Spells Screen then Enemy Turn
				_.dialogue.DisplayText("Used " + spell.name + " Spell!\nHit " + _.enemyStats[ndx].name + " for " + _.attackDamage + " HP!");

				// Audio: Fireball
				AudioManager.S.PlaySFX(eSoundName.fireball);

				_.NextTurn();
			}
		}
	}

	// Fireblast
	public void AttemptAttackAllEnemies(int unusedIntBecauseOfAddFunctionToButtonParameter, Spell spell) {
		SpellHelper();
		ColorScreen.S.PlayClip("Flicker", 1);
		ColorScreen.S.spell = spell;
	}

	public void AttackAllEnemies(int unusedIntBecauseOfAddFunctionToButtonParameter, Spell spell) {
		// Subtract Spell cost from Player's MP
		GameManager.S.SubtractPlayerMP(_.PlayerNdx(), spell.cost);

		// 5% chance to Miss/Dodge...
		// ...but 25% chance if Defender WIS is more than Attacker's 
		if (UnityEngine.Random.value <= 0.10f || (_.enemyStats[0].WIS > Party.S.stats[_.PlayerNdx()].WIS && UnityEngine.Random.value < 0.10f)) {
			// Set anim
			_.playerAnimator[_.PlayerNdx()].CrossFade("Fail_Spell", 0);

			if (UnityEngine.Random.value <= 0.5f) {
				_.dialogue.DisplayText(Party.S.stats[_.PlayerNdx()].name + " attempted the spell... but missed those goons completely!");
			} else {
				_.dialogue.DisplayText(Party.S.stats[_.PlayerNdx()].name + " cast the spell, but these dummies you're fighting deftly dodged out of the way!");
			}

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);

			_.NextTurn();
		} else {
			List<int> deadEnemies = new List<int>();

			// Subtract 12-20 HP
			_.attackDamage = UnityEngine.Random.Range(spell.statEffectMinValue, spell.statEffectMaxValue);
			// Add Player's WIS to Damage
			_.attackDamage += Party.S.stats[_.PlayerNdx()].WIS;

			// Cache AttackDamage. When more than one Defender, prevents splitting it in 1/2 more than once.
			int tAttackDamage = _.attackDamage;

			// Used to Calculate AVERAGE Damage
			int totalAttackDamage = 0;

			// Loop through enemies
			for (int i = 0; i < _.enemyStats.Count; i++) {
				// Subtract Enemy's DEF from Damage
				_.attackDamage -= Battle.S.enemyStats[i].DEF;

				// If DEFENDING, cut AttackDamage in HALF
				StatusEffects.S.CheckIfDefending(false, i);

				if (_.attackDamage < 0) {
					_.attackDamage = 0;
				}

				// Subtract Enemy Heath
				GameManager.S.SubtractEnemyHP(i, _.attackDamage);

				// Add to to TotalAttackDamage (Used to Calculate AVERAGE Damage)
				totalAttackDamage += _.attackDamage;

				// Shake Enemy Anim
				if (!_.enemyStats[i].isDead) {
					DamageEnemyAnimation(i, true);
				}

				// If DEFENDING, Reset AttackDamage for next Enemy
				_.attackDamage = tAttackDamage;

				// If Enemy HP < 0... DEAD!
				if (_.enemyStats[i].HP < 1 && !_.enemyStats[i].isDead) {
					deadEnemies.Add(i);
				}
			}

			// If no one is killed...
			if (deadEnemies.Count <= 0) {
				_.dialogue.DisplayText("Used " + spell.name + " Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, _.enemyStats.Count) + " HP!");

				// Audio: Fireblast
				AudioManager.S.PlaySFX(eSoundName.fireblast);

				_.NextTurn();
			} else {
				EnemiesDeath(deadEnemies, totalAttackDamage);
			}
		}
	}

	// Handle enemy deaths
	public void EnemiesDeath(List<int> deadEnemies, int totalAttackDamage) {
		bool hasStolenItems = false;
		for(int i = 0; i < deadEnemies.Count; i++) {
			// Check if any enemy has stolen an item from the party
			if (_.enemyStats[deadEnemies[i]].stolenItems.Count > 0) {
				hasStolenItems = true;
			}

            // Handle enemy death
            _.end.EnemyDeath(deadEnemies[i], false);
        }

		// Display different text whether the enemy has stolen anything
        if (hasStolenItems) {
			switch (deadEnemies.Count) {
				case 1: _.dialogue.DisplayText("Used Fire BLAST Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, _.enemyStats.Count) + " HP!" + "\nOne enemy has been felled; stolen items are returned to the party!"); break;
				case 2: _.dialogue.DisplayText("Used Fire BLAST Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, _.enemyStats.Count) + " HP!" + "\nTwo enemies have been felled; stolen items are returned to the party!"); break;
				case 3: _.dialogue.DisplayText("Used Fire BLAST Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, _.enemyStats.Count) + " HP!" + "\nThree enemies have been felled; stolen items are returned to the party!"); break;
				case 4: _.dialogue.DisplayText("Used Fire BLAST Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, _.enemyStats.Count) + " HP!" + "\nFour enemies have been felled; stolen items are returned to the party!"); break;
				case 5: _.dialogue.DisplayText("Used Fire BLAST Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, _.enemyStats.Count) + " HP!" + "\nFive enemies have been felled; stolen items are returned to the party!"); break;
			}
		} else {
			switch (deadEnemies.Count) {
				case 1: _.dialogue.DisplayText("Used Fire BLAST Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, _.enemyStats.Count) + " HP!" + "\nOne enemy has been felled!"); break;
				case 2: _.dialogue.DisplayText("Used Fire BLAST Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, _.enemyStats.Count) + " HP!" + "\nTwo enemies have been felled!"); break;
				case 3: _.dialogue.DisplayText("Used Fire BLAST Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, _.enemyStats.Count) + " HP!" + "\nThree enemies have been felled!"); break;
				case 4: _.dialogue.DisplayText("Used Fire BLAST Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, _.enemyStats.Count) + " HP!" + "\nFour enemies have been felled!"); break;
				case 5: _.dialogue.DisplayText("Used Fire BLAST Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, _.enemyStats.Count) + " HP!" + "\nFive enemies have been felled!"); break;
			}
		}

		//for (int i = 0; i < deadEnemies.Count; i++) {
		//	// Handle enemy death
		//	_.end.EnemyDeath(deadEnemies[i], false);
		//}
	}

	////////////////////////////////////////////////////////////////////////////////////////
	// Cure status ailments
	////////////////////////////////////////////////////////////////////////////////////////

	// Detoxify - Detoxify a single party member 
	public void AttemptDetoxifySinglePartyMember(int ndx, Spell spell) {
		SpellHelper();

		if (_.playerDead[ndx]) {
			SpellIsNotUseful(Party.S.stats[ndx].name + " is dead...\n...and dead folk don't need to be detoxified, dummy!");
			return;
		}

		if (StatusEffects.S.CheckIfPoisoned(true, ndx)) {
			ColorScreen.S.PlayClip("Swell", 4);
			ColorScreen.S.targetNdx = ndx;
			ColorScreen.S.spell = spell;
		} else {
			SpellIsNotUseful(Party.S.stats[ndx].name + " is not suffering from the effects of poison...\n...no need to cast this spell!");
		}
	}

	public void DetoxifySinglePartyMember(int ndx, Spell spell) {
		// Subtract Spell cost from Player's MP
		GameManager.S.SubtractPlayerMP(_.PlayerNdx(), spell.cost);

		// Remove poison
		StatusEffects.S.RemovePoisoned(true, ndx);

		// Display Text
		_.dialogue.DisplayText("Used " + spell.name + " Spell!\n" + Party.S.stats[ndx].name + " is no longer poisoned!");

		CurePlayerAnimation(ndx, false);

		// Audio: Buff 1
		AudioManager.S.PlaySFX(eSoundName.buff1);

		_.NextTurn();
	}

	// Mobilize - Mobilize a single party member 
	public void AttemptMobilizeSinglePartyMember(int ndx, Spell spell) {
		SpellHelper();

		if (_.playerDead[ndx]) {
			SpellIsNotUseful(Party.S.stats[ndx].name + " is dead...\n...and dead folk don't need their mobility restored, dummy!");
			return;
		}

		if (StatusEffects.S.CheckIfParalyzed(true, ndx)) {
			ColorScreen.S.PlayClip("Swell", 5);
			ColorScreen.S.targetNdx = ndx;
			ColorScreen.S.spell = spell;
		} else {
			SpellIsNotUseful(Party.S.stats[ndx].name + " is not suffering from the effects of paralysis...\n...no need to cast this spell!");
		}
	}

	public void MobilizeSinglePartyMember(int ndx, Spell spell) {
		// Subtract Spell cost from Player's MP
		GameManager.S.SubtractPlayerMP(_.PlayerNdx(), spell.cost);

		// Remove paralysis
		StatusEffects.S.RemoveParalyzed(true, ndx);

		// Display Text
		_.dialogue.DisplayText("Used " + spell.name + " Spell!\n" + Party.S.stats[ndx].name + " is no longer paralyzed!");

		CurePlayerAnimation(ndx, false);

		// Audio: Buff 1
		AudioManager.S.PlaySFX(eSoundName.buff1);

		_.NextTurn();
	}

	// Wake - Wake a single party member 
	public void AttemptWakeSinglePartyMember(int ndx, Spell spell) {
		SpellHelper();

		if (_.playerDead[ndx]) {
			SpellIsNotUseful(Party.S.stats[ndx].name + " is dead...\n...and dead folk don't need to wake up, dummy!");
			return;
		}

		if (StatusEffects.S.CheckIfSleeping(true, ndx)) {
			ColorScreen.S.PlayClip("Swell", 6);
			ColorScreen.S.targetNdx = ndx;
			ColorScreen.S.spell = spell;
		} else {
			SpellIsNotUseful(Party.S.stats[ndx].name + " is not sleeping...\n...no need to cast this spell!");
		}
	}

	public void WakeSinglePartyMember(int ndx, Spell spell) {
		// Subtract Spell cost from Player's MP
		GameManager.S.SubtractPlayerMP(_.PlayerNdx(), spell.cost);

		// Remove sleeping
		StatusEffects.S.RemoveSleeping(true, ndx);

		// Display Text
		_.dialogue.DisplayText("Used " + spell.name + " Spell!\n" + Party.S.stats[ndx].name + " is no longer sleeping!");

		CurePlayerAnimation(ndx, false);

		// Audio: Buff 1
		AudioManager.S.PlaySFX(eSoundName.buff1);

		_.NextTurn();
	}

	////////////////////////////////////////////////////////////////////////////////////////
	// Inflict status ailments
	////////////////////////////////////////////////////////////////////////////////////////

	// Poison
	public void AttemptPoisonSinglePartyMember(int ndx, Spell spell) {
		SpellHelper();

		if (!StatusEffects.S.CheckIfPoisoned(false, ndx)) {
			ColorScreen.S.PlayClip("Flicker", 4);
			ColorScreen.S.targetNdx = ndx;
			ColorScreen.S.spell = spell;
		} else {
			SpellIsNotUseful(_.enemyStats[ndx].name + " is not already suffering from the effects of poison...\n...no need to cast this spell!");
		}
	}
	public void PoisonSingle(int ndx, Spell spell) {
		// Subtract Spell cost from Player's MP
		GameManager.S.SubtractPlayerMP(_.PlayerNdx(), spell.cost);

		DamageEnemyAnimation(ndx);

		// Poison enemy
		StatusEffects.S.AddPoisoned(ndx);
	}

	// Paralyze
	public void AttemptParalyzeSinglePartyMember(int ndx, Spell spell) {
		SpellHelper();

		if (!StatusEffects.S.CheckIfParalyzed(false, ndx)) {
			ColorScreen.S.PlayClip("Flicker", 5);
			ColorScreen.S.targetNdx = ndx;
			ColorScreen.S.spell = spell;
		} else {
			SpellIsNotUseful(_.enemyStats[ndx].name + " is not already suffering from the effects of paralysis...\n...no need to cast this spell!");
		}
	}
	public void ParalyzeSingle(int ndx, Spell spell) {
		// Subtract Spell cost from Player's MP
		GameManager.S.SubtractPlayerMP(_.PlayerNdx(), spell.cost);

		DamageEnemyAnimation(ndx);

		// Paralyze enemy
		StatusEffects.S.AddParalyzed(ndx);
	}

	// Sleep
	public void AttemptSleepSinglePartyMember(int ndx, Spell spell) {
		SpellHelper();

		if (!StatusEffects.S.CheckIfSleeping(false, ndx)) {
			ColorScreen.S.PlayClip("Flicker", 6);
			ColorScreen.S.targetNdx = ndx;
			ColorScreen.S.spell = spell;
		} else {
			SpellIsNotUseful(_.enemyStats[ndx].name + " is not already suffering from the effects of sleep...\n...no need to cast this spell!");
		}
	}
	public void SleepSingle(int ndx, Spell spell) {
		// Subtract Spell cost from Player's MP
		GameManager.S.SubtractPlayerMP(_.PlayerNdx(), spell.cost);

		DamageEnemyAnimation(ndx);

		// Put enemy to sleep 
		StatusEffects.S.AddSleeping(ndx);
	}

	////////////////////////////////////////////////////////////////////////////////////////
	// Thievery
	////////////////////////////////////////////////////////////////////////////////////////

	// Steal
	public void AttemptStealSinglePartyMember(int ndx, Spell spell) {
		SpellHelper();
		ColorScreen.S.PlayClip("Flicker", 7);
		ColorScreen.S.targetNdx = ndx;
		ColorScreen.S.spell = spell;
	}
	public void StealSingle(int ndx, Spell spell) {
		// Subtract Spell cost from Player's MP
		GameManager.S.SubtractPlayerMP(_.PlayerNdx(), spell.cost);
		
		if(_.enemyStats[ndx].amountToSteal > 0) {
			// 50% chance to miss...
			if (UnityEngine.Random.value <= 0.5f) {
				DamageEnemyAnimation(ndx);

				Item tItem;
				// If the enemy hasn't stolen an item from the party
				if (_.enemyStats[ndx].stolenItems.Count <= 0) {
					// Get random enemy item index and item
					int itemNdx = UnityEngine.Random.Range(0, _.enemyStats[ndx].itemsToDrop.Count);
					tItem = Items.S.GetItem(_.enemyStats[ndx].itemsToDrop[itemNdx]);
				} else {
					// Get an item that was stolen from the party
					tItem = _.enemyStats[ndx].stolenItems[0];

					// Remove item from stolenItems list
					_.enemyStats[ndx].stolenItems.RemoveAt(0);
				}

				_.enemyStats[ndx].amountToSteal -= 1;

				// Add item to party inventory
				Inventory.S.AddItemToInventory(tItem);

				// Display text: item stolen
				_.dialogue.DisplayText(Party.S.stats[_.PlayerNdx()].name + " swiped a " + tItem.name + " from " + _.enemyStats[ndx].name + ".\n" + WordManager.S.GetRandomExclamation() + "!");
			} else {
				// Audio: Deny
				AudioManager.S.PlaySFX(eSoundName.deny);

				// Display text: miss
				_.dialogue.DisplayText(Party.S.stats[_.PlayerNdx()].name + " attempted to loot an item from " + _.enemyStats[ndx].name + "...\n...but missed the mark!\n" + WordManager.S.GetRandomInterjection() + "!");
			}
		} else {
			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);

			// Display text: no items to steal
			_.dialogue.DisplayText(Party.S.stats[_.PlayerNdx()].name + " attempted to steal an item from " + _.enemyStats[ndx].name + "...\n...but they've got nothing!\n" + WordManager.S.GetRandomInterjection() + "!");
		}

		_.NextTurn();
	}

	////////////////////////////////////////////////////////////////////////////////////////
	// Helper functions
	////////////////////////////////////////////////////////////////////////////////////////

	public void SpellIsNotUseful(string message) {
		// Display Text
		_.dialogue.DisplayText(message);

		// Deactivate Cursors
		Utilities.S.SetActiveList(_.UI.targetCursors, false);

		// Audio: Deny
		AudioManager.S.PlaySFX(eSoundName.deny);

		// Switch Mode
		if (StatusEffects.S.HasStatusAilment(true, _.PlayerNdx())) {
			_.mode = eBattleMode.statusAilment;
		} else {
			_.mode = eBattleMode.playerTurn;
		}
	}

	public void CurePlayerAnimation(int ndx, bool displayFloatingScore = false, int scoreAmount = 0) {
		// Get and position Poof game object
		GameObject poof = ObjectPool.S.GetPooledObject("Poof");
		ObjectPool.S.PosAndEnableObj(poof, _.playerSprite[ndx]);

		// Display Floating Score
		if (displayFloatingScore) {
			GameManager.S.InstantiateFloatingScore(_.playerSprite[ndx], scoreAmount.ToString(), Color.green);
		}

		// Set anim
		_.playerAnimator[ndx].CrossFade("Win_Battle", 0);
	}

	public void DamageEnemyAnimation(int ndx, bool displayFloatingScore = false, bool playPlayerAnim = true, bool playDamageSFX = true) {
		// Get and position Explosion game object
		GameObject explosion = ObjectPool.S.GetPooledObject("Explosion");
		ObjectPool.S.PosAndEnableObj(explosion, _.enemySprite[ndx].gameObject);

		// Display Floating Score
		if (displayFloatingScore) {
			GameManager.S.InstantiateFloatingScore(_.enemySprite[ndx].gameObject, _.attackDamage.ToString(), Color.red);
		}

		// Set player anim
		if (playPlayerAnim) {
			_.playerAnimator[_.PlayerNdx()].CrossFade("Win_Battle", 0);
		}

		// Shake Enemy Anim 
		_.enemyAnimator[ndx].CrossFade("Damage", 0);

		// Audio: Damage
		if (playDamageSFX) {
			AudioManager.S.PlayRandomDamageSFX();
		}
	}
}
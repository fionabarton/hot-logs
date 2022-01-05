using System.Collections;
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

    public void AddFunctionToButton(Action<int, Spell> functionToPass, string messageToDisplay, Spell spell) {
		if (Party.S.stats[Battle.S.PlayerNdx()].MP >= spell.cost) {
			// Audio: Confirm
			AudioManager.S.PlaySFX(eSoundName.confirm);

			// Remove all listeners
			Utilities.S.RemoveListeners(BattlePlayerActions.S.playerButtonCS);

			if (spell.type == eSpellType.healing) {
				BattlePlayerActions.S.ButtonsInteractable(false, false, false, false, false, false, false, false, true, true);

				// Set a Player Button as Selected GameObject
				Utilities.S.SetSelectedGO(BattlePlayerActions.S.playerButtonGO[_.PlayerNdx()].gameObject);

				// Set previously selected GameObject
				_.previousSelectedForAudio = BattlePlayerActions.S.playerButtonGO[_.PlayerNdx()].gameObject;

				// Add Item Listeners to Player Buttons
				BattlePlayerActions.S.playerButtonCS[0].onClick.AddListener(delegate { functionToPass(0, spell); });
				BattlePlayerActions.S.playerButtonCS[1].onClick.AddListener(delegate { functionToPass(1, spell); });
				BattlePlayerActions.S.playerButtonCS[2].onClick.AddListener(delegate { functionToPass(2, spell); });

				// If multiple targets
				if (spell.multipleTargets) {
					BattleUI.S.TargetAllPartyMembers();
				}
			} else if (spell.type == eSpellType.offensive) {
				BattlePlayerActions.S.ButtonsInteractable(false, false, false, false, false, true, true, true, false, false);

				// Set an Enemy Button as Selected GameObject
				BattlePlayerActions.S.SetSelectedEnemyButton();

				// Add Item Listeners to Enemy Buttons
				BattlePlayerActions.S.enemyButtonCS[0].onClick.AddListener(delegate { functionToPass(0, spell); });
				BattlePlayerActions.S.enemyButtonCS[1].onClick.AddListener(delegate { functionToPass(1, spell); });
				BattlePlayerActions.S.enemyButtonCS[2].onClick.AddListener(delegate { functionToPass(2, spell); });

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
    public void AttemptHealSelectedPartyMember(int ndx, Spell spell) {
		SpellHelper();

		if (_.playerDead[ndx]) {
			// Display Text
			BattleDialogue.S.DisplayText(Party.S.stats[ndx].name + " is dead...\n...and dead folk can't be healed, dummy!");

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);

			// Switch Mode
			_.battleMode = eBattleMode.playerTurn;

			return;
		}

		if (Party.S.stats[ndx].HP < Party.S.stats[ndx].maxHP) {
			ColorScreen.S.PlayClip("Swell", 0);
			ColorScreen.S.targetNdx = ndx;
			ColorScreen.S.spell = spell;
		} else {
			// Display Text
			BattleDialogue.S.DisplayText(Party.S.stats[ndx].name + " already at full health...\n...no need to cast this spell!");

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);

			// Switch Mode
			_.battleMode = eBattleMode.playerTurn;
		}
	}

    public void HealSelectedPartyMember(int ndx, Spell spell) {
		// Subtract Spell cost from Player's MP
		RPG.S.SubtractPlayerMP(_.PlayerNdx(), spell.cost);
			
		// Get amount and max amount to heal
		int amountToHeal = UnityEngine.Random.Range(spell.statEffectMinValue, spell.statEffectMaxValue);
		int maxAmountToHeal = Party.S.stats[ndx].maxHP - Party.S.stats[ndx].HP;
		// Add Player's WIS to Heal Amount
		amountToHeal += Party.S.stats[ndx].WIS;

		// Add 30-45 HP to TARGET Player's HP
		RPG.S.AddPlayerHP (ndx, amountToHeal);

		// Display Text
		if (amountToHeal >= maxAmountToHeal) {
			BattleDialogue.S.DisplayText("Used " + spell.name + " Spell!\nHealed " + Party.S.stats[ndx].name + " back to Max HP!");

			// Prevents Floating Score being higher than the acutal amount healed
			amountToHeal = maxAmountToHeal;
		} else {
			BattleDialogue.S.DisplayText("Used " + spell.name + " Spell!\nHealed " + Party.S.stats[ndx].name + " for " + amountToHeal + " HP!");
		}

		// Get and position Poof game object
		GameObject poof = ObjectPool.S.GetPooledObject("Poof");
		ObjectPool.S.PosAndEnableObj(poof, _.playerSprite[ndx]);

		// Display Floating Score
		RPG.S.InstantiateFloatingScore(_.playerSprite[ndx], amountToHeal, Color.green);

		// Set anim
		_.playerAnimator[ndx].CrossFade("Win_Battle", 0);

		// Audio: Buff 1
		AudioManager.S.PlaySFX(eSoundName.buff1);

		_.NextTurn ();
	}

	//////////////////////////////////////////////////////////
	/// Fireball - Attack the selected enemy
	////////////////////////////////////////////////////////// 
	public void AttemptAttackSelectedEnemy(int ndx, Spell spell) {
		SpellHelper();
		ColorScreen.S.PlayClip("Flicker", 0);
		ColorScreen.S.targetNdx = ndx;
		ColorScreen.S.spell = spell;
	}

	public void AttackSelectedEnemy(int ndx, Spell spell) {
		// Subtract Spell cost from Player's MP
		RPG.S.SubtractPlayerMP (_.PlayerNdx(), spell.cost);

		// 5% chance to Miss/Dodge...
		// ...but 10% chance if Defender WIS is more than Attacker's 
		if (UnityEngine.Random.value <= 0.05f || (_.enemyStats [ndx].WIS > Party.S.stats[_.PlayerNdx()].WIS && UnityEngine.Random.value < 0.10f)) {
			// Set anim
			_.playerAnimator[_.PlayerNdx()].CrossFade("Fail_Spell", 0);

			if (UnityEngine.Random.value <= 0.5f) {
				BattleDialogue.S.DisplayText(Party.S.stats[_.PlayerNdx()].name + " attempted the spell... but missed " + _.enemyStats[ndx].name + " completely!");
			} else {
				BattleDialogue.S.DisplayText(Party.S.stats[_.PlayerNdx()].name + " cast the spell, but " + _.enemyStats[ndx].name + " deftly dodged out of the way!");
			}

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);

			_.NextTurn ();
		} else {
			// Set anim
			_.playerAnimator[_.PlayerNdx()].CrossFade("Win_Battle", 0);

			// Subtract 8-12 HP
			_.attackDamage = UnityEngine.Random.Range (spell.statEffectMinValue, spell.statEffectMaxValue);
			// Add Player's WIS to Damage
			_.attackDamage += Party.S.stats[_.PlayerNdx()].WIS;
			_.attackDamage += Party.S.stats[_.PlayerNdx()].WIS;
			// Subtract Enemy's DEF from Damage
			_.attackDamage -= _.enemyStats[ndx].DEF;

			// If DEFENDING, cut AttackDamage in HALF
			BattleStats.S.CheckIfDefending (Battle.S.enemyStats [ndx].name);

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
				SpellScreen.S.ScreenOffEnemyDeath(ndx);
			} else {
				// Deactivate Spells Screen then Enemy Turn
				BattleDialogue.S.DisplayText ("Used " + spell.name + " Spell!\nHit " + _.enemyStats [ndx].name + " for " + _.attackDamage + " HP!");

				// Audio: Fireball
				AudioManager.S.PlaySFX(eSoundName.fireball);

				_.NextTurn();
			}
		}
	}

	//////////////////////////////////////////////////////////
	/// Fireblast
	//////////////////////////////////////////////////////////
	public void AttemptAttackAllEnemies(int unusedIntBecauseOfAddFunctionToButtonParameter, Spell spell) {
		SpellHelper();
		ColorScreen.S.PlayClip("Flicker", 1);
		ColorScreen.S.spell = spell;
	}
	
	public void AttackAllEnemies (int unusedIntBecauseOfAddFunctionToButtonParameter, Spell spell) {
		// Subtract Spell cost from Player's MP
		RPG.S.SubtractPlayerMP (_.PlayerNdx(), spell.cost);

		// 5% chance to Miss/Dodge...
		// ...but 25% chance if Defender WIS is more than Attacker's 
		if (UnityEngine.Random.value <= 0.10f || (_.enemyStats[0].WIS > Party.S.stats[_.PlayerNdx()].WIS && UnityEngine.Random.value < 0.10f)) {
			// Set anim
			_.playerAnimator[_.PlayerNdx()].CrossFade("Fail_Spell", 0);

			if (UnityEngine.Random.value <= 0.5f) {
				BattleDialogue.S.DisplayText(Party.S.stats[_.PlayerNdx()].name + " attempted the spell... but missed those goons completely!");
			} else {
				BattleDialogue.S.DisplayText(Party.S.stats[_.PlayerNdx()].name + " cast the spell, but these dummies you're fighting deftly dodged out of the way!");
			}

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);

			_.NextTurn ();
		} else {
			// Set anim
			_.playerAnimator[_.PlayerNdx()].CrossFade("Win_Battle", 0);

			List<int> deadEnemies = new List<int>();

			// Subtract 12-20 HP
			_.attackDamage = UnityEngine.Random.Range (spell.statEffectMinValue, spell.statEffectMaxValue);
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
				BattleStats.S.CheckIfDefending (Battle.S.enemyStats[i].name);

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
				BattleDialogue.S.DisplayText ("Used " + spell.name + " Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, _.enemyStats.Count) + " HP!");

				// Audio: Fireblast
				AudioManager.S.PlaySFX(eSoundName.fireblast);

				_.NextTurn (); 
			} else {
				// Audio: Death
				AudioManager.S.PlaySFX(eSoundName.death);

				EnemiesDeath(deadEnemies, totalAttackDamage);
			}
		}
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
			// Animation: Player WIN BATTLE
			for (int i = 0; i < _.playerAnimator.Count; i++) {
				if (!_.playerDead[i]) {
					_.playerAnimator[i].CrossFade("Win_Battle", 0);
				}
			}

			// Deactivate top display message
			BattleDialogue.S.displayMessageTextTop.gameObject.transform.parent.gameObject.SetActive(false);

			// DropItem or AddExpAndGold
			if (_.droppedItems.Count >= 1) {
				// Switch Mode
				_.battleMode = eBattleMode.dropItem;
			} else {
				// Switch Mode
				_.battleMode = eBattleMode.addExpAndGold;
			}
		} else {
			_.NextTurn(); 
		}
	}

	public void EnemiesDeathHelper(int enemyNdx, string enemyTurnOrder){
		// Drop Enemy Anim
		_.enemyAnimator[enemyNdx].CrossFade("Death", 0);

		// Deactivate Enemy Shield
		_.enemyShields[enemyNdx].SetActive(false);

		// Deactivate Enemy "Help" Word Bubble
		_.enemyHelpBubbles[enemyNdx].SetActive(false);

		// Deactivate Cursors
		BattleUI.S.turnCursor.SetActive(false);
		//BattleUI.S.targetCursor.SetActive(false);
		Utilities.S.SetActiveList(BattleUI.S.targetCursors, false);

		// Deactivate Enemy Button, Stats
		BattlePlayerActions.S.EnemyButtonSetActive(enemyNdx, false);
		
		// Set Selected GameObject (Fight Button)
		_.enemyStats[enemyNdx].isDead = true;

		// Reset this enemy's nextTurnMoveNdx
		_.nextTurnMoveNdx[enemyNdx] = 999;

		// Remove Enemy from Turn Order
		_.turnOrder.Remove (enemyTurnOrder);
		
		// Randomly select DropItem
		BattleEnd.S.AddDroppedItems(enemyNdx);
	}

	//////////////////////////////////////////////////////////
	/// Heal All - Heal all party members 
	//////////////////////////////////////////////////////////
	public void AttemptHealAll(int unusedIntBecauseOfAddFunctionToButtonParameter, Spell spell) {
		SpellHelper();

		if (Party.S.stats[0].HP < Party.S.stats[0].maxHP ||
			Party.S.stats[1].HP < Party.S.stats[1].maxHP ||
			Party.S.stats[2].HP < Party.S.stats[2].maxHP) {
			ColorScreen.S.PlayClip("Swell", 2);
			ColorScreen.S.spell = spell;
		} else {
			// Display Text
			BattleDialogue.S.DisplayText("The party is already at full health...\n...no need to cast this spell!");

			// Deactivate Cursors
			Utilities.S.SetActiveList(BattleUI.S.targetCursors, false);

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);

			// Switch Mode
			_.battleMode = eBattleMode.playerTurn;
		}
	}

	public void HealAll(int unusedIntBecauseOfAddFunctionToButtonParameter, Spell spell) {
		int totalAmountToHeal = 0;

		// Subtract Spell cost from Player's MP
		RPG.S.SubtractPlayerMP(_.PlayerNdx(), spell.cost);

		for (int i = 0; i < _.playerDead.Count; i++) {
			if (!_.playerDead[i]) {
				// Get amount and max amount to heal
				int amountToHeal = UnityEngine.Random.Range(spell.statEffectMinValue, spell.statEffectMaxValue);
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
		BattleDialogue.S.DisplayText("Used " + spell.name + " Spell!\nHealed ALL party members for an average of "
			+ Utilities.S.CalculateAverage(totalAmountToHeal, _.playerDead.Count) + " HP!");

		// Audio: Buff 1
		AudioManager.S.PlaySFX(eSoundName.buff1);

		_.NextTurn();
	}

	public void AttemptReviveSelectedPartyMember(int ndx, Spell spell) {
		SpellHelper();

		if (_.playerDead[ndx]) {
			ColorScreen.S.PlayClip("Swell", 3);
			ColorScreen.S.targetNdx = ndx;
			ColorScreen.S.spell = spell;
		} else {
			// Display Text
			BattleDialogue.S.DisplayText(Party.S.stats[ndx].name + " ain't dead...\n...and dead folk don't need to be revived, dummy!");

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);

			// Switch Mode
			_.battleMode = eBattleMode.playerTurn;
		}
	}

	public void ReviveSelectedPartyMember(int ndx, Spell spell) {
		_.playerDead[ndx] = false;

		// Add to PartyQty 
		_.partyQty += 1;

		// Add Player to Turn Order
		_.turnOrder.Add(Party.S.stats[ndx].name);

		HealSelectedPartyMember(ndx, spell);
	}
}
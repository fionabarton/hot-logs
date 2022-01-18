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

	private static BattleSpells _S;
	public static BattleSpells S { get { return _S; } set { _S = value; } }

	private Battle _;

	void Awake() {
		S = this;
	}

	void Start() {
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

	public void SpellHelper() {
		BattlePlayerActions.S.ButtonsDisableAll();

		Utilities.S.RemoveListeners(BattlePlayerActions.S.playerButtonCS);
		Utilities.S.RemoveListeners(BattlePlayerActions.S.enemyButtonCS);
	}

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
		RPG.S.AddPlayerHP(ndx, amountToHeal);

		CurePlayerAnimation(ndx, true, amountToHeal);
	}

	//////////////////////////////////////////////////////////
	/// Heal - Heal a single party member 
	//////////////////////////////////////////////////////////
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
		RPG.S.SubtractPlayerMP(_.PlayerNdx(), spell.cost);

		Heal(ndx, spell.statEffectMinValue, spell.statEffectMaxValue);

        // Display Text
        if (amountToHeal >= maxAmountToHeal) {
            BattleDialogue.S.DisplayText("Used " + spell.name + " Spell!\nHealed " + Party.S.stats[ndx].name + " back to Max HP!");
        } else {
            BattleDialogue.S.DisplayText("Used " + spell.name + " Spell!\nHealed " + Party.S.stats[ndx].name + " for " + amountToHeal + " HP!");
        }

        // Audio: Buff 1
        AudioManager.S.PlaySFX(eSoundName.buff1);

		_.NextTurn();
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
		RPG.S.SubtractPlayerMP(_.PlayerNdx(), spell.cost);

		// 5% chance to Miss/Dodge...
		// ...but 10% chance if Defender WIS is more than Attacker's 
		if (UnityEngine.Random.value <= 0.05f || (_.enemyStats[ndx].WIS > Party.S.stats[_.PlayerNdx()].WIS && UnityEngine.Random.value < 0.10f)) {
			// Set anim
			_.playerAnimator[_.PlayerNdx()].CrossFade("Fail_Spell", 0);

			if (UnityEngine.Random.value <= 0.5f) {
				BattleDialogue.S.DisplayText(Party.S.stats[_.PlayerNdx()].name + " attempted the spell... but missed " + _.enemyStats[ndx].name + " completely!");
			} else {
				BattleDialogue.S.DisplayText(Party.S.stats[_.PlayerNdx()].name + " cast the spell, but " + _.enemyStats[ndx].name + " deftly dodged out of the way!");
			}

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);

			_.NextTurn();
		} else {
			// Set anim
			_.playerAnimator[_.PlayerNdx()].CrossFade("Win_Battle", 0);

			// Subtract 8-12 HP
			_.attackDamage = UnityEngine.Random.Range(spell.statEffectMinValue, spell.statEffectMaxValue);
			// Add Player's WIS to Damage
			_.attackDamage += Party.S.stats[_.PlayerNdx()].WIS;
			// Subtract Enemy's DEF from Damage
			_.attackDamage -= _.enemyStats[ndx].DEF;

			// If DEFENDING, cut AttackDamage in HALF
			BattleStatusEffects.S.CheckIfDefending(Battle.S.enemyStats[ndx].name);

			if (_.attackDamage < 0) {
				_.attackDamage = 0;
			}

			// Animation: Shake Enemy Anim 
			_.enemyAnimator[ndx].CrossFade("Damage", 0);

			// Get and position Explosion game object
			GameObject explosion = ObjectPool.S.GetPooledObject("Explosion");
			ObjectPool.S.PosAndEnableObj(explosion, _.enemySprite[ndx].gameObject);

			// Display Floating Score
			RPG.S.InstantiateFloatingScore(_.enemySprite[ndx].gameObject, _.attackDamage, Color.red);

			// Subtract Enemy Health
			RPG.S.SubtractEnemyHP(ndx, _.attackDamage);

			if (_.enemyStats[ndx].HP < 1) {
				// Deactivate Spells Screen then Enemy Death
				SpellScreen.S.ScreenOffEnemyDeath(ndx);
			} else {
				// Deactivate Spells Screen then Enemy Turn
				BattleDialogue.S.DisplayText("Used " + spell.name + " Spell!\nHit " + _.enemyStats[ndx].name + " for " + _.attackDamage + " HP!");

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

	public void AttackAllEnemies(int unusedIntBecauseOfAddFunctionToButtonParameter, Spell spell) {
		// Subtract Spell cost from Player's MP
		RPG.S.SubtractPlayerMP(_.PlayerNdx(), spell.cost);

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

			_.NextTurn();
		} else {
			// Set anim
			_.playerAnimator[_.PlayerNdx()].CrossFade("Win_Battle", 0);

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
				BattleStatusEffects.S.CheckIfDefending(Battle.S.enemyStats[i].name);

				if (_.attackDamage < 0) {
					_.attackDamage = 0;
				}

				// Subtract Enemy Heath
				RPG.S.SubtractEnemyHP(i, _.attackDamage);

				// Add to to TotalAttackDamage (Used to Calculate AVERAGE Damage)
				totalAttackDamage += _.attackDamage;

				// Shake Enemy 1, 2, & 3's Anim
				if (!_.enemyStats[i].isDead) {
					// Shake Enemy Anim 
					_.enemyAnimator[i].CrossFade("Damage", 0);

					// Get and position Explosion game object
					GameObject explosion = ObjectPool.S.GetPooledObject("Explosion");
					ObjectPool.S.PosAndEnableObj(explosion, _.enemySprite[i].gameObject);

					// Display Floating Score
					RPG.S.InstantiateFloatingScore(_.enemySprite[i].gameObject, _.attackDamage, Color.red);
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
				BattleDialogue.S.DisplayText("Used " + spell.name + " Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, _.enemyStats.Count) + " HP!");

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
		switch (deadEnemies.Count) {
			case 1: BattleDialogue.S.DisplayText("Used Fire BLAST Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, _.enemyStats.Count) + " HP!" + "\nOne enemy has been felled!"); break;
			case 2: BattleDialogue.S.DisplayText("Used Fire BLAST Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, _.enemyStats.Count) + " HP!" + "\nTwo enemies have been felled!"); break;
			case 3: BattleDialogue.S.DisplayText("Used Fire BLAST Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, _.enemyStats.Count) + " HP!" + "\nThree enemies have been felled!"); break;
		}

		for (int i = 0; i < deadEnemies.Count; i++) {
			BattleEnd.S.EnemyDeath(deadEnemies[i], false);
		}
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
			SpellIsNotUseful("The party is already at full health...\n...no need to cast this spell!");
		}
	}

	public void HealAll(int unusedIntBecauseOfAddFunctionToButtonParameter, Spell spell) {
		int totalAmountToHeal = 0;

		// Subtract Spell cost from Player's MP
		RPG.S.SubtractPlayerMP(_.PlayerNdx(), spell.cost);

		for (int i = 0; i < _.playerDead.Count; i++) {
			if (!_.playerDead[i]) {
				Heal(i, spell.statEffectMinValue, spell.statEffectMaxValue);

				totalAmountToHeal += amountToHeal;
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
		float lowEnd = Party.S.stats[ndx].maxHP * 0.06f;
		float highEnd = Party.S.stats[ndx].maxHP * 0.10f;
		Heal(ndx, (int)lowEnd, (int)highEnd);

		// Display Text
		if (amountToHeal >= maxAmountToHeal) {
			BattleDialogue.S.DisplayText("Used " + spell.name + " Spell!\nHealed " + Party.S.stats[ndx].name + " back to Max HP!");
		} else {
			BattleDialogue.S.DisplayText("Used " + spell.name + " Spell!\nHealed " + Party.S.stats[ndx].name + " for " + amountToHeal + " HP!");
		}

		// Audio: Buff 1
		AudioManager.S.PlaySFX(eSoundName.buff1);

		_.NextTurn();
	}

	//////////////////////////////////////////////////////////
	/// Detoxify - Detoxify a single party member 
	//////////////////////////////////////////////////////////
	public void AttemptDetoxifySinglePartyMember(int ndx, Spell spell) {
		SpellHelper();

		if (_.playerDead[ndx]) {
			SpellIsNotUseful(Party.S.stats[ndx].name + " is dead...\n...and dead folk don't need to be detoxified, dummy!");
			return;
		}

		if (BattleStatusEffects.S.CheckIfPoisoned(Party.S.stats[ndx].name)) {
			ColorScreen.S.PlayClip("Swell", 4);
			ColorScreen.S.targetNdx = ndx;
			ColorScreen.S.spell = spell;
		} else {
			SpellIsNotUseful(Party.S.stats[ndx].name + " is not suffering from the effects of poison...\n...no need to cast this spell!");
		}
	}

	public void DetoxifySinglePartyMember(int ndx, Spell spell) {
		// Subtract Spell cost from Player's MP
		RPG.S.SubtractPlayerMP(_.PlayerNdx(), spell.cost);

		// Remove poison
		BattleStatusEffects.S.RemovePoisoned(Party.S.stats[ndx].name, ndx);

		// Display Text
		BattleDialogue.S.DisplayText("Used " + spell.name + " Spell!\n" + Party.S.stats[ndx].name + " is no longer poisoned!");

		CurePlayerAnimation(ndx, false);

		// Audio: Buff 1
		AudioManager.S.PlaySFX(eSoundName.buff1);

		_.NextTurn();
	}

	//////////////////////////////////////////////////////////
	/// Mobilize - Mobilize a single party member 
	//////////////////////////////////////////////////////////
	public void AttemptMobilizeSinglePartyMember(int ndx, Spell spell) {
		SpellHelper();

		if (_.playerDead[ndx]) {
			SpellIsNotUseful(Party.S.stats[ndx].name + " is dead...\n...and dead folk don't need their mobility restored, dummy!");
			return;
		}

		if (BattleStatusEffects.S.CheckIfParalyzed(Party.S.stats[ndx].name)) {
			ColorScreen.S.PlayClip("Swell", 5);
			ColorScreen.S.targetNdx = ndx;
			ColorScreen.S.spell = spell;
		} else {
			SpellIsNotUseful(Party.S.stats[ndx].name + " is not suffering from the effects of paralysis...\n...no need to cast this spell!");
		}
	}

	public void MobilizeSinglePartyMember(int ndx, Spell spell) {
		// Subtract Spell cost from Player's MP
		RPG.S.SubtractPlayerMP(_.PlayerNdx(), spell.cost);

		// Remove paralysis
		BattleStatusEffects.S.RemoveParalyzed(Party.S.stats[ndx].name, ndx);

		// Display Text
		BattleDialogue.S.DisplayText("Used " + spell.name + " Spell!\n" + Party.S.stats[ndx].name + " is no longer paralyzed!");

		CurePlayerAnimation(ndx, false);

		// Audio: Buff 1
		AudioManager.S.PlaySFX(eSoundName.buff1);

		_.NextTurn();
	}

	//////////////////////////////////////////////////////////
	/// Wake - Wake a single party member 
	//////////////////////////////////////////////////////////
	public void AttemptWakeSinglePartyMember(int ndx, Spell spell) {
		SpellHelper();

		if (_.playerDead[ndx]) {
			SpellIsNotUseful(Party.S.stats[ndx].name + " is dead...\n...and dead folk don't need to wake up, dummy!");
			return;
		}

		if (BattleStatusEffects.S.CheckIfSleeping(Party.S.stats[ndx].name)) {
			ColorScreen.S.PlayClip("Swell", 6);
			ColorScreen.S.targetNdx = ndx;
			ColorScreen.S.spell = spell;
		} else {
			SpellIsNotUseful(Party.S.stats[ndx].name + " is not sleeping...\n...no need to cast this spell!");
		}
	}

	public void WakeSinglePartyMember(int ndx, Spell spell) {
		// Subtract Spell cost from Player's MP
		RPG.S.SubtractPlayerMP(_.PlayerNdx(), spell.cost);

		// Remove sleeping
		BattleStatusEffects.S.RemoveSleeping(Party.S.stats[ndx].name, ndx);

		// Display Text
		BattleDialogue.S.DisplayText("Used " + spell.name + " Spell!\n" + Party.S.stats[ndx].name + " is no longer sleeping!");

		CurePlayerAnimation(ndx, false);

		// Audio: Buff 1
		AudioManager.S.PlaySFX(eSoundName.buff1);

		_.NextTurn();
	}

	public void SpellIsNotUseful(string message) {
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
	}

	public void CurePlayerAnimation(int ndx, bool displayFloatingScore = false, int scoreAmount = 0) {
		// Get and position Poof game object
		GameObject poof = ObjectPool.S.GetPooledObject("Poof");
		ObjectPool.S.PosAndEnableObj(poof, _.playerSprite[ndx]);

		// Display Floating Score
		if (displayFloatingScore) {
			RPG.S.InstantiateFloatingScore(_.playerSprite[ndx], scoreAmount, Color.green);
		}

		// Set anim
		_.playerAnimator[ndx].CrossFade("Win_Battle", 0);
	}
}
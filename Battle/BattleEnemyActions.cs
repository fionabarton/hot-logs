using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleEnemyActions : MonoBehaviour {
	private Battle _;

	void Start () {
		_ = Battle.S;
	}

	// Index = 0
	// Attack ONE Party Member
	public void Attack() {
		// Randomly select party member to attack
		int playerToAttack = _.stats.GetRandomPlayerNdx();
		
		// Calculate Attack Damage
		_.stats.GetAttackDamage(_.enemyStats[_.EnemyNdx()].LVL,
									   _.enemyStats[_.EnemyNdx()].STR, _.enemyStats[_.EnemyNdx()].AGI,
									   Party.S.stats[playerToAttack].DEF, Party.S.stats[playerToAttack].AGI,
									   _.enemyStats[_.EnemyNdx()].name, Party.S.stats[playerToAttack].name,
										Party.S.stats[playerToAttack].HP, true, playerToAttack);

		// Subtract Player Health
		GameManager.S.SubtractPlayerHP (playerToAttack, _.attackDamage);

		// Play attack animations, SFX, and spawn objects
		PlaySingleAttackAnimsAndSFX(playerToAttack);
		//StartCoroutine("MultiAttack");

		// Player Death or Next Turn
		if (Party.S.stats[playerToAttack].HP < 1) {
			_.end.PlayerDeath(playerToAttack);
		} else {
			// If not sleeping or paralyzed...attempt to block!
            if (!StatusEffects.S.CheckIfParalyzed(true, playerToAttack) &&
				!StatusEffects.S.CheckIfSleeping(true, playerToAttack)) {
				// Index of the party member that is blocking
				_.qte.blockerNdx = playerToAttack;

				// Set qteType to Block
				_.qte.qteType = 4;

				// Enable progress bar/timer 
				_.qte.Initialize();

				// Set battleMode to QTE
				_.mode = eBattleMode.qte;
			} else {
				// Deactivate Battle Text
				_.dialogue.displayMessageTextTop.gameObject.transform.parent.gameObject.SetActive(false);

				_.NextTurn();
			}
		}	
	}

	public IEnumerator MultiAttack() {
		PlaySingleAttackAnimsAndSFX(0);
		yield return new WaitForSeconds(0.1f);
		PlaySingleAttackAnimsAndSFX(1);
		yield return new WaitForSeconds(0.1f);
		PlaySingleAttackAnimsAndSFX(0);
		yield return new WaitForSeconds(0.1f);
		PlaySingleAttackAnimsAndSFX(1);
		yield return new WaitForSeconds(0.1f);
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Index = 1
	// Defend
	public void Defend(){
		StatusEffects.S.AddDefender(false, _.EnemyNdx());

		_.dialogue.DisplayText (_.enemyStats [_.EnemyNdx()].name + " defends themself until their next turn!");

		_.NextTurn();
	}
	///////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Index = 2
	// Run
	public void Run(){
		_.end.EnemyRun(_.EnemyNdx());
	}
	///////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Index = 3
	// Stunned
	public void Stunned(){
		// Audio: Deny
		AudioManager.S.PlaySFX(eSoundName.deny);

		_.dialogue.DisplayText (_.enemyStats [_.EnemyNdx()].name + " is stunned and doesn't move!\nWhat a rube!");
		_.NextTurn();
	}
	///////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Index = 4
	// Heal Spell
	public void AttemptHealSpell(){
		// Enough MP
		if (_.enemyStats [_.EnemyNdx()].MP >= 3) {

			// Get index of enemy with the lowest HP
			int enemyNdx = _.stats.GetEnemyWithLowestHP();

			// Not at Full HP
			if (enemyNdx != -1) {
				ColorScreen.S.PlayClip("Swell", 1);
				ColorScreen.S.targetNdx = enemyNdx;
			} else {
				// Already at Full HP
				_.dialogue.DisplayText(_.enemyStats[_.EnemyNdx()].name + " thought about casting a Heal Spell...\n...But then remembered everyone's at full health...\n...and gave up!");

				// Audio: Deny
				AudioManager.S.PlaySFX(eSoundName.deny);

				_.NextTurn();
			}
		} else {
			// Not enough MP
			_.dialogue.DisplayText(_.enemyStats[_.EnemyNdx()].name + " attempts to cast a Heal Spell...\n...But doesn't have enough MP to do so!");

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);

			_.NextTurn(); 
		}
	}

	// Called after BattleBlackScreen "Swell" animation
	public void HealSpell(int ndx) {
		// Subtract Spell cost from Enemy's MP
		_.enemyStats[_.EnemyNdx()].MP -= 3;

		// Get amount and max amount to heal
		int amountToHeal = UnityEngine.Random.Range(30, 45);
		int maxAmountToHeal = _.enemyStats[ndx].maxHP - _.enemyStats[ndx].HP;
		// Add Enemy's WIS to Heal Amount
		amountToHeal += _.enemyStats[ndx].WIS;

		// Add 30-45 HP to TARGET Player's HP
		GameManager.S.AddEnemyHP(ndx, amountToHeal);

		// Display Text
		if (amountToHeal >= maxAmountToHeal) {
			if(ndx == _.EnemyNdx()) {
				_.dialogue.DisplayText(_.enemyStats[ndx].name + " casts a Heal Spell!\nHealed itself back to Max HP!");
			} else {
				_.dialogue.DisplayText(_.enemyStats[_.EnemyNdx()].name + " casts a Heal Spell!\nHealed "+ _.enemyStats[ndx].name + " back to Max HP!");
			}

			// Prevents Floating Score being higher than the acutal amount healed
			amountToHeal = maxAmountToHeal;
		} else {
			if (ndx == _.EnemyNdx()) {
				_.dialogue.DisplayText(_.enemyStats[ndx].name + " casts a Heal Spell!\nHealed itself for " + amountToHeal + " HP!");
			} else {
				_.dialogue.DisplayText(_.enemyStats[_.EnemyNdx()].name + " casts a Heal Spell!\nHealed " + _.enemyStats[ndx].name + " for " + amountToHeal + " HP!");
			}
		}

		// Get and position Poof game object
		GameObject poof = ObjectPool.S.GetPooledObject("Poof");
		ObjectPool.S.PosAndEnableObj(poof, _.enemySprite[ndx].gameObject);

		// Display Floating Score
		GameManager.S.InstantiateFloatingScore(_.enemySprite[ndx].gameObject, amountToHeal.ToString(), Color.green);

		// Audio: Buff
		AudioManager.S.PlaySFX(eSoundName.buff1);

		_.NextTurn();
	}
	///////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Index = 8
	// Single Attack
	public void AttemptAttackSingle() {
		// Enough MP
		if (_.enemyStats[_.EnemyNdx()].MP >= 1) {
			ColorScreen.S.PlayClip("Flicker", 3);
		} else {
			// Not enough MP
			_.dialogue.DisplayText(_.enemyStats[_.EnemyNdx()].name + " attempts to cast Fireball...\n...But doesn't have enough MP to do so!");

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);

			// Animation: Enemy ATTACK in center
			_.enemyAnimator[_.animNdx].CrossFade("Attack", 0);

			_.NextTurn();
		}
	}

	public void AttackSingle() {
		// Subtract Enemy MP
		_.enemyStats[_.EnemyNdx()].MP -= 1;

		// 5% chance to Miss/Dodge...
		// ...but 10% chance if Defender WIS is more than Attacker's 
		if (Random.value <= 0.05f || (_.enemyStats[_.EnemyNdx()].WIS > Party.S.stats[0].WIS && Random.value < 0.10f)) {
			if (Random.value <= 0.5f) {
				_.dialogue.DisplayText(_.enemyStats[_.EnemyNdx()].name + " attempted to cast Fireball... but missed the party completely!");
			} else {
				_.dialogue.DisplayText(_.enemyStats[_.EnemyNdx()].name + " cast Fireball, but the party deftly dodged out of the way!");
			}

			// Animation: Enemy ATTACK in center
			_.enemyAnimator[_.animNdx].CrossFade("Attack", 0);

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);

			_.NextTurn();
		} else {
			// Randomly select party member to attack
			int playerToAttack = _.stats.GetRandomPlayerNdx();

			// Subtract 8-12 HP
			_.attackDamage = Random.Range(8, 12);

			// Subtract Player Health
			GameManager.S.SubtractPlayerHP(playerToAttack, _.attackDamage);

			// Play attack animations, SFX, and spawn objects
			PlaySingleAttackAnimsAndSFX(playerToAttack);

			// Player Death or Next Turn
			if (Party.S.stats[playerToAttack].HP < 1) {
				_.end.PlayerDeath(playerToAttack);
			} else {
				_.dialogue.DisplayText("Used Fireball Spell!\nHit " + Party.S.stats[playerToAttack].name + " for " + _.attackDamage + " HP!");

				// Audio: Fireblast
				AudioManager.S.PlaySFX(eSoundName.fireball);

				_.NextTurn();
			}
		}
	}
	///////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Index = 5
	// Attack All
	public void AttemptAttackAll() {
		// Enough MP
		if (_.enemyStats[_.EnemyNdx()].MP >= 3) {
			ColorScreen.S.PlayClip("Flicker", 2);
		} else {
			// Not enough MP
			_.dialogue.DisplayText(_.enemyStats[_.EnemyNdx()].name + " attempts to cast Fireblast...\n...But doesn't have enough MP to do so!");

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);

			// Animation: Enemy ATTACK in center
			_.enemyAnimator[_.animNdx].CrossFade("Attack", 0);

			_.NextTurn();
		}
	}

	public void AttackAll() {
		// Subtract Enemy MP
		_.enemyStats [_.EnemyNdx()].MP -= 3;

		// Animation: Enemy ATTACK in center
		_.enemyAnimator[_.animNdx].CrossFade("Attack", 0);

		// 5% chance to Miss/Dodge...
		// ...but 10% chance if Defender WIS is more than Attacker's 
		if (Random.value <= 0.05f || (_.enemyStats[_.EnemyNdx()].WIS > Party.S.stats[0].WIS && Random.value < 0.10f)) {
			if (Random.value <= 0.5f) {
				_.dialogue.DisplayText (_.enemyStats [_.EnemyNdx()].name + " attempted to cast Fireblast... but missed the party completely!");
			} else {
				_.dialogue.DisplayText (_.enemyStats [_.EnemyNdx()].name + " cast Fireblast, but the party deftly dodged out of the way!");
			}

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);

			_.NextTurn ();
		} else {
			// Shake Screen Anim
			Battle.S.battleUIAnim.CrossFade ("BattleUI_Shake", 0);

			List<int> deadPlayers = new List<int>();

			// Subtract 12-20 HP
			_.attackDamage = Random.Range (10, 15);

			// Add Enemy's WIS to Damage
			_.attackDamage += _.enemyStats[_.EnemyNdx()].WIS;

			// Cache AttackDamage. When more than one Defender, prevents splitting it in 1/2 more than once.
			int tAttackDamage = _.attackDamage;

			// Used to Calculate AVERAGE Damage
			int totalAttackDamage = 0;

			// Loop through Players
			for (int i = 0; i < (Party.S.partyNdx + 1); i++) {
				// Subtract Player's DEF from Damage
				_.attackDamage -= Party.S.stats[i].DEF;

				// If DEFENDING, cut AttackDamage in HALF
				StatusEffects.S.CheckIfDefending(true, i);

				if (_.attackDamage < 0) {
					_.attackDamage = 0;
				} 

				// Subtract Player Health
				GameManager.S.SubtractPlayerHP (i, _.attackDamage);

				// Add to to TotalAttackDamage (Used to Calculate AVERAGE Damage)
				totalAttackDamage += _.attackDamage;

				// Shake Enemy 1, 2, & 3's Anim
				if (!_.playerDead[i]) {
					// If player doesn't have a status ailment...
					if (!StatusEffects.S.HasStatusAilment(true, i)) {
						// Animation: Player Damage
						_.playerAnimator[i].CrossFade("Damage", 0);
					}

					// Get and position Explosion game object
					GameObject explosion = ObjectPool.S.GetPooledObject("Explosion");
					ObjectPool.S.PosAndEnableObj (explosion, _.playerSprite[i]);

					// Display Floating Score
					GameManager.S.InstantiateFloatingScore(_.playerSprite[i], _.attackDamage.ToString(), Color.red);
				}

				// If DEFENDING, Reset AttackDamage for next Enemy
				_.attackDamage = tAttackDamage;

				// If Player HP < 0, DEAD!
				if (Party.S.stats[i].HP < 1 && !_.playerDead[i]) {
					deadPlayers.Add(i);
				}
			}

			// If no one is killed...
			if (deadPlayers.Count <= 0) {
				_.dialogue.DisplayText ("Used Fireblast Spell!\nHit ENTIRE party for an average of " + Utilities.S.CalculateAverage (totalAttackDamage, (Party.S.partyNdx + 1)) + " HP!");

				// Audio: Fireblast
				AudioManager.S.PlaySFX(eSoundName.fireblast);

				_.NextTurn (); 
			} else {
				PlayersDeath(deadPlayers, totalAttackDamage);
			}
		}
	}
	///////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Index = 7
	// Call for backup next turn
	public void CallForBackupNextTurn() {
		_.nextTurnMoveNdx[_.EnemyNdx()] = 6;

		// Activate Enemy "Help" Word Bubble
		_.enemyHelpBubbles[_.EnemyNdx()].SetActive(true);

		_.dialogue.DisplayText(_.enemyStats[_.EnemyNdx()].name + " is getting ready to call for help!");
		_.NextTurn();
	}

	// Index = 6
	// Call for backup 
	public void CallForBackup() {
		// Deactivate Enemy "Help" Word Bubble
		_.enemyHelpBubbles[_.EnemyNdx()].SetActive(false);

		if (_.enemyStats[0].isDead) {
			CallForBackupHelper(0);
		} else if (_.enemyStats[1].isDead) {
			CallForBackupHelper(1);
		} else if (_.enemyStats[2].isDead) {
			CallForBackupHelper(2);
		} else if (_.enemyStats[3].isDead) {
			CallForBackupHelper(3);
		} else if (_.enemyStats[4].isDead) {
			CallForBackupHelper(4);
		} else {
			_.dialogue.DisplayText(_.enemyStats[_.EnemyNdx()].name + " called for backup...\n...but no one came!");

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);
		}

		_.NextTurn();
	}

	public void CallForBackupHelper(int enemyNdx) {
		// Set Selected GameObject (Fight Button)
		_.enemyStats[enemyNdx].isDead = false;

		// Add to Turn Order
		_.turnOrder.Add(_.enemyStats[enemyNdx].name);

		// Reset HP/MP
		_.enemyStats[enemyNdx].HP = _.enemyStats[enemyNdx].maxHP;
		_.enemyStats[enemyNdx].MP = _.enemyStats[enemyNdx].maxMP;

		// Gold/EXP payout
		_.expToAdd += _.enemyStats[enemyNdx].EXP;
		_.goldToAdd += _.enemyStats[enemyNdx].Gold;

		// Activate/Deactivate Enemy Buttons, Stats, Sprites
		_.playerActions.EnemyButtonSetActive(enemyNdx, true);
		_.enemySprite[enemyNdx].enabled = true;

		// Enable/Update Health Bars
		ProgressBars.S.enemyHealthBarsCS[enemyNdx].transform.parent.gameObject.SetActive(true);
		ProgressBars.S.enemyHealthBarsCS[enemyNdx].UpdateBar(_.enemyStats[enemyNdx].HP, _.enemyStats[enemyNdx].maxHP);

		// Animation: Enemy ARRIVAL 
		_.enemyAnimator[enemyNdx].CrossFade("Arrival", 0);

		// Add to EnemyAmount 
		_.enemyAmount += 1;

		// Audio: Run
		AudioManager.S.PlaySFX(eSoundName.run);

		// Display Text
		_.dialogue.DisplayText(_.enemyStats[_.EnemyNdx()].name + " called for backup...\n...and someone came!");
	}

	public void PlayersDeath(List<int> deadPlayers, int totalAttackDamage) {
		switch (deadPlayers.Count) {
			case 1: _.dialogue.DisplayText("Used Fireblast Spell!\nHit ENTIRE party for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, (Party.S.partyNdx + 1)) + " HP!" + "\nOne party member has been felled!"); break;
			case 2: _.dialogue.DisplayText("Used Fireblast Spell!\nHit ENTIRE party for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, (Party.S.partyNdx + 1)) + " HP!" + "\nTwo party members have been felled!"); break;
			case 3: _.dialogue.DisplayText("Used Fireblast Spell!\nHit ENTIRE party for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, (Party.S.partyNdx + 1)) + " HP!" + "\nThree party members have been felled!"); break;
		}

		for (int i = 0; i < deadPlayers.Count; i++) {
			_.end.PlayerDeath(deadPlayers[i], false);
		}
	}
	///////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Index = 9
	// Charge
	public void Charge() {
		// Audio: Buff
		AudioManager.S.PlaySFX(eSoundName.buff2);

		_.dialogue.DisplayText(_.enemyStats[_.EnemyNdx()].name + " is getting ready to do something cool...\n...what could it be?!");
		_.NextTurn();
	}
	///////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Index = 10
	// Poison
	public void Poison(int ndx) {
		// Poison party member
		StatusEffects.S.AddPoisoned(ndx);

		// Play attack animations, SFX, and spawn objects
		PlaySingleAttackAnimsAndSFX(ndx, true, false);
	}
	///////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Index = 11
	// Paralyze
	public void Paralyze(int ndx) {
		// Paralyze party member
		StatusEffects.S.AddParalyzed(ndx);

		// Play attack animations, SFX, and spawn objects
		PlaySingleAttackAnimsAndSFX(ndx, true, false);
    }
	///////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Index = 12
	// Sleep
	public void Sleep(int ndx) {
		// Put party member to sleep
		StatusEffects.S.AddSleeping(ndx);

		// Play attack animations, SFX, and spawn objects
		PlaySingleAttackAnimsAndSFX(ndx, true, false);
	}
	///////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Index = 13
	// Steal
	public void AttemptSteal() {
		ColorScreen.S.targetNdx = _.stats.GetRandomPlayerNdx();
		ColorScreen.S.PlayClip("Flicker", 8);
	}

	public void Steal(int ndx) {
		if (Inventory.S.GetItemList().Count != 0) {
			// 50% chance to miss...
			if (Random.value <= 0.5f) {
				// Get random party item index and item
				int itemNdx = Random.Range(0, Inventory.S.GetItemList().Count);
				Item tItem = Inventory.S.GetItemList()[itemNdx];

				if(tItem.type != eItemType.Important) {
					// Remove item from party inventory
					Inventory.S.RemoveItemFromInventory(tItem);

					// Add item to stolen items inventory
					_.enemyStats[_.EnemyNdx()].stolenItems.Add(tItem);
					_.enemyStats[_.EnemyNdx()].amountToSteal += 1;

					// Play attack animations, SFX, and spawn objects
					PlaySingleAttackAnimsAndSFX(ndx, true, false);

					// Display text: item stolen
					_.dialogue.DisplayText(_.enemyStats[_.EnemyNdx()].name + " swiped a " + tItem.name + " from " + Party.S.stats[ndx].name + ".\n" + WordManager.S.GetRandomInterjection() + "!");
				} else {
					// Audio: Deny
					AudioManager.S.PlaySFX(eSoundName.deny);

					// Display text: can't steal an important item
					_.dialogue.DisplayText(_.enemyStats[_.EnemyNdx()].name + " attempted to steal a "+ tItem.name + " from " + Party.S.stats[ndx].name + "...\n...but it can't be stolen!\n" + WordManager.S.GetRandomExclamation() + "!");
				}
			} else {
				// Audio: Deny
				AudioManager.S.PlaySFX(eSoundName.deny);

				// Display text: miss
				_.dialogue.DisplayText(_.enemyStats[_.EnemyNdx()].name + " attempted to loot an item from " + Party.S.stats[ndx].name + "...\n...but missed the mark!\n" + WordManager.S.GetRandomExclamation() + "!");
			}
		} else {
			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);

			// Display text: no items to steal
			_.dialogue.DisplayText(_.enemyStats[_.EnemyNdx()].name + " attempted to steal an item from " + Party.S.stats[ndx].name + "...\n...but they've got nothing!\n" + WordManager.S.GetRandomExclamation() + "!");
		}

		_.NextTurn();
	}
	///////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Play attack animations, SFX, and spawn objects
	public void PlaySingleAttackAnimsAndSFX(int playerToAttack, bool playEnemyAnim = true, bool displayFloatingScore = true) {
		// Animation: Enemy ATTACK in center
		if (playEnemyAnim) {
			_.enemyAnimator[_.animNdx].CrossFade("Attack", 0);
		}

        // If player doesn't have a status ailment...
        if (!StatusEffects.S.HasStatusAilment(true, playerToAttack)) {
            // Animation: Player Damage
            _.playerAnimator[playerToAttack].CrossFade("Damage", 0);
		}

		// Audio: Damage
		AudioManager.S.PlayRandomDamageSFX();

		// Animation: Shake Screen
		Battle.S.battleUIAnim.CrossFade("BattleUI_Shake", 0);

		// Get and position Explosion game object
		GameObject explosion = ObjectPool.S.GetPooledObject("Explosion");
		ObjectPool.S.PosAndEnableObj(explosion, _.playerSprite[playerToAttack]);

		// Display Floating Score
		if (displayFloatingScore) {
			GameManager.S.InstantiateFloatingScore(_.playerSprite[playerToAttack], _.attackDamage.ToString(), Color.red);
		}
	}
}
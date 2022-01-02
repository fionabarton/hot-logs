using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleEnemyActions : MonoBehaviour {
	[Header ("Set Dynamically")]
	// Singleton
	private static BattleEnemyActions _S;
	public static BattleEnemyActions S { get { return _S; } set { _S = value; } }

	private Battle _;

	void Awake() {
		S = this;
	}

	void Start () {
		_ = Battle.S;
	}

	// Play attack animations, SFX, and spawn objects
	public void PlaySingleAttackAnimsAndSFX(int playerToAttack) {
		// Animation: Enemy ATTACK in center
		_.enemyAnimator[_.animNdx].CrossFade("Attack", 0);

		// Animation: Player Damage
		_.playerAnimator[playerToAttack].CrossFade("Damage", 0);

		// Audio: Damage
		int randomInt = Random.Range(2, 4);
		AudioManager.S.PlaySFX(randomInt);

		// Animation: Shake Screen
		Battle.S.battleUIAnim.CrossFade("BattleUI_Shake", 0);

		// Get and position Explosion game object
		GameObject explosion = ObjectPool.S.GetPooledObject("Explosion");
		ObjectPool.S.PosAndEnableObj(explosion, _.playerSprite[playerToAttack]);

		// Display Floating Score
		RPG.S.InstantiateFloatingScore(_.playerSprite[playerToAttack], _.attackDamage, Color.red);
	}
	///////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Index = 0
	// Attack ONE Party Member
	public void Attack() {
		// Attempt to run if the enemy's attack won't do any damage to the player
		if (Random.value < _.enemyStats[_.EnemyNdx()].chanceToCallMove) {
			// Calculate Max Damage ((Lvl * 4) + Str - Def)
			int maxAttackerDamage = (_.enemyStats[_.EnemyNdx()].LVL * 4) + _.enemyStats[_.EnemyNdx()].STR - Party.S.stats[0].DEF;

			// If attack doesn't do any damage...
			if (maxAttackerDamage <= 0) {
				// ...RUN!
				_.enemyStats[_.EnemyNdx()].AI = eEnemyAI.RunAway;
				return;
			}
		}

		// Randomly select party member to attack
		int playerToAttack = BattleStats.S.GetRandomPlayerNdx();

		// Calculate Attack Damage
		Battle.S.CalculateAttackDamage(_.enemyStats[_.EnemyNdx()].LVL,
									   _.enemyStats[_.EnemyNdx()].STR, _.enemyStats[_.EnemyNdx()].AGI,
									   Party.S.stats[playerToAttack].DEF, Party.S.stats[playerToAttack].AGI,
									   _.enemyStats[_.EnemyNdx()].name, Party.S.stats[playerToAttack].name,
										Party.S.stats[playerToAttack].HP);

		// Subtract Player Health
		RPG.S.SubtractPlayerHP (playerToAttack, _.attackDamage);

		// Play attack animations, SFX, and spawn objects
		PlaySingleAttackAnimsAndSFX(playerToAttack);

		// Player Death or Next Turn
		if (Party.S.stats[playerToAttack].HP < 1) {
			BattleEnd.S.PlayerDeath(playerToAttack);
		} else {
			// BLOCK!!!

			// Index of the party member that is blocking
			BattleQTE.S.blockerNdx = playerToAttack;

			// Set qteType to Block
			BattleQTE.S.qteType = 4;

			// Enable progress bar/timer 
			BattleQTE.S.Initialize();

			// Set battleMode to QTE
			_.battleMode = eBattleMode.qte;
		}	
	}
	///////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Index = 1
	// Defend
	public void Defend (){
		_.AddDefender(_.enemyStats [_.EnemyNdx()].name);

		// Activate Enemy Shield
		_.enemyShields[_.EnemyNdx()].SetActive(true);

		BattleDialogue.S.DisplayText (_.enemyStats [_.EnemyNdx()].name + " defends themself until their next turn!");

		// Audio: Buff 2
		AudioManager.S.PlaySFX(eSoundName.buff2);

		_.NextTurn ();
	}
	///////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Index = 2
	// Run
	public void Run (){
		BattleEnd.S.EnemyRun(_.EnemyNdx());
	}
	///////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Index = 3
	// Stunned
	public void Stunned (){
		// Audio: Deny
		AudioManager.S.PlaySFX(eSoundName.deny);

		BattleDialogue.S.DisplayText (_.enemyStats [_.EnemyNdx()].name + " is stunned and doesn't move!\nWhat a rube!");
		_.NextTurn ();
	}
	///////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Index = 4
	// Heal Spell
	public void AttemptHealSpell (){
		// Enough MP
		if (_.enemyStats [_.EnemyNdx()].MP >= 3) {

			// Get index of enemy with the lowest HP
			int enemyNdx = BattleStats.S.GetEnemyWithLowestHP();

			// Not at Full HP
			if (enemyNdx != -1) {
				ColorScreen.S.PlayClip("Swell", 1);
				ColorScreen.S.targetNdx = enemyNdx;
			} else {
				// Already at Full HP
				BattleDialogue.S.DisplayText(_.enemyStats[_.EnemyNdx()].name + " thought about casting a Heal Spell...\n...But then remembered everyone's at full health...\n...and gave up!");

				// Audio: Deny
				AudioManager.S.PlaySFX(eSoundName.deny);

				_.NextTurn();
			}
		} else {
			// Not enough MP
			BattleDialogue.S.DisplayText (_.enemyStats[_.EnemyNdx()].name + " attempts to cast a Heal Spell...\n...But doesn't have enough MP to do so!");

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);

			_.NextTurn (); 
		}

		// Otherwise...
		// TBR
		// Animation: Enemy STUNNED
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
		RPG.S.AddEnemyHP(ndx, amountToHeal);

		// Display Text
		if (amountToHeal >= maxAmountToHeal) {
			if(ndx == _.EnemyNdx()) {
				BattleDialogue.S.DisplayText(_.enemyStats[ndx].name + " casts a Heal Spell!\nHealed itself back to Max HP!");
			} else {
				BattleDialogue.S.DisplayText(_.enemyStats[_.EnemyNdx()].name + " casts a Heal Spell!\nHealed "+ _.enemyStats[ndx].name + " back to Max HP!");
			}

			// Prevents Floating Score being higher than the acutal amount healed
			amountToHeal = maxAmountToHeal;
		} else {
			if (ndx == _.EnemyNdx()) {
				BattleDialogue.S.DisplayText(_.enemyStats[ndx].name + " casts a Heal Spell!\nHealed itself for " + amountToHeal + " HP!");
			} else {
				BattleDialogue.S.DisplayText(_.enemyStats[_.EnemyNdx()].name + " casts a Heal Spell!\nHealed " + _.enemyStats[ndx].name + " for " + amountToHeal + " HP!");
			}
		}

		// Get and position Poof game object
		GameObject poof = ObjectPool.S.GetPooledObject("Poof");
		ObjectPool.S.PosAndEnableObj(poof, _.enemySprite[ndx].gameObject);

		// Display Floating Score
		RPG.S.InstantiateFloatingScore(_.enemySprite[ndx].gameObject, amountToHeal, Color.green);

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
			BattleDialogue.S.DisplayText(_.enemyStats[_.EnemyNdx()].name + " attempts to cast Fireball...\n...But doesn't have enough MP to do so!");

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

		// Randomly select party member to attack
		int playerToAttack = BattleStats.S.GetRandomPlayerNdx();

		// Subtract 8-12 HP
		_.attackDamage = Random.Range(8, 12);

		// Subtract Player Health
		RPG.S.SubtractPlayerHP(playerToAttack, _.attackDamage);

		// Play attack animations, SFX, and spawn objects
		PlaySingleAttackAnimsAndSFX(playerToAttack);

		// Player Death or Next Turn
		if (Party.S.stats[playerToAttack].HP < 1) {
			BattleEnd.S.PlayerDeath(playerToAttack);
		} else {
			BattleDialogue.S.DisplayText("Used Fireball Spell!\nHit " + Party.S.stats[playerToAttack].name + " for " + _.attackDamage + " HP!");

			// Audio: Fireblast
			AudioManager.S.PlaySFX(eSoundName.fireball);

			_.NextTurn();
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
			BattleDialogue.S.DisplayText(_.enemyStats[_.EnemyNdx()].name + " attempts to cast Fireblast...\n...But doesn't have enough MP to do so!");

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

		// *** TBR: Reference the Defender (still living) with the highest WIS ***
		// 
		// Miss/Dodge
		// 5% chance to Miss/Dodge...
		// ...but 25% chance if Defender WIS is more than Attacker's 
		if (Random.value <= 0.05f || (_.enemyStats[_.EnemyNdx()].WIS > Party.S.stats[0].WIS && Random.value < 0.25f)) {
			if (Random.value <= 0.5f) {
				BattleDialogue.S.DisplayText (_.enemyStats [_.EnemyNdx()].name + " attempted to cast Fireblast... but missed the party completely!");
			} else {
				BattleDialogue.S.DisplayText (_.enemyStats [_.EnemyNdx()].name + " cast Fireblast, but the party deftly dodged out of the way!");
			}

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);

			_.NextTurn ();
		} else {
			// Shake Screen Anim
			Battle.S.battleUIAnim.CrossFade ("BattleUI_Shake", 0); 

			int qtyKilled = 0;
			bool[] tDead = new bool[3];

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
				_.CheckIfDefending(Party.S.stats[i].name);

				if (_.attackDamage < 0) {
					_.attackDamage = 0;
				} 

				// Subtract Player Health
				RPG.S.SubtractPlayerHP (i, _.attackDamage);

				// Add to to TotalAttackDamage (Used to Calculate AVERAGE Damage)
				totalAttackDamage += _.attackDamage;

				// Shake Enemy 1, 2, & 3's Anim
				if (!_.playerDead[i]) {
					// Animation: Player Damage
					_.playerAnimator[i].CrossFade("Damage", 0);

					// Get and position Explosion game object
					GameObject explosion = ObjectPool.S.GetPooledObject("Explosion");
					ObjectPool.S.PosAndEnableObj (explosion, _.playerSprite[i]);

					// Display Floating Score
					RPG.S.InstantiateFloatingScore(_.playerSprite[i], _.attackDamage, Color.red);
				}

				// If DEFENDING, Reset AttackDamage for next Enemy
				_.attackDamage = tAttackDamage;

				// If Player HP < 0, DEAD!
				if (Party.S.stats[i].HP < 1 && !_.playerDead[i]) {
					qtyKilled += 1;
					tDead[i] = true;
				}
			}

			// If no one is killed...
			if (qtyKilled <= 0) {
				BattleDialogue.S.DisplayText ("Used Fireblast Spell!\nHit ENTIRE party for an average of " + Utilities.S.CalculateAverage (totalAttackDamage, (Party.S.partyNdx + 1)) + " HP!");

				// Audio: Fireblast
				AudioManager.S.PlaySFX(eSoundName.fireblast);

				_.NextTurn (); 
			} else {
				// Audio: Death
				AudioManager.S.PlaySFX(eSoundName.death);

				PlayersDeath(qtyKilled, totalAttackDamage, tDead[0], tDead[1], tDead[2]);
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

		BattleDialogue.S.DisplayText(_.enemyStats[_.EnemyNdx()].name + " is getting ready to call for help!");
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
		} else {
			BattleDialogue.S.DisplayText(_.enemyStats[_.EnemyNdx()].name + " called for backup...\n...but no one came!");

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
		BattlePlayerActions.S.EnemyButtonSetActive(enemyNdx, true);
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
		BattleDialogue.S.DisplayText(_.enemyStats[_.EnemyNdx()].name + " called for backup...\n...and someone came!");
	}
	///////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Index = 9
	// Charge
	public void Charge() {
		// Audio: Buff
		AudioManager.S.PlaySFX(eSoundName.buff2);

		BattleDialogue.S.DisplayText(_.enemyStats[_.EnemyNdx()].name + " is getting ready to do something cool...\n...what could it be?!");
		_.NextTurn();
	}
	///////////////////////////////////////////////////////////////////////////////////////////////////////////
	public void PlayersDeath (int qtyKilled, int totalAttackDamage, bool player1 = false, bool player2 = false, bool player3 = false) {
		// Subtract from PartyQty 
		_.partyQty -= qtyKilled;

		switch (qtyKilled) {
		case 1: BattleDialogue.S.DisplayText("Used Fireblast Spell!\nHit ENTIRE party for an average of " + Utilities.S.CalculateAverage (totalAttackDamage, (Party.S.partyNdx + 1)) + " HP!" + "\nOne party member has been felled!"); break;
		case 2: BattleDialogue.S.DisplayText("Used Fireblast Spell!\nHit ENTIRE party for an average of " + Utilities.S.CalculateAverage (totalAttackDamage, (Party.S.partyNdx + 1)) + " HP!" + "\nTwo party members have been felled!"); break;
		case 3: BattleDialogue.S.DisplayText("Used Fireblast Spell!\nHit ENTIRE party for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, (Party.S.partyNdx + 1)) + " HP!" + "\nThree party members have been felled!"); break;
		}

		if (player1) { PlayersDeathHelper(0, Party.S.stats[0].name); }
		if (player2) { PlayersDeathHelper(1, Party.S.stats[1].name); }
		if (player3) { PlayersDeathHelper(2, Party.S.stats[2].name); }

		// Add PartyDeath or NextTurn
		if (_.partyQty < 0) {
			_.battleMode = eBattleMode.partyDeath;} 
		else {
			_.NextTurn (); 
		}
	}
	public void PlayersDeathHelper(int playerNdx, string playerTurnOrder){
		// Deactivate Player Shield
		_.playerShields[playerNdx].SetActive(false);

		_.playerDead[playerNdx] = true;

		// Animation: Player DEATH
		_.playerAnimator[playerNdx].CrossFade("Death", 0);

		// Remove Player from Turn Order
		_.turnOrder.Remove (playerTurnOrder); 
	}
}
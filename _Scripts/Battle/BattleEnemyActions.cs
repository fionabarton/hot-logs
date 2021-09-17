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

	// Index = 0
	// Attack ONE Party Member
	public void Attack (){
		// Run if Attack Damage = 0
		BattleEnemyAI.S.CheckIfAttackIsUseless ();

		// Randomly select party member to attack
		int playerToAttack = 0;
		if (_.partyQty >= 1) {
			if (Random.value > 0.5f) {
				playerToAttack = 0;
			} else {
				playerToAttack = 1;
			}
		} else {
			if (_.playerDead [0]) {
				playerToAttack = 1;
			} else if (_.playerDead [1]) {
				playerToAttack = 0;
			}
		}

		// Animation: Enemy ATTACK in center
		_.enemyAnimator[_.animNdx].CrossFade("Attack", 0);

		// Animation: Player Damage
		_.playerAnimator[playerToAttack].CrossFade("Damage", 0);

		// Audio: Damage
		int randomInt = Random.Range(2, 4);
		AudioManager.S.PlaySFX(randomInt);

		// Animation: Shake Player 
		Battle.S.battleUIAnim.CrossFade ("BattleUI_Shake", 0); 

		// Calculate Attack Damage
		Battle.S.CalculateAttackDamage(_.enemyStats[_.EnemyNdx()].LVL,
									   _.enemyStats[_.EnemyNdx()].STR, _.enemyStats[_.EnemyNdx()].AGI,
									   Party.stats[playerToAttack].DEF, Party.stats[playerToAttack].AGI,
									   _.enemyStats[_.EnemyNdx()].name, Party.stats[playerToAttack].name,
										Party.stats[playerToAttack].HP);

		// Subtract Player Health
		RPG.S.SubtractPlayerHP (playerToAttack, _.attackDamage);

		// Get and position Explosion game object
		GameObject explosion = ObjectPool.S.GetPooledObject("Explosion");
		switch (playerToAttack) {
		case 0:
			ObjectPool.S.PosAndEnableObj (explosion, _.playerSprite[0]);
			break;
		case 1:
			ObjectPool.S.PosAndEnableObj (explosion, _.playerSprite[1]);
			break;
		}

		// Display Floating Score
		RPG.S.InstantiateFloatingScore(_.playerSprite[playerToAttack], _.attackDamage, Color.red);

		// Player Death or Next Turn
		if (Party.stats[playerToAttack].HP < 1) {
			BattleEnd.S.PlayerDeath (playerToAttack);
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

			//_.NextTurn ();
		}	
	}

	// Index = 1
	// Defend
	public void Defend (){
		_.AddDefender(_.enemyStats [_.EnemyNdx()].name);

		// Activate Enemy Shield
		_.enemyShields[_.EnemyNdx()].SetActive(true);

		BattleDialogue.S.DisplayText (_.enemyStats [_.EnemyNdx()].name + " defends themself until their next turn!");
		_.NextTurn ();
	}

	// Index = 2
	// Run
	public void Run (){
		BattleEnd.S.EnemyRun (_.EnemyNdx());
	}

	// Index = 3
	// Stunned
	public void Stunned (){
		BattleDialogue.S.DisplayText (_.enemyStats [_.EnemyNdx()].name + " is stunned and doesn't move!\nWhat a rube!");
		_.NextTurn ();
	}

	// Index = 4
	// Heal Spell
	public void HealSpell (){
		// Enough MP
		if (_.enemyStats [_.EnemyNdx()].MP >= 3) {
			// Not at Full HP
			if (_.enemyStats [_.EnemyNdx()].HP < _.enemyStats [_.EnemyNdx()].maxHP) {
				// Subtract Spell cost from Enemy's MP
				_.enemyStats [_.EnemyNdx()].MP -= 3;

				// Get amount and max amount to heal
				int amountToHeal = UnityEngine.Random.Range(30, 45);
				int maxAmountToHeal = _.enemyStats[_.EnemyNdx()].maxHP - _.enemyStats[_.EnemyNdx()].HP;
				// Add Enemy's WIS to Heal Amount
				amountToHeal += _.enemyStats[_.EnemyNdx()].WIS;

				// Add 30-45 HP to TARGET Player's HP
				RPG.S.AddEnemyHP (_.EnemyNdx(), amountToHeal);

				// Display Text
				if (amountToHeal >= maxAmountToHeal) {
					BattleDialogue.S.DisplayText (_.enemyStats [_.EnemyNdx()].name + " casts a Heal Spell!\nHealed itself back to Max HP!");

					// Prevents Floating Score being higher than the acutal amount healed
					amountToHeal = maxAmountToHeal;
				} else {
					BattleDialogue.S.DisplayText (_.enemyStats [_.EnemyNdx()].name + " casts a Heal Spell!\nHealed itself for " + amountToHeal + " HP!");
				}

				// Get and position Poof game object
				GameObject poof = ObjectPool.S.GetPooledObject("Poof");
				ObjectPool.S.PosAndEnableObj(poof, _.enemySprite[_.EnemyNdx()].gameObject);

				// Display Floating Score
				RPG.S.InstantiateFloatingScore(_.enemySprite[_.EnemyNdx()].gameObject, amountToHeal, Color.green);

				_.NextTurn ();

				// TBR
				// Animation: Enemy HEAL

			} else {
				// Already at Full HP
				BattleDialogue.S.DisplayText (_.enemyStats [_.EnemyNdx()].name + " thought about casting a Heal Spell...\n...But then remembered they're at full health...\n...and gave up!");
				_.NextTurn (); 
			}
		} else {
			// Not enough MP
			BattleDialogue.S.DisplayText (_.enemyStats [_.EnemyNdx()].name + " attempts to cast a Heal Spell...\n...But doesn't have enough MP to do so!");
			_.NextTurn (); 
		}

		// Otherwise...
		// TBR
		// Animation: Enemy STUNNED
	}

	// Index = 5
	// Attack All
	public void AttackAll (){
		// Enough MP
		if (_.enemyStats [_.EnemyNdx()].MP >= 3) {
			// Subtract Enemy MP
			_.enemyStats [_.EnemyNdx()].MP -= 3;

			// *** TBR: Reference the Defender (still living) with the highest WIS ***
			// 
			// Miss/Dodge
			// 5% chance to Miss/Dodge...
			// ...but 25% chance if Defender WIS is more than Attacker's 
			if (Random.value <= 0.05f || (_.enemyStats[_.EnemyNdx()].WIS > Party.stats[0].WIS && Random.value < 0.25f)) {
				if (Random.value <= 0.5f) {
					BattleDialogue.S.DisplayText (_.enemyStats [_.EnemyNdx()].name + " attempted to cast Crap Blast... but missed the party completely!");
				} else {
					BattleDialogue.S.DisplayText (_.enemyStats [_.EnemyNdx()].name + " cast Crap Blast, but the party deftly dodged out of the way!");
				}
				_.NextTurn ();
			} else {
				// Shake Player Anim
				Battle.S.battleUIAnim.CrossFade ("BattleUI_Shake", 0); 

				int qtyKilled = 0;
				bool[] tDead = new bool[2];

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
					_.attackDamage -= Party.stats[i].DEF;

					// If DEFENDING, cut AttackDamage in HALF
					_.CheckIfDefending(Party.stats[i].name);

					if (_.attackDamage < 0) {
						_.attackDamage = 0;
					} 

					// Subtract Player Health
					RPG.S.SubtractPlayerHP (i, _.attackDamage);

					// Add to to TotalAttackDamage (Used to Calculate AVERAGE Damage)
					totalAttackDamage += _.attackDamage;

					// Shake Enemy 1, 2, & 3's Anim
					if (!_.playerDead [i]) {
						// Get and position Explosion game object
						GameObject explosion = ObjectPool.S.GetPooledObject("Explosion");
						ObjectPool.S.PosAndEnableObj (explosion, _.playerSprite[i]);

						// Display Floating Score
						RPG.S.InstantiateFloatingScore(_.playerSprite[i], _.attackDamage, Color.red);
					}

					// If DEFENDING, Reset AttackDamage for next Enemy
					_.attackDamage = tAttackDamage;

					// If Player HP < 0, DEAD!
					if (Party.stats[i].HP < 1 && !_.playerDead[i]) {
						qtyKilled += 1;
						tDead [i] = true;
					}
				}

				// If no one is killed...
				if (qtyKilled <= 0) {
					BattleDialogue.S.DisplayText ("Used Crap BLAST Spell!\nHit ENTIRE party for an average of " + Utilities.S.CalculateAverage (totalAttackDamage, (_.partyQty + 1)) + " HP!");
					_.NextTurn (); 
				} else {
					PlayersDeath (qtyKilled, totalAttackDamage, tDead [0], tDead [1]);
				}
			}
		} else {
			// Not enough MP
			BattleDialogue.S.DisplayText (_.enemyStats [_.EnemyNdx()].name + " attempts to cast Crap BLAST...\n...But doesn't have enough MP to do so!");
			_.NextTurn (); 
		}
	}
	public void PlayersDeath (int qtyKilled, int totalAttackDamage, bool player1 = false, bool player2 = false) {
		// Subtract from PartyQty 
		_.partyQty -= qtyKilled;

		switch (qtyKilled) {
		case 1: BattleDialogue.S.DisplayText ("Used Crap BLAST Spell!\nHit ENTIRE party for an average of " + Utilities.S.CalculateAverage (totalAttackDamage, (_.partyQty + 2)) + " HP!" + "\nOne party member has been felled!"); break;
		case 2: BattleDialogue.S.DisplayText ("Used Crap BLAST Spell!\nHit ENTIRE party for an average of " + Utilities.S.CalculateAverage (totalAttackDamage, (_.partyQty + 3)) + " HP!" + "\nTwo party members have been felled!"); break;
		}

		if (player1) { PlayersDeathHelper(0, Party.stats[0].name); }
		if (player2) { PlayersDeathHelper(1, Party.stats[1].name); }

		// Add PartyDeath or NextTurn
		if (_.partyQty < 0) { _.battleMode = eBattleMode.partyDeath;} else { _.NextTurn (); }
	}
	public void PlayersDeathHelper(int playerNdx, string playerTurnOrder){
		// Deactivate Player Shield
		_.playerShields[playerNdx].SetActive(false);

		_.playerDead [playerNdx] = true;

		// Animation: Player DEATH
		_.playerAnimator[playerNdx].CrossFade("Death", 0);

		// Remove Player from Turn Order
		_.turnOrder.Remove (playerTurnOrder); 
	}

	// Index = 6
	// Call For Backup
	public void CallForBackup (){

		if (_.enemyStats [0].isDead) {
			CallForBackupHelper(0);
		} else if (_.enemyStats [1].isDead) {
			CallForBackupHelper(1);
		} else if (_.enemyStats [2].isDead) {
			CallForBackupHelper(2);
		} else {
			BattleDialogue.S.DisplayText (_.enemyStats [_.EnemyNdx()].name + " called for backup...\n...but no one came!");
		}

		_.NextTurn ();
	}

	public void CallForBackupHelper(int enemyNdx)
	{
		// Set Selected GameObject (Fight Button)
		_.enemyStats[enemyNdx].isDead = false;

		// Add to Turn Order
		_.turnOrder.Add(_.enemyStats[enemyNdx].name);

		// Reset HP/MP
		_.enemyStats [enemyNdx].HP = _.enemyStats [enemyNdx].maxHP;
		_.enemyStats [enemyNdx].MP = _.enemyStats [enemyNdx].maxMP;

		// Gold/EXP payout
		_.expToAdd += _.enemyStats [enemyNdx].EXP;
		_.goldToAdd += _.enemyStats [enemyNdx].Gold;

		// Activate/Deactivate Enemy Buttons, Stats, Sprites
		BattlePlayerActions.S.EnemyButtonSetActive(enemyNdx, true);
		_.enemySprite [enemyNdx].enabled = true;

		// Enable/Update Health Bars
		ProgressBars.S.enemyHealthBarsCS[enemyNdx].transform.parent.gameObject.SetActive(true);
		ProgressBars.S.enemyHealthBarsCS[enemyNdx].UpdateBar(_.enemyStats[enemyNdx].HP, _.enemyStats[enemyNdx].maxHP);

		// Animation: Enemy ARRIVAL 
		_.enemyAnimator[enemyNdx].CrossFade("Arrival", 0);

		// Add to EnemyAmount 
		_.enemyAmount += 1;
		// Display Text
		BattleDialogue.S.DisplayText (_.enemyStats [_.EnemyNdx()].name + " called for backup...\n...and someone came!");
	}
}
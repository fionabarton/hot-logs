using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleSpells : MonoBehaviour {
	[Header ("Set Dynamically")]
	// Singleton
	private static BattleSpells _S;
	public static BattleSpells S { get { return _S; } set { _S = value; } }

	private Battle _;

	// For Calculating Average Attack Damage
	public int totalAttackDamage = 0;

	void Awake() {
		// Singleton
		S = this;
	}

	void Start () {
		_ = Battle.S;
	}

	public void SpellHelper(){
		BattlePlayerActions.S.ButtonsDisableAll();

		Utilities.S.RemoveListeners(BattlePlayerActions.S.playerButtonCS);
		Utilities.S.RemoveListeners(BattlePlayerActions.S.enemyButtonCS);
	}

	// Heal
	public void ClickedHealSpellButton(){
		// Deactivate PauseMessage
		PauseMessage.S.gameObject.SetActive (false);

		BattlePlayerActions.S.ButtonsInteractable (false, false, false, false, false, false, false, false, true, true);

		// Set a Player Button as Selected GameObject
		BattlePlayerActions.S.SetSelectedPlayerButton ();

		// Add HealSpell Listeners to Player Buttons
		BattlePlayerActions.S.playerButtonCS[0].onClick.AddListener (delegate{HealSpell (0);});
		BattlePlayerActions.S.playerButtonCS[1].onClick.AddListener (delegate{HealSpell (1);});

		// Display Text
		BattleDialogue.S.DisplayText ("Heal which party member?");

		// Switch Mode
		_.battleMode = eBattleMode.canGoBackToFightButton;
	}
	public void HealSpell(int ndx){
        if (_.playerDead[ndx]) {
			// Display Text
			BattleDialogue.S.DisplayText(Stats.S.playerName[ndx] + " is dead...\n...and dead folk can't be healed, dummy!");

			// Switch Mode
			_.battleMode = eBattleMode.playerTurn;

			SpellHelper();

			return;
		}

		if (Stats.S.HP [ndx] < Stats.S.maxHP [ndx]) {
			// Subtract Spell cost from Player's MP
			RPG.S.SubtractPlayerMP (_.PlayerNdx(), 3);

			// Get amount and max amount to heal
			int amountToHeal = UnityEngine.Random.Range(30, 45);
			int maxAmountToHeal = Stats.S.maxHP[ndx] - Stats.S.HP[ndx];
			// Add Player's WIS to Heal Amount
			amountToHeal += Stats.S.WIS [ndx];

			// Add 30-45 MP to TARGET Player's MP
			RPG.S.AddPlayerHP (ndx, amountToHeal);

			// Display Text
			if (amountToHeal >= maxAmountToHeal) {
				BattleDialogue.S.DisplayText ("Used Heal Spell!\nHealed " + Stats.S.playerName [ndx] + " back to Max HP!");

				// Prevents Floating Score being higher than the acutal amount healed
				amountToHeal = maxAmountToHeal;
			} else {
				BattleDialogue.S.DisplayText ("Used Heal Spell!\nHealed " + Stats.S.playerName [ndx] + " for " + amountToHeal + " HP!");
			}

			// Get and position Poof game object
			GameObject poof = ObjectPool.S.GetPooledObject("Poof");
			ObjectPool.S.PosAndEnableObj(poof, _.playerSprite[ndx]);

			// Display Floating Score
			RPG.S.InstantiateFloatingScore(_.playerSprite[ndx], amountToHeal, Color.green);

			_.NextTurn ();
		} else {
			// Display Text
			BattleDialogue.S.DisplayText (Stats.S.playerName [ndx] + " already at full health...\n...no need to cast this spell!");

			// Switch Mode
			_.battleMode = eBattleMode.playerTurn;
		}
		SpellHelper();
	}

	// Fire BALL
	public void ClickedFireSpellButton () {
		// Deactivate PauseMessage
		PauseMessage.S.gameObject.SetActive (false);

		BattlePlayerActions.S.ButtonsInteractable (false, false, false, false, false, true, true, true, false, false);
		BattlePlayerActions.S.SetSelectedEnemyButton ();

		// Calls PlayerAttack() when you click the Enemy1, Enemy2, or Enemy3 Button
		BattlePlayerActions.S.enemyButtonCS[0].onClick.AddListener (delegate{FireballSpell (0);});
		BattlePlayerActions.S.enemyButtonCS[1].onClick.AddListener (delegate{FireballSpell (1);});
		BattlePlayerActions.S.enemyButtonCS[2].onClick.AddListener (delegate{FireballSpell (2);});

		// Display Text
		BattleDialogue.S.DisplayText ("Attack which enemy?");

		// Switch Mode
		_.battleMode = eBattleMode.canGoBackToFightButton;
	}
	public void FireballSpell (int ndx){
		// Subtract Spell cost from Player's MP
		RPG.S.SubtractPlayerMP (_.PlayerNdx(), 2);

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Miss/Dodge
		// 5% chance to Miss/Dodge...
		// ...but 25% chance if Defender WIS is more than Attacker's 
		if (Random.value <= 0.05f || (_.enemyStats [ndx].WIS > Stats.S.WIS [_.PlayerNdx()] && Random.value < 0.25f)) {
			if (Random.value <= 0.5f) {
				BattleDialogue.S.DisplayText (Stats.S.playerName [_.PlayerNdx()] + " attempted the spell... but missed " + _.enemyStats [ndx].name + " completely!");
			} else {
				BattleDialogue.S.DisplayText (Stats.S.playerName [_.PlayerNdx()] + " cast the spell, but " + _.enemyStats [ndx].name + " deftly dodged out of the way!");
			}
			_.NextTurn ();
		} else {
			// Subtract 8-12 HP
			_.attackDamage = Random.Range (8, 12);
			// Add Player's WIS to Damage
			_.attackDamage += Stats.S.WIS [_.PlayerNdx()];
			// Subtract Enemy's DEF from Damage
			_.attackDamage -= _.enemyStats [ndx].DEF;

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

			if (_.enemyStats [ndx].HP < 1) {
				// Deactivate Spells Screen then Enemy Death
				SpellsScreen.S.ScreenOffEnemyDeath (ndx);
			} else {
				// Deactivate Spells Screen then Enemy Turn
				BattleDialogue.S.DisplayText ("Used Fire Spell!\nHit " + _.enemyStats [ndx].name + " for " + _.attackDamage + " HP!");
				_.NextTurn ();

				// Shake Enemy Anim 
				//B.S.enemySpriteAnim [ndx].CrossFade ("EnemySprite_Shake", 0); 
			}
		}
		SpellHelper ();
	}

	// Fire BLAST
	public void FireblastSpell () {
		// Deactivate PauseMessage
		PauseMessage.S.gameObject.SetActive (false);

		// Subtract Spell cost from Player's MP
		RPG.S.SubtractPlayerMP (_.PlayerNdx(), 3);

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// *** TBR: Reference the Defender (still living) with the highest WIS ***
		// 
		// Miss/Dodge
		// 5% chance to Miss/Dodge...
		// ...but 25% chance if Defender WIS is more than Attacker's 
		if (Random.value <= 0.05f || (_.enemyStats [0].WIS > Stats.S.WIS [_.PlayerNdx()] && Random.value < 0.25f)) {
			if (Random.value <= 0.5f) {
				BattleDialogue.S.DisplayText (Stats.S.playerName [_.PlayerNdx()] + " attempted the spell... but missed those goons completely!");
			} else {
				BattleDialogue.S.DisplayText (Stats.S.playerName [_.PlayerNdx()] + " cast the spell, but these dummies you're fighting deftly dodged out of the way!");
			}
			_.NextTurn ();
		} else {
			int qtyKilled = 0;
			bool[] tDead = new bool[3];

			// Subtract 12-20 HP
			_.attackDamage = Random.Range (12, 20);
			// Add Player's WIS to Damage
			_.attackDamage += Stats.S.WIS [_.PlayerNdx()];

			// Cache AttackDamage. When more than one Defender, prevents splitting it in 1/2 more than once.
			int tAttackDamage = _.attackDamage;

			// Used to Calculate AVERAGE Damage
			totalAttackDamage = 0;

			// Loop through enemies
			for (int i = 0; i < _.enemyStats.Count; i++) {

				// Subtract Enemy's DEF from Damage
				_.attackDamage -= Battle.S.enemyStats [i].DEF;

				// If DEFENDING, cut AttackDamage in HALF
				_.CheckIfDefending (Battle.S.enemyStats [i].name);

				if (_.attackDamage < 0) {
					_.attackDamage = 0;
				} 

				// Subtract Enemy Heath
				RPG.S.SubtractEnemyHP (i, _.attackDamage);

				// Add to to TotalAttackDamage (Used to Calculate AVERAGE Damage)
				totalAttackDamage += _.attackDamage;

				// Shake Enemy 1, 2, & 3's Anim
				if (!_.enemyStats [i].isDead) {
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

				// If Enemy HP < 0, DEAD!
				if (_.enemyStats [i].HP < 1 && !_.enemyStats [i].isDead) {
					qtyKilled += 1;
					tDead [i] = true;
				}
			}

			// If no one is killed...
			if (qtyKilled <= 0) {
				BattleDialogue.S.DisplayText ("Used Fire BLAST Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage (totalAttackDamage, _.enemyStats.Count) + " HP!");
				_.NextTurn (); 
			} else {
				EnemiesDeath (qtyKilled, tDead [0], tDead [1], tDead [2]);
			}
		}
		SpellHelper ();
	}

	public void EnemiesDeath (int qtyKilled, bool enemy1 = false, bool enemy2 = false, bool enemy3 = false) {
		// Subtract from EnemyAmount 
		_.enemyAmount -= qtyKilled;

		switch (qtyKilled) {
		case 1: BattleDialogue.S.DisplayText ("Used Fire BLAST Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, _.enemyStats.Count) + " HP!" + "\nOne enemy has been felled!"); break;
		case 2: BattleDialogue.S.DisplayText ("Used Fire BLAST Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, _.enemyStats.Count) + " HP!" + "\nTwo enemies have been felled!"); break;
		case 3: BattleDialogue.S.DisplayText ("Used Fire BLAST Spell!\nHit ALL Enemies for an average of " + Utilities.S.CalculateAverage(totalAttackDamage, _.enemyStats.Count) + " HP!" + "\nThree enemies have been felled!"); break;
		}

		if (enemy1) { EnemiesDeathHelper (0, Battle.S.enemyStats[0].name); }
		if (enemy2) { EnemiesDeathHelper (1, Battle.S.enemyStats[1].name); }
		if (enemy3) { EnemiesDeathHelper (2, Battle.S.enemyStats[2].name); }

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
		} else { _.NextTurn (); }
	}
	public void EnemiesDeathHelper(int enemyNdx, string enemyTurnOrder){
		// Drop Enemy Anim
		_.enemyAnimator[enemyNdx].CrossFade("Death", 0);

		// Deactivate Enemy Shield
		_.enemyShields[enemyNdx].SetActive(false);

		// Deactivate Enemy Button, Stats
		BattlePlayerActions.S.EnemyButtonAndShadowSetActive(enemyNdx, false);
		// Set Selected GameObject (Fight Button)
		_.enemyStats[enemyNdx].isDead = true;
		// Remove Enemy from Turn Order
		_.turnOrder.Remove (enemyTurnOrder);
		// Randomly select DropItem
		BattleEnd.S.AddDroppedItems(enemyNdx);
	}
}
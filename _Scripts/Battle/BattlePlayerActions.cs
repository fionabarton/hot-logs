﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// During battle handles what happens when buttons are pressed
/// (Fight, Spell, Item, Run, Party Members, & Enemies)
/// </summary>
public class BattlePlayerActions : MonoBehaviour
{
	[Header("Set in Inspector")]
	// Action Buttons
	public GameObject		spellGO; 
	public GameObject		fightGO; 
	public GameObject		itemGO; 
	public GameObject		runGO;

	// For each Button, in Inspector, assign BattlePlayerActions._______Button to OnClick
	public Button			spellButton;
	public Button			fightButton; 
	public Button			itemButton; 
	public Button			defendButton; 
	public Button			runButton; 

	// Player Buttons (invisible, used to select party member)
	public List<GameObject> playerButtonGO;
	public List<Button>		playerButtonCS;

	// Enemy Buttons (invisible, used to select enemy)
	public List<GameObject> enemyButtonGO;
	public List<Button>		enemyButtonCS;

	[Header("Set Dynamically")]
	// Singleton
	private static BattlePlayerActions _S;
	public static BattlePlayerActions S { get { return _S; } set { _S = value; } }

	private Battle _;

	void Awake() {
		// Singleton
		S = this;
	}

	void Start() {
		_ = Battle.S;
	}

	// choose an enemy to attack
	public void FightButton() {
		BattleDialogue.S.DisplayText("Attack which enemy?");
		ButtonsInteractable(false, false, false, false, false, true, true, true, false, false);

		SetSelectedEnemyButton();

		// Calls PlayerAttack() when you click the Enemy1 or Enemy2 Button
		enemyButtonCS[0].onClick.AddListener(delegate { ClickedAttackEnemy(0); });
		enemyButtonCS[1].onClick.AddListener(delegate { ClickedAttackEnemy(1); });
		enemyButtonCS[2].onClick.AddListener(delegate { ClickedAttackEnemy(2); });

		// Switch Mode
		_.battleMode = eBattleMode.canGoBackToFightButton;

		// Audio: Confirm
		AudioManager.S.sfxCS[6].Play();
	}

	public void ClickedAttackEnemy(int ndx) {
		_.targetNdx = ndx;
		_.animNdx = _.PlayerNdx();

		// Initialize QTE
		if (_.qteEnabled) {
			// Animation: Player run FIGHT in center
			_.playerAnimator[_.animNdx].CrossFade("RunToCenter", 0);
			BattleUI.S.turnCursor.SetActive(false);

			BattleQTE.S.StartQTE();
		} else {
			// attack enemy
			AttackEnemy(ndx);
		}

		// Otherwise AttackEnemyCo is called once, then twice second turn, thrice third turn, etc.
		Utilities.S.RemoveListeners(enemyButtonCS);
	}

	public void AttackEnemy(int ndx) {
		// Calculate Attack Damage
		_.CalculateAttackDamage(Stats.S.LVL[_.PlayerNdx()], 
								Stats.S.STR[_.PlayerNdx()], Stats.S.AGI[_.PlayerNdx()],
								_.enemyStats[ndx].DEF, _.enemyStats[ndx].AGI,
								Stats.S.playerName[_.PlayerNdx()], _.enemyStats[ndx].name,
								_.enemyStats[ndx].HP);

		// Subtract Enemy Health
		RPG.S.SubtractEnemyHP(ndx, _.attackDamage);

		ButtonsDisableAll();

		// Animation: Player Attack 
		if (!_.qteEnabled) { // otherwise animation is set in Battle.QTE 
			_.playerAnimator[_.animNdx].CrossFade("Attack", 0);
		}

		// Animation: Shake Enemy Anim 
		_.enemyAnimator[ndx].CrossFade("Damage", 0);

		// Audio: Damage
		int randomInt = Random.Range(2, 4);
		AudioManager.S.PlaySFX(randomInt);

		// Get and position Explosion game object
		GameObject explosion = ObjectPool.S.GetPooledObject("Explosion");
		ObjectPool.S.PosAndEnableObj(explosion, _.enemySprite[ndx].gameObject);

		// Display Floating Score
		RPG.S.InstantiateFloatingScore(_.enemySprite[ndx].gameObject, _.attackDamage, Color.red);

		// Enemy Death or Next Turn
		if (_.enemyStats[ndx].HP < 1) {
			BattleEnd.S.EnemyDeath(ndx); 
		} else { 
			_.NextTurn(); 
		}
	}

	// go back to player action buttons (fight, defend, item, run, etc.)
	public void GoBackToFightButton() { // if (Input.GetButtonDown ("Cancel"))
		Utilities.S.RemoveListeners(playerButtonCS);
		Utilities.S.RemoveListeners(enemyButtonCS);

		_.PlayerTurn();

		// Audio: Deny
		AudioManager.S.PlaySFX(7);
	}

	// Run Button
	public void RunButton() {
		ButtonsDisableAll();

		// Not a "boss battle", so the party can attempt to run
		if (_.enemyStats[0].questNdx == 0) {
			if (Random.value < _.chanceToRun) {     // || Stats.S.LVL[0] - enemyStats[0].LVL >= 5
				BattleUI.S.turnCursor.SetActive(false);
				BattleUI.S.targetCursor.SetActive(false);

				// Display Text
				if (_.partyQty >= 1) {
					BattleDialogue.S.DisplayText("The party has fled the battle!");
				} else {
					BattleDialogue.S.DisplayText(Stats.S.playerName[_.PlayerNdx()] + " has fled the battle!");
				}

				// Animation: Party RUN
				for (int i = _.playerDead.Count - 1; i >= 0; i--) {
					if (!_.playerDead[i]) {
						_.playerAnimator[i].CrossFade("Run", 0);
					}
				}

				// Deactivate Player Shields
				for (int i = 0; i < _.playerShields.Count; i++) {
					_.playerShields[i].SetActive(false);
				}

				// You ran away, so the enemy is chasing after you!
				EnemyManager.S.CacheEnemyMovement(eMovement.pursueRun);

				// Return to Overworld
				BattleEnd.S.ReturnToWorldDelay();
			} else {
				BattleDialogue.S.DisplayText(_.enemyStats[0].name + " has blocked the path!");
				_.NextTurn();

				// Increase chance to run
				_.chanceToRun += 0.125f;
			}
		} else { // It's a "boss battle", so the party cannot run
			_.battleMode = eBattleMode.triedToRunFromBoss;

			BattleUI.S.targetCursor.SetActive(false);
			BattleDialogue.S.DisplayText(_.enemyStats[0].name + " is deadly serious...\n...there is ZERO chance of running away!");
		}
		
		// Audio: Confirm
		AudioManager.S.PlaySFX(6);
	}
	public void DefendButton() {
		ButtonsDisableAll();

		// Defend until next turn
		_.AddDefender(Stats.S.playerName[_.PlayerNdx()]);

		// TBR
		// Animation: Player DEFEND
		_.playerShields[_.PlayerNdx()].SetActive(true);

		BattleDialogue.S.DisplayText(Stats.S.playerName[_.PlayerNdx()] + " defends themself until their next turn!");
		_.NextTurn();

		// Audio: Confirm
		AudioManager.S.PlaySFX(6);
	}
	// Spell Button
	public void SpellButton() {
		ButtonsDisableAll();

		if (Stats.S.spellNdx[_.PlayerNdx()] > 0) {

			BattleUI.S.targetCursor.SetActive(false);

			// Switch Mode
			_.battleMode = eBattleMode.itemOrSpellMenu;

			PauseMessage.S.gameObject.SetActive(true);

			// Open Spells Screen
			SpellsScreen.S.LoadSpells(_.PlayerNdx());

			// Update Delgate
			UpdateManager.updateDelegate += SpellsScreen.S.Loop;
		} else {
			// Knows no Spells, go back to Player Turn
			BattleDialogue.S.DisplayText(Stats.S.playerName[_.PlayerNdx()] + " doesn't know any spells!");

			// Switch Mode
			_.battleMode = eBattleMode.playerTurn;
		}

		// Audio: Confirm
		AudioManager.S.PlaySFX(6);
	}
	// Item Button
	public void ItemButton() {
		ButtonsDisableAll();

		// If Player has an Item 
		if (Inventory.S.GetItemList().Count > 0) {

			BattleUI.S.targetCursor.SetActive(false);

			// Switch Mode
			_.battleMode = eBattleMode.itemOrSpellMenu;

			PauseMessage.S.gameObject.SetActive(true);

			// Open Item Screen
			ScreenManager.S.ItemScreenOn();
		} else {
			// Has no Items, go back to Player Turn 
			BattleDialogue.S.DisplayText(Stats.S.playerName[_.PlayerNdx()] + " has no items!");

			// Switch Mode
			_.battleMode = eBattleMode.playerTurn;
		}

		// Audio: Confirm
		AudioManager.S.PlaySFX(6);
	}

	public void ButtonsInteractable(bool fight, bool spell, bool defend, bool run, bool item, bool eButton1, bool eButton2, bool eButton3, bool pButton1, bool pButton2) {
		fightButton.interactable = fight;
		spellButton.interactable = spell;
		defendButton.interactable = defend;
		runButton.interactable = run;
		itemButton.interactable = item;
		enemyButtonCS[0].interactable = eButton1;
		enemyButtonCS[1].interactable = eButton2;
		enemyButtonCS[2].interactable = eButton3;
		playerButtonCS[0].interactable = pButton1;
		playerButtonCS[1].interactable = pButton2;
	}
		
	public void ButtonsInitialInteractable() { ButtonsInteractable(true, true, true, true, true, false, false, false, false, false); }
	
	public void ButtonsDisableAll() { ButtonsInteractable(false, false, false, false, false, false, false, false, false, false); }
	
	public void SetSelectedEnemyButton() {
		for(int i = _.enemyStats.Count - 1; i >= 0; i--) {
			if (!_.enemyStats[i].isDead) {
				Utilities.S.SetSelectedGO(enemyButtonGO[i]);
			}
		}
	}
	
	public void SetSelectedPlayerButton() {
		for (int i = _.playerDead.Count - 1; i >= 0; i--) {
			if (!_.playerDead[i]) {
				Utilities.S.SetSelectedGO(playerButtonGO[i]);
			}
		}
	}

	// Activate/Deactivate Player/Enemy Buttons, Shadows, Progress Bars, Sprites/////////////////////////
	public void EnemyButtonAndShadowSetActive(int enemyNdx, bool setActive) {
		enemyButtonGO[enemyNdx].SetActive(setActive);
		ProgressBars.S.enemyHealthBarsCS[enemyNdx].transform.parent.gameObject.SetActive(setActive);
	}
	public void PlayerButtonAndStatsSetActive(int partyNdx, bool setActive) {
		_.playerSprite[partyNdx].SetActive(setActive);
		playerButtonGO[partyNdx].SetActive(setActive);
	}
}

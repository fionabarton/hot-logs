using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// During battle handles what happens when buttons are pressed
/// (Fight, Spell, Item, Run, Party Members, & Enemies)
/// </summary>
public class BattlePlayerActions : MonoBehaviour {
	[Header("Set in Inspector")]
	// Action Buttons
	public GameObject		spellGO; 
	public GameObject		fightGO; 
	public GameObject		itemGO; 
	public GameObject		runGO;

	// Fight, Spell, Item, Defend, Run
	public List<GameObject> buttonsGO;
	public List<Button>		buttonsCS;

	// Player Buttons (invisible, used to select party member)
	public List<GameObject> playerButtonGO;
	public List<Button>		playerButtonCS;

	// Enemy Buttons (invisible, used to select enemy)
	public List<GameObject> enemyButtonGO;
	public List<Button>		enemyButtonCS;

	[Header("Set Dynamically")]
	private static BattlePlayerActions _S;
	public static BattlePlayerActions S { get { return _S; } set { _S = value; } }

	private Battle _;

	void Awake() {
		S = this;
	}

	void Start() {
		_ = Battle.S;
	}

	// choose an enemy to attack
	public void FightButton(bool playSound = false) {
		// Cache Selected Gameobject (Fight Button) 
		Battle.S.previousSelectedGameObject = buttonsGO[0];

		BattleDialogue.S.DisplayText("Attack which enemy?");
		ButtonsInteractable(false, false, false, false, false, true, true, true, true, true, false, false);

		SetSelectedEnemyButton();

		// Calls PlayerAttack() when you click the Enemy1 or Enemy2 Button
		enemyButtonCS[0].onClick.AddListener(delegate { ClickedAttackEnemy(0); });
		enemyButtonCS[1].onClick.AddListener(delegate { ClickedAttackEnemy(1); });
		enemyButtonCS[2].onClick.AddListener(delegate { ClickedAttackEnemy(2); });
		enemyButtonCS[3].onClick.AddListener(delegate { ClickedAttackEnemy(3); });
		enemyButtonCS[4].onClick.AddListener(delegate { ClickedAttackEnemy(4); });

		// Switch Mode
		_.mode = eBattleMode.canGoBackToFightButton;

        // Audio: Confirm
        if (playSound) {
			AudioManager.S.PlaySFX(eSoundName.confirm);
		}
	}

	public void ClickedAttackEnemy(int ndx) {
		_.targetNdx = ndx;
		_.animNdx = _.PlayerNdx();

		// Initialize QTE
		if (_.qteEnabled) {
			// Audio: Confirm
			AudioManager.S.PlaySFX(eSoundName.confirm);

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
		BattleStats.S.GetAttackDamage(Party.S.stats[_.PlayerNdx()].LVL,
								Party.S.stats[_.PlayerNdx()].STR, Party.S.stats[_.PlayerNdx()].AGI,
								_.enemyStats[ndx].DEF, _.enemyStats[ndx].AGI,
								Party.S.stats[_.PlayerNdx()].name, _.enemyStats[ndx].name,
								_.enemyStats[ndx].HP);

		// Subtract Enemy Health
		RPG.S.SubtractEnemyHP(ndx, _.attackDamage);

		ButtonsDisableAll();

		// Animation: Player Attack 
		if (!_.qteEnabled) { // otherwise animation is set in Battle.QTE 
			_.playerAnimator[_.animNdx].CrossFade("Attack", 0);
		}

		BattleSpells.S.DamageEnemyAnimation(ndx, true, false);

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

		// Deactivate target cursors
		Utilities.S.SetActiveList(BattleUI.S.targetCursors, false);

		_.PlayerTurn(true, false);

		// Audio: Deny
		AudioManager.S.PlaySFX(eSoundName.deny);
	}

	// Run Button
	// 50% chance to run from battle. For each failed attempt, chance increases by 12.5%
	public void RunButton() {
		ButtonsDisableAll();

		// Cache Selected Gameobject (Run Button) 
		Battle.S.previousSelectedGameObject = buttonsGO[4];

		// Not a "boss battle", so the party can attempt to run
		if (_.enemyStats[0].questNdx == 0) {
			if (Random.value < _.chanceToRun) {     // || Stats.S.LVL[0] - enemyStats[0].LVL >= 5
				BattleUI.S.turnCursor.SetActive(false);
				Utilities.S.SetActiveList(BattleUI.S.targetCursors, false);

				// Display Text
				if (_.partyQty >= 1) {
					BattleDialogue.S.DisplayText("The party has fled the battle!");
				} else {
					BattleDialogue.S.DisplayText(Party.S.stats[_.PlayerNdx()].name + " has fled the battle!");
				}

				// Animation: Party RUN
				for (int i = _.playerDead.Count - 1; i >= 0; i--) {
					if (!_.playerDead[i]) {
						_.playerAnimator[i].CrossFade("Run", 0);
					}
				}

				// Deactivate Player Shields
				Utilities.S.SetActiveList(BattleStatusEffects.S.playerShields, false);;

				// You ran away, so the enemy is chasing after you!
				EnemyManager.S.GetEnemyMovement(eMovement.pursueRun);

				// Return to Overworld
				BattleEnd.S.ReturnToWorldDelay();

				// Audio: Run
				AudioManager.S.PlaySFX(eSoundName.run);
			} else {
				BattleDialogue.S.DisplayText(_.enemyStats[0].name + " has blocked the path!");
				_.NextTurn();

				// Increase chance to run
				_.chanceToRun += 0.125f;

				// Audio: Deny
				AudioManager.S.PlaySFX(eSoundName.deny);
			}
		} else { // It's a "boss battle", so the party cannot run
			_.mode = eBattleMode.triedToRunFromBoss;

			Utilities.S.SetActiveList(BattleUI.S.targetCursors, false);
			BattleDialogue.S.DisplayText(_.enemyStats[0].name + " is deadly serious...\n...there is ZERO chance of running away!");

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);
		}
	}
	// Defend Button
	// Reduces attack damage by 50%
	public void DefendButton() {
		ButtonsDisableAll();

		// Defend until next turn
		BattleStatusEffects.S.AddDefender(Party.S.stats[_.PlayerNdx()].name, true);

		BattleDialogue.S.DisplayText(Party.S.stats[_.PlayerNdx()].name + " defends themself until their next turn!");

		_.NextTurn();
	}
	// Spell Button
	public void SpellButton() {
		ButtonsDisableAll();

		// Cache Selected Gameobject (Spell Button) 
		Battle.S.previousSelectedGameObject = buttonsGO[1];

		if (Party.S.stats[_.PlayerNdx()].spellNdx > 0) {
			Utilities.S.SetActiveList(BattleUI.S.targetCursors, false);

			// Switch Mode
			_.mode = eBattleMode.itemOrSpellMenu;

			PauseMessage.S.gameObject.SetActive(true);

			// Activate PlayerButtons
			PlayerButtons.S.gameObject.SetActive(true);
			PlayerButtons.S.rectTrans.anchoredPosition = new Vector2(0, -25);

			// Open Spells Screen
			SpellScreen.S.LoadSpells(_.PlayerNdx());

			// Update Delgate
			UpdateManager.updateDelegate += SpellScreen.S.Loop;
		} else {
			// Knows no Spells, go back to Player Turn
			BattleDialogue.S.DisplayText(Party.S.stats[_.PlayerNdx()].name + " doesn't know any spells!");

			// Switch Mode
			_.mode = eBattleMode.playerTurn;

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);
		}
	}
	// Item Button
	public void ItemButton() {
		ButtonsDisableAll();

		// Cache Selected Gameobject (Item Button) 
		Battle.S.previousSelectedGameObject = buttonsGO[2];

		// If Player has an Item 
		if (Inventory.S.GetItemList().Count > 0) {
			Utilities.S.SetActiveList(BattleUI.S.targetCursors, false);

			// Switch Mode
			_.mode = eBattleMode.itemOrSpellMenu;

			PauseMessage.S.gameObject.SetActive(true);

			// Activate PlayerButtons
			PlayerButtons.S.gameObject.SetActive(true);
			PlayerButtons.S.rectTrans.anchoredPosition = new Vector2(0, -25);

			// Open Item Screen
			ItemScreen.S.Activate();
		} else {
			// Has no Items, go back to Player Turn 
			BattleDialogue.S.DisplayText(Party.S.stats[_.PlayerNdx()].name + " has no items!");

			// Switch Mode
			_.mode = eBattleMode.playerTurn;

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);
		}
	}

	public void ButtonsInteractable(bool fight, bool spell, bool defend, bool run, bool item, bool eButton1, bool eButton2, bool eButton3, bool eButton4, bool eButton5, bool pButton1, bool pButton2) {
		buttonsCS[0].interactable = fight;
		buttonsCS[1].interactable = spell;
		buttonsCS[2].interactable = defend;
		buttonsCS[3].interactable = run;
		buttonsCS[4].interactable = item;

		enemyButtonCS[0].interactable = eButton1;
		enemyButtonCS[1].interactable = eButton2;
		enemyButtonCS[2].interactable = eButton3;
		enemyButtonCS[3].interactable = eButton4;
		enemyButtonCS[4].interactable = eButton5;
		playerButtonCS[0].interactable = pButton1;
		playerButtonCS[1].interactable = pButton2;
	}
		
	public void ButtonsInitialInteractable() { ButtonsInteractable(true, true, true, true, true, false, false, false, false, false, false, false); }
	
	public void ButtonsDisableAll() { ButtonsInteractable(false, false, false, false, false, false, false, false, false, false, false, false); }
	
	public void SetSelectedEnemyButton() {
		for(int i = _.enemyStats.Count - 1; i >= 0; i--) {
			if (!_.enemyStats[i].isDead) {
				Utilities.S.SetSelectedGO(enemyButtonGO[i]);

				// Set previously selected GameObject
				_.previousSelectedForAudio = enemyButtonGO[i];
			}
		}
	}

	// Activate/Deactivate Player/Enemy Buttons, Shadows, Progress Bars, Sprites
	public void EnemyButtonSetActive(int enemyNdx, bool setActive) {
		enemyButtonGO[enemyNdx].SetActive(setActive);
		ProgressBars.S.enemyHealthBarsCS[enemyNdx].transform.parent.gameObject.SetActive(setActive);
	}
	public void PlayerButtonAndStatsSetActive(int partyNdx, bool setActive) {
		_.playerSprite[partyNdx].SetActive(setActive);
		playerButtonGO[partyNdx].SetActive(setActive);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Battle : MonoBehaviour {
	[Header("Set in Inspector")]
	///////////////////////////////// SPRITES /////////////////////////////////
	// Player/Enemy Images
	public List<GameObject>		playerSprite;
	public List<SpriteRenderer> enemySprite;

	// Player/Enemy Defense Shields
	public List<GameObject>	playerShields;
	public List<GameObject>	enemyShields;

	// Enemy "Help!" Word Bubbles
	public List<GameObject> enemyHelpBubbles;

	// '...' Word Bubble
	public GameObject		dotDotDotWordBubble;

	public List<Animator>	playerAnimator;
	public List<Animator>	enemyAnimator;

	///////////////////////////////// ENEMY ANIMATORS /////////////////////////////////
	public List<Animator>	enemyAnims;

	// Shakes the canvas back and forth
	public Animator			battleUIAnim;

	/////////////////////////////////// QTE /////////////////////////////////
	//// QTE enabled
	public bool				qteEnabled = true;

	[Header("Set Dynamically")]
	// Singleton
	private static Battle	_S;
	public static Battle	S { get { return _S; } set { _S = value; } }

	// DontDestroyOnLoad
	private static bool		exists;

	public eBattleMode		battleMode;

	// Attack Damage, Random Factor
	public int				attackDamage, randomFactor, qteBonusDamage;

	public int				enemyAmount, partyQty;

	// The names of currently engaged Party Members & Enemies
	public List<string>		turnOrder;

	// The index of which character's turn it currently is
	public int				turnNdx;

	// Incremented each time all combatants have taken their turn
	public int				roundNdx;

	public List<EnemyStats> enemyStats = new List<EnemyStats>();
	public List<GameObject> enemyGameObjectHolders = new List<GameObject>();

	public List<bool>		playerDead;

	public int				expToAdd, goldToAdd;

	// Dropped Items
	public List<Item>		droppedItems = new List<Item>();

	// cache index of enemy that is currently being targeted!
	public int				targetNdx;

	public int				animNdx;

	public float			chanceToRun = 0.5f;

	// Allows parts of Loop() to be called once rather than repeatedly every frame.
	public bool				canUpdate;

	public GameObject		previousSelectedGameObject;

	// Ensures audio is only played once when button is selected
	public GameObject		previousSelectedForAudio;

	// If an enemy announces its intent to perform a certain move during its next turn,
	// store the move's index
	public List<int>		nextTurnMoveNdx = new List<int>();

	void Awake() {
		S = this;

		// DontDestroyOnLoad
		if (!exists) {
			exists = true;
			DontDestroyOnLoad(transform.gameObject);
		} else {
			Destroy(gameObject);
		}
	}

    #region Update Loop
    public void Loop() {
		if (BattleDialogue.S.dialogueFinished) {
			// Dialogue Loop
			BattleDialogue.S.Loop();

			// Reset canUpdate
			if (Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f) {
				canUpdate = true;
			}

			if (BattleDialogue.S.dialogueNdx <= 0) {
				switch (battleMode) {
					case eBattleMode.actionButtons:
						break;
					case eBattleMode.canGoBackToFightButton:
						if (Input.GetButtonDown("SNES Y Button")) {
							BattlePlayerActions.S.GoBackToFightButton();
						}
						break;
					case eBattleMode.canGoBackToFightButtonMultipleTargets:
						if (Input.GetButtonDown("SNES Y Button")) {
							BattlePlayerActions.S.GoBackToFightButton();
						}
						break;
					case eBattleMode.playerTurn:
						if (Input.GetButtonDown("SNES B Button")) {
							PlayerTurn();
						}
						break;
					case eBattleMode.qteInitialize:
						BattleQTE.S.Loop();

						if (Input.GetButtonDown("SNES Y Button")) {
							playerAnimator[animNdx].CrossFade("Idle", 0);

							// Audio: Deny
							AudioManager.S.PlaySFX(eSoundName.deny);

							BattlePlayerActions.S.FightButton();
						}

						break;
					case eBattleMode.qte:
						BattleQTE.S.Loop();
						break;
					case eBattleMode.triedToRunFromBoss:
						if (Input.GetButtonDown("SNES B Button")) {
							PlayerTurn();
						}
						break;
					case eBattleMode.enemyTurn:
						if (Input.GetButtonDown("SNES B Button")) {
							EnemyTurn();
						}
						break;
					case eBattleMode.enemyAction:
						if (Input.GetButtonDown("SNES B Button")) {
							// If the enemy announced what move it would perform during its previous turn...
							if (nextTurnMoveNdx[EnemyNdx()] != 999) {
								// Cache move index
								int moveNdx = nextTurnMoveNdx[EnemyNdx()];

								// Reset this enemy's nextTurnMoveNdx
								nextTurnMoveNdx[EnemyNdx()] = 999;

								// ...call previously announced move this turn
								BattleEnemyAI.S.CallEnemyMove(moveNdx);
							// If the enemy didn't announce what move it would perform during its previous turn...
							} else {
								// ...let its AI dictate what move to perform
								BattleEnemyAI.S.EnemyAI(enemyStats[EnemyNdx()].id);
							}
						}
						break;
					case eBattleMode.statusAilment:
						if (Input.GetButtonDown("SNES B Button")) {
							// If this turn is a player's turn...
							if (PlayerNdx() != -1) {
								// If paralyzed or sleeping...
								if (BattleStatusEffects.S.CheckIfParalyzed(Party.S.stats[PlayerNdx()].name)||
									BattleStatusEffects.S.CheckIfSleeping(Party.S.stats[PlayerNdx()].name)) {
									// Skip turn
									NextTurn();

									// If next turn is a player's turn...
									if (PlayerNdx() != -1) {
										PlayerTurn();
									} else {
										EnemyTurn();
									}
								} else {
									// Deactivate status ailment icons
									BattleStatusEffects.S.playerParalyzedIcons[PlayerNdx()].SetActive(false);
									BattleStatusEffects.S.playerSleepingIcons[PlayerNdx()].SetActive(false);

									PlayerTurn();
								}
							} else {
								EnemyTurn();
							}
						}
						break;
					case eBattleMode.partyDeath:
						if (Input.GetButtonDown("SNES B Button")) {
							BattleEnd.S.PartyDeath();
						}
						break;
					case eBattleMode.dropItem:
						if (Input.GetButtonDown("SNES B Button")) {
							BattleEnd.S.DropItem();
						}
						break;
					case eBattleMode.addExpAndGold:
						if (Input.GetButtonDown("SNES B Button")) {
							BattleEnd.S.AddExpAndGold(false);
						}
						break;
					case eBattleMode.addExpAndGoldNoDrops:
						if (Input.GetButtonDown("SNES B Button")) {
							BattleEnd.S.AddExpAndGold(true);
						}
						break;
					case eBattleMode.levelUp:
						if (Input.GetButtonDown("SNES B Button")) {
							BattleEnd.S.LevelUp();
						}
						break;
					case eBattleMode.multiLvlUp:
						if (Input.GetButtonDown("SNES B Button")) {
							BattleEnd.S.MultiLvlUp(BattleEnd.S.membersToLevelUp[0]);
						}
						break;
					case eBattleMode.returnToWorld:
						if (Input.GetButtonDown("SNES B Button")) {
							BattleEnd.S.ReturnToWorld();
						}
						break;
				}
			}	
		}

		// Separate loop that ignores if BattleDialogue.S.dialogueFinished
		switch (battleMode) {
			case eBattleMode.actionButtons:
				// Set buttons' text color
				for (int i = 0; i < BattlePlayerActions.S.buttonsCS.Count; i++) {
					if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == BattlePlayerActions.S.buttonsGO[i]) {
						// Set selected button text color	
						BattlePlayerActions.S.buttonsGO[i].GetComponentInChildren<Text>().color = new Color32(205, 208, 0, 255);
					} else {
						// Set non-selected button text color
						BattlePlayerActions.S.buttonsGO[i].GetComponentInChildren<Text>().color = new Color32(39, 201, 255, 255);
					}
				}
			break;
			case eBattleMode.qte:
				BattleQTE.S.BlockLoop();
			break;
		}
    }

	public void FixedLoop() {
        switch (battleMode) {
			case eBattleMode.actionButtons:
                // Set Target Cursor Position: player action buttons (fight, defend, item, run, etc.)
				BattleUI.S.TargetCursorPosition(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject, 0);

				// Activate Text
				BattleDialogue.S.displayMessageTextTop.gameObject.transform.parent.gameObject.SetActive(true);
				// Display Player Stats
				BattleDialogue.S.displayMessageTextTop.text =
					Party.S.stats[0].name + " - HP: " + Party.S.stats[0].HP + "/" + Party.S.stats[0].maxHP +
					" - MP: " + Party.S.stats[0].MP + "/" + Party.S.stats[0].maxMP + "\n" +
					Party.S.stats[1].name + " - HP: " + Party.S.stats[1].HP + "/" + Party.S.stats[1].maxHP +
					" - MP: " + Party.S.stats[1].MP + "/" + Party.S.stats[1].maxMP;

				if (canUpdate) {
					// Audio: Selection (when a new gameObject is selected)
					Utilities.S.PlayButtonSelectedSFX(ref previousSelectedForAudio);

					canUpdate = false;
				}
				break;
			case eBattleMode.canGoBackToFightButton:
                // Set Target Cursor Position: Enemies or Party
                BattleUI.S.TargetCursorPosition(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject, 0);

                // Activate Text
                BattleDialogue.S.displayMessageTextTop.gameObject.transform.parent.gameObject.SetActive(true);
                // Display Party or Enemies names
                switch (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name) {
                    case "Enemy1Button":
                        BattleDialogue.S.displayMessageTextTop.text = enemyStats[0].name;
                        break;
                    case "Enemy2Button":
                        BattleDialogue.S.displayMessageTextTop.text = enemyStats[1].name;
                        break;
                    case "Enemy3Button":
                        BattleDialogue.S.displayMessageTextTop.text = enemyStats[2].name;
                        break;
                    case "Player1Button":
                        BattleDialogue.S.displayMessageTextTop.text = Party.S.stats[0].name;
                        break;
                    case "Player2Button":
                        BattleDialogue.S.displayMessageTextTop.text = Party.S.stats[1].name;
                        break;
                }

				if (canUpdate) {
					// Audio: Selection (when a new gameObject is selected)
					Utilities.S.PlayButtonSelectedSFX(ref previousSelectedForAudio);

					canUpdate = false;
				}
				break;
		}

		switch (battleMode) {
			case eBattleMode.qte:
				BattleQTE.S.FixedLoop();
				break;
		}
	}
	#endregion

	///////////////////////////////// IMPORT STATS /////////////////////////////////
	public void ImportEnemyStats(List<EnemyStats> eStats, int enemyAmount) {
		// Set enemy amount
		this.enemyAmount = enemyAmount;
		
		// Clear/Reset Enemy Stats
		enemyStats.Clear();

		for (int i = 0; i < eStats.Count; i++) {
			// Add Enemy Stats
			enemyStats.Add(eStats[i]);

			// Set Enemy Sprites
			enemySprite[i].sprite = eStats[i].sprite;

			// Set Enemy Animators
			enemyAnimator[i].runtimeAnimatorController = enemyAnims[eStats[i].animNdx].runtimeAnimatorController;
		}
	}

	// Called in RPGLevelManager.LoadSettings when Scene changes to Battle
	public void InitializeBattle() {
		// Ensures Fight button is selected for first player turn
		previousSelectedGameObject = BattlePlayerActions.S.buttonsGO[0];

		BattlePlayerActions.S.ButtonsDisableAll();
		
		// Reset Dialogue
		BattleDialogue.S.dialogueFinished = true;
		BattleDialogue.S.dialogueNdx = 0;
		BattleDialogue.S.message.Clear();

		// Deactivate Battle Text
		BattleDialogue.S.displayMessageTextTop.gameObject.transform.parent.gameObject.SetActive(false);

		// Clear Defend: Players & enemyStats
		BattleStatusEffects.S.defenders.Clear();

		// Clear Dropped Items
		droppedItems.Clear();

		// Reset next turn move indexes
		for(int i = 0; i < nextTurnMoveNdx.Count; i++) {
			nextTurnMoveNdx[i] = 999;
		}

		// Reset BattlePlayerActions.S.buttonsCS text color
		Utilities.S.SetTextColor(BattlePlayerActions.S.buttonsCS, new Color32(39, 201, 255, 255));

		// Set Mode
		battleMode = eBattleMode.actionButtons;

		// Deactivate Cursors
		BattleUI.S.turnCursor.SetActive(false);
		Utilities.S.SetActiveList(BattleUI.S.targetCursors, false);

		// Deactivate Player Shields
		Utilities.S.SetActiveList(playerShields, false);

		// Deactivate Enemy Shields
		Utilities.S.SetActiveList(enemyShields, false);

		// Deactivate Enemy "Help" Word Bubbles
		Utilities.S.SetActiveList(enemyHelpBubbles, false);

		// Deactivate '...' Word Bubble
		dotDotDotWordBubble.SetActive(false);

		// Deactivate Status Ailment Icons (Paralyzed, Poisoned, Sleeping)
		Utilities.S.SetActiveList(BattleStatusEffects.S.playerParalyzedIcons, false);
		Utilities.S.SetActiveList(BattleStatusEffects.S.playerPoisonedIcons, false);
		Utilities.S.SetActiveList(BattleStatusEffects.S.playerSleepingIcons, false);

		// Reset Exp/Gold to add
		expToAdd = 0;
		goldToAdd = 0;

		// Reset Chance to Run
		chanceToRun = 0.5f;

		// Reset roundNdx
		roundNdx = 1;

		BattleInitiative.S.Initiative();

		// Switch mode (playerTurn or enemyTurn) based off of turnNdx
		if (turnNdx == turnOrder.IndexOf(Party.S.stats[0].name) || turnNdx == turnOrder.IndexOf(Party.S.stats[1].name) || turnNdx == turnOrder.IndexOf(Party.S.stats[2].name)) {
			battleMode = eBattleMode.playerTurn;
		} else if (turnNdx == turnOrder.IndexOf(enemyStats[0].name) || turnNdx == turnOrder.IndexOf(enemyStats[1].name) || turnNdx == turnOrder.IndexOf(enemyStats[2].name)) {
			battleMode = eBattleMode.enemyTurn;
		}

		// Display Turn Order
		BattleUI.S.DisplayTurnOrder();
	}

	public void NextTurn() {
		// Change turnNdx
		if (turnNdx <= ((enemyAmount - 1) + (partyQty))) {
			turnNdx += 1; 
		} else { 
			turnNdx = 0;
			roundNdx += 1;
		}

		// Deactivate cursors
		Utilities.S.SetActiveList(BattleUI.S.targetCursors, false);

		// Reset button to be selected (Fight Button)
		previousSelectedGameObject = BattlePlayerActions.S.buttonsGO[0];

		// Switch mode (playerTurn or enemyTurn) based off of turnNdx
		if(PlayerNdx() != -1) {
			battleMode = eBattleMode.playerTurn;
        } else {
			battleMode = eBattleMode.enemyTurn;
		}
	}

	// Return current player turn index, otherwise return -1	
	public int PlayerNdx() {
		for (int i = 0; i < Party.S.stats.Count; i++) {
			if (turnNdx == turnOrder.IndexOf(Party.S.stats[i].name)) {
				return i;
			}
		}
		return -1;
	}

	// Return current enemy turn index, otherwise return -1																																						
	public int EnemyNdx() {
		for (int i = 0; i < enemyStats.Count; i++) {
			if (turnNdx == turnOrder.IndexOf(enemyStats[i].name)) {
				return i;
			}
		}
		return -1;
	}

	public void PlayerTurn(bool setPreviousSelected = true) { // if (Input.GetButtonDown ("Submit"))
		// Reset Animation: Player Idle
		for (int i = 0; i <= Party.S.partyNdx; i++) {
			if (!playerDead[i]) {
				// If doesn't have a status ailment...
                if (!BattleStatusEffects.S.HasStatusAilment(Party.S.stats[i].name)) {
					playerAnimator[i].CrossFade("Idle", 0);
				}
			} else {
				playerAnimator[i].CrossFade("Death", 0);
			}
		}
		// Reset Animation: Enemy Idle
		for (int i = 0; i < enemyAnimator.Count; i++) {
			if (!enemyStats[i].isDead) {
				enemyAnimator[i].CrossFade("Idle", 0);
			}
		}

		BattleUI.S.DisplayTurnOrder();

		// Deactivate '...' Word Bubble
		dotDotDotWordBubble.SetActive(false);

		// If Defended previous turn, remove from Defenders list
		BattleStatusEffects.S.RemoveDefender(Party.S.stats[PlayerNdx()].name);
		playerShields[PlayerNdx()].SetActive(false);

		// If paralyzed...
		if (BattleStatusEffects.S.CheckIfParalyzed(Party.S.stats[PlayerNdx()].name)) {
			BattleStatusEffects.S.Paralyzed(Party.S.stats[PlayerNdx()].name);
			return;
		}

		// If sleeping...
		if (BattleStatusEffects.S.CheckIfSleeping(Party.S.stats[PlayerNdx()].name)) {
			BattleStatusEffects.S.Sleeping(Party.S.stats[PlayerNdx()].name);
			return;
		}

		// If poisoned...


		BattlePlayerActions.S.ButtonsInitialInteractable();

		// Set Selected Button 
		Utilities.S.SetSelectedGO(previousSelectedGameObject);

        // Set previously selected GameObject
        if (setPreviousSelected) {
			previousSelectedForAudio = previousSelectedGameObject;
		}

		BattleDialogue.S.DisplayText("What will you do, " + Party.S.stats[PlayerNdx()].name + "?");

		// Switch Mode
		battleMode = eBattleMode.actionButtons;

		// Set Turn Cursor Position
		BattleUI.S.TurnCursorPosition(playerSprite[PlayerNdx()].gameObject);
	}

    // Enemy is about to act!
    public void EnemyTurn () { // if (Input.GetButtonDown ("Submit"))
		// Reset Animation: Player Idle
		for (int i = 0; i <= Party.S.partyNdx; i++) {
			if (!playerDead[i]) {
				// If doesn't have a status ailment...
				if (!BattleStatusEffects.S.HasStatusAilment(Party.S.stats[i].name)) {
					playerAnimator[i].CrossFade("Idle", 0);
				}
			} else {
				playerAnimator[i].CrossFade("Death", 0);
			}
		}
		// Reset Animation: Enemy Idle
		for (int i = 0; i < enemyAnimator.Count; i++) {
			if (!enemyStats[i].isDead) {
				enemyAnimator[i].CrossFade("Idle", 0);
			}
		}

		// Reset BattlePlayerActions.S.buttonsCS text color
		Utilities.S.SetTextColor(BattlePlayerActions.S.buttonsCS, new Color32(39, 201, 255, 255));

		// Deactivate Text, was just displaying whether QTE Attack was successful or failed
		BattleDialogue.S.displayMessageTextTop.gameObject.transform.parent.gameObject.SetActive(false);

		BattleUI.S.TurnCursorPosition (enemySprite[EnemyNdx()].gameObject);

		BattleUI.S.DisplayTurnOrder();

		// If Defended previous turn, remove from Defenders list
		BattleStatusEffects.S.RemoveDefender(enemyStats[EnemyNdx()].name);
		enemyShields[EnemyNdx()].SetActive(false);

		// Deactivate '...' Word Bubble
		dotDotDotWordBubble.SetActive(false);

		//DisplayText: THE ENEMY IS ABOUT TO ACT!
		BattleDialogue.S.DisplayText (enemyStats[EnemyNdx()].name + " is about to act!");

		// Switch Mode
		battleMode = eBattleMode.enemyAction;

		// Animation: Enemy RUN to center
		animNdx = EnemyNdx();
		enemyAnimator[animNdx].CrossFade("RunToCenter", 0);
		BattleUI.S.turnCursor.SetActive(false);
	}
}
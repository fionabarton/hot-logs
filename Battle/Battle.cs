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
	public eBattleMode		mode;

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

	// Stores index of enemy that's currently being targeted
	public int				targetNdx;

	// Stores index of player or enemy that's taking their turn
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

	public BattleInitiative		initiative;
	public BattleDialogue		dialogue;
	public BattleEnd			end;
	public BattleEnemyActions	enemyActions;
	public BattleEnemyAI		enemyAI;
	public BattlePlayerActions	playerActions;
	public BattleUI				UI;
	public BattleQTE			qte;
	public BattleStats			stats;

	private static Battle _S;
	public static Battle S { get { return _S; } set { _S = value; } }

	// DontDestroyOnLoad
	private static bool exists;

	void Awake() {
		S = this;

		// Get components
		initiative = GetComponent<BattleInitiative>();
		dialogue = GetComponent<BattleDialogue>();
		end = GetComponent<BattleEnd>();
		enemyActions = GetComponent<BattleEnemyActions>();
		enemyAI = GetComponent<BattleEnemyAI>();
		playerActions = GetComponent<BattlePlayerActions>();
		UI = GetComponent<BattleUI>();
		qte = GetComponent<BattleQTE>();
		stats = GetComponent<BattleStats>();

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
		if (dialogue.dialogueFinished) {
			// Dialogue Loop
			dialogue.Loop();

			// Reset canUpdate
			if (Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f) {
				canUpdate = true;
			}

			if (dialogue.dialogueNdx <= 0) {
				switch (mode) {
					case eBattleMode.actionButtons:
						break;
					case eBattleMode.canGoBackToFightButton:
						if (Input.GetButtonDown("SNES Y Button")) {
							playerActions.GoBackToFightButton();
						}
						break;
					case eBattleMode.canGoBackToFightButtonMultipleTargets:
						if (Input.GetButtonDown("SNES Y Button")) {
							playerActions.GoBackToFightButton();
						}
						break;
					case eBattleMode.playerTurn:
						if (Input.GetButtonDown("SNES B Button")) {
							PlayerTurn();
						}
						break;
					case eBattleMode.qteInitialize:
						qte.Loop();

						if (Input.GetButtonDown("SNES Y Button")) {
							playerAnimator[animNdx].CrossFade("Idle", 0);

							// Audio: Deny
							AudioManager.S.PlaySFX(eSoundName.deny);

							playerActions.FightButton();
						}
						break;
					case eBattleMode.qte:
						qte.Loop();
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
								enemyAI.CallEnemyMove(moveNdx);
							// If the enemy didn't announce what move it would perform during its previous turn...
							} else {
								// ...let its AI dictate what move to perform
								enemyAI.EnemyAI(enemyStats[EnemyNdx()].id);
							}
						}
						break;
					case eBattleMode.statusAilment:
						if (Input.GetButtonDown("SNES B Button")) {
							// If this turn is a player's turn...
							if (PlayerNdx() != -1) {
								// If paralyzed or sleeping...
								if (StatusEffects.S.CheckIfParalyzed(true, PlayerNdx()) ||
									StatusEffects.S.CheckIfSleeping(true, PlayerNdx())) {
									// Skip turn
									NextTurn();

									// If next turn is a player's turn...
									if (PlayerNdx() != -1) {
										PlayerTurn();
									} else {
										EnemyTurn();
									}
								} else {
									PlayerTurn(true, false);
								}
							} else {
								// If paralyzed or sleeping...
								if (StatusEffects.S.CheckIfParalyzed(false, EnemyNdx()) ||
									StatusEffects.S.CheckIfSleeping(false, EnemyNdx())) {
									// Skip turn
									NextTurn();

									// If next turn is a player's turn...
									if (PlayerNdx() != -1) {
										PlayerTurn();
									} else {
										EnemyTurn();
									}
								} else {
									EnemyTurn(false);
								}
							}
						}
						break;
					case eBattleMode.partyDeath:
						if (Input.GetButtonDown("SNES B Button")) {
							end.PartyDeath();
						}
						break;
					case eBattleMode.dropItem:
						if (Input.GetButtonDown("SNES B Button")) {
							end.DropItem(droppedItems[0]);
						}
						break;
					case eBattleMode.addExpAndGold:
						if (Input.GetButtonDown("SNES B Button")) {
							end.AddExpAndGold(false);
						}
						break;
					case eBattleMode.addExpAndGoldNoDrops:
						if (Input.GetButtonDown("SNES B Button")) {
							end.AddExpAndGold(true);
						}
						break;
					case eBattleMode.levelUp:
						if (Input.GetButtonDown("SNES B Button")) {
							end.LevelUp(end.membersToLevelUp[0]);
						}
						break;
					case eBattleMode.returnToWorld:
						if (Input.GetButtonDown("SNES B Button")) {
							end.ReturnToWorld();
						}
						break;
				}
			}	
		}

		// Separate loop that ignores if dialogue.dialogueFinished
		switch (mode) {
			case eBattleMode.actionButtons:
				// Set buttons' text color
				for (int i = 0; i < playerActions.buttonsCS.Count; i++) {
					if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == playerActions.buttonsGO[i]) {
						// Set selected button text color	
						playerActions.buttonsGO[i].GetComponentInChildren<Text>().color = new Color32(205, 208, 0, 255);
					} else {
						// Set non-selected button text color
						playerActions.buttonsGO[i].GetComponentInChildren<Text>().color = new Color32(39, 201, 255, 255);
					}
				}
			break;
			case eBattleMode.qte:
				qte.BlockLoop();
			break;
		}
    }

	public void FixedLoop() {
        switch (mode) {
			case eBattleMode.actionButtons:
                // Set Target Cursor Position: player action buttons (fight, defend, item, run, etc.)
				UI.TargetCursorPosition(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject, 0);

				// Activate Text
				dialogue.displayMessageTextTop.gameObject.transform.parent.gameObject.SetActive(true);
				// Display Player Stats
				dialogue.displayMessageTextTop.text =
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
                UI.TargetCursorPosition(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject, 0);

                // Activate Text
                dialogue.displayMessageTextTop.gameObject.transform.parent.gameObject.SetActive(true);
                // Display Party or Enemies names
                switch (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name) {
                    case "Enemy1Button":
                        dialogue.displayMessageTextTop.text = enemyStats[0].name;
                        break;
                    case "Enemy2Button":
                        dialogue.displayMessageTextTop.text = enemyStats[1].name;
                        break;
                    case "Enemy3Button":
                        dialogue.displayMessageTextTop.text = enemyStats[2].name;
                        break;
					case "Enemy4Button":
						dialogue.displayMessageTextTop.text = enemyStats[3].name;
						break;
					case "Enemy5Button":
						dialogue.displayMessageTextTop.text = enemyStats[4].name;
						break;
					case "Player1Button":
                        dialogue.displayMessageTextTop.text = Party.S.stats[0].name;
                        break;
                    case "Player2Button":
                        dialogue.displayMessageTextTop.text = Party.S.stats[1].name;
                        break;
                }

				if (canUpdate) {
					// Audio: Selection (when a new gameObject is selected)
					Utilities.S.PlayButtonSelectedSFX(ref previousSelectedForAudio);

					canUpdate = false;
				}
				break;
		}

		switch (mode) {
			case eBattleMode.qte:
				qte.FixedLoop();
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
		previousSelectedGameObject = playerActions.buttonsGO[0];

		playerActions.ButtonsDisableAll();
		
		dialogue.Initialize();
		StatusEffects.S.Initialize();

		// Clear Dropped Items
		droppedItems.Clear();

		// Reset next turn move indexes & deactivate help bubbles
		for(int i = 0; i < nextTurnMoveNdx.Count; i++) {
			StopCallingForHelp(i);
		}

		// Reset playerActions.buttonsCS text color
		Utilities.S.SetTextColor(playerActions.buttonsCS, new Color32(39, 201, 255, 255));

		// Set Mode
		mode = eBattleMode.actionButtons;

		// Deactivate Cursors
		UI.turnCursor.SetActive(false);
		Utilities.S.SetActiveList(UI.targetCursors, false);

		// Deactivate Enemy "Help" Word Bubbles
		Utilities.S.SetActiveList(enemyHelpBubbles, false);

		// Deactivate '...' Word Bubble
		dotDotDotWordBubble.SetActive(false);

		// Reset Exp/Gold to add
		expToAdd = 0;
		goldToAdd = 0;

		// Reset Chance to Run
		chanceToRun = 0.5f;

		// Reset roundNdx
		roundNdx = 1;

		initiative.SetInitiative();

		// Switch mode (playerTurn or enemyTurn) based off of turnNdx
		if (turnNdx == turnOrder.IndexOf(Party.S.stats[0].name) || turnNdx == turnOrder.IndexOf(Party.S.stats[1].name) || turnNdx == turnOrder.IndexOf(Party.S.stats[2].name)) {
			mode = eBattleMode.playerTurn;
		} else if (turnNdx == turnOrder.IndexOf(enemyStats[0].name) || turnNdx == turnOrder.IndexOf(enemyStats[1].name) || turnNdx == turnOrder.IndexOf(enemyStats[2].name) || turnNdx == turnOrder.IndexOf(enemyStats[3].name) || turnNdx == turnOrder.IndexOf(enemyStats[4].name)) {
			mode = eBattleMode.enemyTurn;
		}

		// Display Turn Order
		UI.DisplayTurnOrder();

		SetAllCombatantAnimations();
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
		Utilities.S.SetActiveList(UI.targetCursors, false);

		// Reset button to be selected (Fight Button)
		previousSelectedGameObject = playerActions.buttonsGO[0];

		// Switch mode (playerTurn or enemyTurn) based off of turnNdx
		if(PlayerNdx() != -1) {
			mode = eBattleMode.playerTurn;
        } else {
			mode = eBattleMode.enemyTurn;
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

	public void PlayerTurn(bool setPreviousSelected = true, bool checkForAilment = true) { // if (Input.GetButtonDown ("Submit"))
		TurnHelper(Party.S.stats[PlayerNdx()].name, PlayerNdx(), playerSprite[PlayerNdx()].gameObject, true);

        // If player has status ailment...
        if (checkForAilment) {
			if (HasAilment(Party.S.stats[PlayerNdx()].name, true, PlayerNdx())) {
				return;
			}
		}

		playerActions.ButtonsInitialInteractable();

		// Set Selected Button 
		Utilities.S.SetSelectedGO(previousSelectedGameObject);

        // Set previously selected GameObject
        if (setPreviousSelected) {
			previousSelectedForAudio = previousSelectedGameObject;
		}

		dialogue.DisplayText("What will you do, " + Party.S.stats[PlayerNdx()].name + "?");

		// Switch Mode
		mode = eBattleMode.actionButtons;
	}

    // Enemy is about to act!
    public void EnemyTurn (bool checkForAilment = true) { // if (Input.GetButtonDown ("Submit"))
		TurnHelper(enemyStats[EnemyNdx()].name, EnemyNdx(), enemySprite[EnemyNdx()].gameObject, false);

		// If enemy has status ailment...
		if (checkForAilment) {
			if (HasAilment(enemyStats[EnemyNdx()].name, false, EnemyNdx())) {
				return;
			}
		}

		//DisplayText: THE ENEMY IS ABOUT TO ACT!
		dialogue.DisplayText (enemyStats[EnemyNdx()].name + " is about to act!");

		// Animation: Enemy RUN to center
		animNdx = EnemyNdx();
		enemyAnimator[animNdx].CrossFade("RunToCenter", 0);
		UI.turnCursor.SetActive(false);

		// Switch Mode
		mode = eBattleMode.enemyAction;
	}

	public void TurnHelper(string name, int ndx, GameObject gameObject, bool isPlayerTurn) {
		SetAllCombatantAnimations();

		UI.DisplayTurnOrder();

		// Deactivate '...' Word Bubble
		dotDotDotWordBubble.SetActive(false);

		// If Defended previous turn, remove from Defenders list
		StatusEffects.S.RemoveDefender(isPlayerTurn, ndx);

		UI.TurnCursorPosition(gameObject);

		// Reset playerActions.buttonsCS text color
		Utilities.S.SetTextColor(playerActions.buttonsCS, new Color32(39, 201, 255, 255));

		// Deactivate Text, was just displaying whether QTE Attack was successful or failed
		dialogue.displayMessageTextTop.gameObject.transform.parent.gameObject.SetActive(false);
	}

	public void SetAllCombatantAnimations() {
		// Set Player Animations
		for (int i = 0; i <= Party.S.partyNdx; i++) {
			if (!playerDead[i]) {
				// If doesn't have a status ailment...
				if (!StatusEffects.S.HasStatusAilment(true, i)) {
					playerAnimator[i].CrossFade("Idle", 0);
				} else if (StatusEffects.S.CheckIfPoisoned(true, i)) {
					playerAnimator[i].CrossFade("Poisoned", 0);
				}
			} else {
				playerAnimator[i].CrossFade("Death", 0);
			}
		}
		// Set Enemy Animations
		for (int i = 0; i < enemyAnimator.Count; i++) {
			if (!enemyStats[i].isDead) {
				enemyAnimator[i].CrossFade("Idle", 0);
			}
		}
	}

	bool HasAilment(string name, bool isPlayer, int ndx) {
		// If poisoned...
		if (StatusEffects.S.CheckIfPoisoned(isPlayer, ndx)) {
			StatusEffects.S.Poisoned(name, isPlayer, ndx);
			return true;
		}

		// If paralyzed...
		if (StatusEffects.S.CheckIfParalyzed(isPlayer, ndx)) {
			StatusEffects.S.Paralyzed(name, isPlayer, ndx);
			return true;
		}

		// If sleeping...
		if (StatusEffects.S.CheckIfSleeping(isPlayer, ndx)) {
			StatusEffects.S.Sleeping(name, isPlayer, ndx);
			return true;
		}
		return false;
	}

	// Reset next turn move index & deactivate help bubble
	public void StopCallingForHelp(int ndx) {
		// Reset next turn move index
		nextTurnMoveNdx[ndx] = 999;

		// Deactivate Enemy "Help" Word Bubble
		enemyHelpBubbles[ndx].SetActive(false);
	}
}
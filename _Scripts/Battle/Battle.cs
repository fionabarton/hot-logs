using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public enum eBattleMode {
actionButtons, playerTurn, canGoBackToFightButton, qteInitialize, qte, itemOrSpellMenu, triedToRunFromBoss, 
enemyTurn, enemyAction, 
dropItem, addExpAndGold, partyDeath, levelUp, multiLvlUp, returnToWorld };

public class Battle : MonoBehaviour {
	[Header("Set in Inspector")]
	///////////////////////////////// SPRITES /////////////////////////////////
	// Player/Enemy Images
	public List<GameObject>		playerSprite;
	public List<SpriteRenderer> enemySprite;

	// Player/Enemy Defense Shields
	public List<GameObject>	playerShields;
	public List<GameObject>	enemyShields;

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
	public int				attackDamage, randomFactor, bonusDamage;

	public int				enemyAmount, partyQty;

	// The names of currently engaged Party Members & Enemies
	public List<string>		turnOrder;

	// The index of which character's turn it currently is
	public int				turnNdx;

	public List<EnemyStats> enemyStats = new List<EnemyStats>();
	public List<GameObject> enemyGameObjectHolders = new List<GameObject>();

	public List<bool>		playerDead;

	public int				expToAdd, goldToAdd;

	// Dropped Items
	public List<Item>		droppedItems = new List<Item>();

	// Defend: Players & Enemies
	private List<string>	defenders = new List<string>();

	// cache index of enemy that is currently being targeted!
	public int				targetNdx;

	public int				animNdx;

	public float			chanceToRun = 0.5f;

	void Awake() {
		// Singleton
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

			if (BattleDialogue.S.dialogueNdx <= 0) {
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
					case eBattleMode.canGoBackToFightButton:
						if (Input.GetButtonDown("SNES B Button")) {
							BattlePlayerActions.S.GoBackToFightButton();
						}
						break;
					case eBattleMode.playerTurn:
						if (Input.GetButtonDown("SNES A Button")) {
							// Reset Animation: Player Idle
							for (int i = 0; i < playerAnimator.Count; i++) {
								if (!playerDead[i]) {
									playerAnimator[i].CrossFade("Idle", 0);
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

							PlayerTurn();
						}
						break;
					case eBattleMode.qteInitialize:
						BattleQTE.S.Loop();

						if (Input.GetButtonDown("SNES B Button")) {
							playerAnimator[animNdx].CrossFade("Idle", 0);
							BattlePlayerActions.S.FightButton();
						}

						break;
					case eBattleMode.qte:
						BattleQTE.S.Loop();
						break;
					case eBattleMode.triedToRunFromBoss:
						if (Input.GetButtonDown("SNES A Button")) {
							PlayerTurn();
						}
						break;
					case eBattleMode.enemyTurn:
						if (Input.GetButtonDown("SNES A Button")) {
							// Reset Animation: Player Idle
							for (int i = 0; i < playerAnimator.Count; i++) {
								if (!playerDead[i]) {
									playerAnimator[i].CrossFade("Idle", 0);
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
							EnemyTurn();
						}
						break;
					case eBattleMode.enemyAction:
						if (Input.GetButtonDown("SNES A Button")) {
							BattleEnemyAI.S.EnemyAI(enemyStats[EnemyNdx()].AI);
						}
						break;
					case eBattleMode.partyDeath:
						if (Input.GetButtonDown("SNES A Button")) {
							BattleEnd.S.PartyDeath();
						}
						break;
					case eBattleMode.dropItem:
						if (Input.GetButtonDown("SNES A Button")) {
							BattleEnd.S.DropItem();
						}
						break;
					case eBattleMode.addExpAndGold:
						if (Input.GetButtonDown("SNES A Button")) {
							BattleEnd.S.AddExpAndGold();
						}
						break;
					case eBattleMode.levelUp:
						if (Input.GetButtonDown("SNES A Button")) {
							BattleEnd.S.LevelUp();
						}
						break;
					case eBattleMode.multiLvlUp:
						if (Input.GetButtonDown("SNES A Button")) {
							BattleEnd.S.MultiLvlUp();
						}
						break;
					case eBattleMode.returnToWorld:
						if (Input.GetButtonDown("SNES A Button")) {
							BattleEnd.S.ReturnToWorld();
						}
						break;
				}
			}	
		}

		// Separate loop for blocking: Accept input to block regardless of BattleDialogue.S.dialogueFinished
		if (battleMode == eBattleMode.qte) {
            BattleQTE.S.BlockLoop();
        }
    }

	public void FixedLoop() {
        if (BattleDialogue.S.dialogueFinished) {
            switch (battleMode) {
				case eBattleMode.actionButtons:
                    // Set Target Cursor Position: player action buttons (fight, defend, item, run, etc.)
					BattleUI.S.TargetCursorPosition(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject, 0);

					// Activate Text
					BattleDialogue.S.displayMessageTextTop.gameObject.transform.parent.gameObject.SetActive(true);
					// Display Player Stats
					BattleDialogue.S.displayMessageTextTop.text =
						PartyStats.S.playerName[0] + " HP: " + PartyStats.S.HP[0] + "/" + PartyStats.S.maxHP[0] +
						" MP: " + PartyStats.S.MP[0] + "/" + PartyStats.S.maxMP[0] + "\n" +
						PartyStats.S.playerName[1] + " HP: " + PartyStats.S.HP[1] + "/" + PartyStats.S.maxHP[1] +
						" MP: " + PartyStats.S.MP[1] + "/" + PartyStats.S.maxMP[1];
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
							BattleDialogue.S.displayMessageTextTop.text = PartyStats.S.playerName[0];
							break;
						case "Player2Button":
							BattleDialogue.S.displayMessageTextTop.text = PartyStats.S.playerName[1];
							break;
					}
					break;
			}
		}

		switch (battleMode) {
			case eBattleMode.qte:
				BattleQTE.S.FixedLoop();
				break;
		}
	}
	#endregion

	///////////////////////////////// IMPORT STATS /////////////////////////////////
	public void ImportEnemyStats(List<EnemyStats> eStats) {
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
		RPG.S.previousSelectedGameObject = BattlePlayerActions.S.buttonsGO[0];

		BattlePlayerActions.S.ButtonsDisableAll();
		
		// Reset Dialogue
		BattleDialogue.S.dialogueSentences = null;
		BattleDialogue.S.dialogueFinished = true;
		BattleDialogue.S.dialogueNdx = 0;
		BattleDialogue.S.message.Clear();

		// Deactivate Battle Text
		BattleDialogue.S.displayMessageTextTop.gameObject.transform.parent.gameObject.SetActive(false);

		// Clear Defend: Players & enemyStats
		defenders.Clear();

		// Clear Dropped Items
		droppedItems.Clear();

		// Reset BattlePlayerActions.S.buttonsCS text color
		Utilities.S.SetTextColor(BattlePlayerActions.S.buttonsCS, new Color32(39, 201, 255, 255));

		// Switch Mode
		battleMode = eBattleMode.actionButtons;

		// Deactivate Cursors
		BattleUI.S.turnCursor.SetActive(false);
		BattleUI.S.targetCursor.SetActive(false);

		// Deactivate Player Shields
		for (int i = 0; i < playerShields.Count; i++) {
			playerShields[i].SetActive(false);
		}

		// Deactivate Enemy Shields
		for (int i = 0; i < enemyShields.Count; i++) {
			enemyShields[i].SetActive(false);
		}

		// Reset Exp/Gold to add
		expToAdd = 0;
		goldToAdd = 0;

		// Reset Chance to Run
		chanceToRun = 0.5f;

		BattleInitiative.S.Initiative();

		// Switch mode (playerTurn or enemyTurn) based off of turnNdx
		if (turnNdx == turnOrder.IndexOf(PartyStats.S.playerName[0]) || turnNdx == turnOrder.IndexOf(PartyStats.S.playerName[1])) {
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
			turnNdx += 1; } else { turnNdx = 0; }

		BattleUI.S.targetCursor.SetActive(false);

		// Reset button to be selected (Fight Button)
		RPG.S.previousSelectedGameObject = BattlePlayerActions.S.buttonsGO[0];

		// Switch mode (playerTurn or enemyTurn) based off of turnNdx
		if (turnNdx == turnOrder.IndexOf(PartyStats.S.playerName[0]) || turnNdx == turnOrder.IndexOf(PartyStats.S.playerName[1])) {
			battleMode = eBattleMode.playerTurn;
		} else if (turnNdx == turnOrder.IndexOf(enemyStats[0].name) || turnNdx == turnOrder.IndexOf(enemyStats[1].name) || turnNdx == turnOrder.IndexOf(enemyStats[2].name)) {
			battleMode = eBattleMode.enemyTurn;
		}
	}

	// return current player turn index
	public int PlayerNdx() {
		if (turnNdx == turnOrder.IndexOf(PartyStats.S.playerName[0])) { return 0; } else if (turnNdx == turnOrder.IndexOf(PartyStats.S.playerName[1])) { return 1; } else { return 0; } }

	// return current enemy turn index																																						
	public int EnemyNdx() {
		if (turnNdx == turnOrder.IndexOf(enemyStats[0].name)) { return 0; } else if (turnNdx == turnOrder.IndexOf(enemyStats[1].name)) { return 1; } else { return 2; }
	}

	public void PlayerTurn() { // if (Input.GetButtonDown ("Submit"))
		BattleUI.S.DisplayTurnOrder();

		BattlePlayerActions.S.ButtonsInitialInteractable();

		// Set Selected Button 
		Utilities.S.SetSelectedGO(RPG.S.previousSelectedGameObject);

		// If Defended previous turn, remove from Defenders list
		RemoveDefender(PartyStats.S.playerName[PlayerNdx()]);
		playerShields[PlayerNdx()].SetActive(false);

		BattleDialogue.S.DisplayText("What will you do, " + PartyStats.S.playerName[PlayerNdx()] + "?");

		// Switch Mode
		battleMode = eBattleMode.actionButtons;

		// Set Turn Cursor Position
		BattleUI.S.TurnCursorPosition(playerSprite[PlayerNdx()].gameObject);
	}

    // Enemy is about to act!
    public void EnemyTurn () { // if (Input.GetButtonDown ("Submit"))
		// Reset BattlePlayerActions.S.buttonsCS text color
		Utilities.S.SetTextColor(BattlePlayerActions.S.buttonsCS, new Color32(39, 201, 255, 255));

		// Deactivate Text, was just displaying whether QTE Attack was successful or failed
		BattleDialogue.S.displayMessageTextTop.gameObject.transform.parent.gameObject.SetActive(false);

		BattleUI.S.TurnCursorPosition (enemySprite[EnemyNdx()].gameObject);

		BattleUI.S.DisplayTurnOrder();

		// If Defended previous turn, remove from Defenders list
		RemoveDefender(enemyStats[EnemyNdx()].name);
		enemyShields[EnemyNdx()].SetActive(false);

		//DisplayText: THE ENEMY IS ABOUT TO ACT!
		BattleDialogue.S.DisplayText (enemyStats[EnemyNdx()].name + " is about to act!");

		// Switch Mode
		battleMode = eBattleMode.enemyAction;

		// Animation: Enemy RUN to center
		animNdx = EnemyNdx();
		enemyAnimator[animNdx].CrossFade("RunToCenter", 0);
		BattleUI.S.turnCursor.SetActive(false);
	}

	///////////////////////////////// ATTACK /////////////////////////////////
	public void CalculateAttackDamage(
		int attackerLVL,
		int attackerSTR, int attackerAGI,
		int defenderDEF, int defenderAGI,
		string attackerName, string defenderName,
		int defenderHP) {

		// Get Level
		int tLevel = attackerLVL;

		// Reset Attack Damage
		attackDamage = 0;

		// 5% chance to Miss/Dodge...
		// ...AND 25% chance to Miss/Dodge if Defender AGI is more than Attacker's 
		if (Random.value <= 0.05f || (defenderAGI > attackerAGI && Random.value < 0.25f)) {
			if (bonusDamage > 0) {
				// Add QTE Bonus Damage
				attackDamage = bonusDamage;

				if (defenderHP > bonusDamage) {
					if (Random.value <= 0.5f) {
						BattleDialogue.S.DisplayText(attackerName + "'s attack attempt nearly failed, but scraped " + defenderName + " for " + attackDamage + " points!");
					} else {
						BattleDialogue.S.DisplayText(attackerName + " nearly missed the mark, but knicked " + defenderName + " for " + attackDamage + " points!");
					}
				}
			} else {
				if (Random.value <= 0.5f) {
					BattleDialogue.S.DisplayText(attackerName + " attempted to attack " + defenderName + "... but missed!");
				} else {
					BattleDialogue.S.DisplayText(attackerName + " missed the mark! " + defenderName + " dodged out of the way!");
				}
			}
		} else {
			// Critical Hit
			bool isCriticalHit = false;
			if (Random.value < 0.05f) {
				tLevel *= 2;
				isCriticalHit = true;
			}

			// Roll Lvl Dice
			for (int i = 0; i < tLevel; i++) {
				int tRandom = Random.Range(1, 4);
				attackDamage += tRandom;
			}

			// Add Modifier (Strength)
			attackDamage += attackerSTR;

			// Subtract Modifier (Defense)
			attackDamage -= defenderDEF;

			if (attackDamage < 0) {
				attackDamage = 0;
			}

			// Add QTE Bonus Damage
			attackDamage += bonusDamage;

			// If DEFENDING, cut AttackDamage in HALF
			CheckIfDefending(defenderName);

			if (defenderHP > attackDamage) {
				// Display Text
				if (isCriticalHit) {
					BattleDialogue.S.DisplayText("Critical hit!\n" + attackerName + " struck " + defenderName + " for " + attackDamage + " points!");
				} else {
					BattleDialogue.S.DisplayText(attackerName + " struck " + defenderName + " for " + attackDamage + " points!");
				}
			}
		}
		// Reset QTE Bonus Damage
		bonusDamage = 0;
	}

	///////////////////////////////// DEFEND /////////////////////////////////
	public void AddDefender(string defender){
		defenders.Add (defender);
	}
	void RemoveDefender(string defender){
		if (defenders.Contains (defender)) {
			defenders.Remove (defender);
		}
	}

	// If DEFENDING, cut AttackDamage in HALF
	public void CheckIfDefending(string defender){
		if (defenders.Contains (defender)) {
			attackDamage /= 2;
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class BattleInitiative : MonoBehaviour {
	[Header("Set in Inspector")]
	// Enemy GameObject Positions (varies depending on amount of enemies)
	List<Vector3>            enemyPositions = new List<Vector3> { 
		new Vector3(4.5f, 1.375f, 0),
		new Vector3(6, -0.5f, 0),
		new Vector3(7.5f, 0.5f, 0),
		new Vector3(4.5f, 0.25f, 0)
	};

	[Header ("Set Dynamically")]
	// Singleton
	private static BattleInitiative _S;
	public static BattleInitiative S { get { return _S; } set { _S = value; } }

	private Battle 					_;

	// Initiative
	private int d20;
	// Key: Character Name, Value: Turn Order
	private Dictionary<string, int> 	turnOrder = new Dictionary<string, int> ();

	void Awake() {
		// Singleton
		S = this;
	}

	void Start () {
		_ = Battle.S;
	}

	public void Initiative () {
		// Clear TurnOrder List
		_.turnOrder.Clear();

		// Reset turnNdx
		_.turnNdx = 0;

		// Set Party Amount to partyNdx
		_.partyQty = PartyStats.S.partyNdx;

		//////////////////////////////////////////// PARTY MEMBERS ////////////////////////////////////////////
		// De/Activate Party Member Stats/Sprite
		if (_.partyQty < 1) {
			BattlePlayerActions.S.PlayerButtonAndStatsSetActive (0, true);
			BattlePlayerActions.S.PlayerButtonAndStatsSetActive (1, false);

			// Reset PlayerDead bools
			_.playerDead[0] = false;
			_.playerDead[1] = true;

			// Update Health Bars
			ProgressBars.S.playerHealthBarsCS[0].UpdateBar(PartyStats.S.HP[0], PartyStats.S.maxHP[0]);

			// Update Magic Bars
			ProgressBars.S.playerMagicBarsCS[0].UpdateBar(PartyStats.S.MP[0], PartyStats.S.maxMP[0]);

		} else if (_.partyQty >= 1) {
			BattlePlayerActions.S.PlayerButtonAndStatsSetActive (0, true);
			BattlePlayerActions.S.PlayerButtonAndStatsSetActive (1, true);

			// Reset PlayerDead bools
			_.playerDead[0] = false;
			_.playerDead[1] = false;

			// Update Health Bars
			ProgressBars.S.playerHealthBarsCS[0].UpdateBar(PartyStats.S.HP[0], PartyStats.S.maxHP[0]);
			ProgressBars.S.playerHealthBarsCS[1].UpdateBar(PartyStats.S.HP[1], PartyStats.S.maxHP[1]);

			// Update Magic Bars
			ProgressBars.S.playerMagicBarsCS[0].UpdateBar(PartyStats.S.MP[0], PartyStats.S.maxMP[0]);
			ProgressBars.S.playerMagicBarsCS[1].UpdateBar(PartyStats.S.MP[1], PartyStats.S.maxMP[1]);
		}

		//////////////////////////////////////////// ENEMIES ////////////////////////////////////////////

		// Randomly Set Enemy Amount
		if (_.enemyStats[0].partyAmount == 0) {
			_.randomFactor = Random.Range(0, 100);
			if (_.randomFactor < 33){
				_.enemyAmount = 1;
			}
			else if (_.randomFactor >= 33 && _.randomFactor <= 66){
				_.enemyAmount = 2;
			}
			else if (_.randomFactor > 66){
				_.enemyAmount = 3;
			}
		}
        // Set Enemy Amount to a fixed number
        else{
            _.enemyAmount = _.enemyStats[0].partyAmount;
		}

        // Set Enemy Amount (for testing)
        //_.enemyAmount = 3;

        // 1 Enemy
        if (_.enemyAmount >= 1) {
			// Activate/Deactivate Enemy Buttons, Stats, Sprites
			BattlePlayerActions.S.EnemyButtonAndShadowSetActive(0, true);
			_.enemySprite[0].enabled = true;
			BattlePlayerActions.S.EnemyButtonAndShadowSetActive(1, false);
			_.enemySprite[1].enabled = false;
			BattlePlayerActions.S.EnemyButtonAndShadowSetActive(2, false);
			_.enemySprite[2].enabled = false;

			Battle.S.enemyAnimator[0].CrossFade("Arrival", 0);

			// Set Enemy Sprite Positions
			_.enemyGameObjectHolders[0].transform.position = enemyPositions[3];

			// HP
			_.enemyStats[0].HP = _.enemyStats[0].maxHP;

			// Gold/EXP payout
			_.expToAdd += _.enemyStats [0].EXP;
			_.goldToAdd += _.enemyStats [0].Gold;

            // Reset EnemyDead bools:  For EnemyDeaths
            _.enemyStats[0].isDead = false;
            _.enemyStats[1].isDead = true;
            _.enemyStats[2].isDead = true;

            // Enable/Update Health Bars
            ProgressBars.S.enemyHealthBarsCS[0].transform.parent.gameObject.SetActive(true);
			ProgressBars.S.enemyHealthBarsCS[1].transform.parent.gameObject.SetActive(false);
			ProgressBars.S.enemyHealthBarsCS[2].transform.parent.gameObject.SetActive(false);
			ProgressBars.S.enemyHealthBarsCS[0].UpdateBar(_.enemyStats[0].HP, _.enemyStats[0].maxHP);

			// 2 enemies
			if (_.enemyAmount >= 2) {
				// Activate Enemy Buttons, Stats, Sprites
				BattlePlayerActions.S.EnemyButtonAndShadowSetActive(1, true);
				_.enemySprite[1].enabled = true;

				Battle.S.enemyAnimator[1].CrossFade("Arrival", 0);

				// Set Enemy Sprite Positions
				_.enemyGameObjectHolders[0].transform.position = enemyPositions[0];
				_.enemyGameObjectHolders[1].transform.position = enemyPositions[1];

				// HP
				_.enemyStats[1].HP = _.enemyStats[1].maxHP;

				// Gold/EXP payout
				_.expToAdd += _.enemyStats [1].EXP;
				_.goldToAdd += _.enemyStats [1].Gold;

				// Reset EnemyDead bools:  For EnemyDeaths
				_.enemyStats[1].isDead = false;

				// Update Health Bars
				ProgressBars.S.enemyHealthBarsCS[1].transform.parent.gameObject.SetActive(true);
				ProgressBars.S.enemyHealthBarsCS[1].UpdateBar(_.enemyStats[1].HP, _.enemyStats[1].maxHP);

				// 3 Enemies
				if (_.enemyAmount >= 3) {
					// Activate Enemy Buttons, Stats, Sprites
					BattlePlayerActions.S.EnemyButtonAndShadowSetActive(2, true);
					_.enemySprite[2].enabled = true;

					Battle.S.enemyAnimator[2].CrossFade("Arrival", 0);

					// Set Enemy Sprite Positions
					_.enemyGameObjectHolders[0].transform.position = enemyPositions[0];
					_.enemyGameObjectHolders[1].transform.position = enemyPositions[1];
					_.enemyGameObjectHolders[2].transform.position = enemyPositions[2];

					// HP
					_.enemyStats[2].HP = _.enemyStats[2].maxHP;

					// Gold/EXP payout
					_.expToAdd += _.enemyStats [2].EXP;
					_.goldToAdd += _.enemyStats [2].Gold;

					// Reset EnemyDead bools: For EnemyDeaths
					_.enemyStats[2].isDead = false;

					// Update Health Bars
					ProgressBars.S.enemyHealthBarsCS[2].transform.parent.gameObject.SetActive(true);
					ProgressBars.S.enemyHealthBarsCS[2].UpdateBar(_.enemyStats[2].HP, _.enemyStats[2].maxHP);
				}
			} 

			// Set Turn Order
			_.randomFactor = Random.Range (0, 100);
			// No Surprise  
			if (_.randomFactor >= 50) {
				BattleDialogue.S.DisplayText ("Beware! A " + _.enemyStats[0].name + " has appeared!");

				// Calculate Initiative
				CalculateInitiative ();

			// Surprise! Initiative Randomized!
			} else if (_.randomFactor < 50) {
				// Party goes first!
				if (_.randomFactor < 25) {
					BattleDialogue.S.DisplayText(PartyStats.S.playerName[0] + " surprises the Enemy!");

                    // Calculate Initiative
                    CalculateInitiative ("party");

				// Enemies go first!
				} else {
					BattleDialogue.S.DisplayText(_.enemyStats[0].name + " surprises the Player!");

                    // Calculate Initiative
                    CalculateInitiative ("enemies");
				}
			}
		}
	}

	void CalculateInitiative(string whoGoesFirst = "no one"){
		// Reset Dictionary
		turnOrder.Clear ();

		// For all characters to engage in battle, calculate their turn order

		// Player 1
		RollInitiative (PartyStats.S.playerName[0], PartyStats.S.AGI[0], PartyStats.S.LVL [0], true, whoGoesFirst);
		// Player 2
		if (_.partyQty >= 1) {
			RollInitiative (PartyStats.S.playerName[1], PartyStats.S.AGI[1], PartyStats.S.LVL [1], true, whoGoesFirst);
		}
			
		// Enemy 1
		RollInitiative (_.enemyStats[0].name, _.enemyStats[0].AGI, _.enemyStats[0].LVL, false, whoGoesFirst);
		// Enemy 2
		if (_.enemyAmount >= 2) { 
			RollInitiative (_.enemyStats[1].name, _.enemyStats[1].AGI, _.enemyStats[1].LVL, false, whoGoesFirst);
		// Enemy 3
			if (_.enemyAmount >= 3) {
				RollInitiative (_.enemyStats[2].name, _.enemyStats[2].AGI, _.enemyStats[2].LVL, false, whoGoesFirst);
			}
		}

		// Sort Dictionary by turnOrder
		var items = from pair in turnOrder
			orderby pair.Value descending
			select pair;

		// Add each Dictionary Key (party member or enemy name as a string) to Battle.TurnOrder
		foreach (KeyValuePair<string, int> pair in items) {
			_.turnOrder.Add (pair.Key);
		}
	}

	public void RollInitiative (string name, int AGI, int LVL, bool playerOrEnemy, string whoGoesFirst){
		// Roll Player/Enemy's Initiative
		d20 = Random.Range (1, 20);
		d20 += AGI + LVL;

		// If one group catches the other off guard, ensure their turn order is higher
		switch (whoGoesFirst) {
		case "party":
			if (playerOrEnemy) {
				d20 += 1000;
			}
			break;
		case "enemies":
			if (!playerOrEnemy) {
				d20 += 1000;
			}
			break;
		}
			
		// Add character to TurnOrder
		turnOrder.Add (name, d20);
	}
}

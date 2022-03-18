using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class BattleInitiative : MonoBehaviour {
	[Header("Set in Inspector")]
	// Enemy GameObject Positions (varies depending on amount of enemies)
	List<Vector3> enemyPositions = new List<Vector3> {
        // If battle starts w/ two or more enemies
		new Vector3(4.25f, 1.25f, 0f),
        new Vector3(4.75f, -0.5f, 0f),
        new Vector3(6.25f, 0.5f, 0f),
        new Vector3(7.5f, 1.5f, 0f),
        new Vector3(7.75f, -0.75f, 0f),

		// If battle starts w/ one enemy
		new Vector3(4.25f, 0.5f, 0f),
		new Vector3(5.75f, 1.25f, 0f),
		new Vector3(6.25f, -0.5f, 0f),
		new Vector3(7.5f, 1.5f, 0f),
		new Vector3(8f, -0.75f, 0f)
	};

	List<int> enemySpriteSortingOrders = new List<int> {
		// If battle starts w/ two or more enemies
		2, 4, 3, 1, 5,

		// If battle starts w/ one enemy
		3, 2, 4, 1, 5
	};

	private Battle 					_;

	// Initiative
	private int d20;
	// Key: Character Name, Value: Turn Order
	private Dictionary<string, int> 	turnOrder = new Dictionary<string, int> ();

	void Start () {
		_ = Battle.S;
	}

	public void SetInitiative () {
		// Clear TurnOrder List
		_.turnOrder.Clear();

		// Reset turnNdx
		_.turnNdx = 0;

		// Set Party Amount to partyNdx
		_.partyQty = Party.S.partyNdx;

		//////////////////////////////////////////// PARTY MEMBERS ////////////////////////////////////////////
		
		// Deactivate Party Member Stats/Sprite
		for(int i = 0; i < _.playerDead.Count; i++) {
			_.playerActions.PlayerButtonAndStatsSetActive(i, false);

			// Reset PlayerDead bools
			_.playerDead[i] = true;
		}

		// Activate Party Member Stats/Sprite
		for (int i = 0; i <= _.partyQty; i++) {
			_.playerActions.PlayerButtonAndStatsSetActive(i, true);

			// Reset PlayerDead bools
			_.playerDead[i] = false;
		}

		// Update progress bars
		ProgressBars.S.playerHealthBarsCS[0].UpdateBar(Party.S.stats[0].HP, Party.S.stats[0].maxHP);
		ProgressBars.S.playerHealthBarsCS[1].UpdateBar(Party.S.stats[1].HP, Party.S.stats[1].maxHP);
		ProgressBars.S.playerHealthBarsCS[2].UpdateBar(Party.S.stats[2].HP, Party.S.stats[2].maxHP);
		ProgressBars.S.playerMagicBarsCS[0].UpdateBar(Party.S.stats[0].MP, Party.S.stats[0].maxMP, false);
		ProgressBars.S.playerMagicBarsCS[1].UpdateBar(Party.S.stats[1].MP, Party.S.stats[1].maxMP, false);
		ProgressBars.S.playerMagicBarsCS[2].UpdateBar(Party.S.stats[2].MP, Party.S.stats[2].maxMP, false);

		//////////////////////////////////////////// ENEMIES ////////////////////////////////////////////

		// Randomly Set Enemy Amount
		if (_.enemyAmount == 999) {
			_.randomFactor = Random.Range(0, 100);
			if (_.randomFactor < 20){
				_.enemyAmount = 1;
			} else if (_.randomFactor >= 20 && _.randomFactor <= 40){
				_.enemyAmount = 2;
			} else if (_.randomFactor >= 40 && _.randomFactor <= 60) {
				_.enemyAmount = 3;
			} else if (_.randomFactor >= 60 && _.randomFactor <= 80) {
				_.enemyAmount = 4;
			} else if (_.randomFactor > 66) {
				_.enemyAmount = 5;
			}
		} else if(_.enemyAmount == 0) {
			_.enemyAmount = 1;
		}

		// Set Enemy Amount (for testing)
		//_.enemyAmount = 1;

		// Deactivate all enemies
		for (int i = 0; i < _.enemySprite.Count; i++) {
			// Deactivate Enemy Buttons, Stats, Sprites
			_.playerActions.EnemyButtonSetActive(i, false);
			_.enemySprite[i].enabled = false;

			// Reset EnemyDead bools:  For EnemyDeaths
			_.enemyStats[i].isDead = true;

			ProgressBars.S.enemyHealthBarsCS[i].transform.parent.gameObject.SetActive(false);
		}

		// Activate enemies
		for (int i = 0; i < _.enemyAmount; i++) {
			// Deactivate Enemy Buttons, Stats, Sprites
			_.playerActions.EnemyButtonSetActive(i, true);
			_.enemySprite[i].enabled = true;

			Battle.S.enemyAnimator[i].CrossFade("Arrival", 0);

			// HP/MP
			_.enemyStats[i].HP = _.enemyStats[i].maxHP;
			_.enemyStats[i].MP = _.enemyStats[i].maxMP;

			// Amount of items to steal
			_.enemyStats[i].amountToSteal = _.enemyStats[i].maxAmountToSteal;
			_.enemyStats[i].stolenItems.Clear();

			// Gold/EXP payout
			_.expToAdd += _.enemyStats[i].EXP;
			_.goldToAdd += _.enemyStats[i].Gold;

			// Reset EnemyDead bools:  For EnemyDeaths
			_.enemyStats[i].isDead = false;

			// Update Health Bars
			ProgressBars.S.enemyHealthBarsCS[i].transform.parent.gameObject.SetActive(true);
			ProgressBars.S.enemyHealthBarsCS[i].UpdateBar(_.enemyStats[i].HP, _.enemyStats[i].maxHP);
		}

        // Set enemy sprite positions and sorting order
        switch (_.enemyAmount) {
			case 1:
				for (int i = 0; i < _.enemyGameObjectHolders.Count; i++) {
					_.enemyGameObjectHolders[i].transform.position = enemyPositions[i + 5];
					_.enemySprite[i].sortingOrder = enemySpriteSortingOrders[i + 5];
				}
				break;
			case 2:
				for (int i = 0; i < _.enemyGameObjectHolders.Count; i++) {
					_.enemyGameObjectHolders[i].transform.position = enemyPositions[i];
					_.enemySprite[i].sortingOrder = enemySpriteSortingOrders[i];
				}
				break;
			default:
				if(Random.value >= 0.5f) {
					goto case 1;
                } else {
					//goto case 2;
					goto case 1;
				}
        }

		// Set button positions directly over their respective enemy sprite
		for (int i = 0; i < _.enemyGameObjectHolders.Count; i++) {
			Utilities.S.SetUIObjectPosition(_.enemyGameObjectHolders[i], _.playerActions.enemyButtonGO[i]);
		}

		// Set Turn Order
		_.randomFactor = Random.Range (0, 100);
		// No Surprise  
		if (_.randomFactor >= 50) {
			_.dialogue.DisplayText ("Beware! A " + _.enemyStats[0].name + " has appeared!");

			// Calculate Initiative
			CalculateInitiative ();

		// Surprise! Initiative Randomized!
		} else if (_.randomFactor < 50) {
			// Party goes first!
			if (_.randomFactor < 25) {
				_.dialogue.DisplayText(Party.S.stats[0].name + " surprises the Enemy!");

				// Calculate Initiative
				CalculateInitiative ("party");

			// Enemies go first!
			} else {
				_.dialogue.DisplayText(_.enemyStats[0].name + " surprises the Player!");

                // Calculate Initiative
                CalculateInitiative ("enemies");
			}
		}
	}

	void CalculateInitiative(string whoGoesFirst = "no one"){
		// Reset Dictionary
		turnOrder.Clear ();

		// For all characters to engage in battle, calculate their turn order

		// Player 1
		RollInitiative(Party.S.stats[0].name, Party.S.stats[0].AGI, Party.S.stats[0].LVL, true, whoGoesFirst);
		// Player 2
		if (_.partyQty >= 1) {
			RollInitiative(Party.S.stats[1].name, Party.S.stats[1].AGI, Party.S.stats[1].LVL, true, whoGoesFirst);
		// Player 3
			if (_.partyQty >= 2) {
				RollInitiative(Party.S.stats[2].name, Party.S.stats[2].AGI, Party.S.stats[2].LVL, true, whoGoesFirst);
			}
		}

		// Enemy 1
		RollInitiative (_.enemyStats[0].name, _.enemyStats[0].AGI, _.enemyStats[0].LVL, false, whoGoesFirst);
		// Enemy 2
		if (_.enemyAmount >= 2) { 
			RollInitiative (_.enemyStats[1].name, _.enemyStats[1].AGI, _.enemyStats[1].LVL, false, whoGoesFirst);
			// Enemy 3
			if (_.enemyAmount >= 3) {
				RollInitiative (_.enemyStats[2].name, _.enemyStats[2].AGI, _.enemyStats[2].LVL, false, whoGoesFirst);
				// Enemy 4
				if (_.enemyAmount >= 4) {
					RollInitiative(_.enemyStats[3].name, _.enemyStats[3].AGI, _.enemyStats[3].LVL, false, whoGoesFirst);
					// Enemy 5
					if (_.enemyAmount >= 5) {
						RollInitiative(_.enemyStats[4].name, _.enemyStats[4].AGI, _.enemyStats[4].LVL, false, whoGoesFirst);
					}
				}
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
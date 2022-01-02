using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEnemyAI : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static BattleEnemyAI _S;
	public static BattleEnemyAI S { get { return _S; } set { _S = value; } }

	private Battle _;

	void Awake() {
		S = this;
	}

	void Start() {
		_ = Battle.S;
	}

	// More Spells, Use Items, Call for Backup
	public void EnemyAI(int enemyId) {
		BattlePlayerActions.S.ButtonsDisableAll();

        switch (enemyId) {
            case 0:

                break;
            case 1:

                break;
            case 2:

                break;
            case 3:

                break;
            case 4:

                break;
            case 5:

                break;
            case 6:

                break;
            case 7:

                break;
            case 8:

                break;
        }

        switch (_.enemyStats[_.EnemyNdx()].AI) {
            case eEnemyAI.FightWisely:
                // If HP is less than 10%...
                if (Utilities.S.GetPercentage(_.enemyStats[_.EnemyNdx()].HP, _.enemyStats[_.EnemyNdx()].maxHP) < 0.1f) {
                    // If MP is less than 10%...
                    if (Utilities.S.GetPercentage(_.enemyStats[_.EnemyNdx()].MP, _.enemyStats[_.EnemyNdx()].maxMP) < 0.1f) {
                        // Run OR Defend
                        ChanceToCallMove(2, 1);
                    } else {
                        // Heal Spell OR Defend
                        ChanceToCallMove(4, 1);
                    }
                } else {
                    // If MP is less than 10%...
                    if (Utilities.S.GetPercentage(_.enemyStats[_.EnemyNdx()].MP, _.enemyStats[_.EnemyNdx()].maxMP) < 0.1f) {
                        // Attack OR Defend
                        ChanceToCallMove(0, 1);
                    } else {
                        // Attack All Spell OR Attack
                        ChanceToCallMove(5, 0);
                    }
                }
                break;
            case eEnemyAI.FocusOnAttack:
                // If MP is less than 10%...
                if (Utilities.S.GetPercentage(_.enemyStats[_.EnemyNdx()].MP, _.enemyStats[_.EnemyNdx()].maxMP) < 0.1f) {
                    // Attack
                    ChanceToCallMove(0);
                } else {
                    if (_.partyQty == 0) {
                        // Attack Single or Attack
                        ChanceToCallMove(8, 0);
                    } else {
                        // Attack All OR Attack Single 
                        ChanceToCallMove(5, 8);
                    }
                }
                break;
            case eEnemyAI.FocusOnDefend:
                // Defend or Run
                ChanceToCallMove(1, 2);
                break;
            case eEnemyAI.FocusOnHeal:
                // If any enemy's HP is less than 25%...
                if (BattleStats.S.EnemiesNeedsHeal()) {
                    // If MP is less than 10%...
                    if (Utilities.S.GetPercentage(_.enemyStats[_.EnemyNdx()].MP, _.enemyStats[_.EnemyNdx()].maxMP) < 0.1f) {
                        // Defend OR Run 
                        ChanceToCallMove(1, 2);
                    } else {
                        // Heal Spell OR Defend
                        ChanceToCallMove(4, 1);
                    }
                } else {
                    // Attack OR Defend
                    ChanceToCallMove(0, 1);
                }
                break;
            case eEnemyAI.Random:
                // Select Random Move
                CallRandomMove();
                break;
            case eEnemyAI.RunAway:
                // Run or Defend
                ChanceToCallMove(2, 1);
                break;
            case eEnemyAI.CallForBackup:
                // Call for backup next turn
                if (Random.value < _.enemyStats[_.EnemyNdx()].chanceToCallMove / 3) {
                    ChanceToCallMove(7);
                } else {
                    // Attack or Defend 
                    ChanceToCallMove(0, 1);
                }
                break;
            case eEnemyAI.Charge:
                if(_.roundNdx % 3 == 0) { // Every third round...
                    if (_.partyQty == 0) {
                        // Attack Single or Attack
                        ChanceToCallMove(8, 0);
                    } else {
                        // Attack All OR Attack Single 
                        ChanceToCallMove(5, 8);
                    }
                } else {
                    BattleEnemyActions.S.Charge();
                }
                break;
            case eEnemyAI.DontUseMP:
            default:
                break;
        }
    }

	// If Enemy is lucky, check if it has desired move
	void ChanceToCallMove(int firstChoiceNdx, int secondChoiceNdx = 999) {
		// If lucky, call first choice move...
		if (Random.value < _.enemyStats[_.EnemyNdx()].chanceToCallMove) {
			// Loop through the Enemy's Moves, check if Enemy knows move
			for (int i = 0; i < _.enemyStats[_.EnemyNdx()].moveList.Count; i++) {
                if (firstChoiceNdx == _.enemyStats[_.EnemyNdx()].moveList[i]) {
                    CallEnemyMove(firstChoiceNdx);
					return;
                }
            }
        } else {
			// ...otherwise test your on luck on second choice move...
			if (secondChoiceNdx != 999) {
				ChanceToCallMove(secondChoiceNdx);
				return;
			} 
		}

		// If all else fails, call a random move
		CallRandomMove();
	}

    // Randomly select move that the Enemy knows
    void CallRandomMove() {
		// Get random index
		int randomNdx = _.enemyStats[_.EnemyNdx()].moveList[Random.Range(0, _.enemyStats[_.EnemyNdx()].moveList.Count)];

		// Call random move 
		CallEnemyMove(randomNdx);
	}

	public void CallEnemyMove(int moveNdx) {
		switch (moveNdx) {
			case 0: BattleEnemyActions.S.Attack(); break;
			case 1: BattleEnemyActions.S.Defend(); break;
			case 2: BattleEnemyActions.S.Run(); break;
			case 3: BattleEnemyActions.S.Stunned(); break;
			case 4: BattleEnemyActions.S.AttemptHealSpell(); break;
			case 5: BattleEnemyActions.S.AttemptAttackAll(); break;
			case 6: BattleEnemyActions.S.CallForBackup(); break;
			case 7: BattleEnemyActions.S.CallForBackupNextTurn(); break;
			case 8: BattleEnemyActions.S.AttemptAttackSingle(); break;
            case 9: BattleEnemyActions.S.Charge(); break;
            default:BattleEnemyActions.S.Attack(); break;
		}
	}
}
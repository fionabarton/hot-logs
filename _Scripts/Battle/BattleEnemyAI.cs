using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEnemyAI : MonoBehaviour {
	[Header("Set in Inspector")]


	[Header("Set Dynamically")]
	// Singleton
	private static BattleEnemyAI _S;
	public static BattleEnemyAI S { get { return _S; } set { _S = value; } }

	private Battle _;

	void Awake() {
		// Singleton
		S = this;
	}

	void Start() {
		_ = Battle.S;
	}

	// Enemy makes a decision, and ACTS!
	//void EnemyAction () {
	//	BattlePlayerActions.S.ButtonsDisableAll ();

	//	// Select & execute Enemy Move
	//	EnemyAI (enemyStats[EnemyNdx()].AI);
	//}

	// More Spells, Use Items, Call for Backup
	public void EnemyAI(eEnemyAI enemyAI) {
		BattlePlayerActions.S.ButtonsDisableAll();

		switch (enemyAI) {
			case eEnemyAI.FightWisely:
				// Low Health
				//if (enemyStats[EnemyNdx()].HP < 5) {
				if (Utilities.S.GetPercentage(_.enemyStats[_.EnemyNdx()].HP, _.enemyStats[_.EnemyNdx()].maxHP) < 0.1f) {
					// Low MP
					//if (enemyStats[EnemyNdx()].MP < 3) {
					if (Utilities.S.GetPercentage(_.enemyStats[_.EnemyNdx()].MP, _.enemyStats[_.EnemyNdx()].maxMP) < 0.1f) {
						// Run OR Defend
						ChanceToCallMove(2, _.enemyStats[_.EnemyNdx()].chanceToCallMove, 1);
					} else {
						// Heal Spell OR Defend
						ChanceToCallMove(5, _.enemyStats[_.EnemyNdx()].chanceToCallMove, 1);
					}
				} else {
					//if (enemyStats[EnemyNdx()].MP < 3) {
					if (Utilities.S.GetPercentage(_.enemyStats[_.EnemyNdx()].MP, _.enemyStats[_.EnemyNdx()].maxMP) < 0.1f) {
						// Attack OR Defend
						ChanceToCallMove(0, _.enemyStats[_.EnemyNdx()].chanceToCallMove, 1);
					} else {
						// Attack All Spell OR Attack
						ChanceToCallMove(5, _.enemyStats[_.EnemyNdx()].chanceToCallMove, 0);
					}
				}
				break;
			case eEnemyAI.FocusOnAttack:
				//if (enemyStats[EnemyNdx()].MP < 3) {
				if (Utilities.S.GetPercentage(_.enemyStats[_.EnemyNdx()].MP, _.enemyStats[_.EnemyNdx()].maxMP) < 0.1f) {
					// Attack
					ChanceToCallMove(0, _.enemyStats[_.EnemyNdx()].chanceToCallMove);
				} else {
					// Attack All Spell
					ChanceToCallMove(5, _.enemyStats[_.EnemyNdx()].chanceToCallMove);
				}
				break;
			case eEnemyAI.FocusOnDefend:
				// Defend or Run
				ChanceToCallMove(1, _.enemyStats[_.EnemyNdx()].chanceToCallMove, 2);
				break;
			case eEnemyAI.FocusOnHeal:
				// Heal Spell
				CheckEnemyHasAction(4);
				break;
			case eEnemyAI.Random:
				// Select Random Move
				EnemyCallRandomMove();
				break;
			case eEnemyAI.RunAway:
				// Run
				CheckEnemyHasAction(2);
				break;
			case eEnemyAI.CallForBackup:
				// Call for Backup
				CheckEnemyHasAction(6);
				break;
			case eEnemyAI.DontUseMP:
			default:
				break;
		}
	}

	// Attack Party Member w/ lowest HP
	// Heal Enemy w/ lowest HP
	// Attack most dangeous Party Member (one w/ highest damage)

	// Check if Attack Damage = 0 (currently used in BattleEnemyActions.cs)
	public void CheckIfAttackIsUseless() {
		// Calculate Max Damage ((Lvl * 4) + Str - Def)
		int maxAttackerDamage = (_.enemyStats[_.EnemyNdx()].LVL * 4) + _.enemyStats[_.EnemyNdx()].STR - Stats.S.DEF[0];

		// Fight or Run
		if (maxAttackerDamage <= 0) {
			// RUN!
			_.enemyStats[_.EnemyNdx()].AI = eEnemyAI.RunAway;
		} else {

		}
	}

	// If Enemy is lucky, check if it has desired move
	void ChanceToCallMove(int firstChoiceNdx, float chanceValue = 0, int secondChoiceNdx = 999) {

		// If lucky, call first choice move...
		if (Random.value < chanceValue) {
			CheckEnemyHasAction(firstChoiceNdx);
		} else {
			// ...otherwise test your on luck on second choice move
			if (secondChoiceNdx != 999) {
				ChanceToCallMove(secondChoiceNdx, chanceValue);
				// if no second choice, call a random move
			} else {
				EnemyCallRandomMove();
			}
		}
	}

	// If Enemy has desired move, call it
	//public void CheckEnemyHasAction(int firstChoiceNdx, float chanceValue = 0, int secondChoiceNdx = 999) {
	public void CheckEnemyHasAction(int firstChoiceNdx) {
		bool hasMove = false;

		// Loop through the Enemy's Moves, check if Enemy has Move
		for (int i = 0; i < _.enemyStats[_.EnemyNdx()].moveList.Count; i++) {
			if (firstChoiceNdx == _.enemyStats[_.EnemyNdx()].moveList[i]) {
				hasMove = true;
			}
		}

		if (hasMove) {
			CallEnemyMove(firstChoiceNdx);
		} else {
			// if enemy doesn't have move, call random move
			EnemyCallRandomMove();
		}
	}

	// Randomly Select Move that the Enemy knows
	void EnemyCallRandomMove() {
		// Randomly Select Move that the Enemy knows
		int randomEnemyMoveNdx = _.enemyStats[_.EnemyNdx()].moveList[Random.Range(0, _.enemyStats[_.EnemyNdx()].moveList.Count)];

		// Call Random Move 
		CallEnemyMove(randomEnemyMoveNdx);
	}

	void CallEnemyMove(int enemyMoveNdx) {
		// Call method 
		switch (enemyMoveNdx) {
			case 0: BattleEnemyActions.S.Attack(); break;
			case 1: BattleEnemyActions.S.Defend(); break;
			case 2: BattleEnemyActions.S.Run(); break;
			case 3: BattleEnemyActions.S.Stunned(); break;
			case 4: BattleEnemyActions.S.HealSpell(); break;
			case 5: BattleEnemyActions.S.AttackAll(); break;
			case 6: BattleEnemyActions.S.CallForBackup(); break;
			default:
				break;
		}
	}
}
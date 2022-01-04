using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleStats : MonoBehaviour {
	[Header("Set Dynamically")]
	private static BattleStats _S;
	public static BattleStats S { get { return _S; } set { _S = value; } }

	private Battle _;

	void Awake() {
		S = this;
	}

	void Start() {
		_ = Battle.S;
	}

	// Returns the index of the party member with the lowest HP
	public int GetPlayerWithLowestHP() {
		int ndx = 0;
		int lowestHP = 9999;

		for (int i = 0; i <= Party.S.partyNdx; i++) {
			if (!_.playerDead[i]) {
				if (Party.S.stats[i].HP < lowestHP) {
					lowestHP = Party.S.stats[i].HP;
					ndx = i;
				}
			}
		}
		return ndx;
	}

	// Returns the index of the enemy with the lowest HP
	public int GetEnemyWithLowestHP() {
		int ndx = -1;
		int lowestHP = 9999;

		for (int i = 0; i < _.enemyStats.Count; i++) {
			if (!_.enemyStats[i].isDead) {
				if (_.enemyStats[i].HP < _.enemyStats[i].maxHP) {
					if (_.enemyStats[i].HP < lowestHP) {
						lowestHP = _.enemyStats[i].HP;
						ndx = i;
					}
				}
			}
		}
		return ndx;
	}

	// Returns true if one of the enemy's HP is less than 25%
	public bool EnemiesNeedsHeal() {
		for (int i = 0; i < _.enemyStats.Count; i++) {
			if (!_.enemyStats[i].isDead) {
				if (Utilities.S.GetPercentage(_.enemyStats[i].HP, _.enemyStats[i].maxHP) < 0.25f) {
					return true;
				}
			}
		}
		return false;
	}

	// The enemy attempts to run away if their attack won't damage the player
	public void RunIfAttackUseless() {
		if (Random.value < _.enemyStats[_.EnemyNdx()].chanceToCallMove) {
			// Calculate attack damage to player ((Lvl * 4) + Str - Def)
			int attackDamage = ((_.enemyStats[_.EnemyNdx()].LVL * 4) + _.enemyStats[_.EnemyNdx()].STR) - Party.S.stats[0].DEF;

			// If attack doesn't do any damage...
			if (attackDamage <= 0) {
				// ...the enemy focuses on running away
				_.enemyStats[_.EnemyNdx()].AI = eEnemyAI.RunAway;
				return;
			}
		}
	}

	// Returns a random party member index
	public int GetRandomPlayerNdx() {
		int randomNdx = 0;
		float randomValue = Random.value;
		if (_.partyQty == 0) {
			for (int i = 0; i < _.playerDead.Count; i++) {
				if (!_.playerDead[i]) {
					randomNdx = i;
					break;
				}
			}
		} else if (_.partyQty == 1) {
			if (randomValue > 0.5f) {
				for (int i = 0; i < _.playerDead.Count; i++) {
					if (!_.playerDead[i]) {
						randomNdx = i;
						break;
					}
				}
			} else {
				for (int i = _.playerDead.Count - 1; i >= 0; i--) {
					if (!_.playerDead[i]) {
						randomNdx = i;
						break;
					}
				}
			}
		} else if (_.partyQty == 2) {
			if (randomValue >= 0 && randomValue <= 0.33f) {
				randomNdx = 0;
			} else if (randomValue > 0.33f && randomValue <= 0.66f) {
				randomNdx = 1;
			} else if (randomValue > 0.66f && randomValue <= 1.0f) {
				randomNdx = 2;
			}
		}
		return randomNdx;
	}

	// Get basic physical attack damage
	public void GetAttackDamage(int attackerLVL,int attackerSTR, int attackerAGI, int defenderDEF, int defenderAGI, string attackerName, string defenderName, int defenderHP) {
		// Get Level
		int tLevel = attackerLVL;

		// Reset Attack Damage
		_.attackDamage = 0;

		// 5% chance to Miss/Dodge...
		// ...AND 25% chance to Miss/Dodge if Defender AGI is more than Attacker's 
		if (Random.value <= 0.05f || (defenderAGI > attackerAGI && Random.value < 0.25f)) {
			if (_.bonusDamage > 0) {
				// Add QTE Bonus Damage
				_.attackDamage = _.bonusDamage;

				if (defenderHP > _.bonusDamage) {
					if (Random.value <= 0.5f) {
						BattleDialogue.S.DisplayText(attackerName + "'s attack attempt nearly failed, but scraped " + defenderName + " for " + _.attackDamage + " points!");
					} else {
						BattleDialogue.S.DisplayText(attackerName + " nearly missed the mark, but knicked " + defenderName + " for " + _.attackDamage + " points!");
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
				_.attackDamage += tRandom;
			}

			// Add Modifier (Strength)
			_.attackDamage += attackerSTR;

			// Subtract Modifier (Defense)
			_.attackDamage -= defenderDEF;

			if (_.attackDamage < 0) {
				_.attackDamage = 0;
			}

			// Add QTE Bonus Damage
			_.attackDamage += _.bonusDamage;

			// If DEFENDING, cut AttackDamage in HALF
			CheckIfDefending(defenderName);

			if (defenderHP > _.attackDamage) {
				// Display Text
				if (isCriticalHit) {
					BattleDialogue.S.DisplayText("Critical hit!\n" + attackerName + " struck " + defenderName + " for " + _.attackDamage + " points!");
				} else {
					BattleDialogue.S.DisplayText(attackerName + " struck " + defenderName + " for " + _.attackDamage + " points!");
				}
			}
		}
		// Reset QTE Bonus Damage
		_.bonusDamage = 0;
	}

	// If defending, cut attackDamage in half
	public void CheckIfDefending(string defender) {
		if (_.defenders.Contains(defender)) {
			_.attackDamage /= 2;
		}
	}
}
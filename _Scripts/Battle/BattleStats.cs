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
}
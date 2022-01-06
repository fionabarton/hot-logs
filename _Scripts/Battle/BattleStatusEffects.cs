using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleStatusEffects : MonoBehaviour {
	[Header("Set in Inspector")]
	// Player/Enemy Defense Shields
	public List<GameObject> playerShields;
	public List<GameObject> enemyShields;

	[Header("Set Dynamically")]
	private static BattleStatusEffects _S;
	public static BattleStatusEffects S { get { return _S; } set { _S = value; } }

	// Defending party members & enemies
	public List<string>		defenders = new List<string>();

	// Party members & enemies afflicted by status effects:
	// Poison, Paralysis, Sleep
	public List<string>		thePoisoned = new List<string>();
	public List<string>		theParalyzed = new List<string>();
	public List<string>		theSleeping = new List<string>();

	// Amount of time left for a status ailment to subside:
	// Paralysis, Sleep
	public List<int>		paralysisCount = new List<int>();
	public List<int>		sleepCount = new List<int>();

	private Battle _;
	void Awake() {
		S = this;
	}

	void Start() {
		_ = Battle.S;
	}

	// Defending /////////////////////////////////////////////////////////////
	public void AddDefender(string defender) {
		defenders.Add(defender);
	}
	public void RemoveDefender(string defender) {
		if (defenders.Contains(defender)) {
			defenders.Remove(defender);
		}
	}

	// If defending, cut attackDamage in half
	public void CheckIfDefending(string defender) {
		if (defenders.Contains(defender)) {
			_.attackDamage /= 2;
		}
	}

	// Poisoned /////////////////////////////////////////////////////////////
	public void AddPoisoned(string poisoned) {
		if (!thePoisoned.Contains(poisoned)) {
			thePoisoned.Add(poisoned);
		}
	}
	void RemovePoisoned(string poisoned) {
		if (thePoisoned.Contains(poisoned)) {
			thePoisoned.Remove(poisoned);
		}
	}

	public bool CheckIfPoisoned(string poisoned) {
		if (thePoisoned.Contains(poisoned)) {
			return true;
		}
		return false;
	}
}
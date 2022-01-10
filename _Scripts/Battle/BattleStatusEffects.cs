using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleStatusEffects : MonoBehaviour {
	[Header("Set in Inspector")]
	// Player/Enemy Defense Shields
	public List<GameObject> playerShields;
	public List<GameObject> enemyShields;

	// Player/Enemy Paralyzed Symbols
	public List<GameObject> playerParalyzedIcons;
	public List<GameObject> enemyParalyzedIcons;

	// Player/Enemy Poisoned Symbols
	public List<GameObject> playerPoisonedIcons;
	public List<GameObject> enemyPoisonedIcons;

	// Player/Enemy Sleeping Symbols
	public List<GameObject> playerSleepingIcons;
	public List<GameObject> enemySleepingIcons;

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
	// Paralyzed /////////////////////////////////////////////////////////////
	public void AddParalyzed(string paralyzed) {
		if (!theParalyzed.Contains(paralyzed)) {
			theParalyzed.Add(paralyzed);
			paralysisCount.Add(Random.Range(2, 4));
		}
	}
	void RemoveParalyzed(string paralyzed) {
		if (theParalyzed.Contains(paralyzed)) {
			paralysisCount.RemoveAt(theParalyzed.IndexOf(paralyzed));
			theParalyzed.Remove(paralyzed);
		}
	}
	public bool CheckIfParalyzed(string paralyzed) {
		if (theParalyzed.Contains(paralyzed)) {
			return true;
		}
		return false;
	}

	public void Paralyzed(string paralyzed) {
		// Decrement counter
		paralysisCount[theParalyzed.IndexOf(paralyzed)] -= 1;

		// If counter depleted...
		if (paralysisCount[theParalyzed.IndexOf(paralyzed)] <= 0) {
			// ...no longer paralyzed
			RemoveParalyzed(paralyzed);

			// Display text
			BattleDialogue.S.DisplayText(paralyzed + " is no longer paralyzed!");

			// Audio: Buff 2
			AudioManager.S.PlaySFX(eSoundName.buff2);
		} else {
			// Display text
			BattleDialogue.S.DisplayText(paralyzed + " is paralyzed and cannot move!");

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);
		}
		_.battleMode = eBattleMode.statusAilment;
	}
	// Sleeping /////////////////////////////////////////////////////////////
	public void AddSleeping(string sleeping) {
		if (!theSleeping.Contains(sleeping)) {
			theSleeping.Add(sleeping);
			sleepCount.Add(Random.Range(2, 4));
		}
	}
	void RemoveSleeping(string sleeping) {
		if (theSleeping.Contains(sleeping)) {
			sleepCount.RemoveAt(theSleeping.IndexOf(sleeping));
			theSleeping.Remove(sleeping);
		}
	}
	public bool CheckIfSleeping(string sleeping) {
		if (theSleeping.Contains(sleeping)) {
			return true;
		}
		return false;
	}

	public void Sleeping(string sleeping) {
		// Decrement counter
		sleepCount[theSleeping.IndexOf(sleeping)] -= 1;

		// If counter depleted...
		if (sleepCount[theSleeping.IndexOf(sleeping)] <= 0) {
			// ...no longer sleeping
			RemoveSleeping(sleeping);

			// Display text
			BattleDialogue.S.DisplayText(sleeping + " is no longer asleep!");

			// Audio: Buff 2
			AudioManager.S.PlaySFX(eSoundName.buff2);
		} else {
			// Display text
			BattleDialogue.S.DisplayText(sleeping + " is asleep and won't wake up!");

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);
		}
		_.battleMode = eBattleMode.statusAilment;
	}

	// Returns true if the combatant has a status ailment
	public bool HasStatusAilment(string name) {
		if(CheckIfParalyzed(name) || CheckIfPoisoned(name) || CheckIfSleeping(name)) {
			return true;
        }
		return false;
	}
	//////////////////////////////////////////////////////////////////////////
	//public int GetCombatantNdx(string name) {
	//    for (int i = 0; i < Party.S.stats.Count; i++) {
	//        if (Party.S.stats[i].name == name) {
	//            return i;
	//        }
	//    }
	//    for (int i = 0; i < _.enemyStats.Count; i++) {
	//        if (_.enemyStats[i].name == name) {
	//            return i;
	//        }
	//    }
	//    return -1;
	//}
}
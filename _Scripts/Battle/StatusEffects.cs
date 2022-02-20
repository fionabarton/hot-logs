using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffects : MonoBehaviour {
	[Header("Set in Inspector")]
	// Player/Enemy Defense Shields
	public List<GameObject> playerShields;
	public List<GameObject> enemyShields;

	// Player/Enemy Paralyzed Icons
	public List<GameObject> playerParalyzedIcons;
	public List<GameObject> enemyParalyzedIcons;

	// Player/Enemy Poisoned Icons (Battle)
	public List<GameObject> playerPoisonedIcons;
	public List<GameObject> enemyPoisonedIcons;

	// Player/Enemy Sleeping Icons
	public List<GameObject> playerSleepingIcons;
	public List<GameObject> enemySleepingIcons;

	// PlayerButtons Poisoned Icons (Overworld)
	public List<GameObject> playerButtonsPoisonedIcons;

	// PauseScreen Poisoned Icons (Overworld)
	public List<GameObject> pauseScreenPoisonedIcons;

	// Player GameObject Poisoned Icon (Overworld)
	public GameObject		playerPoisonedIcon;

	[Header("Set Dynamically")]
	// Defending party members & enemies
	public List<string>		defenders = new List<string>();

	// Party members & enemies afflicted by status effects:
	// Paralysis, Sleep, Poison
	public List<string>		theParalyzed = new List<string>();
	public List<string>		theSleeping = new List<string>();
	public List<string>		thePoisoned = new List<string>();

	// Amount of time left for a status ailment to subside:
	// Paralysis, Sleep
	public List<int>		paralyzedCount = new List<int>();
	public List<int>		sleepingCount = new List<int>();

	private static StatusEffects _S;
	public static StatusEffects S { get { return _S; } set { _S = value; } }

	private Battle _;

	void Awake() {
		S = this;
	}

	void Start() {
		_ = Battle.S;
	}

	// Called at start of a battle
	public void Initialize() {
		// Clear defenders
		defenders.Clear();

		// Deactivate defense shields
		Utilities.S.SetActiveList(playerShields, false);
		Utilities.S.SetActiveList(enemyShields, false);

		// Deactivate Status Ailment Icons (Paralyzed, Poisoned, Sleeping)
		Utilities.S.SetActiveList(playerParalyzedIcons, false);
		Utilities.S.SetActiveList(playerPoisonedIcons, false);
		Utilities.S.SetActiveList(playerSleepingIcons, false);
		Utilities.S.SetActiveList(enemyParalyzedIcons, false);
		Utilities.S.SetActiveList(enemyPoisonedIcons, false);
		Utilities.S.SetActiveList(enemySleepingIcons, false);

		// Clear status ailment lists
		theParalyzed.Clear();
		theSleeping.Clear();
		paralyzedCount.Clear();
		sleepingCount.Clear();

		for (int i = 0; i <= Party.S.partyNdx; i++) {
			// If already poisoned...
			if (CheckIfPoisoned(Party.S.stats[i].name)) {
				// ...activate poison icon
				playerPoisonedIcons[i].SetActive(true);
            }
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////
	// Defend
	////////////////////////////////////////////////////////////////////////////////////////
	public void AddDefender(string defender, bool isPlayer) {
		defenders.Add(defender);

		// Activate defense shield
		if (isPlayer) {
			playerShields[_.PlayerNdx()].SetActive(true);
		} else {
			enemyShields[_.EnemyNdx()].SetActive(true);
		}

		// Audio: Buff 2
		AudioManager.S.PlaySFX(eSoundName.buff2);
	}
	public void RemoveDefender(string defender, bool isPlayer, int ndx) {
		if (defenders.Contains(defender)) {
			defenders.Remove(defender);

			// Deactivate defense shield
			if (isPlayer) {
				playerShields[ndx].SetActive(false);
			} else {
				enemyShields[ndx].SetActive(false);
			}
		}
	}
	// If defending, cut attackDamage in half
	public void CheckIfDefending(string defender) {
		if (defenders.Contains(defender)) {
			_.attackDamage /= 2;
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////
	// Poison
	////////////////////////////////////////////////////////////////////////////////////////
	public void AddPoisoned(string poisoned, int ndx) {
		if (!thePoisoned.Contains(poisoned)) {
			thePoisoned.Add(poisoned);

			// If this turn is a player's turn...
			if (_.PlayerNdx() != -1) {
				// Activate poisoned icon
				enemyPoisonedIcons[ndx].SetActive(true);

				BattleDialogue.S.DisplayText(Party.S.stats[_.PlayerNdx()].name + " has poisoned " + _.enemyStats[ndx].name + " indefinitely" + "...\n...not nice!");
			} else {
				// Anim
				_.playerAnimator[ndx].CrossFade("Poisoned", 0);

				// Activate poisoned icon
				playerPoisonedIcons[ndx].SetActive(true);

				BattleDialogue.S.DisplayText(_.enemyStats[_.EnemyNdx()].name + " has poisoned " + Party.S.stats[ndx].name + " indefinitely" + " ...\n...not nice!");
			}

			// Audio: Buff 2
			AudioManager.S.PlaySFX(eSoundName.buff2);

			_.NextTurn();
		}
	}
	public void RemovePoisoned(string poisoned, bool isPlayer, int ndx) {
		if (thePoisoned.Contains(poisoned)) {
			//poisonedCount.RemoveAt(thePoisoned.IndexOf(poisoned));
			thePoisoned.Remove(poisoned);

			// If this turn is a player's turn...
			if (isPlayer) {
				// Deactivate status ailment icon
				playerPoisonedIcons[ndx].SetActive(false);
			} else {
				// Deactivate status ailment icon
				enemyPoisonedIcons[ndx].SetActive(false);
			}

			// Display text
			BattleDialogue.S.DisplayText(poisoned + " is no longer poisoned!");

			// Audio: Buff 2
			AudioManager.S.PlaySFX(eSoundName.buff2);
		}
	}
	public bool CheckIfPoisoned(string poisoned) {
		if (thePoisoned.Contains(poisoned)) {
			return true;
		}
		return false;
	}

	public void Poisoned(string poisoned, bool isPlayer, int ndx) {
		// If this turn is a player's turn...
		if (isPlayer) {
			// Get 6-10% of max HP
			float lowEnd = Party.S.stats[ndx].maxHP * 0.06f;
			float highEnd = Party.S.stats[ndx].maxHP * 0.10f;
			_.attackDamage = (int)Random.Range(lowEnd, highEnd);

			// Play attack animations, SFX, and spawn objects
			BattleEnemyActions.S.PlaySingleAttackAnimsAndSFX(ndx, false);

			// Decrement HP
			RPG.S.SubtractPlayerHP(ndx, _.attackDamage);

			// Display text
			BattleDialogue.S.DisplayText(poisoned + " suffers the consequences of being poisoned...\n...damaged for " + _.attackDamage + " HP!");

			// Check if dead
			if (Party.S.stats[ndx].HP < 1) {
				BattleEnd.S.PlayerDeath(ndx);
				return;
			}
		} else {
			// Get 6-10% of max HP
			float lowEnd = _.enemyStats[ndx].maxHP * 0.06f;
			float highEnd = _.enemyStats[ndx].maxHP * 0.10f;
			_.attackDamage = Mathf.Max(1, (int)Random.Range(lowEnd, highEnd));

			BattleSpells.S.DamageEnemyAnimation(ndx, true, false);

			// Decrement HP
			RPG.S.SubtractEnemyHP(ndx, _.attackDamage);

			// Display text
			BattleDialogue.S.DisplayText(poisoned + " suffers the consequences of being poisoned...\n...damaged for " + _.attackDamage + " HP!");

			// Check if dead
			if (_.enemyStats[ndx].HP < 1) {
				BattleEnd.S.EnemyDeath(ndx);
				return;
			}
		}
		_.mode = eBattleMode.statusAilment;
	}

	////////////////////////////////////////////////////////////////////////////////////////
	// Paralyze
	////////////////////////////////////////////////////////////////////////////////////////
	public void AddParalyzed(string paralyzed, int ndx) {
		if (!theParalyzed.Contains(paralyzed)) {
			theParalyzed.Add(paralyzed);
			paralyzedCount.Add(Random.Range(2, 4));

			// If this turn is a player's turn...
			if (_.PlayerNdx() != -1) {
				// Activate paralyzed icon
				enemyParalyzedIcons[ndx].SetActive(true);

				// Reset next turn move index & deactivate help bubble
				_.StopCallingForHelp(ndx);

				// If defending...stop defending
				RemoveDefender(paralyzed, false, ndx);

				BattleDialogue.S.DisplayText(Party.S.stats[_.PlayerNdx()].name + " has temporarily paralyzed " + _.enemyStats[ndx].name + "...\n...not nice!");
			} else {
				// Anim
				_.playerAnimator[ndx].CrossFade("Paralyzed", 0);

				// Activate paralyzed icon
				playerParalyzedIcons[ndx].SetActive(true);

				// If defending...stop defending
				RemoveDefender(paralyzed, true, ndx);

				BattleDialogue.S.DisplayText(_.enemyStats[_.EnemyNdx()].name + " has temporarily paralyzed " + Party.S.stats[ndx].name + "...\n...not nice!");
			}

			// Audio: Buff 2
			AudioManager.S.PlaySFX(eSoundName.buff2);

			_.NextTurn();
		}
	}
	public void RemoveParalyzed(string paralyzed, bool isPlayer, int ndx) {
		if (theParalyzed.Contains(paralyzed)) {
			paralyzedCount.RemoveAt(theParalyzed.IndexOf(paralyzed));
			theParalyzed.Remove(paralyzed);

			// If this turn is a player's turn...
			if (isPlayer) {
				// Deactivate status ailment icon
				playerParalyzedIcons[ndx].SetActive(false);
			} else {
				// Deactivate status ailment icon
				enemyParalyzedIcons[ndx].SetActive(false);
			}

			// Display text
			BattleDialogue.S.DisplayText(paralyzed + " is no longer paralyzed!");

			// Audio: Buff 2
			AudioManager.S.PlaySFX(eSoundName.buff2);
		}
	}
	public bool CheckIfParalyzed(string paralyzed) {
		if (theParalyzed.Contains(paralyzed)) {
			return true;
		}
		return false;
	}

	public void Paralyzed(string paralyzed, bool isPlayer, int ndx) {
		// Decrement counter
		paralyzedCount[theParalyzed.IndexOf(paralyzed)] -= 1;

		// If counter depleted...
		if (paralyzedCount[theParalyzed.IndexOf(paralyzed)] <= 0) {
			// ...no longer paralyzed
			RemoveParalyzed(paralyzed, isPlayer, ndx);

			// Anim
			if (isPlayer) {
				_.playerAnimator[ndx].CrossFade("Win_Battle", 0);
			}
		} else {
			// Display text
			BattleDialogue.S.DisplayText(paralyzed + " is paralyzed and cannot move!");

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);
		}
		_.mode = eBattleMode.statusAilment;
	}

	////////////////////////////////////////////////////////////////////////////////////////
	// Sleep
	////////////////////////////////////////////////////////////////////////////////////////
	public void AddSleeping(string sleeping, int ndx) {
		if (!theSleeping.Contains(sleeping)) {
			theSleeping.Add(sleeping);
			sleepingCount.Add(Random.Range(2, 4));

			// If this turn is a player's turn...
			if (_.PlayerNdx() != -1) {
				// Activate sleeping icon
				enemySleepingIcons[ndx].SetActive(true);

				// Reset next turn move index & deactivate help bubble
				_.StopCallingForHelp(ndx);

				// If defending...stop defending
				RemoveDefender(sleeping, false, ndx);

				BattleDialogue.S.DisplayText(Party.S.stats[_.PlayerNdx()].name + " has temporarily put " + _.enemyStats[ndx].name + " to sleep...\n...not nice!");
			} else {
				// Anim
				_.playerAnimator[ndx].CrossFade("Sleeping", 0);

				// Activate sleeping icon
				playerSleepingIcons[ndx].SetActive(true);

				// If defending...stop defending
				RemoveDefender(sleeping, true, ndx);

				BattleDialogue.S.DisplayText(_.enemyStats[_.EnemyNdx()].name + " has temporarily put " + Party.S.stats[ndx].name + " to sleep...\n...not nice!");
			}

			// Audio: Buff 2
			AudioManager.S.PlaySFX(eSoundName.buff2);

			_.NextTurn();
		}
	}
	public void RemoveSleeping(string sleeping, bool isPlayer, int ndx) {
		if (theSleeping.Contains(sleeping)) {
			sleepingCount.RemoveAt(theSleeping.IndexOf(sleeping));
			theSleeping.Remove(sleeping);

			// If this turn is a player's turn...
			if (isPlayer) {
				// Deactivate status ailment icon
				playerSleepingIcons[ndx].SetActive(false);
			} else {
				// Deactivate status ailment icon
				enemySleepingIcons[ndx].SetActive(false);
			}

			// Display text
			BattleDialogue.S.DisplayText(sleeping + " is no longer asleep!");

			// Audio: Buff 2
			AudioManager.S.PlaySFX(eSoundName.buff2);
		}
	}
	public bool CheckIfSleeping(string sleeping) {
		if (theSleeping.Contains(sleeping)) {
			return true;
		}
		return false;
	}

	public void Sleeping(string sleeping, bool isPlayer, int ndx) {
		// Decrement counter
		sleepingCount[theSleeping.IndexOf(sleeping)] -= 1;

		// If counter depleted...
		if (sleepingCount[theSleeping.IndexOf(sleeping)] <= 0) {
			// ...no longer sleeping
			RemoveSleeping(sleeping, isPlayer, ndx);

			// Anim
			if (isPlayer) {
				_.playerAnimator[ndx].CrossFade("Win_Battle", 0);
			}
		} else {
			// Display text
			BattleDialogue.S.DisplayText(sleeping + " is asleep and won't wake up!");

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);
		}
		_.mode = eBattleMode.statusAilment;
	}

	////////////////////////////////////////////////////////////////////////////////////////
	// Helper functions
	////////////////////////////////////////////////////////////////////////////////////////
	
	// Returns true if the combatant has a status ailment ////////////////////
	public bool HasStatusAilment(string name) {
		if(CheckIfParalyzed(name) || CheckIfPoisoned(name) || CheckIfSleeping(name)) {
			return true;
        }
		return false;
	}

	// Remove all status ailments from a combatant
	public void RemoveAllStatusAilments(string name, bool isPlayer, int ndx) {
		RemoveDefender(name, isPlayer, ndx);
		RemoveParalyzed(name, isPlayer, ndx);
		RemovePoisoned(name, isPlayer, ndx);
		RemoveSleeping(name, isPlayer, ndx);
    }
}
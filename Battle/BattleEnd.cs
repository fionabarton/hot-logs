using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEnd : MonoBehaviour {
	[Header("Set Dynamically")]
	public LevelUpMessage levelUpMessage;

	private Battle _;

	void Start() {
		// Get components
		levelUpMessage = GetComponent<LevelUpMessage>();

		_ = Battle.S;
    }

	public void RemoveEnemy(int ndx) {
        // Subtract from EnemyAmount 
        _.enemyAmount -= 1;

		// Deactivate Enemy Button & Shadow
		_.playerActions.EnemyButtonSetActive(ndx, false);

		// Set Selected GameObject (Fight Button)
		_.enemyStats[ndx].isDead = true;

		// Reset next turn move index & deactivate help bubble
		_.StopCallingForHelp(ndx);

		// Remove enemy from turn order
		_.turnOrder.Remove(_.enemyStats[ndx].name);

		// Remove all status ailments 
		StatusEffects.S.RemoveAllStatusAilments(false, ndx);

		// Deactivate '...' Word Bubble
		_.dotDotDotWordBubble.SetActive(false);

		_.UI.turnCursor.SetActive(false); // Deactivate Turn Cursor
		Utilities.S.SetActiveList(_.UI.targetCursors, false);
	}

    public void EnemyRun(int ndx) {
		// Run (50% chance)
		if (Random.value >= 0.5f) {
			RemoveEnemy(ndx);

			// Animation: Enemy RUN
			_.enemyAnimator[ndx].CrossFade("Run", 0);

			_.dialogue.DisplayText(_.enemyStats[ndx].name + " ran away!\nCOWARD! HO HO HO!");

			// Subtract EXP/Gold
			_.expToAdd -= _.enemyStats[ndx].EXP;
            _.goldToAdd -= _.enemyStats[ndx].Gold;

			_.turnNdx -= 1; // Lower Turn Index

			// Randomly select DropItem
			AddDroppedItems(ndx);

			// Audio: Run
			AudioManager.S.PlaySFX(eSoundName.run);

			// Don't Deactivate Enemy in Overworld
			if (_.enemyAmount <= 0) {
				// The enemy ran away, so you're chasing after them!
				EnemyManager.S.GetEnemyMovement(eMovement.flee);

				// DropItem or AddExpAndGold
				if (_.droppedItems.Count >= 1) {
					// Switch Mode
					_.mode = eBattleMode.dropItem;
				} else {
                    // Switch Mode
                    _.mode = eBattleMode.addExpAndGold;
				}
			} else {
				_.NextTurn();
			}
		} else {
			_.dialogue.DisplayText(_.enemyStats[ndx].name + " attempts to run...\n...but the party has blocked the path!");

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);

			_.NextTurn();
		}
	}

	public void EnemyDeath(int ndx, bool displayText = true) {
		RemoveEnemy(ndx);

		// If the enemy has stolen an item from the party
		if (_.enemyStats[ndx].stolenItems.Count >= 1) {
			for(int i = 0; i < _.enemyStats[ndx].stolenItems.Count; i++) {
				// Return stolen items
				Inventory.S.AddItemToInventory(_.enemyStats[ndx].stolenItems[i]);
			}

			if (displayText) {
				_.dialogue.DisplayText(_.enemyStats[ndx].name + " has been felled!\nWhat they've stolen is returned to the party!");
			}
		} else {
			if (displayText) {
				_.dialogue.DisplayText(_.enemyStats[ndx].name + " has been felled!");
			}
		}

		// Randomly drop an item if everything hasn't aleady been stolen
		if (_.enemyStats[ndx].amountToSteal > 0) {
			AddDroppedItems(ndx);
		}

		// Audio: Death
		AudioManager.S.PlaySFX(eSoundName.death);

		// Animation: Enemy DEATH
		_.enemyAnimator[ndx].CrossFade("Death", 0);
		
		CheckIfAllEnemiesDead();
	}

	public void CheckIfAllEnemiesDead() {
		// Add Exp & Gold or Next Turn
		if (_.enemyAmount <= 0) {
			// Animation: Player WIN BATTLE
			for (int i = 0; i < _.playerAnimator.Count; i++) {
				if (!_.playerDead[i]) {
					_.playerAnimator[i].CrossFade("Win_Battle", 0);
				}
			}

			// Remove temporary status effects 
			for (int i = 0; i <= Party.S.partyNdx; i++) {
				StatusEffects.S.RemoveDefender(true, i);
				StatusEffects.S.RemoveParalyzed(true, i, false);
				StatusEffects.S.RemoveSleeping(true, i, false);
			}

			// Deactivate top display message
			_.dialogue.displayMessageTextTop.gameObject.transform.parent.gameObject.SetActive(false);

			// DropItem or AddExpAndGold
			if (_.droppedItems.Count >= 1) {
				// Switch Mode
				_.mode = eBattleMode.dropItem;
			} else {
				// Switch Mode
				_.mode = eBattleMode.addExpAndGoldNoDrops;
			}
		} else { _.NextTurn(); }
	}

	public void AddDroppedItems(int ndx) {
		if(_.enemyStats[ndx].amountToSteal > 0) {
			if (Random.value < _.enemyStats[ndx].chanceToDrop) {
				// Get random item index
				int randomNdx = Random.Range(0, _.enemyStats[ndx].itemsToDrop.Count);

				// Add item to droppedItems list
				_.droppedItems.Add(Items.S.GetItem(_.enemyStats[ndx].itemsToDrop[randomNdx]));
			}
		}
    }

	public void DropItem(Item item) { 
		// Add dropped item to inventory
		Inventory.S.AddItemToInventory(item);

		// Audio: Win
		AudioManager.S.PlaySong(eSongName.win);

		// Display text
		_.dialogue.DisplayText(WordManager.S.GetRandomExclamation() + "!\nThey dropped a " + item.name + "...\n...y'all add it to the inventory!") ;

		// Remove item from list
		Battle.S.droppedItems.RemoveAt(0);

        // If there are any more dropped items to add...
        if (Battle.S.droppedItems.Count <= 0) {
			// Switch Mode
			_.mode = eBattleMode.addExpAndGold;
		}
	}

	public void PlayerDeath(int ndx, bool displayText = true) {
		// For EnemyAttack (prevents attacking dead party members)
		_.playerDead[ndx] = true;

		// Subtract from PartyQty 
		_.partyQty -= 1;

		// Audio: Death
		AudioManager.S.PlaySFX(eSoundName.death);

		// Animation: Player DEATH
		_.playerAnimator[ndx].CrossFade("Death", 0);

		// Remove all status ailments 
		StatusEffects.S.RemoveAllStatusAilments(true, ndx);

		// Remove player from turn order
		_.turnOrder.Remove(Party.S.stats[ndx].name);

        if (displayText) {
			_.dialogue.DisplayText("Oh no!\n" + Party.S.stats[ndx].name + " has been felled!");
		}

		// Add PartyDeath or NextTurn 
		// Switch Mode
		if (_.partyQty < 0) { _.mode = eBattleMode.partyDeath; } else { _.NextTurn(); }
	}
	public void PartyDeath() {
		_.UI.turnCursor.SetActive(false);
		Utilities.S.SetActiveList(_.UI.targetCursors, false);

		// You died, so the enemy is chasing after you!
		EnemyManager.S.GetEnemyMovement(eMovement.pursueRun);

		_.dialogue.DisplayText("Failure is the party!\nY'all've been wiped out/felled!");

		// Audio: Lose
		AudioManager.S.PlaySong(eSongName.lose);

		// Return to Overworld
		ReturnToWorldDelay();
	}

	// Add Gold and EXP, Check for Level UP  
	public void AddExpAndGold(bool playSound) {
        // Audio: Win
        if (playSound) {
			AudioManager.S.PlaySong(eSongName.win);
		}

		// Complete Quest
		if (_.enemyStats[0].questNdx != 0) {
			QuestManager.S.completed[_.enemyStats[0].questNdx] = true;
		}

		if (_.goldToAdd <= 0) { _.goldToAdd = 0; }
		if (_.expToAdd <= 0) { _.expToAdd = 0; }

		// Add Gold
		Party.S.gold += _.goldToAdd;

		// Add EXP
		for (int i = 0; i <= Party.S.partyNdx; i++) {
			if (!Battle.S.playerDead[i]) { Party.S.stats[i].EXP += _.expToAdd; }
		}

		// Display Text
		if (_.partyQty >= 1) {
			_.dialogue.message = new List<string>() { "The party has earned " + _.expToAdd + " EXP...",
			"...and stolen " + _.goldToAdd + " GP!"};
			_.dialogue.DisplayText(_.dialogue.message);
		} else {
			if (!_.playerDead[0]) {
				_.dialogue.DisplayText(Party.S.stats[0].name + " has earned " + _.expToAdd + " EXP " + "\nand stolen " + _.goldToAdd + " GP!");
			} else if (!_.playerDead[1]) {
				_.dialogue.DisplayText(Party.S.stats[1].name + " has earned " + _.expToAdd + " EXP " + "\nand stolen " + _.goldToAdd + " GP!");
			} else if (!_.playerDead[2]) {
				_.dialogue.DisplayText(Party.S.stats[2].name + " has earned " + _.expToAdd + " EXP " + "\nand stolen " + _.goldToAdd + " GP!");
			}
		}

		// LevelUp or ReturnToWorldDelay
		Party.S.CheckForLevelUp();
		if (Party.S.stats[0].hasLeveledUp || Party.S.stats[1].hasLeveledUp || Party.S.stats[2].hasLeveledUp) {
			// Get all members that have levelled up
			for (int i = 0; i < Party.S.stats.Count; i++) {
				if (Party.S.stats[i].hasLeveledUp) {
					membersToLevelUp.Add(i);
				}
			}

			LevelUpDelay();
		} else {
			// Return to Overworld
			ReturnToWorldDelay();
		}
	}
	void LevelUpDelay() {
		// Switch Mode
		_.mode = eBattleMode.levelUp;
	}

	public List<int> membersToLevelUp = new List<int>();
	public void LevelUp(int ndx) {
		levelUpMessage.Initialize(ndx);

		// Audio: Buff 1
		AudioManager.S.PlaySFX(eSoundName.buff1);

		_.dialogue.DisplayText(
		Party.S.stats[ndx].name + " has reached level " + Party.S.stats[ndx].LVL + "!" +
			"\nHP +" + Party.S.GetHPUpgrade(ndx) +
			", MP +" + Party.S.GetMPUpgrade(ndx) + "," +
			"\nSTR +" + Party.S.GetSTRUpgrade(ndx) +
			", DEF +" + Party.S.GetDEFUpgrade(ndx) +
			", WIS +" + Party.S.GetWISUpgrade(ndx) +
			", AGI +" + Party.S.GetAGIUpgrade(ndx));
		Party.S.stats[ndx].hasLeveledUp = false;

		GameManager.S.InstantiateFloatingScore(_.playerSprite[ndx], "Lvl\nUp!", Color.green);

		// Remove member index from list
		membersToLevelUp.RemoveAt(0);

		// If there are any more members that have levelled up...
		if (membersToLevelUp.Count <= 0) {
			levelUpMessage.Initialize(ndx);

			ReturnToWorldDelay();
		}
	}

	public void ReturnToWorldDelay() {
		// Switch Mode
		_.mode = eBattleMode.returnToWorld;
	}
	public void ReturnToWorld() {
		levelUpMessage.levelUpMessageGO.SetActive(false);

		_.mode = eBattleMode.actionButtons;

		_.dialogue.dialogueNdx = 99;

		CancelInvoke(); // Cancels coroutines so they don't continue past this point

		// Activate Black Screen
		ColorScreen.S.ActivateBlackScreen();

		// Set HP to 1 for Overworld
		for (int i = 0; i <= Party.S.partyNdx; i++) {
			if (_.playerDead[i]) { Party.S.stats[i].HP = 1; }
		}

		Invoke("LoadOverworldDelay", 0.5f);
	}
	void LoadOverworldDelay() { 
		// Remove Update & Fixed Update Delegate
		UpdateManager.updateDelegate -= _.Loop;
		UpdateManager.fixedUpdateDelegate -= _.FixedLoop;

		// Reset turnNdx (prevents occasional bug that occurs when battle scene is loaded and ItemScreenOff() is called)
		_.turnNdx = 0;

		// Load Previous Scene
		GameManager.S.LoadLevel(GameManager.S.previousScene);

		// Make Player Invincible in Overworld
		Player.S.invincibility.StartInvincibility();

		// Allow Enemy to start battles w/ Player OnCollision
		Player.S.isBattling = false;
	}
}
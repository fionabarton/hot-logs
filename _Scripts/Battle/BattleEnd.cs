using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEnd : MonoBehaviour {
    [Header("Set Dynamically")]
	// Singleton
	private static BattleEnd _S;
	public static BattleEnd S { get { return _S; } set { _S = value; } }

	private Battle _;

    void Awake() {
		S = this;
	}

	void Start() {
        _ = Battle.S;
    }

	public void RemoveEnemy(int ndx) {
        // Subtract from EnemyAmount 
        _.enemyAmount -= 1;

		// Deactivate Enemy Button & Shadow
		BattlePlayerActions.S.EnemyButtonSetActive(ndx, false);

		// Set Selected GameObject (Fight Button)
		_.enemyStats[ndx].isDead = true;

		// Remove Enemy 1, 2, or 3 from Turn Order
		if (ndx == 0) { _.turnOrder.Remove(_.enemyStats[0].name); } else if (ndx == 1) { _.turnOrder.Remove(_.enemyStats[1].name); } else if (ndx == 2) { _.turnOrder.Remove(_.enemyStats[2].name); }
	}

    public void EnemyRun(int ndx) {
		// Run (50% chance)
		if (Random.value >= 0.5f) {
			RemoveEnemy(ndx);

			// Animation: Enemy RUN
			_.enemyAnimator[ndx].CrossFade("Run", 0);

			// Deactivate Enemy Shield
			_.enemyShields[ndx].SetActive(false);

			BattleDialogue.S.DisplayText(_.enemyStats[ndx].name + " ran away!\nCOWARD! HO HO HO!");

			// Subtract EXP/Gold
			_.expToAdd -= _.enemyStats[ndx].EXP;
            _.goldToAdd -= _.enemyStats[ndx].Gold;

			_.turnNdx -= 1; // Lower Turn Index

			BattleUI.S.turnCursor.SetActive(false); // Deactivate Turn Cursor
			Utilities.S.SetActiveList(BattleUI.S.targetCursors, false);

			// Randomly select DropItem
			AddDroppedItems(ndx);

			// Audio: Run
			AudioManager.S.PlaySFX(eSoundName.run);

			// Don't Deactivate Enemy in Overworld
			if (_.enemyAmount <= 0) {
				// The enemy ran away, so you're chasing after them!
				EnemyManager.S.CacheEnemyMovement(eMovement.flee);

				// DropItem or AddExpAndGold
				if (_.droppedItems.Count >= 1) {
					// Switch Mode
					_.battleMode = eBattleMode.dropItem;
				} else {
                    // Switch Mode
                    _.battleMode = eBattleMode.addExpAndGold;
				}
			} else {
				_.NextTurn();
			}
		} else {
			BattleDialogue.S.DisplayText(_.enemyStats[ndx].name + " attempts to run...\n...but the party has blocked the path!");

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);

			_.NextTurn();
		}
	}

	public void EnemyDeath(int ndx) {
		RemoveEnemy(ndx);

		// Audio: Death
		AudioManager.S.PlaySFX(eSoundName.death);

        // Animation: Enemy DEATH
        _.enemyAnimator[ndx].CrossFade("Death", 0);

		// Deactivate Enemy Shield
		_.enemyShields[ndx].SetActive(false);

		BattleUI.S.turnCursor.SetActive(false); // Deactivate Turn Cursor
		Utilities.S.SetActiveList(BattleUI.S.targetCursors, false);

		BattleDialogue.S.DisplayText(_.enemyStats[ndx].name + " has been felled!");

		// Randomly select DropItem
		AddDroppedItems(ndx);

		// Add Exp & Gold or Next Turn
		if (_.enemyAmount <= 0) {
			// Animation: Player WIN BATTLE
			for (int i = 0; i < _.playerAnimator.Count; i++) {
				if (!_.playerDead[i]) {
					_.playerAnimator[i].CrossFade("Win_Battle", 0);
                }
            }

			// DropItem or AddExpAndGold
			if (_.droppedItems.Count >= 1) {
				// Switch Mode
				_.battleMode = eBattleMode.dropItem;
			} else {
                // Switch Mode
                _.battleMode = eBattleMode.addExpAndGoldNoDrops;
			}
		} else { _.NextTurn(); }
	}

	public void AddDroppedItems(int ndx) {
		if (Random.value < _.enemyStats[ndx].chanceToDrop) {
            Inventory.S.AddItemToInventory(ItemManager.S.GetItem(_.enemyStats[ndx].itemToDrop));

            // Add to List of Items on this script
            _.droppedItems.Add(ItemManager.S.GetItem(_.enemyStats[ndx].itemToDrop));
		}
    }

	public void DropItem() { // For DisplayText, if (partyQty >= 1) && if (!playerDead [0]), else if (!playerDead [1])
		// Audio: Win
		AudioManager.S.PlaySong(eSongName.win);

		// 1 Item Dropped!
		if (_.droppedItems.Count <= 1) {
			BattleDialogue.S.DisplayText(_.enemyStats[0].name + " dropped a " + _.droppedItems[0].name + "!\n" + Party.stats[0].name + " adds it to the inventory!");
			// 2 Items Dropped!
		} else if (_.droppedItems.Count <= 2) {
			if (_.droppedItems[0] == _.droppedItems[1]) {
				BattleDialogue.S.DisplayText(_.enemyStats[0].name + " dropped two " + _.droppedItems[0].name + "s" + "!\n" + Party.stats[0].name + " adds them to the inventory!");
			} else {
				BattleDialogue.S.DisplayText(_.enemyStats[0].name + " dropped a " + _.droppedItems[0].name + " and a " + _.droppedItems[1].name + "!\n" + Party.stats[0].name + " adds them to the inventory!");
			}
			// MANY Items Dropped!
		} else {
			BattleDialogue.S.DisplayText(_.enemyStats[0].name + " dropped a ton of crap.\nCheck your inventory later to find out what!");
		}
		// Switch Mode
		_.battleMode = eBattleMode.addExpAndGold;
	}

	public void PlayerDeath(int ndx) {
		// For EnemyAttack (prevents attacking dead party members)
		_.playerDead[ndx] = true;

		// Subtract from PartyQty 
		_.partyQty -= 1;

		// Audio: Death
		AudioManager.S.PlaySFX(eSoundName.death);

		// Animation: Player DEATH
		_.playerAnimator[ndx].CrossFade("Death", 0);

		// Deactivate Player Shield
		_.playerShields[ndx].SetActive(false);

		// Remove Player 1 or 2 from Turn Order
		if (ndx == 0) { _.turnOrder.Remove(Party.stats[0].name); } else if (ndx == 1) { _.turnOrder.Remove(Party.stats[1].name); }

		BattleDialogue.S.DisplayText("Oh no!\n" + Party.stats[ndx].name + " has been felled!");

		// Add PartyDeath or NextTurn 
		// Switch Mode
		if (_.partyQty < 0) { _.battleMode = eBattleMode.partyDeath; } else { _.NextTurn(); }
	}
	public void PartyDeath() {
		BattleUI.S.turnCursor.SetActive(false);
		Utilities.S.SetActiveList(BattleUI.S.targetCursors, false);

		// You died, so the enemy is chasing after you!
		EnemyManager.S.CacheEnemyMovement(eMovement.pursueRun);

		BattleDialogue.S.DisplayText("Failure is the party!\nY'all've been wiped out/felled!");

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
		if(Party.S.partyNdx >= 1) {
			if (!Battle.S.playerDead[0]) { Party.stats[0].EXP += _.expToAdd; }
			if (!Battle.S.playerDead[1]) { Party.stats[1].EXP += _.expToAdd; }
		} else {
			Party.stats[0].EXP += _.expToAdd;
		}

		// Display Text
		if (_.partyQty >= 1) {
			BattleDialogue.S.message = new List<string>() { "The party has earned " + _.expToAdd + " EXP...",
			"...and stolen " + _.goldToAdd + " GP!"};
			BattleDialogue.S.DisplayText(BattleDialogue.S.message);
		} else {
			if (!_.playerDead[0]) {
				BattleDialogue.S.DisplayText(Party.stats[0].name + " has earned " + _.expToAdd + " EXP " + "\nand stolen " + _.goldToAdd + " GP!");
			} else if (!_.playerDead[1]) {
				BattleDialogue.S.DisplayText(Party.stats[1].name + " has earned " + _.expToAdd + " EXP " + "\nand stolen " + _.goldToAdd + " GP!");
			}
		}

		// LevelUp or ReturnToWorldDelay
		Party.S.CheckForLevelUp();
		if (Party.stats[0].hasLeveledUp || Party.stats[1].hasLeveledUp) {
			LevelUpDelay();
		} else {
			// Return to Overworld
			ReturnToWorldDelay();
		}
	}
	void LevelUpDelay() {
		// Switch Mode
		_.battleMode = eBattleMode.levelUp;
	}
	public void LevelUp() {
		// Display Text
		if (Party.stats[0].hasLeveledUp && Party.stats[1].hasLeveledUp) {
			BattleDialogue.S.DisplayText(Party.stats[0].name + " level up!" + "\nHP = " + Party.stats[0].HP + ", MP = " + Party.stats[0].MP + "," + "\nSTR = " + Party.stats[0].STR + ", DEF = " + Party.stats[0].DEF + ", WIS = " + Party.stats[0].WIS + ", AGI = " + Party.stats[0].AGI);
			Party.stats[0].hasLeveledUp = false;

			_.battleMode = eBattleMode.multiLvlUp;

		} else if (Party.stats[0].hasLeveledUp) {
			BattleDialogue.S.DisplayText(Party.stats[0].name + " level up!" + "\nHP = " + Party.stats[0].HP + ", MP = " + Party.stats[0].MP + "," + "\nSTR = " + Party.stats[0].STR + ", DEF = " + Party.stats[0].DEF + ", WIS = " + Party.stats[0].WIS + ", AGI = " + Party.stats[0].AGI);
			Party.stats[0].hasLeveledUp = false;

			ReturnToWorldDelay();
		} else if (Party.stats[1].hasLeveledUp) {
			BattleDialogue.S.DisplayText(Party.stats[1].name + " level up!" + "\nHP = " + Party.stats[1].HP + ", MP = " + Party.stats[1].MP + "," + "\nSTR = " + Party.stats[1].STR + ", DEF = " + Party.stats[1].DEF + ", WIS = " + Party.stats[1].WIS + ", AGI = " + Party.stats[1].AGI);
			Party.stats[1].hasLeveledUp = false;

			ReturnToWorldDelay();
		}
	}
	public void MultiLvlUp() {
		BattleDialogue.S.DisplayText(Party.stats[1].name + " level up!" + "\nHP = " + Party.stats[1].HP + ", MP = " + Party.stats[1].MP + "," + "\nSTR = " + Party.stats[1].STR + ", DEF = " + Party.stats[1].DEF + ", WIS = " + Party.stats[1].WIS + ", AGI = " + Party.stats[1].AGI);
		Party.stats[1].hasLeveledUp = false;

		// Return to Overworld
		ReturnToWorldDelay();
	}

	public void ReturnToWorldDelay() {
		// Switch Mode
		_.battleMode = eBattleMode.returnToWorld;
	}
	public void ReturnToWorld() {
		_.battleMode = eBattleMode.actionButtons;

		BattleDialogue.S.dialogueNdx = 99;

		CancelInvoke(); // Cancels coroutines so they don't continue past this point

		// Enable Black Screen
		RPG.S.blackScreen.enabled = true;

		// Set HP to 1 for Overworld
		if (_.playerDead[0]) { Party.stats[0].HP = 1; }
		if (Party.S.partyNdx >= 1) { if (_.playerDead[1]) { Party.stats[1].HP = 1; } }

		Invoke("LoadOverworldDelay", 0.5f);
	}
	void LoadOverworldDelay() { 
		// Remove Update & Fixed Update Delegate
		UpdateManager.updateDelegate -= _.Loop;
		UpdateManager.fixedUpdateDelegate -= _.FixedLoop;

		// Reset turnNdx (prevents occasional bug that occurs when battle scene is loaded and ItemScreenOff() is called)
		_.turnNdx = 0;

		// Load Previous Scene
		RPG.S.LoadLevel(RPG.S.previousSceneName);

		// Make Player Invincible in Overworld
		Player.S.invincibility.StartInvincibility();

		// Allow Enemy to start battles w/ Player OnCollision
		Player.S.isBattling = false;
	}
}
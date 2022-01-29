﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InnkeeperTrigger : ActivateOnButtonPress {
	protected override void Action() {
		// Set Camera to Innkeeper gameObject
		CamManager.S.ChangeTarget(gameObject, true);

		// Set SubMenu Text
		RPG.S.gameSubMenu.SetText("Yes", "No");

		DialogueManager.S.DisplayText("<color=yellow><Inn Keeper></color> Rooms are 10 gold a night. Restores your HP & MP. Would you like to spend the night?");

		// Activate Sub Menu after Dialogue 
		DialogueManager.S.activateSubMenu = true;
		// Don't activate Text Box Cursor 
		DialogueManager.S.dontActivateCursor = true;
		// Gray Out Text Box after Dialogue 
		DialogueManager.S.grayOutTextBox = true;

		// Set OnClick Methods
		Utilities.S.RemoveListeners(RPG.S.gameSubMenu.buttonCS);
		RPG.S.gameSubMenu.buttonCS[0].onClick.AddListener(Yes);
        RPG.S.gameSubMenu.buttonCS[1].onClick.AddListener(No);
		//RPG.S.gameSubMenu.subMenuButtonCS[2].onClick.AddListener(Option2);
		//RPG.S.gameSubMenu.subMenuButtonCS[3].onClick.AddListener(Option3);

		// Set button navigation
		Utilities.S.SetButtonNavigation(RPG.S.gameSubMenu.buttonCS[0], RPG.S.gameSubMenu.buttonCS[1], RPG.S.gameSubMenu.buttonCS[1]);
		Utilities.S.SetButtonNavigation(RPG.S.gameSubMenu.buttonCS[1], RPG.S.gameSubMenu.buttonCS[0], RPG.S.gameSubMenu.buttonCS[0]);
	}

	public void Yes() {
		AudioManager.S.PlaySFX(eSoundName.confirm);

		int price = 10;

		if (Party.S.gold >= price) {
			// Subtract item price from Player's Gold
			Party.S.gold -= price;

			// Max HP/MP
			Party.S.stats[0].HP = Party.S.stats[0].maxHP;
			Party.S.stats[0].MP = Party.S.stats[0].maxMP;
			Party.S.stats[1].HP = Party.S.stats[1].maxHP;
			Party.S.stats[1].MP = Party.S.stats[1].maxMP;

			StartCoroutine("CloseCurtains");
		} else {
			// Display Text: Not enough Gold
			DialogueManager.S.DisplayText("Begone with you, penniless fool! Waste not my worthless time!"); 
		}
	}

	// 1) Freeze input, close curtains
	IEnumerator CloseCurtains() {
		// Deactivate player input
		RPG.S.paused = true;

		BattleCurtain.S.Close();

		// Audio: Win
		StartCoroutine(AudioManager.S.PlaySongThenResumePreviousSong(6));

		yield return new WaitForSeconds(1.5f);
		StartCoroutine("OpenCurtains");
	}
	// 2) Open curtains
	IEnumerator OpenCurtains() {
		DialogueManager.S.DeactivateTextBox();

		BattleCurtain.S.Open();

		yield return new WaitForSeconds(0.75f);
		DisplayDialogue();
	}
	// 3) Dialogue, unfreeze input
	void DisplayDialogue() {
		// Audio: Buff 1
		AudioManager.S.PlaySFX(eSoundName.buff1);

		// Reactivate player input
		RPG.S.paused = false;

		DialogueManager.S.ResetSettings();
		DialogueManager.S.DisplayText("Health and magic restored. Bless your heart, babe!");
	}

	public void No() {
		AudioManager.S.PlaySFX(eSoundName.deny);

		DialogueManager.S.ResetSettings();
		DialogueManager.S.DisplayText("That's cool. Later, bro.");
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InnkeeperTrigger : ActivateOnButtonPress {

	protected override void Action() {
		// Set Camera to Innkeeper gameObject
		CamManager.S.ChangeTarget(gameObject, true);

		// Set SubMenu Text
		SubMenu.S.SetText("Yes!", "No...");

		DialogueManager.S.DisplayText("<color=yellow><Inn Keeper></color> Rooms are 10 gold a night. Restores your HP & MP. Would you like to spend the night?");

		// Activate Sub Menu after Dialogue 
		DialogueManager.S.activateSubMenu = true;
		// Don't activate Text Box Cursor 
		DialogueManager.S.dontActivateCursor = true;
		// Gray Out Text Box after Dialogue 
		DialogueManager.S.grayOutTextBox = true;

		// Set OnClick Methods
		Utilities.S.RemoveListeners(SubMenu.S.buttonCS);
		SubMenu.S.buttonCS[0].onClick.AddListener(Yes);
        SubMenu.S.buttonCS[1].onClick.AddListener(No);
        //SubMenu.S.subMenuButtonCS[2].onClick.AddListener(Option2);
        //SubMenu.S.subMenuButtonCS[3].onClick.AddListener(Option3);
    }

	public void Yes() {
		DialogueManager.S.ResetSubMenuSettings();

		int price = 10;

		if (Party.S.Gold >= price) {
			// Subtract item price from Player's Gold
			Party.S.Gold -= price;

			// Max HP/MP
			Party.stats[0].HP = Party.stats[0].maxHP;
			Party.stats[0].MP = Party.stats[0].maxMP;
			Party.stats[1].HP = Party.stats[1].maxHP;
			Party.stats[1].MP = Party.stats[1].maxMP;

			// Display Text: HP/MP Restored
			DialogueManager.S.DisplayText("Health and magic restored. Bless your heart, babe!");
		} else {
			// Display Text: Not enough Gold
			DialogueManager.S.DisplayText("Begone with you, penniless fool! Waste not my worthless time!"); 
		}
	}

	public void No() {
		DialogueManager.S.ResetSubMenuSettings();
		DialogueManager.S.DisplayText("That's cool. Later, bro.");
	}
}
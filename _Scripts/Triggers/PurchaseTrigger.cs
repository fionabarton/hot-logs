using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurchaseTrigger : ActivateOnButtonPress {
    [Header("Set in Inspector")]
    public eItem item;

    protected override void Action() {
        // Set Camera to Item gameObject
        CamManager.S.ChangeTarget(gameObject, true);

        DialogueManager.S.DisplayText("I'm a " + ItemManager.S.items[(int)item].name + 
                                         "! Wanna purchase me for " + ItemManager.S.items[(int)item].value + 
                                         " gold?");

        // Set SubMenu Text
        SubMenu.S.SetText("Yes", "No");

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
    }

    void Yes() {
        DialogueManager.S.ResetSubMenuSettings();

        Item tItem = ItemManager.S.items[(int)item];

        if (Party.S.Gold >= tItem.value) {
            // Added to Player Inventory
            Inventory.S.AddItemToInventory(tItem);

            DialogueManager.S.DisplayText("Yahoo! Thank you for purchasing me!");

            // Subtract item price from Player's Gold
            Party.S.Gold -= tItem.value;
        } else {
            DialogueManager.S.DisplayText("You ain't got enough money, jerk!");
        }
    }

    void No() {
        DialogueManager.S.ResetSubMenuSettings();
        DialogueManager.S.DisplayText("That's cool. Later, bro.");
    }
}

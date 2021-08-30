using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// If there is a child gameObject that also has a collider (ex. Solid Collider):
/// Make sure that the collider has a RigidBody set to Kinematic to prevent it from
/// triggering something unintentionally
/// </summary>
public class ShopkeeperTrigger : ActivateOnButtonPress {
	[Header("Set in Inspector")]
	// Shop Inventory
	public List<eItem> sellerInventory; 

	int mode = 0; // 0: Nothing, 1: Buy, 2: Sell

    protected override void Action() {
        // Set Camera to Shopkeeper gameObject
        CamManager.S.ChangeTarget(gameObject, true);

        DialogueManager.S.DisplayText("<color=yellow><Shop Keeper></color> Wanna buy some hot junk? Or maybe sell some hot junk?");

		// Set SubMenu Text
		SubMenu.S.SetText("Buy junk!", "Sell junk!", "No thanks.", "", 3);

        ShopScreen.S.ImportInventory(sellerInventory);

		// Activate Sub Menu after Dialogue 
		DialogueManager.S.activateSubMenu = true;
		// Don't activate Text Box Cursor 
		DialogueManager.S.dontActivateCursor = true;
		// Gray Out Text Box after Dialogue 
		DialogueManager.S.grayOutTextBox = true;

		// Set OnClick Methods
        Utilities.S.RemoveListeners(SubMenu.S.buttonCS);
        SubMenu.S.buttonCS[0].onClick.AddListener(Buy);
        SubMenu.S.buttonCS[1].onClick.AddListener(Sell);
        SubMenu.S.buttonCS[2].onClick.AddListener(No);
        //SubMenu.S.subMenuButtonCS[3].onClick.AddListener(Option3);
    }

	void Buy() {
		DialogueManager.S.ResetSubMenuSettings();
        DialogueManager.S.DisplayText("That so fab. What you wanna buy?");

        // Allow input to deactivate text and open shop screen
        mode = 1;
    }

	void Sell() {
		DialogueManager.S.ResetSubMenuSettings();
        DialogueManager.S.DisplayText("How grand! What you wanna to sell?");

        // Sell Mode
        ItemScreen.S.useOrSellMode = eUseOrSellMode.sellMode;

        // Allow input to deactivate text and open item screen
        mode = 2;
    }

	void No() {
		DialogueManager.S.ResetSubMenuSettings();
		DialogueManager.S.DisplayText("That coo. Come again, little stinker.");
	}

	public void ThisLoop() {
        // Would prefer better solution... 
        // This prevents an occasional bug when the Player is within this trigger on scene change
        // by removing ThisLoop from updateDelegate on scene change
        if (!RPG.S.canInput) {
            UpdateManager.updateDelegate -= ThisLoop;
        }

        if (Input.GetButtonDown("SNES A Button")) {
            // Activate Shop Screen
            if (DialogueManager.S.dialogueFinished) {
                switch (mode) {
                    case 1:
                        // Activate Shop Screen
                        ScreenManager.S.ShopScreenOn();

                        DialogueManager.S.DeactivateTextBox();
                        break;
                    case 2:
                        // Sell Mode
                        ItemScreen.S.useOrSellMode = eUseOrSellMode.sellMode;

                        // Activate Item Screen
                        ScreenManager.S.ItemScreenOn();

                        DialogueManager.S.DeactivateTextBox(false);
                        break;
                }
            }
        }   
	}

	protected override void OnTriggerEnter2D(Collider2D coll) {
        if (coll.gameObject.CompareTag("PlayerTrigger")) {
            // Update Delgate
            UpdateManager.updateDelegate += ThisLoop;

            base.OnTriggerEnter2D(coll);
        }
    }

	protected override void OnTriggerExit2D(Collider2D coll) {
		if (coll.gameObject.CompareTag("PlayerTrigger")) {
            base.OnTriggerExit2D(coll);
            
            // Update Delgate
            UpdateManager.updateDelegate -= ThisLoop;

            // Reset ability to input
            mode = 0;
        }
    }
}

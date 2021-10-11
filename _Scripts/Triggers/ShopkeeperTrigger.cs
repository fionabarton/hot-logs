using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eShopkeeperMode { no, buy, sell };

/// <summary>
/// If there is a child gameObject that also has a collider (ex. Solid Collider):
/// Make sure that the collider has a RigidBody set to Kinematic to prevent it from
/// triggering something unintentionally
/// </summary>
public class ShopkeeperTrigger : ActivateOnButtonPress {
	[Header("Set in Inspector")]
	// Shop Inventory
	public List<eItem>       itemsToPopulateInventory;

    public eShopkeeperMode   mode = eShopkeeperMode.no; // 0: Nothing, 1: Buy, 2: Sell

    [Header("Set Dynamically")]
    public List<Item>        inventory = new List<Item>();

    private void Start() {
        // Convert eItem enumeration into Item
        inventory.Clear();
        for (int i = 0; i < itemsToPopulateInventory.Count; i++) {
            inventory.Add(ItemManager.S.GetItem(itemsToPopulateInventory[i]));
        }
    }

    protected override void Action() {
        // Set Camera to Shopkeeper gameObject
        CamManager.S.ChangeTarget(gameObject, true);

        // Set Text
        DialogueManager.S.DisplayText("<color=yellow><Shop Keeper></color> Wanna buy some hot junk? Or maybe sell some hot junk?");
		SubMenu.S.SetText("Buy junk!", "Sell junk!", "No thanks.", "", 3);

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

        // Set button navigation
        Utilities.S.SetButtonNavigation(SubMenu.S.buttonCS[0], SubMenu.S.buttonCS[1], SubMenu.S.buttonCS[2]);
        Utilities.S.SetButtonNavigation(SubMenu.S.buttonCS[1], SubMenu.S.buttonCS[2], SubMenu.S.buttonCS[0]);
        Utilities.S.SetButtonNavigation(SubMenu.S.buttonCS[2], SubMenu.S.buttonCS[0], SubMenu.S.buttonCS[1]);
    }

	void Buy() {
        // Audio: Confirm
        AudioManager.S.PlaySFX(eSoundName.confirm);

        DialogueManager.S.ResetSubMenuSettings();
        DialogueManager.S.DisplayText("That so fab. What you wanna buy?");
        mode = eShopkeeperMode.buy;
    }

	void Sell() {
        // Audio: Confirm
        AudioManager.S.PlaySFX(eSoundName.confirm);

        DialogueManager.S.ResetSubMenuSettings();
        DialogueManager.S.DisplayText("How grand! What you wanna to sell?");
        mode = eShopkeeperMode.sell;
    }

	void No() {
        // Audio: Deny
        AudioManager.S.PlaySFX(eSoundName.deny);

        DialogueManager.S.ResetSubMenuSettings();
		DialogueManager.S.DisplayText("That coo. Come again, little stinker.");
	}

	public void ThisLoop() {
        // Remove ThisLoop() from UpdateManager delegate on scene change.
        // This prevents an occasional bug when the Player is within this trigger on scene change.
        // Would prefer a better solution... 
        if (!RPG.S.canInput) {
            UpdateManager.updateDelegate -= ThisLoop;
        }

        if (Input.GetButtonDown("SNES A Button")) {
            // Activate Shop Screen
            if (DialogueManager.S.dialogueFinished) {
                if (mode != eShopkeeperMode.no) {
                    if (mode == eShopkeeperMode.buy) {
                        // Import shopkeeper inventory
                        ShopScreen.S.ImportInventory(inventory);
                        ShopScreen.S.buyOrSellMode = true;
                    } else if (mode == eShopkeeperMode.sell) {
                        // Import party inventory
                        ShopScreen.S.ImportInventory(Inventory.S.GetItemList());
                        ShopScreen.S.buyOrSellMode = false;
                    }

                    // Activate Shop Screen
                    ShopScreen.S.gameObject.SetActive(true);

                    DialogueManager.S.DeactivateTextBox();

                    // Subscribe ResetTrigger() to the OnShopScreenDeactivated event
                    EventManager.OnShopScreenDeactivated += ResetTrigger;

                    // Reset ability to input
                    mode = 0;
                }
            }
        }   
	}

	protected override void OnTriggerEnter2D(Collider2D coll) {
        if (coll.gameObject.CompareTag("PlayerTrigger")) {
            // Add ThisLoop() to Update Delgate
            UpdateManager.updateDelegate += ThisLoop;

            base.OnTriggerEnter2D(coll);
        }
    }

	protected override void OnTriggerExit2D(Collider2D coll) {
		if (coll.gameObject.CompareTag("PlayerTrigger")) {
            base.OnTriggerExit2D(coll);

            // Remove ThisLoop() from Update Delgate
            UpdateManager.updateDelegate -= ThisLoop;

            // Unsubscribe ResetTrigger() from the OnShopScreenDeactivated event
            EventManager.OnShopScreenDeactivated -= ResetTrigger;
        }
    }
}
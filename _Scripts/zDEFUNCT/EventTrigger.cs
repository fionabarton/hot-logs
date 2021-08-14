using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eEventAction {
    killEnemy, freezeCam, unFreezeCam, timeBuffer, displayText, freezePlayer, unfreezePlayer, switchBossMode, clampPlayer, unClampPlayer,
    startBattle, subMenu, innKeeper, hpPotionPurchase, mpPotionPurchase, sellItems, shopKeep, buyItems, playArcadeGame, changeCameraTarget, npcNdx
};
public class EventTrigger : MonoBehaviour {
	[Header("Set in Inspector")]
	public List<eEventAction>	eventActions = new List<eEventAction> ();
	public List<eEventAction>	option0Actions = new List<eEventAction> ();
	public List<eEventAction>	option1Actions = new List<eEventAction> ();
	public List<eEventAction>	option2Actions = new List<eEventAction> ();
	public List<eEventAction>	option3Actions = new List<eEventAction> ();

	// Display Text
	public List<string>	 		messages0;
	public List<string>	 		messages1;
	public List<string> 		option0Message;
	public List<string> 		option1Message;
	public List<string> 		option2Message;
	public List<string> 		option3Message;

	// Sub Menu 
	public List<string>	 		subMenuMessage0;

	// Sub Menu Choices/Options
	public List<string>			subMenuOption0;
	public List<string>			subMenuOption1;
	public List<string>			subMenuOption2;
	public List<string>			subMenuOption3;

	// Amount of Sub Menu Options
	public List<int>			optionAmount;

	// Purchase Item
	public List<string>			purchasedMessage;

	// Camera Position
	public Vector3 				camPos;

	// Battle
	//public Enemy				eStats;
	public List<EnemyStats>			enemyStats;

	// Shop Inventory
	public List<string> 	 	sellerInventory; // Health Potion, Magic Potion, Shit Sword

	[Header("Set Dynamically")]
	public int					eventNdx;
	// Display Text
	public int					messageNdx;
	// For Display Text
	public eEventAction			currentEvent;

	// Event Path (Default, option0, option1, etc.)
	public string 				eventPath = "default";

	public List<string>			dynamicMessage;

	// Prevents calling OnTriggerEnter2D more than once
	//public bool					alreadyTriggered; 

	public bool 				triggerActive; // Prevents constantly calling Update()
	public bool					activeOnColl;

	void OnDisable() {
		// Remove Delgate
		UpdateManager.updateDelegate -= Loop;
	}

	public void Loop () {
        if (!triggerActive){
			// Activate on Button Press
			if (!Player.S.alreadyTriggered) {
				if (!RPG.S.paused) {
					if (!activeOnColl) {
						if (Input.GetButtonDown("SNES A Button")) {
							// Interactable Trigger
							InteractableCursor.S.Activate(false);

							InitializeEvents();
						}
					}
				}
			}
        } else {
			// Deactivate Text Box or Clear for Next Line
			switch (currentEvent) {
				case eEventAction.displayText:
				case eEventAction.mpPotionPurchase:
				case eEventAction.hpPotionPurchase:
				if (!RPG.S.paused) {
					if (Input.GetButtonDown("SNES A Button")) {
						if (DialogueManager.S.dialogueFinished && DialogueManager.S.ndx <= 0) {
							DialogueManager.S.DeactivateTextBox();

							messageNdx += 1;

							CallNextEvent();

						// For Multiple Lines
						} else if (DialogueManager.S.dialogueFinished && DialogueManager.S.ndx > 0) {
							// Reset Text & Cursor
							DialogueManager.S.ClearForNextLine();

							List<string> testList = new List<string>();

							switch (eventPath) {
								case "default":
									switch (messageNdx) {
										case 0:
											testList = messages0;
											break;
										case 1:
											testList = messages1;
											break;
									}
									break;
								case "option0":
									testList = option0Message;
									break;
								case "option1":
									testList = option1Message;
									break;
								case "option2":
									testList = option2Message;
									break;
								case "option3":
									testList = option3Message;
									break;
								case "dynamic":
									testList = dynamicMessage;
									break;
							}

							testList.RemoveAt(0);

							// Call DisplayText() with one less line of "messages"
							DialogueManager.S.DisplayText(testList);
						}
					}
				}
				break;
			}
		}
	}

    void InitializeEvents(){
		Player.S.alreadyTriggered = true;
		triggerActive = true;

		// Start Event Sequence
		LaunchEvent(eventActions[eventNdx]);
	}

    // Activate on Collision
    void OnTriggerEnter2D(Collider2D coll){
		if (!Player.S.alreadyTriggered) {
			if (!RPG.S.paused) {
				if (coll.gameObject.CompareTag("PlayerTrigger")) {
					// Player RigidBody
					Player.S.rigid.sleepMode = RigidbodySleepMode2D.NeverSleep;

					// Interactable Trigger
					InteractableCursor.S.Activate(true, gameObject);

					// Update Delgate
					UpdateManager.updateDelegate += Loop;

					if (activeOnColl) {
					    InitializeEvents();
					}
				}
			}
		}
	}

	void OnTriggerExit2D(Collider2D coll){
		// Player RigidBody
		if (coll.gameObject.CompareTag("PlayerTrigger")) {
			Player.S.rigid.sleepMode = RigidbodySleepMode2D.StartAwake;

			triggerActive = false;

			// Update Delgate
			UpdateManager.updateDelegate -= Loop;

			// Interactable Trigger
			InteractableCursor.S.Activate(false);
		}
	}

	void LaunchEvent (eEventAction eventAction) {
		currentEvent = eventAction;
		Debug.Log ("CURRENT EVENT: " + eventAction.ToString ());
		eventNdx += 1;

		switch (eventAction) {
		case eEventAction.freezeCam: FreezeCam (); break;
		case eEventAction.unFreezeCam: UnfreezeCam (); break;
		case eEventAction.changeCameraTarget: ChangeCameraTarget(); break;

		case eEventAction.timeBuffer: TimeBufferDelay (); break;
		case eEventAction.displayText: DisplayText (); break;

		case eEventAction.freezePlayer: FreezePlayer (); break;
		case eEventAction.unfreezePlayer: UnfreezePlayer (); break;

		case eEventAction.startBattle: StartBattle (); break;

		case eEventAction.subMenu: EnableSubMenu(); break;
		//case eEventAction.innKeeper: InnKeeper (); break;
		//case eEventAction.hpPotionPurchase: HPPurchase (); break;
		//case eEventAction.mpPotionPurchase: MPPotionPurchase (); break;
		
		//case eEventAction.sellItems: SellItems (); break;
		//case eEventAction.shopKeep: ShopKeep (); break;
		//case eEventAction.buyItems: BuyItems (); break;

		//case eEventAction.playArcadeGame: PlayArcadeGame(); break;
		}
	}

	//////////////////////////////// CALL NEXT EVENT ////////////////////////////////
	public void CallNextEvent(){
		switch (eventPath) { 
		case "default":
			if (eventNdx < eventActions.Count) {
				LaunchEvent (eventActions [eventNdx]);
			} else {
				ResetSettings ();
			}
			break;
		case "option1":
			if (eventNdx < option1Actions.Count) {
				LaunchEvent (option1Actions [eventNdx]);
			} else {
				ResetSettings ();
			}
			break;
		case "option2":
			if (eventNdx < option2Actions.Count) {
				LaunchEvent (option2Actions [eventNdx]);
			} else {
				ResetSettings ();
			}
			break;
		case "option3":
			if (eventNdx < option3Actions.Count) {
				LaunchEvent (option3Actions [eventNdx]);
			} else {
				ResetSettings ();
			}
			break;
		case "option0":
		case "dynamic":
			if (eventNdx < option0Actions.Count) {
				LaunchEvent (option0Actions [eventNdx]);
			} else {
				ResetSettings ();
			}
			break;
		}
	}

	void ResetSettings(){
        //"Resets" currentEvent to default to prevent ArgumentOutOfRangeException
        currentEvent = eEventAction.killEnemy; 

		eventNdx = 0;
		messageNdx = 0;
		eventPath = "default";

		//RPGPlayer.S.alreadyTriggered = false;
		triggerActive = false;
	}

	//// Shop Keeper	*** MUST call DisplayText as an EventAction afterwards, otherwise DynamicMessage will be set, but won't appear ***
	//public void ShopKeep(){
	//	SubMenu.S.SetText (subMenuOption0[0], subMenuOption1[0], subMenuOption2[0], subMenuOption3[0], optionAmount[0]);

	//	ShopScreen.S.ImportInventory (sellerInventory);

	//	RPGDialogueManager.S.DisplayText (subMenuMessage0);

	//	// Activate Sub Menu after Dialogue 
	//	RPGDialogueManager.S.activateSubMenu = true;
	//	// Don't activate Text Box Cursor 
	//	RPGDialogueManager.S.dontActivateCursor = true;
	//	// Gray Out Text Box after Dialogue 
	//	RPGDialogueManager.S.grayOutTextBox = true;

	//	// Set OnClick Methods
	//	for (int i = 0; i <= SubMenu.S.subMenuButtonCS.Count - 1; i++) {
	//		SubMenu.S.subMenuButtonCS [i].onClick.RemoveAllListeners ();
	//	}
	//	SubMenu.S.subMenuButtonCS[0].onClick.AddListener (Option0);
	//	SubMenu.S.subMenuButtonCS[1].onClick.AddListener (Option1);
	//	SubMenu.S.subMenuButtonCS[2].onClick.AddListener (Option2);
	//	SubMenu.S.subMenuButtonCS[3].onClick.AddListener (Option3);
	//}

	//// Buy Item 
	//public void BuyItems(){
	//	// Activate Shop Screen
	//	ScreenManager.S.ShopScreenOn ();

	//	CallNextEvent ();
	//}

	//// Sell Item 
	//public void SellItems(){
	//	// Sell Mode
	//	ItemScreen.S.useOrSellMode = eUseOrSellMode.sellMode;

	//	// Activate Item Screen
	//	ScreenManager.S.ItemScreenOn ();

	//	CallNextEvent ();
	//}

	//// Inn Keeper 
	//public void InnKeeper () {
	//	int price = 10;

	//	if (Stats.S.Gold >= price) {
	//		// Subtract item price from Player's Gold
	//		Stats.S.Gold -= price;

	//		// Max HP/MP
	//		Stats.S.HP[0] = Stats.S.maxHP[0];
	//		Stats.S.MP[0] = Stats.S.maxMP[0];
	//		Stats.S.HP[1] = Stats.S.maxHP[1];
	//		Stats.S.MP[1] = Stats.S.maxMP[1];

	//		// Display Text: HP/MP Restored
	//		DynamicMessage("Health and magic restored. Bless your heart, babe!"); // Currently only works w/ YES EventPath
	//	} else {
	//		// Display Text: Not enough Gold
	//		DynamicMessage("Begone with you, penniless fool! Waste not my worthless time!"); // Currently only works w/ YES EventPath
	//	}
	//	CallNextEvent ();
	//}

	//// HP Potion Purchase 
	//void HPPurchase () {
	//	PurchaseItem (ItemManager.S._items[0]);
	//	CallNextEvent ();
	//}
	//// MP Potion Purchase 
	//void MPPotionPurchase () {
	//	PurchaseItem (ItemManager.S._items[1]);
	//	CallNextEvent ();
	//}
	//// Purchase Item
	//// *** MUST call DisplayText as an EventAction afterwards, 
	//// otherwise DynamicMessage will be set, but won't appear ***
	//void PurchaseItem (Item item) {
	//	if (Stats.S.Gold >= item.itemValue) {	
	//		// Added to Player Inventory
	//		ItemScreen.S.AddItem (item);

	//		// Set Dynamic Message
	//		DynamicMessage(purchasedMessage[0]);

	//		// Subtract item price from Player's Gold
	//		Stats.S.Gold -= item.itemValue;
	//	} else {
	//		// Set Dynamic Message
	//		DynamicMessage("Not enough money!"); // Currently only works w/ YES EventPath
	//	}
	//}

	//////////////////// DIALOGUE EVENTS ////////////////////

	// Display Text 
	public void DisplayText () {
		switch (eventPath) { 
		case "default":
			switch (messageNdx) {
			case 0: DialogueManager.S.DisplayText (messages0); break;
			case 1: DialogueManager.S.DisplayText (messages1); break;
			}
			break;
		case "option0": DialogueManager.S.DisplayText (option0Message); break;
		case "option1": DialogueManager.S.DisplayText (option1Message); break;
		case "option2": DialogueManager.S.DisplayText (option2Message); break;
		case "option3": DialogueManager.S.DisplayText (option3Message); break;
		case "dynamic": DialogueManager.S.DisplayText (dynamicMessage); break; 
		}
	}

	// Sub Menu 
	public void EnableSubMenu () {

		// Freeze Player
		Player.S.canMove = false;

		// Set SubMenu Text
		SubMenu.S.SetText (subMenuOption0[0], subMenuOption1[0], subMenuOption2[0], subMenuOption3[0], optionAmount[0]);

		DialogueManager.S.DisplayText (subMenuMessage0);

		// Activate Sub Menu after Dialogue 
		DialogueManager.S.activateSubMenu = true;
		// Don't activate Text Box Cursor 
		DialogueManager.S.dontActivateCursor = true;
		// Gray Out Text Box after Dialogue 
		DialogueManager.S.grayOutTextBox = true;

		// Set OnClick Methods
		for (int i = 0; i <= SubMenu.S.subMenuButtonCS.Count - 1; i++) {
			SubMenu.S.subMenuButtonCS [i].onClick.RemoveAllListeners ();
		}
		SubMenu.S.subMenuButtonCS[0].onClick.AddListener (Option0);
		SubMenu.S.subMenuButtonCS[1].onClick.AddListener (Option1);
		SubMenu.S.subMenuButtonCS[2].onClick.AddListener (Option2);
		SubMenu.S.subMenuButtonCS[3].onClick.AddListener (Option3);
	}

	public void Option0 () {
		eventPath = "option0";
		OptionHelper ();
	}
	public void Option1 () {
		eventPath = "option1";
		OptionHelper ();
	}
	public void Option2 () {
		eventPath = "option2";
		OptionHelper ();
	}
	public void Option3 () {
		eventPath = "option3";
		OptionHelper ();
	}
	public void OptionHelper(){
		DialogueManager.S.ResetSubMenuSettings ();

		// Deactivate Text
		DialogueManager.S.DeactivateTextBox (false);

		eventNdx = 0;
		messageNdx = 0;

		CallNextEvent ();
	}

	// Freeze Camera 
	public void FreezeCam () {
		// Set Freeze Camera Position
		CamManager.S.camMode = eCamMode.freezeCam;
		CamManager.S.transform.position = camPos;

		CallNextEvent ();
	}
	// Unfreeze Camera 
	public void UnfreezeCam(){
		// Camera Follows Player
		CamManager.S.camMode = eCamMode.followAll;
		CallNextEvent ();
	}
    // Change Cam Target to THIS gameObject
    // TBR: set target to other gameObject
    public void ChangeCameraTarget()
    {
		CamManager.S.ChangeTarget(gameObject, true);
		CallNextEvent();
	}

	// Time Buffer 
	public void TimeBufferDelay(){
		Invoke ("TimeBuffer", 2);
	}
	public void TimeBuffer () {
		CallNextEvent ();
	}

	// Freeze Player (BEWARE: DialogueManCS.DeactivateTextBox() unfreezes Player)
	public void FreezePlayer () {
		Player.S.canMove = false;
		Player.S.mode = eRPGMode.idle;
		CallNextEvent ();
	}
	// Unfreeze Player 
	public void UnfreezePlayer(){
		Player.S.canMove = true;
		CallNextEvent ();
	}

	// Start Battle 
	public void StartBattle () {
        if (enemyStats != null) {
			RPG.S.StartBattle(enemyStats);
		} else {
            Debug.LogWarning("EnemyStats not assigned in Inspector!");
        }
    }

	// Switchess EventPath (Ex. enough/not enough $) (Currently only works w/ YES EventPath)
	void DynamicMessage(string messageToDisplay){ 
		// Clear Message
		dynamicMessage.Clear();
		// Add Message to List
		dynamicMessage.Add (messageToDisplay);
		// Switch Event Path to display the Message
		eventPath = "dynamic";
	}

	//   void PlayArcadeGame()
	//   {
	//       // Get ArcadeCab.cs
	//	ArcadeCab tArcade = GetComponent<ArcadeCab>();
	//	// Get Animator
	//	Animator anim = GetComponent<Animator>();

	//       if (tArcade){
	//		tArcade.LoadGame();

	//           if (anim){
	//			anim.Play("TV_Static");
	//           }
	//       }else {
	//		Debug.LogWarning("NO ArcadeCab script attached to GameObject!!!");
	//       }

	//	CallNextEvent();
	//}

	/*
	//////////////////////////////// TBR ////////////////////////////////
	/////////////////////////////////////////////////////////////////////
	
	//////////////////////////////// ITEMS ////////////////////////////////
	void GiveItem (Item item) {
		// Added to Player Inventory
		ItemScreen.S.AddItem (item);
	}

    //////////////////////////////// Camera ////////////////////////////////
    // MOVE MANUALLY
		// Set Speed
		// Set Direction to move
		// Set Distance to move

    // FOLLOW GAMEOBJECT
        // Enable CanFollow, and set Target
		// Hone in on GameObject

    //////////////////////////////// QUESTS ////////////////////////////////
    // Activate Quest 
	void ActivateQuest (int questNdx) {
		B.S.questManagerCS.activated [questNdx] = true;
	}
    // Complete Quest 
	void CompleteQuest (int questNdx) {
		B.S.questManagerCS.completed [questNdx] = true;
	}
	*/
}


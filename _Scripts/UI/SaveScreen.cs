using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
/*
TO BE IMPLEMENTED:
Current HP/MP, Steps, Party Members, Inventory, Equipment, Doors/Chests/KeyItems, Quests Completed/Activated
 */
public enum eSaveScreenMode { pickAction, pickFile, pickedFile };

public class SaveScreen : MonoBehaviour {
	[Header ("Set in Inspector")]
	public List<Button>			slotButtons;

	public List<Text> 			slotDataText;

	// Load, Save, Delete
	public List<Button>			actionButtons;

	[Header ("Set Dynamically")]
	// Singleton
	private static SaveScreen	_S;
	public static SaveScreen	S { get { return _S; } set { _S = value; } }

	// For Input & Display Message
	public eSaveScreenMode		saveScreenMode;

	// Allows parts of Loop() to be called once rather than repeatedly every frame.
	public bool					canUpdate;

	// Ensures audio is only played once when button is selected
	GameObject					previousSelectedActionButton;
	GameObject					previousSelectedSlotButton;
	
	void Awake() {
		S = this;
	}

	void OnEnable() {
		// Ensures first slot is selected when screen enabled
		previousSelectedActionButton = actionButtons[0].gameObject;

		SetUp();

		// Add Loop() to Update Delgate
		UpdateManager.updateDelegate += Loop;
	}

	void SetUp() {
		try{
			// Freeze Player
			RPG.S.paused = true;
			Player.S.mode = eRPGMode.idle;

			// Switch ScreenMode 
			saveScreenMode = eSaveScreenMode.pickAction;

			// Remove Listeners and Update GUI 
			Utilities.S.RemoveListeners(actionButtons);
			Utilities.S.RemoveListeners(slotButtons);
			UpdateGUI();

			PauseMessage.S.DisplayText("Would you like to\nLoad, Save, or Delete a file?");

			// Buttons Interactable
			Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, false);

			//Buttons Interactable
			Utilities.S.ButtonsInteractable(slotButtons, false);
			Utilities.S.ButtonsInteractable(actionButtons, true);

			// Set Selected GameObject
			Utilities.S.SetSelectedGO(previousSelectedActionButton);

			// Add Listeners
			actionButtons[0].onClick.AddListener(delegate { ClickedLoadSaveOrDelete(0); });
			actionButtons[1].onClick.AddListener(delegate { ClickedLoadSaveOrDelete(1); });
			actionButtons[2].onClick.AddListener(delegate { ClickedLoadSaveOrDelete(2); });
		}
		catch(NullReferenceException){}	
	}

	public void Activate() {
		gameObject.SetActive(true);

		// Audio: Confirm
		AudioManager.S.PlaySFX(eSoundName.confirm);
	}

	public void Deactivate(bool playSound = false) {
		if (RPG.S.currentScene != "Battle") {
			// Buttons Interactable
			Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, true);

			// Set previously selected GameObject
			PauseScreen.S.previousSelectedGameObject = PauseScreen.S.buttonGO[4];

			// Set Selected Gameobject (Pause Screen: Save Button)
			Utilities.S.SetSelectedGO(PauseScreen.S.buttonGO[4]);

			PauseMessage.S.DisplayText ("Welcome to the Pause Screen!");

			PauseScreen.S.canUpdate = true;
		}

		if (playSound) {
			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);
		}

		// Update Delegate
		UpdateManager.updateDelegate -= Loop;

		// Deactivate this gameObject
		gameObject.SetActive(false);
	}

    public void Loop(){
		// Reset canUpdate
		if (Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f) {
			canUpdate = true;
		}

		if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null) {
			Utilities.S.PositionCursor(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject, 100);
		}

		switch (saveScreenMode) {
			case eSaveScreenMode.pickAction:
				// Audio: Selection (when a new gameObject is selected)
				if (canUpdate) {
					Utilities.S.PlayButtonSelectedSFX(ref previousSelectedActionButton);
				}

				if (Input.GetButtonDown("SNES B Button")) {
					Deactivate(true);
				}
				break;
			case eSaveScreenMode.pickFile:
				// Audio: Selection (when a new gameObject is selected)
				if (canUpdate) {
					Utilities.S.PlayButtonSelectedSFX(ref previousSelectedSlotButton);
				}

				if (Input.GetButtonDown("SNES B Button")) {
					// Audio: Deny
					AudioManager.S.PlaySFX(eSoundName.deny);
					SetUp();
				}
				break;
			case eSaveScreenMode.pickedFile:
				if (PauseMessage.S.dialogueFinished) {
					if (Input.GetButtonDown("SNES A Button")) {
						SetUp();
					}
				}
				break;
		}
	}

	void ClickedLoadSaveOrDelete(int loadSaveOrDelete){
		// Remove Listeners and Update GUI 
		Utilities.S.RemoveListeners(actionButtons);
		UpdateGUI();

		//Buttons Interactable
		Utilities.S.ButtonsInteractable(slotButtons, true);
		Utilities.S.ButtonsInteractable(actionButtons, false);

		// Set Selected GameObject
		Utilities.S.SetSelectedGO(slotButtons[0].gameObject);

		// Set previously selected GameObject
		previousSelectedSlotButton = slotButtons[0].gameObject;

		switch (loadSaveOrDelete) {
		case 0:
			slotButtons[0].onClick.AddListener (delegate{LoadFile (0);});
			PauseMessage.S.DisplayText ("Load which file?");
			break;
		case 1:
			slotButtons[0].onClick.AddListener (delegate{SaveFile (0);});
			PauseMessage.S.DisplayText ("Save which file?");
			break;
		case 2:
			slotButtons[0].onClick.AddListener (delegate{DeleteFile (0);});
			PauseMessage.S.DisplayText ("Delete which file?");
			break;
		}

		// Audio: Confirm
		AudioManager.S.PlaySFX(eSoundName.confirm);

		saveScreenMode = eSaveScreenMode.pickFile;
	}

	void LoadFile(int ndx){
		// Slot 1
		if (PlayerPrefs.HasKey("Player1Level")) { Party.stats[0].LVL = PlayerPrefs.GetInt("Player1Level"); }
		if (PlayerPrefs.HasKey("Player2Level")) { Party.stats[1].LVL = PlayerPrefs.GetInt("Player2Level"); }
		if (PlayerPrefs.HasKey("Player3Level")) { Party.stats[2].LVL = PlayerPrefs.GetInt("Player3Level"); }
		if (PlayerPrefs.HasKey("Player1Exp")){ Party.stats[0].EXP = PlayerPrefs.GetInt("Player1Exp"); }
		if (PlayerPrefs.HasKey("Player2Exp")){ Party.stats[1].EXP = PlayerPrefs.GetInt("Player2Exp"); }
		if (PlayerPrefs.HasKey("Player3Exp")){ Party.stats[2].EXP = PlayerPrefs.GetInt("Player3Exp"); }
		if (PlayerPrefs.HasKey("Time")){ PauseScreen.S.fileStatsNumText.text = PlayerPrefs.GetString ("Time"); } // Stores Time in 0:00 format
		if (PlayerPrefs.HasKey("Seconds")) { PauseScreen.S.seconds = PlayerPrefs.GetInt("Seconds"); }
		if (PlayerPrefs.HasKey("Minutes")) { PauseScreen.S.minutes = PlayerPrefs.GetInt("Minutes"); }
		if (PlayerPrefs.HasKey("Name")) { Party.stats[0].name = PlayerPrefs.GetString("Name"); }

		// Level Up
		Party.S.CheckForLevelUp ();
		Party.stats[0].hasLeveledUp = false;
		Party.stats[1].hasLeveledUp = false;
		Party.stats[2].hasLeveledUp = false;

		FileHelper("Loaded game!");
	}
	void SaveFile(int ndx){
		// Slot 1
		PlayerPrefs.SetInt("Player1Level", Party.stats[0].LVL);
		PlayerPrefs.SetInt("Player2Level", Party.stats[1].LVL);
		PlayerPrefs.SetInt("Player3Level", Party.stats[2].LVL);
		PlayerPrefs.SetInt("Player1Exp", Party.stats[0].EXP);
		PlayerPrefs.SetInt("Player2Exp", Party.stats[1].EXP);
		PlayerPrefs.SetInt("Player3Exp", Party.stats[2].EXP);
		PlayerPrefs.SetInt("Gold", Party.S.gold);
		PlayerPrefs.SetString("Time", PauseScreen.S.GetTime()); // Stores Time in 0:00 format
		PlayerPrefs.SetInt("Seconds", PauseScreen.S.seconds);
		PlayerPrefs.SetInt("Minutes", PauseScreen.S.minutes);
		PlayerPrefs.SetString("Name", Party.stats[0].name);

		FileHelper("Saved game!");
	}
	void DeleteFile(int ndx){
		// Slot 1		
		PlayerPrefs.SetInt("Player1Level", 0);
		PlayerPrefs.SetInt("Player2Level", 0);
		PlayerPrefs.SetInt("Player3Level", 0);
		PlayerPrefs.SetInt("Player1Exp", 0);
		PlayerPrefs.SetInt("Player2Exp", 0);
		PlayerPrefs.SetInt("Player3Exp", 0);
		PlayerPrefs.SetInt("Gold", 0);
		PlayerPrefs.SetString("Time", "0:00"); // Stores Time in 0:00 format
		PlayerPrefs.SetInt("Seconds", 0);
		PlayerPrefs.SetInt("Minutes", 0);
		PlayerPrefs.SetString("Name", ""); 

		FileHelper("Deleted game!");
	}

	void FileHelper(string message) {
		// Remove Listeners and Update GUI 
		Utilities.S.RemoveListeners(slotButtons);
		UpdateGUI();

		//Buttons Interactable
		Utilities.S.ButtonsInteractable(slotButtons, false);
		Utilities.S.ButtonsInteractable(actionButtons, false);

		// Audio: Buff 1
		AudioManager.S.PlaySFX(eSoundName.buff1);

		saveScreenMode = eSaveScreenMode.pickedFile;

		PauseMessage.S.DisplayText(message);
	}

	void UpdateGUI(){
		if (PlayerPrefs.HasKey ("Player1Exp")) {
			slotDataText [0].text =
				"Name: " + PlayerPrefs.GetString("Name") + " " + "Level: " + PlayerPrefs.GetInt ("Player1Level") + "\n" +
				"Time: " + PlayerPrefs.GetString("Time") + " " + "Gold: " + PlayerPrefs.GetInt ("Gold");
		}

		PauseScreen.S.UpdateGUI();
	}
}
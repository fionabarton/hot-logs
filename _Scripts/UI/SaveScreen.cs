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

	void Awake() {
		S = this;
	}

	void OnEnable() {
		SetUp();

		// Add Loop() to Update Delgate
		UpdateManager.updateDelegate += Loop;
	}

	void SetUp() {
		// Freeze Player
		RPG.S.paused = true;
		Player.S.mode = eRPGMode.idle;

		// Switch ScreenMode 
		saveScreenMode = eSaveScreenMode.pickAction;

		try{
			// Remove Listeners and Update GUI 
			Utilities.S.RemoveListeners(actionButtons);
			Utilities.S.RemoveListeners(slotButtons);
			UpdateGUI();

			PauseMessage.S.DisplayText("Would you like to\nLoad, Save, or Delete a file?");

			// Buttons Interactable
			Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, false);
		}
		catch(NullReferenceException){}

		//Buttons Interactable
		Utilities.S.ButtonsInteractable(slotButtons, false);
		Utilities.S.ButtonsInteractable(actionButtons, true);

		// Set Selected GameObject (Save Screen: Save Slot 1)
		Utilities.S.SetSelectedGO(actionButtons[0].gameObject);

		// Add Listeners
		actionButtons[0].onClick.AddListener (delegate{ClickedLoadSaveOrDelete (0);});
		actionButtons[1].onClick.AddListener (delegate{ClickedLoadSaveOrDelete (1);});
		actionButtons[2].onClick.AddListener (delegate{ClickedLoadSaveOrDelete (2);});
	}

	public void Deactivate() {
		if (RPG.S.currentSceneName != "Battle") {
			// Buttons Interactable
			Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, true);
			// Set Selected Gameobject (Pause Screen: Save Button)
			Utilities.S.SetSelectedGO(PauseScreen.S.buttonGO[3]);

			PauseMessage.S.DisplayText ("Welcome to the Pause Screen!");

			PauseScreen.S.canUpdate = true;
		}

		// Update Delegate
		UpdateManager.updateDelegate -= Loop;

		// Deactivate this gameObject
		gameObject.SetActive(false);
	}

    public void Loop(){
		if(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null) {
			Utilities.S.PositionCursor(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject, 100);
		}

		switch (saveScreenMode) {
		case eSaveScreenMode.pickAction:
			if (Input.GetButtonDown ("SNES B Button")) {
				Deactivate();
			}
			break;
		case eSaveScreenMode.pickFile:
			if (Input.GetButtonDown ("SNES B Button")) {
				SetUp();
			}
			break;
		case eSaveScreenMode.pickedFile:
			if(PauseMessage.S.dialogueFinished){
				if (Input.GetButtonDown ("SNES A Button")) {
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
					
		saveScreenMode = eSaveScreenMode.pickFile;
	}

	void LoadFile(int ndx){
		// Slot 1
		if (PlayerPrefs.HasKey("Player1Level")) { PartyStats.S.LVL[0] = PlayerPrefs.GetInt("Player1Level"); }
		if (PlayerPrefs.HasKey("Player2Level")) { PartyStats.S.LVL[1] = PlayerPrefs.GetInt("Player2Level"); }
		if (PlayerPrefs.HasKey("Player1Exp")){ PartyStats.S.EXP[0] = PlayerPrefs.GetInt ("Player1Exp"); }
		if (PlayerPrefs.HasKey("Player2Exp")){ PartyStats.S.EXP[1] = PlayerPrefs.GetInt ("Player2Exp"); }
		if (PlayerPrefs.HasKey("Time")){ PauseScreen.S.fileStatsNumText.text = PlayerPrefs.GetString ("Time"); } // Stores Time in 0:00 format
		if (PlayerPrefs.HasKey("Seconds")) { PauseScreen.S.seconds = PlayerPrefs.GetInt("Seconds"); }
		if (PlayerPrefs.HasKey("Minutes")) { PauseScreen.S.minutes = PlayerPrefs.GetInt("Minutes"); }
		if (PlayerPrefs.HasKey("Name")) { PartyStats.S.playerName[0] = PlayerPrefs.GetString("Name"); }

		// Level Up
		PartyStats.S.CheckForLevelUp ();
		PartyStats.S.hasLevelledUp[0] = false;
		PartyStats.S.hasLevelledUp[1] = false;

		FileHelper("Loaded game!");
	}
	void SaveFile(int ndx){
		// Slot 1
		PlayerPrefs.SetInt("Player1Level", PartyStats.S.LVL[0]);
		PlayerPrefs.SetInt("Player1Level", PartyStats.S.LVL[1]);
		PlayerPrefs.SetInt("Player1Exp", PartyStats.S.EXP[0]);
		PlayerPrefs.SetInt("Player2Exp", PartyStats.S.EXP[1]);
		PlayerPrefs.SetInt("Gold", PartyStats.S.Gold);
		PlayerPrefs.SetString("Time", PauseScreen.S.GetTime()); // Stores Time in 0:00 format
		PlayerPrefs.SetInt("Seconds", PauseScreen.S.seconds);
		PlayerPrefs.SetInt("Minutes", PauseScreen.S.minutes);
		PlayerPrefs.SetString("Name", PartyStats.S.playerName[0]);

		FileHelper("Saved game!");
	}
	void DeleteFile(int ndx){
		// Slot 1		
		PlayerPrefs.SetInt("Player1Level", 0);
		PlayerPrefs.SetInt("Player1Level", 0);
		PlayerPrefs.SetInt("Player1Exp", 0);
		PlayerPrefs.SetInt("Player2Exp", 0);
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/*
ALREADY IMPLEMENTED:
Exp, Gold, Minutes, Seconds

TO BE IMPLEMENTED:
Current HP/MP, Steps, Party Members, Inventory, Equipment, Doors/Chests/KeyItems, Quests Completed/Activated
 */

public enum eSaveScreenMode { pickAction, pickFile, pickedFile };

public class SaveScreen : MonoBehaviour {

	[Header ("Set in Inspector")]
	public List<Button> fileSlots;
	public List<Text> 	fileTxt;

	public Button 		loadButton, saveButton, deleteButton;

	[Header ("Set Dynamically")]
	// Singleton
	private static SaveScreen _S;
	public static SaveScreen S { get { return _S; } set { _S = value; } }

	// For Input & Display Message
	public eSaveScreenMode  saveScreenMode = eSaveScreenMode.pickAction;

	void Awake() {
		// Singleton
		S = this;
	}

	void OnEnable () {
		// Set Selected GameObject (Save Screen: Save Slot 1)
		Utilities.S.SetSelectedGO(loadButton.gameObject);

		// Freeze Player
		RPG.S.paused = true;
		Player.S.mode = eRPGMode.idle;

		// Update Delgate
		UpdateManager.updateDelegate += Loop;

		// Switch ScreenMode /////////////////////
		saveScreenMode = eSaveScreenMode.pickAction;

		try{
			RemoveListeners_UpdateGUI ();

			PauseMessage.S.DisplayText("Would you like to\nLoad, Save, or Delete a file?");

			// Buttons Interactable
			Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, false);
		}
		catch(NullReferenceException){}

		ButtonsInteractable (false, false, false, true, true, true);

		// Add Listeners
		loadButton.onClick.AddListener (delegate{ClickedLoadSaveOrDelete (0);});
		saveButton.onClick.AddListener (delegate{ClickedLoadSaveOrDelete (1);});
		deleteButton.onClick.AddListener (delegate{ClickedLoadSaveOrDelete (2);});

        if (Utilities.S) {
			Utilities.S.SetSelectedGO(loadButton.gameObject);
		}
	}

    private void OnDisable() {
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
	}

    public void Loop(){
		Utilities.S.PositionCursor(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject, 100);

		switch (saveScreenMode) {
		case eSaveScreenMode.pickAction:
			if (Input.GetButtonDown ("SNES B Button")) {
				gameObject.SetActive(false);
			}
			break;
		case eSaveScreenMode.pickFile:

			if (Input.GetButtonDown ("SNES B Button")) {
				OnEnable ();
			}

			break;
		case eSaveScreenMode.pickedFile:
			if(PauseMessage.S.dialogueFinished){
				if (Input.GetButtonDown ("SNES A Button")) {
					OnEnable ();
				}
			}
			break;
		}
	}

	void ClickedLoadSaveOrDelete(int loadSaveOrDelete){
		RemoveListeners_UpdateGUI ();
		ButtonsInteractable (true, true, true, false, false, false);

		// Set Selected GameObject
		Utilities.S.SetSelectedGO(fileSlots[0].gameObject);

		switch (loadSaveOrDelete) {
		case 0:
			fileSlots[0].onClick.AddListener (delegate{LoadFile (0);});
			PauseMessage.S.DisplayText ("Load which file?");
			break;
		case 1:
			fileSlots[0].onClick.AddListener (delegate{SaveFile (0);});
			PauseMessage.S.DisplayText ("Save which file?");
			break;
		case 2:
			fileSlots[0].onClick.AddListener (delegate{DeleteFile (0);});
			PauseMessage.S.DisplayText ("Delete which file?");
			break;
		}
					
		saveScreenMode = eSaveScreenMode.pickFile;
	}

	void LoadFile(int ndx){
		// Slot 1
		if(PlayerPrefs.HasKey("Player1Exp")){ PartyStats.S.EXP[0] = PlayerPrefs.GetInt ("Player1Exp"); }
		if(PlayerPrefs.HasKey("Player2Exp")){ PartyStats.S.EXP[1] = PlayerPrefs.GetInt ("Player2Exp"); }
		if(PlayerPrefs.HasKey("Minutes")){ PauseScreen.S.minutes = PlayerPrefs.GetInt ("Minutes"); }
		if (PlayerPrefs.HasKey("Seconds")) { PauseScreen.S.seconds = PlayerPrefs.GetInt("Seconds"); }

		// Level Up
		PartyStats.S.CheckForLevelUp ();
		PartyStats.S.hasLevelledUp[0] = false;
		PartyStats.S.hasLevelledUp[1] = false;

		RemoveListeners_UpdateGUI ();

		ButtonsInteractable (false, false, false, false, false, false);

		saveScreenMode = eSaveScreenMode.pickedFile;

		PauseMessage.S.DisplayText ("Loaded game!");
	}
	void SaveFile(int ndx){
		// Slot 1
		PlayerPrefs.SetInt ("Player1Exp", PartyStats.S.EXP[0]);
		PlayerPrefs.SetInt ("Player2Exp", PartyStats.S.EXP[1]);
		PlayerPrefs.SetInt ("Gold", PartyStats.S.Gold);
		PlayerPrefs.SetInt("Minutes", PauseScreen.S.minutes);
		PlayerPrefs.SetInt("Seconds", PauseScreen.S.seconds);

		RemoveListeners_UpdateGUI ();

		ButtonsInteractable (false, false, false, false, false, false);

		saveScreenMode = eSaveScreenMode.pickedFile;

		PauseMessage.S.DisplayText ("Saved game!");
	}
	void DeleteFile(int ndx){
		// Slot 1		
		PlayerPrefs.SetInt ("Player1Exp", 0);
		PlayerPrefs.SetInt ("Player2Exp", 0);
		PlayerPrefs.SetInt ("Gold", 0);
		PlayerPrefs.SetInt("Minutes", 0);
		PlayerPrefs.SetInt("Seconds", 0);

		RemoveListeners_UpdateGUI ();

		ButtonsInteractable (false, false, false, false, false, false);

		saveScreenMode = eSaveScreenMode.pickedFile;

		PauseMessage.S.DisplayText ("Deleted game!");
	}

	void ButtonsInteractable(bool fileSlot1, bool fileSlot2, bool fileSlot3, bool load, bool save, bool delete){
		fileSlots [0].interactable = fileSlot1;
		fileSlots [1].interactable = fileSlot2;
		fileSlots [2].interactable = fileSlot3;
		loadButton.interactable = load;
		saveButton.interactable = save;
		deleteButton.interactable = delete;
	}

	void RemoveListeners_UpdateGUI(){
		// Remove Listeners
		fileSlots [0].onClick.RemoveAllListeners ();
		loadButton.onClick.RemoveAllListeners ();
		saveButton.onClick.RemoveAllListeners ();
		deleteButton.onClick.RemoveAllListeners ();

		// Update GUI
		UpdateGUI ();
		PauseScreen.S.UpdateGUI ();
	}

	void UpdateGUI(){
		if (PlayerPrefs.HasKey ("Player1Exp")) {
			fileTxt [0].text = "Lvl: " + PlayerPrefs.GetInt ("Player1Level") + "\n" +"Gold: " + PlayerPrefs.GetInt ("Gold");
		} 
			//Player Name/Lvl
			//Time, Gold, Location
	}
}
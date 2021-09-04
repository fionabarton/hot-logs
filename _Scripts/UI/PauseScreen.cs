using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PauseScreen : MonoBehaviour {
	[Header ("Set in Inspector")]
	// Stats
	public List<Text> 	playerNameText;
	public List<Text> 	statsNumText;
	public Text 		fileStatsNumText;

	public GameObject	player2PauseGO;// deactivates if Party has one member

	// Items, Equip, Spells, Save Buttons
	public List<GameObject>	buttonGO; // 0: Items, 1: Equip, 2: Spells, 3: Save
	public List<Button>		buttonCS; // 0: Items, 1: Equip, 2: Spells, 3: Save

    [Header ("Set Dynamically")]
	// Singleton
	private static PauseScreen _S;
	public static PauseScreen S { get { return _S; } set { _S = value; } }

	// Stats
	public int 			seconds;
    public int			minutes;

    // Resets timer
	float               timeDone;

    // Account for time that PauseScreen is not active
	float               timeWhenEnabled;
    float				timeWhenDisabled;

	GameObject			previousSelectedGameObject;

	// Allows parts of Loop() to be called once rather than repeatedly every frame.
	public bool 		canUpdate;

	void Awake() {
		S = this;
	}

    void OnEnable() {
		canUpdate = true;

		try{
			// Display Player Stats (Level, HP, MP, EXP)
			UpdateGUI ();

			// (De)Activate Player2 Button
			if (PartyStats.S.partyNdx >= 1) {
				player2PauseGO.SetActive (true);
			} else {
				player2PauseGO.SetActive (false);
			}

			// Activate Cursor
			ScreenCursor.S.cursorGO.SetActive (true);
		}catch(NullReferenceException){}

		// Account for time that PauseScreen is not active
		timeWhenEnabled = Time.time;
		float tTime = timeWhenEnabled - timeWhenDisabled;
		minutes += (int)tTime / 60;
		seconds += (int)tTime % 60;

		StartCoroutine ("FixedUpdateCoroutine");
	}

	void OnDisable () {
		// Deactivate Cursor
		ScreenCursor.S.cursorGO.SetActive(false);

		// Account for time that PauseScreen is not active
		timeWhenDisabled = Time.time;

		StopCoroutine ("FixedUpdateCoroutine");
    }

    public void Loop() {
		if (RPG.S.paused) {
			// Reset canUpdate
			if (Input.GetAxisRaw ("Horizontal") != 0f || Input.GetAxisRaw ("Vertical") != 0f) {
				canUpdate = true;
			}

			if (canUpdate) {
				for (int i = 0; i < buttonGO.Count; i++) {
					if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == buttonGO [i]) {
						// Set Cursor Position to Selected Button
						Utilities.S.PositionCursor(buttonGO[i], 160);
					}
				}
                canUpdate = false;
			}
		}
	}

	public IEnumerator FixedUpdateCoroutine () {
		// If Active
		if (isActiveAndEnabled) {
			// Increment seconds & reset timer
			if (timeDone <= Time.time)
			{
				seconds += 1;
				timeDone = 1 + Time.time;
			}

			// Increment minutes & reset seconds
			if (seconds > 59)
			{
				minutes += 1;
				seconds = 0;
			}

			// Display Time, Step Count, & Gold
			// Add 0 to Seconds if necessary ("Time: 0:01" rather than "Time 0:1")
			if (seconds > 9) {
				Time_Steps_Gold_TXT ();
			} else {
				Time_Steps_Gold_TXT ("0");
			}
		}
		yield return new WaitForFixedUpdate ();
		StartCoroutine ("FixedUpdateCoroutine");
	}

	// ************ PAUSE ************ \\
	public void Pause() {
		// If SubMenu enabled when Paused, re-select this GO when Unpaused
		// TBR: Account for SubMenu having 2 to 4 options
		for (int i = 0; i < SubMenu.S.buttonGO.Count; i++) {
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == SubMenu.S.buttonGO[i]) {
				previousSelectedGameObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
			}
			SubMenu.S.buttonCS[i].interactable = false;
		}

		// Overworld Player Stats
		PlayerButtons.S.gameObject.SetActive(false);

		gameObject.SetActive(true);

		// Buttons Interactable
		Utilities.S.ButtonsInteractable(buttonCS, true);
		// Set Selected Gameobject (Pause Screen: Items Button)
		Utilities.S.SetSelectedGO(buttonGO[0]);

		// Freeze Player
		RPG.S.paused = true;
		Player.S.mode = eRPGMode.idle;

		// Activate PauseMessage
		PauseMessage.S.DisplayText("Welcome to the Pause Screen!");

		// Update Delgate
		UpdateManager.updateDelegate += Loop;
	}
	public void UnPause() {
		// Overworld Player Stats
		Player.S.playerUITimer = Time.time + 1.5f;

		gameObject.SetActive(false);

		// Unpause
		RPG.S.paused = false;

		// Deactivate PauseMessage
		PauseMessage.S.gameObject.SetActive(false);

		// If SubMenu enabled when Paused, re-select this GO when Unpaused
		// TBR: Account for SubMenu having 2 to 4 options
		for (int i = 0; i < SubMenu.S.buttonGO.Count; i++) {
			if (previousSelectedGameObject == SubMenu.S.buttonGO[i]) {
				Utilities.S.SetSelectedGO(previousSelectedGameObject);
			}
			SubMenu.S.buttonCS[i].interactable = true;
		}

		// Update Delegate
		UpdateManager.updateDelegate -= Loop;
	}

	// Display Time, Steps, & Gold
	void Time_Steps_Gold_TXT (string zeroString = null) {
        if (Player.S) {
			// Time
			fileStatsNumText.text = minutes.ToString() + ":" + zeroString + seconds.ToString() + "\n" +
			// Steps Count
			Player.S.stepsCount + "\n" +
			// Gold
			PartyStats.S.Gold;
		}
	}

	// Display Player Stats (Level, HP, MP, EXP)
	public void UpdateGUI () {
		// Player 1 ////////////////////////////////////
		playerNameText[0].text = PartyStats.S.playerName[0];

		statsNumText[0].text = PartyStats.S.LVL[0] + "\n" +
			PartyStats.S.HP[0] + "/" + PartyStats.S.maxHP[0] + "\n" +
			PartyStats.S.MP[0] + "/" + PartyStats.S.maxMP[0] + "\n" +
			PartyStats.S.EXP[0];

        // Player 2 ////////////////////////////////////
        playerNameText[1].text = PartyStats.S.playerName[1];

		if (PartyStats.S.partyNdx >= 1) {
			statsNumText[1].text = PartyStats.S.LVL[1] + "\n" +
				PartyStats.S.HP[1] + "/" + PartyStats.S.maxHP[1] + "\n" +
				PartyStats.S.MP[1] + "/" + PartyStats.S.maxMP[1] + "\n" +
				PartyStats.S.EXP[1];
		}
	}
}
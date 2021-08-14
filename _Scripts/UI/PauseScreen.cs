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

	public bool 		canUpdate;

	void Awake() {
		// Singleton
		S = this;
	}

    void OnEnable() {
		canUpdate = true;

		try{
			// Display Player Stats (Level, HP, MP, EXP)
			UpdateGUI ();

			// (De)Activate Player2 Button
			if (Stats.S.partyNdx >= 1) {
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
		ScreenCursor.S.cursorGO.SetActive (false);

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
						// Cursor Position set to Selected Button
						float tPosX = buttonGO [i].gameObject.GetComponent<RectTransform> ().localPosition.x;
                        float tPosY = buttonGO[i].gameObject.GetComponent<RectTransform>().localPosition.y;

						float tParentX = buttonGO [i].gameObject.transform.parent.GetComponent<RectTransform> ().anchoredPosition.x;
						float tParentY = buttonGO [i].gameObject.transform.parent.GetComponent<RectTransform> ().anchoredPosition.y;

						ScreenCursor.S.rectTrans.anchoredPosition = new Vector2 ((tPosX + tParentX + 160), (tPosY + tParentY));
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

	// Display Time, Steps, & Gold
	void Time_Steps_Gold_TXT (string zeroString = null) {
        if (Player.S) {
			// Time
			fileStatsNumText.text = minutes.ToString() + ":" + zeroString + seconds.ToString() + "\n" +
			// Steps Count
			Player.S.stepsCount + "\n" +
			// Gold
			Stats.S.Gold;
		}
	}

	// Display Player Stats (Level, HP, MP, EXP)
	public void UpdateGUI () {
		// Player 1 ////////////////////////////////////
		playerNameText[0].text = Stats.S.playerName[0];

		statsNumText[0].text = Stats.S.LVL[0] + "\n" +
			Stats.S.HP[0] + "/" + Stats.S.maxHP[0] + "\n" +
			Stats.S.MP[0] + "/" + Stats.S.maxMP[0] + "\n" +
			Stats.S.EXP[0];

        // Player 2 ////////////////////////////////////
        playerNameText[1].text = Stats.S.playerName[1];

		if (Stats.S.partyNdx >= 1) {
			statsNumText[1].text = Stats.S.LVL[1] + "\n" +
				Stats.S.HP[1] + "/" + Stats.S.maxHP[1] + "\n" +
				Stats.S.MP[1] + "/" + Stats.S.maxMP[1] + "\n" +
				Stats.S.EXP[1];
		}
	}
}

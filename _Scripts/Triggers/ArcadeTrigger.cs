using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcadeTrigger : ActivateOnButtonPress {
	[Header("Set in Inspector")]
	public string	gameToLoad;
	// public int 			costToPlay;

	public string	message = "Would you like to play?";

	public bool		isArcadeCabinet = true;

	protected override void Action() {
		// Set Camera to Arcade Game gameObject
		CamManager.S.ChangeTarget(gameObject, true);

		DialogueManager.S.DisplayText(message);

		// Set SubMenu Text
		SubMenu.S.SetText("Let's play!", "No thanks.");

		// Activate Sub Menu after Dialogue 
		DialogueManager.S.activateSubMenu = true;
		// Don't activate Text Box Cursor 
		DialogueManager.S.dontActivateCursor = true;
		// Gray Out Text Box after Dialogue 
		DialogueManager.S.grayOutTextBox = true;

		// Set OnClick Methods
		Utilities.S.RemoveListeners(SubMenu.S.buttonCS);
		SubMenu.S.buttonCS[0].onClick.AddListener(Play);
		SubMenu.S.buttonCS[1].onClick.AddListener(No);
		//SubMenu.S.subMenuButtonCS[2].onClick.AddListener(No);
		//SubMenu.S.subMenuButtonCS[3].onClick.AddListener(Option3);

		// Set button navigation
		Utilities.S.SetButtonNavigation(SubMenu.S.buttonCS[0], SubMenu.S.buttonCS[1], SubMenu.S.buttonCS[1]);
		Utilities.S.SetButtonNavigation(SubMenu.S.buttonCS[1], SubMenu.S.buttonCS[0], SubMenu.S.buttonCS[0]);
	}

	void Play() {
		// Audio: Buff 1
		AudioManager.S.PlaySFX(11);

		DialogueManager.S.ResetSubMenuSettings();
		LoadGame();
	}

	void No() {
		// Audio: Deny
		AudioManager.S.PlaySFX(7);

		DialogueManager.S.ResetSubMenuSettings();
		DialogueManager.S.DisplayText("I ain't got time for no games! Ho yeah!");
	}

	public void LoadGame() {
        if (!isArcadeCabinet) {
			// Get Animator
			Animator anim = GetComponent<Animator>();

			// Play TV static screen animation
			if (anim) {
                anim.Play("TV_Static");
            }
        } 

		StartCoroutine("LoadVideoGame", 1);
	}

	public IEnumerator LoadVideoGame() {
		// Set Respawn Position
		Player.S.respawnPos = Player.S.gameObject.transform.position;

		// Pause Game
		RPG.S.paused = true;
		Player.S.mode = eRPGMode.idle;

		// Enable Black Screen
		RPG.S.blackScreen.enabled = true;

		yield return new WaitForSeconds(1);

        // Load Level
        RPG.S.LoadLevel(gameToLoad);

		// Update Delgate
		UpdateManager.updateDelegate += ArcadeManager.S.Loop;
	}
}
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
		RPG.S.gameSubMenu.SetText("Let's play!", "No thanks.");

		// Activate Sub Menu after Dialogue 
		DialogueManager.S.activateSubMenu = true;
		// Don't activate Text Box Cursor 
		DialogueManager.S.dontActivateCursor = true;
		// Gray Out Text Box after Dialogue 
		DialogueManager.S.grayOutTextBox = true;

		// Set OnClick Methods
		Utilities.S.RemoveListeners(RPG.S.gameSubMenu.buttonCS);
		RPG.S.gameSubMenu.buttonCS[0].onClick.AddListener(Play);
		RPG.S.gameSubMenu.buttonCS[1].onClick.AddListener(No);
		//RPG.S.gameSubMenu.subMenuButtonCS[2].onClick.AddListener(No);
		//RPG.S.gameSubMenu.subMenuButtonCS[3].onClick.AddListener(Option3);

		// Set button navigation
		Utilities.S.SetButtonNavigation(RPG.S.gameSubMenu.buttonCS[0], RPG.S.gameSubMenu.buttonCS[1], RPG.S.gameSubMenu.buttonCS[1]);
		Utilities.S.SetButtonNavigation(RPG.S.gameSubMenu.buttonCS[1], RPG.S.gameSubMenu.buttonCS[0], RPG.S.gameSubMenu.buttonCS[0]);
	}

	void Play() {
		// Audio: Buff 1
		AudioManager.S.PlaySFX(eSoundName.buff1);

		DialogueManager.S.ResetSettings();
		LoadGame();
	}

	void No() {
		// Audio: Deny
		AudioManager.S.PlaySFX(eSoundName.deny);

		DialogueManager.S.ResetSettings();
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
		Player.S.mode = ePlayerMode.idle;

		// Activate Black Screen
		ColorScreen.S.ActivateBlackScreen();

		yield return new WaitForSeconds(1);

        // Load Level
        RPG.S.LoadLevel(gameToLoad);

		// Update Delgate
		UpdateManager.updateDelegate += ArcadeManager.S.Loop;
	}
}
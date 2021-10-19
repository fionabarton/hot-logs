using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsScreen : MonoBehaviour {
	[Header("Set in Inspector")]
	public Slider bgmSlider;
	public Slider sfxSlider;

	[Header("Set Dynamically")]
	// Singleton
	private static OptionsScreen _S;
	public static OptionsScreen S { get { return _S; } set { _S = value; } }

	void Awake() {
		S = this;
	}

	public void OnEnable() {
		// Add Loop() to Update Delgate
		UpdateManager.updateDelegate += Loop;
	}

	// Set in Inspector on OptionsScreen
	public void Activate() {
		// Buttons Interactable
		Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, false);

		// Set Cursor Position set to Selected Button
		Utilities.S.PositionCursor(bgmSlider.gameObject, -170, 0, 0);

		gameObject.SetActive(true);

		// Set Selected Gameobject (Pause Screen: Options Button)
		Utilities.S.SetSelectedGO(bgmSlider.gameObject);

		// Audio: Confirm
		AudioManager.S.PlaySFX(eSoundName.confirm);
	}

	public void Deactivate(bool playSound = false) {
		if (RPG.S.currentSceneName != "Battle") {
			// Buttons Interactable
			Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, true);

			// Set previously selected GameObject
			PauseScreen.S.previousSelectedGameObject = PauseScreen.S.buttonGO[3];

			// Set Selected Gameobject (Pause Screen: Options Button)
			Utilities.S.SetSelectedGO(PauseScreen.S.buttonGO[3]);

			PauseMessage.S.DisplayText("Welcome to the Pause Screen!");

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

	public void Loop() {
		if (RPG.S.currentSceneName != "Battle") {
			if (Input.GetButtonDown("SNES B Button")) {
				Deactivate(true);
			}
		}
	}

	// Set in Inspector on BGMSlider
	public void SetBGMVolume() {
		// Set the volume of all BGMs to the value of its slider
		AudioManager.S.SetBGMVolume(bgmSlider.value);

		// Audio: Confirm
		AudioManager.S.PlaySFX(eSoundName.confirm);
	}

	// Set in Inspector on SFXSlider
	public void SetSFXVolume() {
		// Set the volume of all SFXs to the value of its slider
		AudioManager.S.SetSFXVolume(sfxSlider.value);

		// Audio: Confirm
		AudioManager.S.PlaySFX(eSoundName.confirm);
	}
}
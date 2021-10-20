using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsScreen : MonoBehaviour {
	[Header("Set in Inspector")]
	public Slider			masterVolSlider;
	public Slider			bgmVolSlider;
	public Slider			sfxVolSlider;

	public List<Slider>		volumeSliders;

	public List<GameObject> volumeTextGOs;

	[Header("Set Dynamically")]
	// Singleton
	private static OptionsScreen _S;
	public static OptionsScreen S { get { return _S; } set { _S = value; } }

	// Allows parts of Loop() to be called once rather than repeatedly every frame.
	public bool				canUpdate;

	void Awake() {
		S = this;
	}

	public void OnEnable() {
		// Add Loop() to Update Delgate
		UpdateManager.updateDelegate += Loop;
	}

    public void Start() {
		// Load settings
		if (PlayerPrefs.HasKey("Master Volume")) { masterVolSlider.value = PlayerPrefs.GetFloat("Master Volume"); }
		if (PlayerPrefs.HasKey("BGM Volume")) { bgmVolSlider.value = PlayerPrefs.GetFloat("BGM Volume"); }
		if (PlayerPrefs.HasKey("SFX Volume")) { sfxVolSlider.value = PlayerPrefs.GetFloat("SFX Volume"); }
	}

    // Set in Inspector on OptionsScreen
    public void Activate() {
		// Buttons Interactable
		Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, false);

		gameObject.SetActive(true);

		// Set Selected Gameobject 
		Utilities.S.SetSelectedGO(masterVolSlider.gameObject);

		// Set Cursor Position set to Selected Button
		//SetCursorPosition();
		// Set Cursor Position set to Selected Button
		Utilities.S.PositionCursor(volumeTextGOs[0], -125, 0, 0);

		// Audio: Confirm
		AudioManager.S.PlaySFX(eSoundName.confirm);

		// Load settings
		if (PlayerPrefs.HasKey("Master Volume")) { masterVolSlider.value = PlayerPrefs.GetFloat("Master Volume"); }
		if (PlayerPrefs.HasKey("BGM Volume")) { bgmVolSlider.value = PlayerPrefs.GetFloat("BGM Volume"); }
		if (PlayerPrefs.HasKey("SFX Volume")) { sfxVolSlider.value = PlayerPrefs.GetFloat("SFX Volume"); }
	}

	public void Deactivate(bool playSound = false) {
		if (RPG.S.currentScene != "Battle") {
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
		// Reset canUpdate
		if (Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f) {
			canUpdate = true;
		}

		if (canUpdate) {
			SetCursorPosition();
			canUpdate = false;
		}

		if (RPG.S.currentScene != "Battle") {
			if (Input.GetButtonDown("SNES B Button")) {
				Deactivate(true);
			}
		}
	}

	// Set in Inspector on BGMSlider
	public void SetBGMVolume() {
		// Set the volume of all BGMs to the value of its slider
		AudioManager.S.SetBGMVolume(bgmVolSlider.value);

		// Save settings
		PlayerPrefs.SetFloat("BGM Volume", bgmVolSlider.value);

		// Audio: Confirm
		//AudioManager.S.PlaySFX(eSoundName.confirm);
	}

	// Set in Inspector on MasterVolumeSlider
	public void SetMasterVolume() {
		// Set the volume of the AudioListener to the value of its slider
		AudioManager.S.SetMasterVolume(masterVolSlider.value);

		// Save settings
		PlayerPrefs.SetFloat("Master Volume", masterVolSlider.value);

		// Audio: Confirm
		//AudioManager.S.PlaySFX(eSoundName.confirm);
	}

	// Set in Inspector on SFXVolumeSlider
	public void SetSFXVolume() {
		// Set the volume of all SFXs to the value of its slider
		AudioManager.S.SetSFXVolume(sfxVolSlider.value);

		// Save settings
		PlayerPrefs.SetFloat("SFX Volume", sfxVolSlider.value);

		// Audio: Selection
		AudioManager.S.PlaySFX(eSoundName.selection);
	}

	public void SetCursorPosition() {
		for (int i = 0; i < volumeTextGOs.Count; i++) {
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == volumeSliders[i].gameObject) {
				//PauseMessage.S.SetText(Inventory.S.GetItemList()[i].description);

				// Set Cursor Position set to Selected Button
				Utilities.S.PositionCursor(volumeTextGOs[i], -125, 0, 0);

				// Set selected button text color	
				volumeTextGOs[i].GetComponent<Text>().color = new Color32(205, 208, 0, 255);

				// Audio: Selection (when a new gameObject is selected)
				Utilities.S.PlayButtonSelectedSFX(ref ItemScreen.S.previousSelectedGameObject);
			} else {
				// Set non-selected button text color
				volumeTextGOs[i].GetComponent<Text>().color = new Color32(255, 255, 255, 255);
			}
		}
	}
}
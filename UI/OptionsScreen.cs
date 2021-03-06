using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsScreen : MonoBehaviour {
	[Header("Set in Inspector")]
	public List<Slider>		sliders;
	public List<GameObject> sliderTextGO;
	public List<string>		sliderDescriptions = new List<string> { 
		"Set the master volume!", 
		"Set the background music volume!", 
		"Set the sound effects volume!", 
		"Set the rate at which text is displayed!" };

	[Header("Set Dynamically")]
	// Singleton
	private static OptionsScreen _S;
	public static OptionsScreen S { get { return _S; } set { _S = value; } }

	// Allows parts of Loop() to be called once rather than repeatedly every frame.
	public bool				canUpdate;

	public float			textSpeed = 0.1f;

	void Awake() {
		S = this;
	}

	public void OnEnable() {
		// Add Loop() to Update Delgate
		UpdateManager.updateDelegate += Loop;
	}

    public void Start() {
		// Load settings
		if (PlayerPrefs.HasKey("Master Volume")) {
			sliders[0].value = PlayerPrefs.GetFloat("Master Volume");
			AudioManager.S.SetMasterVolume(sliders[0].value);
        } else {
			AudioManager.S.SetMasterVolume(0.25f);
		}
		if (PlayerPrefs.HasKey("BGM Volume")) {
			sliders[1].value = PlayerPrefs.GetFloat("BGM Volume");
			AudioManager.S.SetBGMVolume(sliders[1].value);
		} else {
			AudioManager.S.SetBGMVolume(0.5f);
		}
		if (PlayerPrefs.HasKey("SFX Volume")) {
			sliders[2].value = PlayerPrefs.GetFloat("SFX Volume");
			AudioManager.S.SetSFXVolume(sliders[2].value);
		} else {
			AudioManager.S.SetSFXVolume(0.5f);
		}
		if (PlayerPrefs.HasKey("Text Speed")) {
			sliders[3].value = PlayerPrefs.GetFloat("Text Speed");
			textSpeed = sliders[3].value;
		} else {
			textSpeed = 0.05f;
		}

		// Adds a listener to each slider and invokes a method when the value changes
		sliders[0].onValueChanged.AddListener(delegate { SetMasterVolume(); });
		sliders[1].onValueChanged.AddListener(delegate { SetBGMVolume(); });
		sliders[2].onValueChanged.AddListener(delegate { SetSFXVolume(); });
		sliders[3].onValueChanged.AddListener(delegate { SetTextSpeed(); });
	}

    // Set in Inspector on OptionsScreen
    public void Activate() {
		// Buttons Interactable
		Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, false);

		gameObject.SetActive(true);

		// Set Selected Gameobject 
		Utilities.S.SetSelectedGO(sliders[0].gameObject);

		// Set Cursor Position set to Selected Button
		Utilities.S.PositionCursor(sliderTextGO[0], -125, 0, 0);

		PauseMessage.S.DisplayText("Set the master volume!");

		// Audio: Confirm
		AudioManager.S.PlaySFX(eSoundName.confirm);

		canUpdate = true;
	}

	public void Deactivate(bool playSound = false) {
		if (GameManager.S.currentScene != "Battle" || GameManager.S.currentScene != "Title_Screen") {
			// Buttons Interactable
			Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, true);

			// Set previously selected GameObject
			PauseScreen.S.previousSelectedGameObject = PauseScreen.S.buttonGO[3];

			// Set Selected Gameobject (Pause Screen: Options Button)
			Utilities.S.SetSelectedGO(PauseScreen.S.buttonGO[3]);

			PauseMessage.S.DisplayText("Welcome to the Pause Screen!");

			PauseScreen.S.canUpdate = true;
		}

		if (GameManager.S.currentScene == "Title_Screen") {
			// Set Selected GameObject (New Game Button)
			Utilities.S.SetSelectedGO(TitleScreen.S.previousSelectedButton);

			PauseMessage.S.gameObject.SetActive(false);
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
		if (Input.GetAxisRaw("Vertical") != 0f) {
			canUpdate = true;
		}

		if (canUpdate) {
			SetCursorPosition();
			canUpdate = false;
		}

		if (GameManager.S.currentScene != "Battle") {
			if (Input.GetButtonDown("SNES Y Button")) {
				Deactivate(true);
			}
		}
	}

	public void SetMasterVolume() {
		// Set the volume of the AudioListener to the value of its slider
		AudioManager.S.SetMasterVolume(sliders[0].value);

		// Save settings
		PlayerPrefs.SetFloat("Master Volume", sliders[0].value);

		// Audio: Selection
		AudioManager.S.masterVolSelection.Play();
	}

	public void SetBGMVolume() {
		// Set the volume of all BGMs to the value of its slider
		AudioManager.S.SetBGMVolume(sliders[1].value);

		// Save settings
		PlayerPrefs.SetFloat("BGM Volume", sliders[1].value);

		// Audio: Selection
		AudioManager.S.bgmCS[8].Play();
	}

	public void SetSFXVolume() {
		// Set the volume of all SFXs to the value of its slider
		AudioManager.S.SetSFXVolume(sliders[2].value);

		// Save settings
		PlayerPrefs.SetFloat("SFX Volume", sliders[2].value);

		// Audio: Selection
		AudioManager.S.PlaySFX(eSoundName.selection);
	}

	public void SetTextSpeed() {
		// Set the text speed to the value of its slider
		textSpeed = sliders[3].value;

		// Save settings
		PlayerPrefs.SetFloat("Text Speed", sliders[3].value);

		// Display text
		PauseMessage.S.DisplayText("With the 'Text Speed' set at this value, text will be displayed on screen this quickly!");
	}

	public void SetCursorPosition() {
		for (int i = 0; i < sliderTextGO.Count; i++) {
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == sliders[i].gameObject) {
				PauseMessage.S.SetText(sliderDescriptions[i]);

				// Set Cursor Position set to Selected Button
				Utilities.S.PositionCursor(sliderTextGO[i], -125, 0, 0);

				// Set selected button text color	
				sliderTextGO[i].GetComponent<Text>().color = new Color32(205, 208, 0, 255);

				// Audio: Selection (when a new gameObject is selected)
				Utilities.S.PlayButtonSelectedSFX(ref Items.S.menu.previousSelectedGameObject);
			} else {
				// Set non-selected button text color
				sliderTextGO[i].GetComponent<Text>().color = new Color32(255, 255, 255, 255);
			}
		}
	}
}
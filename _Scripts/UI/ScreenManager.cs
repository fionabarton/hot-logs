using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static ScreenManager _S;
	public static ScreenManager S { get { return _S; } set { _S = value; } }

	// If SubMenu enabled when Paused, re-select this GO when Unpaused
	GameObject			previousSelectedGameObject;

	[Header("Set in Inspector")]
	public GameObject	playerButtonsGO;

	void Awake() {
		// Singleton
		S = this;
	}

    void Start() {
		// Add Loop() to UpdateManager
		UpdateManager.updateDelegate += Loop;
	}

    // Pause Screen Input
    public void Loop (){
		if (!ItemScreen.S.gameObject.activeInHierarchy && 
			!SpellsScreen.S.gameObject.activeInHierarchy && 
			!EquipScreen.S.gameObject.activeInHierarchy && 
			!ShopScreen.S.gameObject.activeInHierarchy && 
			!SaveScreen.S.gameObject.activeInHierarchy) {
			if (!RPG.S.paused && RPG.S.currentSceneName != "Battle") {
				if (Input.GetButtonDown("Pause")) { Pause(); }
			} else {
				if (Input.GetButtonDown("Pause") || Input.GetButtonDown("SNES B Button")) { UnPause(); }
			}
		} 
	}

	// ************ PAUSE ************ \\
	public void Pause(){
		// If SubMenu enabled when Paused, re-select this GO when Unpaused
		if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == SubMenu.S.buttonCS[0] || 
			UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == SubMenu.S.buttonCS[1] || 
			UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == SubMenu.S.buttonCS[2] || 
			UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == SubMenu.S.buttonCS[3]) {

			previousSelectedGameObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
			SubMenu.S.buttonCS[0].interactable = false;
			SubMenu.S.buttonCS[1].interactable = false;
			SubMenu.S.buttonCS[2].interactable = false;
			SubMenu.S.buttonCS[3].interactable = false;
		}

		// Overworld Player Stats
		playerButtonsGO.SetActive (false);

		PauseScreen.S.gameObject.SetActive (true);

		// Buttons Interactable
		Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, true);
		// Set Selected Gameobject (Pause Screen: Items Button)
		Utilities.S.SetSelectedGO(PauseScreen.S.buttonGO[0]);

		// Freeze Player
		RPG.S.paused = true;
		Player.S.mode = eRPGMode.idle;

		// Activate PauseMessage
		PauseMessage.S.DisplayText ("Welcome to the Pause Screen!");

		// Update Delgate
		UpdateManager.updateDelegate += PauseScreen.S.Loop;
	}
	public void UnPause(){
		// Overworld Player Stats
		Player.S.playerUITimer = Time.time + 1.5f;

		PauseScreen.S.gameObject.SetActive (false);

		// Unpause
		RPG.S.paused = false;

		// Deactivate PauseMessage
		PauseMessage.S.gameObject.SetActive (false);

		// If SubMenu enabled when Paused, re-select this GO when Unpaused
		if (previousSelectedGameObject == SubMenu.S.buttonCS[0] || 
			previousSelectedGameObject == SubMenu.S.buttonCS[1] || 
			previousSelectedGameObject == SubMenu.S.buttonCS[2] || 
			previousSelectedGameObject == SubMenu.S.buttonCS[3]) {

			Utilities.S.SetSelectedGO (previousSelectedGameObject);
			SubMenu.S.buttonCS[0].interactable = true;
			SubMenu.S.buttonCS[1].interactable = true;
			SubMenu.S.buttonCS[2].interactable = true;
			SubMenu.S.buttonCS[3].interactable = true;
		}

		// Update Delegate
		UpdateManager.updateDelegate -= PauseScreen.S.Loop;
	}

	// ************ ITEM SCREEN ************ \\ 
	public void ItemScreenOn () {
		// Activate Item Screen
		ItemScreen.S.gameObject.SetActive(true);

		// Buttons Interactable
		Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, false);

		// Also called in OnEnable, but called here again because: 
		// Turn ItemScreen on 2 times, and ItemSlot1 is SELECTED, but NOT HIGHLIGHTED
		Utilities.S.SetSelectedGO(ItemScreen.S.itemButtons[0].gameObject);

		// Update Delgate
		UpdateManager.updateDelegate += ItemScreen.S.Loop;

		// Audio: Confirm
		AudioManager.S.PlaySFX(4);
	}
	public void ItemScreenOff () {
		// Remove Listeners
		Utilities.S.RemoveListeners(ItemScreen.S.itemButtons);

		if (ItemScreen.S.useOrSellMode != eUseOrSellMode.sellMode) {
			if (RPG.S.currentSceneName != "Battle") {
				// Buttons Interactable
				Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, true);
				// Set Selected Gameobject (Pause Screen: Items Button)
				Utilities.S.SetSelectedGO (PauseScreen.S.buttonGO[0]);

				PauseMessage.S.DisplayText ("Welcome to the Pause Screen!");

				PauseScreen.S.canUpdate = true;
			} else {
				// If Player didn't use an Item, go back to Player Turn
				if (ItemScreen.S.itemScreenMode != eItemScreenMode.pickPartyMember) {
					if (Battle.S.battleMode == eBattleMode.itemOrSpellMenu) {
						Battle.S.PlayerTurn();
					}
				}
			}
		}

		// Deactivate PlayerButtons
		playerButtonsGO.SetActive(false);

        // Deactivate Item Screen
        ItemScreen.S.gameObject.SetActive(false);

        // Update Delegate
        UpdateManager.updateDelegate -= ItemScreen.S.Loop;
	}


	// ************ SPELLS SCREEN ************ \\ 
	public void SpellsScreenOn() {
		// Activate Spell Screen
		SpellsScreen.S.gameObject.SetActive(true);

		// Buttons Interactable
		Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, false);

		// Update Delgate
		UpdateManager.updateDelegate += SpellsScreen.S.Loop;

		// Audio: Confirm
		AudioManager.S.PlaySFX(4);
	}

	public void SpellsScreenOff () {
		// Remove Listeners
		Utilities.S.RemoveListeners(SpellsScreen.S.spellsButtons);

		if (RPG.S.currentSceneName != "Battle") {
			// Buttons Interactable
			Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, true);
			// Set Selected Gameobject (Pause Screen: Spells Button)
			Utilities.S.SetSelectedGO(PauseScreen.S.buttonGO[2]);

			PauseMessage.S.DisplayText ("Welcome to the Pause Screen!");

			PauseScreen.S.canUpdate = true;
		} else {
			// If Player didn't use a Spell, go back to Player Turn
			if (SpellsScreen.S.spellScreenMode != eSpellScreenMode.pickSpell) {
				if (Battle.S.battleMode == eBattleMode.itemOrSpellMenu) {
					Battle.S.PlayerTurn();
				}
			}
		}

		// Deactivate PlayerButtons
		playerButtonsGO.SetActive(false);

		// Deactivate Item Screen
		SpellsScreen.S.gameObject.SetActive (false);

		// Update Delegate
		UpdateManager.updateDelegate -= SpellsScreen.S.Loop;
	}

	// ************ EQUIP SCREEN ************ \\ 
	//public void EquipScreenOn () {
	//	// Activate Equip Screen
	//	EquipScreen.S.gameObject.SetActive (true);

	//	// Buttons Interactable
	//	PauseButtonsInteractable(false, false, false, false);

	//	// Update Delgate
	//	UpdateManager.updateDelegate += EquipScreen.S.Loop;

	//	// Audio: Confirm
	//	AudioManager.S.PlaySFX(4);
	//}
	//public void EquipScreenOff () {
	//	if (RPG.S.currentSceneName != "Battle") {
	//		// Buttons Interactable
	//		PauseButtonsInteractable(true, true, true, true);
	//		// Set Selected Gameobject (Pause Screen: Equip Button)
	//		Utilities.S.SetSelectedGO(PauseScreen.S.buttonGO[1]);

	//		PauseMessage.S.DisplayText ("Welcome to the Pause Screen!");

	//		PauseScreen.S.canUpdate = true;
	//	}

	//	// Deactivate PlayerButtons
	//	playerButtonsGO.SetActive(false);

	//	// Deactivate Equip Screen
	//	EquipScreen.S.gameObject.SetActive (false);

	//	// Update Delegate
	//	UpdateManager.updateDelegate -= EquipScreen.S.Loop;
	//}

	// ************ SAVE SCREEN ************ \\ 
	public void SaveScreenOn () {
		// Activate Save Screen
		SaveScreen.S.gameObject.SetActive (true);

		// Buttons Interactable
		Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, false);
		// Set Selected GameObject (Save Screen: Save Slot 1)
		Utilities.S.SetSelectedGO (SaveScreen.S.loadButton.gameObject);

		// Freeze Player
		RPG.S.paused = true;
		Player.S.mode = eRPGMode.idle;

		// Update Delgate
		UpdateManager.updateDelegate += SaveScreen.S.Loop;

		// Audio: Confirm
		AudioManager.S.PlaySFX(4);
	}
	public void SaveScreenOff () {
		if (RPG.S.currentSceneName != "Battle") {
			// Buttons Interactable
			Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, true);
			// Set Selected Gameobject (Pause Screen: Save Button)
			Utilities.S.SetSelectedGO(PauseScreen.S.buttonGO[3]);

			PauseMessage.S.DisplayText ("Welcome to the Pause Screen!");

			PauseScreen.S.canUpdate = true;
		}

		// Deactivate Save Screen
		SaveScreen.S.gameObject.SetActive (false);

		// Update Delegate
		UpdateManager.updateDelegate -= SaveScreen.S.Loop;
	}

	// ************ SHOP SCREEN ************ \\ 
	public void ShopScreenOn () {
		// Activate Shop Screen
		ShopScreen.S.gameObject.SetActive (true);

		// Set Selected GameObject (Save Screen: Shop Slot 1)
		Utilities.S.SetSelectedGO(ShopScreen.S.itemButtons[0].gameObject);

		// Freeze Player
		RPG.S.paused = true;
		Player.S.mode = eRPGMode.idle;
	
		// Update Delgate
		UpdateManager.updateDelegate += ShopScreen.S.Loop;
	}

	public void ShopScreenOff () {
		// Unpause
		RPG.S.paused = false;

		// Deactivate PauseMessage
		PauseMessage.S.gameObject.SetActive (false);

		// Deactivate PlayerButtons
		playerButtonsGO.SetActive(false);

		// Deactivate Shop Screen
		ShopScreen.S.gameObject.SetActive (false);

		// Update Delegate
		UpdateManager.updateDelegate -= ShopScreen.S.Loop;
	}
}
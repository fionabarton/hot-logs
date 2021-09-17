using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EquipScreen Mode/Step 1: PickPartyMember
/// - Select which party member to equip
/// </summary>
public class EquipScreen_PickPartyMemberMode : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static EquipScreen_PickPartyMemberMode _S;
	public static EquipScreen_PickPartyMemberMode S { get { return _S; } set { _S = value; } }

	void Awake() {
		S = this;
	}

	public void SetUp(EquipScreen equipScreen) {
		// Set anim
		equipScreen.playerAnim.CrossFade("Idle", 0);

		// Switch mode
		equipScreen.SwitchMode(eEquipScreenMode.pickPartyMember, PlayerButtons.S.buttonsCS[equipScreen.playerNdx].gameObject, false);

		// Buttons Interactable
		Utilities.S.ButtonsInteractable(equipScreen.equippedButtons, false);
		Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, true);
		Utilities.S.ButtonsInteractable(PauseScreen.S.buttonCS, false);

		// Remove & Add Listeners
		Utilities.S.RemoveListeners(PlayerButtons.S.buttonsCS);
		PlayerButtons.S.buttonsCS[0].onClick.AddListener(delegate { EquipScreen_PickTypeToEquipMode.S.SetUp(0, equipScreen); });
		PlayerButtons.S.buttonsCS[1].onClick.AddListener(delegate { EquipScreen_PickTypeToEquipMode.S.SetUp(1, equipScreen); });

		// Activate PlayerButtons
		PlayerButtons.S.gameObject.SetActive(true);

		// Display Text
		PauseMessage.S.DisplayText("Assign whose equipment?!");

		// Activate Cursor
		ScreenCursor.S.cursorGO.SetActive(true);
	}

	public void Loop(EquipScreen equipScreen) {
		if (EquipScreen.S.previousSelectedGameObject != UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject) {
			// Position Cursor
			Utilities.S.PositionCursor(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject, 0, 60, 3);

			// Display currently selected Member's Stats/Equipment 
			for (int i = 0; i < PlayerButtons.S.buttonsCS.Count; i++) {
				// Set anims
				PlayerButtons.S.anim[i].CrossFade("Idle", 0);

				if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == PlayerButtons.S.buttonsCS[i].gameObject) {
					// Cache Selected Gameobject 
					EquipScreen.S.previousSelectedGameObject = PlayerButtons.S.buttonsCS[i].gameObject;

					equipScreen.DisplayCurrentStats(i);
					equipScreen.DisplayCurrentEquipmentNames(i);

					// Set anim
					equipScreen.playerAnim.runtimeAnimatorController = PlayerButtons.S.anim[i].runtimeAnimatorController;
					PlayerButtons.S.anim[i].CrossFade("Walk", 0);
				}
			}
		}

		// Deactivate EquipScreen
		if (Input.GetButtonDown("SNES B Button")) {
			equipScreen.Deactivate();
		}
	}
}
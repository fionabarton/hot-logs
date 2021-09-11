using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System;

/// <summary>
/// Outside of battle, handles what happens when a spell button is clicked
/// </summary>
public class WorldSpells : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static WorldSpells _S;
	public static WorldSpells S { get { return _S; } set { _S = value; } }

	void Awake() {
		S = this;
	}

	//public void AddFunctionToButton(Action<int> functionToPass, string messageToDisplay) {
	//	// Buttons Interactable
	//	Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, true);
	//	Utilities.S.ButtonsInteractable(ItemScreen.S.itemButtons, false);

	//	// Set Selected GameObject
	//	Utilities.S.SetSelectedGO(PlayerButtons.S.buttonsCS[0].gameObject);

	//	// Remove Listeners
	//	Utilities.S.RemoveListeners(PlayerButtons.S.buttonsCS);

	//	// Display Text
	//	PauseMessage.S.DisplayText(messageToDisplay);

	//	// Add Listeners
	//	PlayerButtons.S.buttonsCS[0].onClick.AddListener(delegate { functionToPass(0); });
	//	PlayerButtons.S.buttonsCS[1].onClick.AddListener(delegate { functionToPass(1); });
	//}

	//////////////////////////////////////////////////////////
	/// Heal
	//////////////////////////////////////////////////////////

	// Select which party member to heal
	public void SelectPartyMemberToHeal() { // Overworld
		//if (Stats.S.MP[SpellScreen.S.playerNdx] >= 3) {
		if (Stats.member[SpellScreen.S.playerNdx].MP >= 3) {
			// Set animation to idle
			PlayerButtons.S.SetAnim("Idle");

			// Buttons Interactable
			Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, true);
			Utilities.S.ButtonsInteractable(SpellScreen.S.spellsButtons, false);
			// Set Selected GameObject
			Utilities.S.SetSelectedGO(SpellScreen.S.previousSelectedPlayerGO);

			// Add Listeners
			PlayerButtons.S.buttonsCS[0].onClick.AddListener(delegate { HealSelectedPartyMember(0); });
			PlayerButtons.S.buttonsCS[1].onClick.AddListener(delegate { HealSelectedPartyMember(1); });

			// Display Text
			PauseMessage.S.DisplayText("Heal which party member?");

			SpellScreen.S.canUpdate = true;
			// Switch ScreenMode
			SpellScreen.S.mode = eSpellScreenMode.pickWhichMemberToHeal;
		} else {
			SpellManager.S.CantUseSpell("Not enough MP to cast this spell!");
		}
	}

	// Heal the selected party member 
	public void HealSelectedPartyMember(int ndx) { // Overworld
		//if (Stats.S.HP[ndx] < Stats.S.maxHP[ndx]) {
		if (Stats.member[ndx].HP < Stats.member[ndx].maxHP) {
			// Set animation to success
			PlayerButtons.S.anim[ndx].CrossFade("Success", 0);

			// Subtract Spell cost from CASTING Player's MP 
			RPG.S.SubtractPlayerMP(SpellScreen.S.playerNdx, 3);

			// Add 30-45 HP to TARGET Player's HP
			int randomValue = UnityEngine.Random.Range(30, 45);
			RPG.S.AddPlayerHP(ndx, randomValue);

			// Display Text
			//if (Stats.S.HP[ndx] >= Stats.S.maxHP[ndx]) {
			if (Stats.member[ndx].HP >= Stats.member[ndx].maxHP) {
				//PauseMessage.S.DisplayText("Used Heal Spell!\nHealed " + Stats.S.playerName[ndx] + " back to Max HP!");
				PauseMessage.S.DisplayText("Used Heal Spell!\nHealed " + Stats.member[ndx].name + " back to Max HP!");
			} else {
				//PauseMessage.S.DisplayText("Used Heal Spell!\nHealed " + Stats.S.playerName[ndx] + " for " + randomValue + " HP!");
				PauseMessage.S.DisplayText("Used Heal Spell!\nHealed " + Stats.member[ndx].name + " for " + randomValue + " HP!");
			}
		} else {
			// Display Text
			//PauseMessage.S.DisplayText(Stats.S.playerName[ndx] + " already at full health...\n...no need to cast this spell!");
			PauseMessage.S.DisplayText(Stats.member[ndx].name + " already at full health...\n...no need to cast this spell!");
		}
		SpellManager.S.SpellHelper();
	}

	//////////////////////////////////////////////////////////
	/// Warp
	//////////////////////////////////////////////////////////

	public void WarpSpell() {
		//if (Stats.S.MP[0] >= 3) {
		if (Stats.member[0].MP >= 3) {
			// Warp to a random location...
			// ...but this should open a list of destinations to choose from instead 
			if (UnityEngine.Random.value > 0.5f) {
				StartCoroutine(WarpManager.S.Warp(Vector3.zero, true, "Town_1"));
			} else {
				StartCoroutine(WarpManager.S.Warp(new Vector3(0, -1, 0), true, "Area_2"));
			}
		} else {
			SpellManager.S.CantUseSpell("Not enough MP to cast this spell!");
		}
	}
}
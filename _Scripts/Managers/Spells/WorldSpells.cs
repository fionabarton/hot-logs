using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

    public void AddFunctionToButton(Action<int> functionToPass, string messageToDisplay, Spell spell) {
		if (Party.stats[SpellScreen.S.playerNdx].MP >= spell.cost) {
			// Audio: Confirm
			AudioManager.S.PlaySFX(eSoundName.confirm);

			// Buttons Interactable
			Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, true);
			Utilities.S.ButtonsInteractable(SpellScreen.S.spellsButtons, false);

			// Set Selected GameObject
			Utilities.S.SetSelectedGO(SpellScreen.S.previousSelectedPlayerGO);

			// Set previously selected GameObject
			SpellScreen_PickWhichMemberToHeal.S.previousSelectedPlayerGO = SpellScreen.S.previousSelectedPlayerGO;

			// Display Text
			PauseMessage.S.DisplayText(messageToDisplay);

			// Add Listeners
			PlayerButtons.S.buttonsCS[0].onClick.AddListener(delegate { functionToPass(0); });
			PlayerButtons.S.buttonsCS[1].onClick.AddListener(delegate { functionToPass(1); });

			SpellScreen.S.canUpdate = true;
		} else {
			SpellManager.S.CantUseSpell("Not enough MP to cast this spell!");
			return;
		}

		// If multiple targets
		if (!spell.multipleTargets) {
			// Set animation to idle
			PlayerButtons.S.SetSelectedAnim("Idle");

			SpellScreen.S.mode = eSpellScreenMode.pickWhichMemberToHeal;
		} else {
			// Set cursor positions
			Utilities.S.PositionCursor(PlayerButtons.S.buttonsCS[0].gameObject, 0, 60, 3, 0);
			Utilities.S.PositionCursor(PlayerButtons.S.buttonsCS[1].gameObject, 0, 60, 3, 1);
			
			// Set animations to walk
			PlayerButtons.S.anim[0].CrossFade("Walk", 0);
			PlayerButtons.S.anim[1].CrossFade("Walk", 0);

			// Set button colors
			PlayerButtons.S.SetButtonsColor(PlayerButtons.S.buttonsCS, new Color32(253, 255, 116, 255));

			// Activate cursors
			Utilities.S.SetActiveList(ScreenCursor.S.cursorGO, true);

			SpellScreen.S.mode = eSpellScreenMode.pickAllMembersToHeal;
		}
	}

	//////////////////////////////////////////////////////////
	/// Heal - Heal the selected party member 
	//////////////////////////////////////////////////////////
	public void HealSelectedPartyMember(int ndx) { 
		if (Party.stats[ndx].HP < Party.stats[ndx].maxHP) {
			// Set animation to success
			PlayerButtons.S.anim[ndx].CrossFade("Success", 0);

			// Subtract Spell cost from CASTING Player's MP 
			RPG.S.SubtractPlayerMP(SpellScreen.S.playerNdx, 3);

			// Add 30-45 HP to TARGET Player's HP
			int randomValue = UnityEngine.Random.Range(30, 45);
			RPG.S.AddPlayerHP(ndx, randomValue);

			// Display Text
			if (Party.stats[ndx].HP >= Party.stats[ndx].maxHP) {
				PauseMessage.S.DisplayText("Used Heal Spell!\nHealed " + Party.stats[ndx].name + " back to Max HP!");
			} else {
				PauseMessage.S.DisplayText("Used Heal Spell!\nHealed " + Party.stats[ndx].name + " for " + randomValue + " HP!");
			}

			// Audio: Buff 1
			AudioManager.S.PlaySFX(eSoundName.buff1);
		} else {
			// Display Text
			PauseMessage.S.DisplayText(Party.stats[ndx].name + " already at full health...\n...no need to cast this spell!");

			// Set animation to idle
			PlayerButtons.S.anim[ndx].CrossFade("Idle", 0);

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);
		}
		SpellManager.S.SpellHelper();
	}

	//////////////////////////////////////////////////////////
	/// Warp
	//////////////////////////////////////////////////////////
	public void WarpSpell() {
		if (Party.stats[0].MP >= 1) {
			// Subtract Spell cost from CASTING Player's MP 
			RPG.S.SubtractPlayerMP(SpellScreen.S.playerNdx, 1);

			SpellScreen.S.mode = eSpellScreenMode.pickWhereToWarp;

			// Set Selected GameObject
			Utilities.S.SetSelectedGO(SpellScreen.S.spellsButtons[0].gameObject);

			// Set previously selected GameObject
			WarpManager.S.previousSelectedLocationGO = SpellScreen.S.spellsButtons[0].gameObject;

			// Use SpellScreen's buttons to select/display warp locations
			WarpManager.S.DeactivateUnusedButtonSlots(SpellScreen.S.spellsButtons);
			WarpManager.S.AssignButtonEffect(SpellScreen.S.spellsButtons);
			WarpManager.S.AssignButtonNames(SpellScreen.S.spellsButtonNameTexts);
			WarpManager.S.SetButtonNavigation(SpellScreen.S.spellsButtons);

			// Audio: Confirm
			AudioManager.S.PlaySFX(eSoundName.confirm);
		} else {
			SpellManager.S.CantUseSpell("Not enough MP to cast this spell!");
		}
	}

	//////////////////////////////////////////////////////////
	/// Heal All - Heal all party members 
	//////////////////////////////////////////////////////////
	public void HealAllPartyMembers(int unusedIntBecauseOfAddFunctionToButtonParameter = 0) {
		int totalAmountToHeal = 0;

		if (Party.stats[0].HP < Party.stats[0].maxHP ||
			Party.stats[1].HP < Party.stats[1].maxHP) {
			// Subtract Spell cost from Player's MP
			RPG.S.SubtractPlayerMP(SpellScreen.S.playerNdx, 6);

			for (int i = 0; i < Party.stats.Count; i++) {
				// Get amount and max amount to heal
				int amountToHeal = UnityEngine.Random.Range(12, 20);
				int maxAmountToHeal = Party.stats[i].maxHP - Party.stats[i].HP;
				// Add Player's WIS to Heal Amount
				amountToHeal += Party.stats[i].WIS;

				// Add 12-20 HP to TARGET Player's HP
				RPG.S.AddPlayerHP(i, amountToHeal);

				// Cap amountToHeal to maxAmountToHeal
				if (amountToHeal >= maxAmountToHeal) {
					amountToHeal = maxAmountToHeal;
				}

				totalAmountToHeal += amountToHeal;
			}

			// Display Text
			PauseMessage.S.DisplayText("Used Heal All Spell!\nHealed ALL party members for an average of "
				+ Utilities.S.CalculateAverage(totalAmountToHeal, Party.stats.Count) + " HP!");

			// Set animations to success
			PlayerButtons.S.anim[0].CrossFade("Success", 0);
			PlayerButtons.S.anim[1].CrossFade("Success", 0);

			// Audio: Buff 1
			AudioManager.S.PlaySFX(eSoundName.buff1);
		} else {
			// Display Text
			PauseMessage.S.DisplayText("The party is already at full health...\n...no need to cast this spell!");

			// Set animations to idle
			PlayerButtons.S.anim[0].CrossFade("Idle", 0);
			PlayerButtons.S.anim[1].CrossFade("Idle", 0);

			// Audio: Deny
			AudioManager.S.PlaySFX(eSoundName.deny);
		}

		// Reset button colors
		PlayerButtons.S.SetButtonsColor(PlayerButtons.S.buttonsCS, new Color32(255, 255, 255, 200));

		// Deactivate screen cursors
		Utilities.S.SetActiveList(ScreenCursor.S.cursorGO, false);

		SpellManager.S.SpellHelper();
	}
}
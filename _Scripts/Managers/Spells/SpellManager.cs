using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellManager : MonoBehaviour {
    [Header("Set Dynamically")]
    // Singleton
    private static SpellManager _S;
    public static SpellManager S { get { return _S; } set { _S = value; } }

	public Spell[] spells;

	void Awake() {
        S = this;

		spells = new Spell[9];

		// Heal
		spells[0] = new Spell("Heal", 
			eSpellType.healing, eSpellStatEffect.HP, eSpellUseableMode.any, 30, 45, 3,
			"Replenishes at least 30 HP! What a heal spell!" + "\n Cost: 3 MP");

		// Fireball
		spells[1] = new Spell("Fireball",
			eSpellType.offensive, eSpellStatEffect.HP, eSpellUseableMode.battle, 8, 12, 2,
			"Blasts Enemy for at least 8 HP! What a damage spell!" + "\n Cost: 2 MP");

		// Warp
		spells[2] = new Spell("Warp",
			eSpellType.world, eSpellStatEffect.none, eSpellUseableMode.world, 0, 0, 1,
			"Warp to a previously visited land!" + "\n Cost: 1 MP");

		// Fireblast
		spells[3] = new Spell("Fireblast",
			eSpellType.offensive, eSpellStatEffect.HP, eSpellUseableMode.battle, 12, 20, 3,
			"Blasts ALL enemies for at least 12 HP! Hot damn!" + "\n Cost: 3 MP", true);

		// Heal All
		spells[4] = new Spell("Heal All",
			eSpellType.healing, eSpellStatEffect.HP, eSpellUseableMode.battle, 12, 20, 6,
			"Heals ALL party members for at least 12 HP!" + "\n Cost: 6 MP", true);

		// Revive
		spells[5] = new Spell("Revive",
			eSpellType.healing, eSpellStatEffect.HP, eSpellUseableMode.battle, 12, 20, 6,
			"Revives a fallen party member and restores a small amount of their HP." + "\n Cost: 6 MP");
	}

	// Spell Utilities ////////////////////////////////////////////

	public void SpellHelper() {
		// Buttons Interactable
		Utilities.S.ButtonsInteractable(PlayerButtons.S.buttonsCS, false);
		Utilities.S.ButtonsInteractable(SpellScreen.S.spellsButtons, false);

		// Update GUI
		PlayerButtons.S.UpdateGUI();
		PauseScreen.S.UpdateGUI();

		// Deactivate screen cursors
		Utilities.S.SetActiveList(ScreenCursor.S.cursorGO, false);

		SpellScreen.S.canUpdate = true;

		// Switch ScreenMode 
		SpellScreen.S.mode = eSpellScreenMode.usedSpell;
	}

	public void CantUseSpell(string message) {
		PauseMessage.S.DisplayText(message);

		// Audio: Deny
		AudioManager.S.PlaySFX(eSoundName.deny);

		// if Battle
		if (RPG.S.currentSceneName == "Battle") {
			Utilities.S.RemoveListeners(SpellScreen.S.spellsButtons);

			SpellScreen.S.canUpdate = true;

			// Switch ScreenMode 
			SpellScreen.S.mode = eSpellScreenMode.cantUseSpell;
        } else {
			SpellHelper();
		}
	}
}

public enum eSpellUseableMode { battle, world, any };
public enum eSpellType { healing, offensive, world };
public enum eSpellStatEffect { HP, MP, STR, DEF, WIS, AGI, none };
public class Spell {
	public string name;
	public eSpellType type;
	public eSpellStatEffect statEffect;
	public eSpellUseableMode useableMode;
	public int statEffectMinValue;
	public int statEffectMaxValue;
	public int cost;
	public string description;
	public bool multipleTargets;

	public Spell(string spellName,
				eSpellType spellType, eSpellStatEffect spellStatEffect, eSpellUseableMode spellUseableMode,
				int spellStatEffectMinValue, int spellStatEffectMaxValue, int spellCost,
				string spellDescription, bool spellMultipleTargets = false) {
		name = spellName;
		type = spellType;
		statEffect = spellStatEffect;
		useableMode = spellUseableMode;
		statEffectMinValue = spellStatEffectMinValue;
		statEffectMaxValue = spellStatEffectMaxValue;
		cost = spellCost;
		description = spellDescription;
		multipleTargets = spellMultipleTargets;
	}
}
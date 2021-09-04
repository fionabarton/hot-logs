using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipStatsEffect : MonoBehaviour {
	[Header("Set in Inspector")]
	// Potential Stats
	public GameObject		potentialStatHolder;
	public Text				currentAttributeAmounts; // STR, DEF, WIS, AGI
	public Text				potentialStats;
	public List<GameObject> arrowGO;
	public List<Animator>	arrowAnim;

	[Header("Set Dynamically")]
	// Singleton
	private static EquipStatsEffect _S;
	public static EquipStatsEffect S { get { return _S; } set { _S = value; } }

	void Awake() {
		S = this;
	}

	void OnDisable() {
		// Deactivate Arrow Sprites
		for (int i = 0; i <= arrowGO.Count - 1; i++) {
			arrowGO[i].SetActive(false);
		}
	}

	// Add item's stat effect to party member's stats
	public void AddItemEffect(int playerNdx, Item item) {
		item.isEquipped = true;

		switch (item.statEffect) {
			case eItemStatEffect.AGI: PartyStats.S.AGI[playerNdx] += item.statEffectValue; break;
			case eItemStatEffect.DEF: PartyStats.S.DEF[playerNdx] += item.statEffectValue; break;
			case eItemStatEffect.HP: PartyStats.S.HP[playerNdx] += item.statEffectValue; break;
			case eItemStatEffect.MP: PartyStats.S.MP[playerNdx] += item.statEffectValue; break;
			case eItemStatEffect.STR: PartyStats.S.STR[playerNdx] += item.statEffectValue; break;
			case eItemStatEffect.WIS: PartyStats.S.WIS[playerNdx] += item.statEffectValue; break;
		}
	}

	// Remove item's stat effect from party member's stats
	public void RemoveItemEffect(int playerNdx, Item item) {
		item.isEquipped = false;

		// Subtract Item Effect
		switch (item.statEffect) {
			case eItemStatEffect.AGI: PartyStats.S.AGI[playerNdx] -= item.statEffectValue; break;
			case eItemStatEffect.DEF: PartyStats.S.DEF[playerNdx] -= item.statEffectValue; break;
			case eItemStatEffect.HP: PartyStats.S.HP[playerNdx] -= item.statEffectValue; break;
			case eItemStatEffect.MP: PartyStats.S.MP[playerNdx] -= item.statEffectValue; break;
			case eItemStatEffect.STR: PartyStats.S.STR[playerNdx] -= item.statEffectValue; break;
			case eItemStatEffect.WIS: PartyStats.S.WIS[playerNdx] -= item.statEffectValue; break;
		}
	}

	// Display party member's stats if they equipped this item
	public void DisplayPotentialStats(int playerNdx, Item tItem, List<List<Item>> playerEquipment) {
		// Deactivate Arrow GameObjects
		for (int i = 0; i <= arrowGO.Count - 1; i++) {
			arrowGO[i].SetActive(false);
		}

		// Get Current Stats
		List<int> potential = new List<int>() { PartyStats.S.STR[playerNdx], PartyStats.S.DEF[playerNdx], PartyStats.S.WIS[playerNdx], PartyStats.S.AGI[playerNdx] };

        // Subtract stats of currently equipped item 
        switch (playerEquipment[playerNdx][(int)tItem.type].statEffect) {
            case eItemStatEffect.STR: potential[0] -= playerEquipment[playerNdx][(int)tItem.type].statEffectValue; break;
            case eItemStatEffect.DEF: potential[1] -= playerEquipment[playerNdx][(int)tItem.type].statEffectValue; break;
            case eItemStatEffect.WIS: potential[2] -= playerEquipment[playerNdx][(int)tItem.type].statEffectValue; break;
            case eItemStatEffect.AGI: potential[3] -= playerEquipment[playerNdx][(int)tItem.type].statEffectValue; break;
        }

        // Add stats of item to be potentially equipped
        switch (tItem.statEffect) {
			case eItemStatEffect.STR: potential[0] += tItem.statEffectValue; break;
			case eItemStatEffect.DEF: potential[1] += tItem.statEffectValue; break;
			case eItemStatEffect.WIS: potential[2] += tItem.statEffectValue; break;
			case eItemStatEffect.AGI: potential[3] += tItem.statEffectValue; break;
		}

		// Find difference between current & potential Stats
		List<int> statDifference = new List<int>() { potential[0] - PartyStats.S.STR[playerNdx], potential[1] - PartyStats.S.DEF[playerNdx], potential[2] - PartyStats.S.WIS[playerNdx], potential[3] - PartyStats.S.AGI[playerNdx] };

		// If Current Stats != Potential Stats, activate potential stats & arrows
		if (potential[0] != PartyStats.S.STR[playerNdx]) {
			ActivatePotentialStatsAndArrow(0, statDifference[0]);
		}
		if (potential[1] != PartyStats.S.DEF[playerNdx]) {
			ActivatePotentialStatsAndArrow(1, statDifference[1]);
		}
		if (potential[2] != PartyStats.S.WIS[playerNdx]) {
			ActivatePotentialStatsAndArrow(2, statDifference[2]);
		}
		if (potential[3] != PartyStats.S.AGI[playerNdx]) {
			ActivatePotentialStatsAndArrow(3, statDifference[3]);
		}

		// Update GUI
		potentialStats.text = potential[0] + "\n" + potential[1] + "\n" + potential[2] + "\n" + potential[3];
	}

	// Activate potential stats and animate up or down arrows 
	void ActivatePotentialStatsAndArrow(int ndx, int amount) {
		// Activate Potential Stat
		potentialStatHolder.SetActive(true);

		// Activate Arrow GameObject
		arrowGO[ndx].SetActive(true);

		// Set Arrow Animation
		if (amount > 0) {
			arrowAnim[ndx].CrossFade("Arrow_Up", 0);
		} else {
			arrowAnim[ndx].CrossFade("Arrow_Down", 0);
		}
	}
}
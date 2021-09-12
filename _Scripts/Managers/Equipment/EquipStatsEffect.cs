﻿using System.Collections;
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
			case eItemStatEffect.AGI: Party.stats[playerNdx].AGI += item.statEffectValue; break;
			case eItemStatEffect.DEF: Party.stats[playerNdx].DEF += item.statEffectValue; break;
			case eItemStatEffect.HP: Party.stats[playerNdx].HP += item.statEffectValue; break;
			case eItemStatEffect.MP: Party.stats[playerNdx].MP += item.statEffectValue; break;
			case eItemStatEffect.STR: Party.stats[playerNdx].STR += item.statEffectValue; break;
			case eItemStatEffect.WIS: Party.stats[playerNdx].WIS += item.statEffectValue; break;
		}
	}

	// Remove item's stat effect from party member's stats
	public void RemoveItemEffect(int playerNdx, Item item) {
		item.isEquipped = false;

		// Subtract Item Effect
		switch (item.statEffect) {
			case eItemStatEffect.AGI: Party.stats[playerNdx].AGI -= item.statEffectValue; break;
			case eItemStatEffect.DEF: Party.stats[playerNdx].DEF -= item.statEffectValue; break;
			case eItemStatEffect.HP: Party.stats[playerNdx].HP -= item.statEffectValue; break;
			case eItemStatEffect.MP: Party.stats[playerNdx].MP -= item.statEffectValue; break;
			case eItemStatEffect.STR: Party.stats[playerNdx].STR -= item.statEffectValue; break;
			case eItemStatEffect.WIS: Party.stats[playerNdx].WIS -= item.statEffectValue; break;
		}
	}

	// Display party member's stats if they equipped this item
	public void DisplayPotentialStats(int playerNdx, Item tItem, List<List<Item>> playerEquipment) {
		// Deactivate Arrow GameObjects
		for (int i = 0; i <= arrowGO.Count - 1; i++) {
			arrowGO[i].SetActive(false);
		}

		// Get Current Stats
		List<int> potential = new List<int>() { Party.stats[playerNdx].STR, Party.stats[playerNdx].DEF, Party.stats[playerNdx].WIS, Party.stats[playerNdx].AGI };

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
		List<int> statDifference = new List<int>() { potential[0] - Party.stats[playerNdx].STR, potential[1] - Party.stats[playerNdx].DEF, potential[2] - Party.stats[playerNdx].WIS, potential[3] - Party.stats[playerNdx].AGI };

		// If Current Stats != Potential Stats, activate potential stats & arrows
		if (potential[0] != Party.stats[playerNdx].STR) {
			ActivatePotentialStatsAndArrow(0, statDifference[0]);
		}
		if (potential[1] != Party.stats[playerNdx].DEF) {
			ActivatePotentialStatsAndArrow(1, statDifference[1]);
		}
		if (potential[2] != Party.stats[playerNdx].WIS) {
			ActivatePotentialStatsAndArrow(2, statDifference[2]);
		}
		if (potential[3] != Party.stats[playerNdx].AGI) {
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
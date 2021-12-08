using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopScreen_DisplayPotentialStats : MonoBehaviour {
	[Header("Set in Inspector")]
	public List<Text> nameText = new List<Text>();
	public List<Text> statsNameText = new List<Text>();
	public List<Text> statsAmountText = new List<Text>();

	public List<Animator> anim;

	[Header("Set Dynamically")]
	// Singleton
	private static ShopScreen_DisplayPotentialStats _S;
	public static ShopScreen_DisplayPotentialStats S { get { return _S; } set { _S = value; } }

	void Awake() {
		S = this;
	}

    public void ActivatePotentialStats() {
		// Deactivate all potential stats gameObjects
		for (int i = 0; i < nameText.Count; i++) {
			nameText[i].gameObject.transform.parent.gameObject.SetActive(false);
		}

		// Set party member names
		for (int i = 0; i <= Party.S.partyNdx; i++) {
			nameText[i].text = Party.S.stats[i].name;
		}
	}

	// Display party member's stats if they potentially equipped this item
	public void DisplayPotentialStats(Item tItem) {
		// Loop through all party members
		for (int i = 0; i <= Party.S.partyNdx; i++) {
			// If it's not equippable, deactivate and skip over this iteration
			if (tItem.statEffect != eItemStatEffect.STR && tItem.statEffect != eItemStatEffect.DEF && tItem.statEffect != eItemStatEffect.WIS && tItem.statEffect != eItemStatEffect.AGI) {
				nameText[i].gameObject.transform.parent.gameObject.SetActive(false);
				continue;
			}

			nameText[i].gameObject.transform.parent.gameObject.SetActive(true);

			// Get Current Stats
			List<int> potential = new List<int>() { Party.S.stats[i].STR, Party.S.stats[i].DEF, Party.S.stats[i].WIS, Party.S.stats[i].AGI };

			// Subtract stats of currently equipped item 
			switch (EquipScreen.S.playerEquipment[i][(int)tItem.type].statEffect) {
				case eItemStatEffect.STR: potential[0] -= EquipScreen.S.playerEquipment[i][(int)tItem.type].statEffectMaxValue; break;
				case eItemStatEffect.DEF: potential[1] -= EquipScreen.S.playerEquipment[i][(int)tItem.type].statEffectMaxValue; break;
				case eItemStatEffect.WIS: potential[2] -= EquipScreen.S.playerEquipment[i][(int)tItem.type].statEffectMaxValue; break;
				case eItemStatEffect.AGI: potential[3] -= EquipScreen.S.playerEquipment[i][(int)tItem.type].statEffectMaxValue; break;
			}

			// Add stats of item to be potentially equipped
			switch (tItem.statEffect) {
				case eItemStatEffect.STR: potential[0] += tItem.statEffectMaxValue; break;
				case eItemStatEffect.DEF: potential[1] += tItem.statEffectMaxValue; break;
				case eItemStatEffect.WIS: potential[2] += tItem.statEffectMaxValue; break;
				case eItemStatEffect.AGI: potential[3] += tItem.statEffectMaxValue; break;
			}

			// Find difference between current & potential Stats
			List<int> statDifference = new List<int>() { potential[0] - Party.S.stats[i].STR, potential[1] - Party.S.stats[i].DEF, potential[2] - Party.S.stats[i].WIS, potential[3] - Party.S.stats[i].AGI };

			string nameString = "";
			string amountString = "";
			int totaStatDifference = 0;

			// Build strings
			if (statDifference[0] != 0) {
				nameString += "STR\n";

				if (statDifference[0] > 0) {
					amountString += "+";
				}

				amountString += statDifference[0] + "\n";
				totaStatDifference += statDifference[0];
			}
			if (statDifference[1] != 0) {
				nameString += "DEF\n";

				if (statDifference[1] > 0) {
					amountString += "+";
				}

				amountString += statDifference[1] + "\n";
				totaStatDifference += statDifference[1];
			}
			if (statDifference[2] != 0) {
				nameString += "WIS\n";

				if (statDifference[2] > 0) {
					amountString += "+";
				}

				amountString += statDifference[2] + "\n";
				totaStatDifference += statDifference[2];
			}
			if (statDifference[3] != 0) {
				nameString += "AGI\n";

				if (statDifference[3] > 0) {
					amountString += "+";
				}

				amountString += statDifference[3] + "\n";
				totaStatDifference += statDifference[3];
			}

			// Update GUI
			statsNameText[i].text = nameString;
			statsAmountText[i].text = amountString;

			// Set animation
			if(totaStatDifference > 0) {
				anim[i].CrossFade("Success", 0);
            } else if (totaStatDifference < 0) {
				anim[i].CrossFade("Fail", 0);
            } else {
				anim[i].CrossFade("Idle", 0);
			}
		}
	}
}
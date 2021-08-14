using System.Collections;
using System.Collections.Generic;
using UnityEngine;
	
// AI, Status Effects (Poison, Blind, Confuse, etc.)

public class Stats : MonoBehaviour {
	[Header("Set in Inspector")] // only initial Lvl 1 Stats
	// Player Stats
	public List <string> 	playerName;
	public List <int> 		HP; 
	public List <int> 		MP;
	public List <int> 		STR;
	public List <int> 		DEF;
	public List <int> 		WIS;
	public List <int> 		AGI;
	public List <int> 		EXP;
	public List <int> 		LVL;
	public List <int> 		maxHP;
	public List <int> 		maxMP; 
	public List <int> 		spellNdx; // TBR

	// For Equipment
	public List <int> 		baseSTR;
	public List <int> 		baseDEF;
	public List <int> 		baseWIS;
	public List <int> 		baseAGI;
	public List <int> 		baseMaxHP;
	public List <int> 		baseMaxMP; 

	public int				Gold;

	[Header("Set Dynamically")]
	// Singleton
	private static Stats _S;
	public static Stats S { get { return _S; } set { _S = value; } }

	public int 				partyNdx;

	// Prevents setting HP & MP to max value every scene change
	public bool[] 	p1Lvl;
	public bool[] 	p2Lvl;

	// Checked in Battle.cs. Player would Level Up after every Battle without this bool.
	public bool[] 	hasLevelledUp;

	void Awake() {
		// Singleton
		S = this;
	}

	// Set initial Base Stats
	void Start(){
		for (int i = 0; i < partyNdx + 1; i++) {
			baseSTR [i] = STR [i];
			baseDEF [i] = DEF [i];
			baseWIS [i] = WIS [i];
			baseAGI [i] = AGI [i];
			baseMaxHP [i] = maxHP [i];
			baseMaxMP [i] = maxMP [i];
		}
	}

	// HP
	public int CalculateHP(int playerNdx){
		int tHP;
		switch (playerNdx) {
		case 0: 
			tHP = ((10) * (3 + LVL [playerNdx])); // Fiona: Lvl 1 = 40 
			break;
		case 1:
			tHP = ((8) * (3 + LVL [playerNdx])); // Chani: Lvl 1 = 32
			break;
		default: tHP = 0; break;
		}
		return tHP;
	}
	// MP
	public int CalculateMP(int playerNdx){
		int tMP = 0;
		switch (playerNdx) {
		case 0: 
			tMP = (6 * LVL [playerNdx]); // Fiona: Lvl 1 = 6
			break;
		case 1: 
			tMP = ((9 * LVL [playerNdx]) + 6); // Chani: Lvl 1 = 15
			break;
		default: tMP = 0; break;
		}
		return tMP;
	}
	// STR
	public int CalculateSTR(int playerNdx){
		int tSTR = 0;
		switch (playerNdx) {
		case 0: 
			tSTR = (int)(2 * LVL [playerNdx]); // Fiona: Lvl 1 = 2
			break;
		case 1: 
			tSTR = (int)(1.5f * LVL [playerNdx]); // Chani: Lvl 1 = 1
			break;
		default: tSTR = 0; break;
		}
		return tSTR;
	}
	// DEF
	public int CalculateDEF(int playerNdx){
		int tDEF = 0;
		switch (playerNdx) {
		case 0: 
			tDEF = (int)(2 * LVL [playerNdx]); // Fiona: Lvl 1 = 2
			break;
		case 1:
			tDEF = (int)(1.5f * LVL [playerNdx]); // Chani: Lvl 1 = 1
			break;
		default: tDEF = 0; break;
		}
		return tDEF;
	}
	// WIS
	public int CalculateWIS(int playerNdx){
		int tWIS = 0;
		switch (playerNdx) {
		case 0: 
			tWIS = (int)(1.5f * LVL [playerNdx]); // Fiona: Lvl 1 = 1
			break;
		case 1: 
			tWIS = (int)(2 * LVL [playerNdx]); // Chani: Lvl 1 = 2
			break;
		default: tWIS = 0; break;
		}
		return tWIS;
	}
	// AGI
	public int CalculateAGI(int playerNdx){
		int tAGI = 0;
		switch (playerNdx) {
		case 0: 
			tAGI = (int)(1.5f * LVL [playerNdx]); // Fiona: Lvl 1 = 1
			break;
		case 1: 
			tAGI = (int)(2 * LVL [playerNdx]); // Chani: Lvl 1 = 2
			break;
		default: tAGI = 0; break;
		}
		return tAGI;
	}
	// SpellNdx
	public int CalculateSpellNdx(int playerNdx){
		int tSpellNdx = 0;
		switch (playerNdx) {
		case 0: 
			tSpellNdx = (int)(0.5f * LVL [playerNdx]); // Fiona: Lvl 1 = 0
			break;
		case 1: 
			tSpellNdx = (int)(1.0f * LVL [playerNdx] + 1); // Chani: Lvl 1 = 2
			break;
		default: tSpellNdx = 0; break;
		}
		return tSpellNdx;
	}

	public void CheckForLevelUp () {
		if (EXP[0] > 2000 && !p1Lvl[10]) { InputStats (10, 0);
		} else if (EXP[0] > 1300  && !p1Lvl[9]) { InputStats (9, 0);
		} else if (EXP[0] > 800  && !p1Lvl[8]) { InputStats (8, 0);
		} else if (EXP[0] > 450 && !p1Lvl[7]) { InputStats (7, 0);
		} else if (EXP[0] > 220 && !p1Lvl[6]) { InputStats (6, 0);
		} else if (EXP[0] > 110 && !p1Lvl[5]) { InputStats (5, 0);
		} else if (EXP[0] > 47 &&  !p1Lvl[4]) { InputStats (4, 0);
		} else if (EXP[0] > 23 && !p1Lvl[3]) { InputStats (3, 0);
		} else if (EXP[0] > 7 && !p1Lvl[2]) { InputStats (2, 0);
			}
	
		if (Stats.S.partyNdx >= 1) {
			if (EXP[1] > 2100 && !p2Lvl [10]) { InputStats (10, 1);
			} else if (EXP[1] > 1300  && !p2Lvl [9]) { InputStats (9, 1);
			} else if (EXP[1] > 850  && !p2Lvl [8]) { InputStats (8, 1);
			} else if (EXP[1] > 450 && !p2Lvl [7]) { InputStats (7, 1);
			} else if (EXP[1] > 250 && !p2Lvl [6]) { InputStats (6, 1);
			} else if (EXP[1] > 110 && !p2Lvl [5]) { InputStats (5, 1);
			} else if (EXP[1] > 55 && !p2Lvl [4]) { InputStats (4, 1);
			} else if (EXP[1] > 23 && !p2Lvl [3]) { InputStats (3, 1);
			} else if (EXP[1] > 9 && !p2Lvl [2]) { InputStats (2, 1);
			}
		}
	}
		
	void InputStats (int newLVL, int playerNdx) {
		hasLevelledUp [playerNdx] = true;

		LVL[playerNdx] = newLVL;

		//Stats.S.spellNdx [playerNdx] = spellNdx;
		spellNdx [playerNdx] = CalculateSpellNdx(playerNdx);

		// Assign Stats
		maxHP[playerNdx] = CalculateHP (playerNdx);
		maxMP[playerNdx] = CalculateMP (playerNdx);
		STR[playerNdx] = CalculateSTR (playerNdx);
		AGI[playerNdx] = CalculateAGI (playerNdx);
		DEF[playerNdx] = CalculateDEF (playerNdx);
		WIS[playerNdx] = CalculateWIS (playerNdx);

		// Base Stats (w/o Equipment)
		baseMaxHP[playerNdx] = CalculateHP (playerNdx);
		baseMaxMP[playerNdx] = CalculateMP (playerNdx);
		baseSTR[playerNdx] = CalculateSTR (playerNdx);
		baseAGI[playerNdx] = CalculateAGI (playerNdx);
		baseDEF[playerNdx] = CalculateDEF (playerNdx);
		baseWIS[playerNdx] = CalculateWIS (playerNdx);

		// Add current equipment's stat effect(s) to party member's stats
		//if (EquipScreen.S.equippedArmor [playerNdx] != null) {
		//	EquipScreen.S.AddItemStatEffect (playerNdx, EquipScreen.S.equippedArmor [playerNdx]);
		//}
		//if (EquipScreen.S.equippedHelmet [playerNdx] != null) {
		//	EquipScreen.S.AddItemStatEffect (playerNdx, EquipScreen.S.equippedHelmet [playerNdx]);
		//}
		//if (EquipScreen.S.equippedOther [playerNdx] != null) {
		//	EquipScreen.S.AddItemStatEffect (playerNdx, EquipScreen.S.equippedOther [playerNdx]);
		//}
		//if (EquipScreen.S.equippedWeapon [playerNdx] != null) {
		//	EquipScreen.S.AddItemStatEffect (playerNdx, EquipScreen.S.equippedWeapon [playerNdx]);
		//}

        //for(int i = 0; i < EquipScreen.S.player1Equipment.Length; i++) {
        //	if (playerNdx == 0) {
        //		if (EquipScreen.S.player1Equipment[i] != null) {
        //			EquipScreen.S.AddItemStatEffect(playerNdx, EquipScreen.S.player1Equipment[i]);
        //		}
        //	} else {
        //		if (EquipScreen.S.player2Equipment[i] != null) {
        //			EquipScreen.S.AddItemStatEffect(playerNdx, EquipScreen.S.player2Equipment[i]);
        //		}
        //	}
        //}

        for (int i = 0; i < EquipScreen.S.playerEquipment[0].Count; i++) {
            if (EquipScreen.S.playerEquipment[playerNdx][i] != null) {
                EquipScreen.S.AddItemStatEffect(playerNdx, EquipScreen.S.playerEquipment[playerNdx][i]);
            }
        }

        // Set HP & MP to new max value
        HP[playerNdx] = maxHP[playerNdx];
		MP[playerNdx] = maxMP[playerNdx];

		// Mark that this Level has been reached
		if (playerNdx == 0) { p1Lvl [newLVL] = true; }
		if (playerNdx == 1) { p2Lvl [newLVL] = true; }
	}
}
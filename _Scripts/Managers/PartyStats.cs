using System.Collections;
using System.Collections.Generic;
using UnityEngine;
	
// AI, Status Effects (Poison, Blind, Confuse, etc.)

public class PartyStats : MonoBehaviour {
	[Header("Set in Inspector")] // Set the party's stats at Level 1
	// Party Stats
	public List <string> 		playerName;
	public List <int> 			HP; 
	public List <int> 			MP;
	public List <int> 			STR;
	public List <int> 			DEF;
	public List <int> 			WIS;
	public List <int> 			AGI;
	public List <int> 			EXP;
	public List <int> 			LVL;
	public List <int> 			maxHP;
	public List <int> 			maxMP;
	// When increased by 1, the party member "learns" 1 spell they're capable of learning (found in spells)
	public List <int> 			spellNdx;

	// Party Stats without their equipment
	public List <int> 			baseSTR;
	public List <int> 			baseDEF;
	public List <int> 			baseWIS;
	public List <int> 			baseAGI;
	public List <int> 			baseMaxHP;
	public List <int> 			baseMaxMP; 

	public int					Gold;

	[Header("Set Dynamically")]
	// Singleton
	private static PartyStats	_S;
	public static PartyStats	S { get { return _S; } set { _S = value; } }

	public List<Stats>			stats = new List<Stats>();  

	public int 					partyNdx;

    // Prevents setting HP & MP to max value every scene change
    public bool[]				p1Lvl;
    public bool[]				p2Lvl;
	
	// Spells each member is capable of learning
	public List<List<Spell>>	spells = new List<List<Spell>>();

	// Checked in Battle.cs. Player would Level Up after every Battle without this bool.
	public bool[] 				hasLevelledUp;

	void Awake() {
		S = this;
	}

	void Start(){
		// Set initial Base Stats
		for (int i = 0; i < partyNdx + 1; i++) {
			baseSTR [i] = STR [i];
			baseDEF [i] = DEF [i];
			baseWIS [i] = WIS [i];
			baseAGI [i] = AGI [i];
			baseMaxHP [i] = maxHP [i];
			baseMaxMP [i] = maxMP [i];
		}

		// Set which spells each member is capable of learning
		spells.Add(new List<Spell> { 
			SpellManager.S.spells[0], 
			SpellManager.S.spells[2]
		});
		spells.Add(new List<Spell> { 
			SpellManager.S.spells[1], 
			SpellManager.S.spells[3]
		});

		//// Player 1
		//stats.Add(new Stats("Blob", 40, 40, 40, 6, 6, 6,
		//	2, 2, 2, 2, 1, 1, 1, 1, 
		//	0, 1, 0, 
		//	new List<Spell> { SpellManager.S.spells[0], SpellManager.S.spells[2] },
		//	new List<bool>(30) )
		//);

		//// Player 2
		//stats.Add(new Stats("Bill", 32, 32, 32, 15, 15, 15,
		//	1, 1, 1, 1, 2, 2, 2, 2,
		//	0, 1, 0,
		//	new List<Spell> { SpellManager.S.spells[1], SpellManager.S.spells[3] },
		//	new List<bool>(30))
		//);
	}

	// HP
	public int CalculateHP(int playerNdx){
		int tHP;
		if (playerNdx == 0) {
			tHP = ((10) * (3 + LVL[playerNdx])); // Blob: Lvl 1 = 40
		} else {
			tHP = ((8) * (3 + LVL[playerNdx])); // Chani: Lvl 1 = 32
		}
		return tHP;
	}
	// MP
	public int CalculateMP(int playerNdx){
		int tMP; 
		if (playerNdx == 0) {
			tMP = (6 * LVL[playerNdx]); // Blob: Lvl 1 = 6
		} else {
			tMP = ((9 * LVL[playerNdx]) + 6); // Chani: Lvl 1 = 15
		}
		return tMP;
	}
	// STR
	public int CalculateSTR(int playerNdx){
		int tSTR;
		if (playerNdx == 0) {
			tSTR = (int)(2 * LVL[playerNdx]); // Blob: Lvl 1 = 2
		} else {
			tSTR = (int)(1.5f * LVL[playerNdx]); // Chani: Lvl 1 = 1
		}
		return tSTR;
	}
	// DEF
	public int CalculateDEF(int playerNdx){
		int tDEF;
		if (playerNdx == 0) {
			tDEF = (int)(2 * LVL[playerNdx]); // Blob: Lvl 1 = 2
		} else {
			tDEF = (int)(1.5f * LVL[playerNdx]); // Chani: Lvl 1 = 1
		}
		return tDEF;
	}
	// WIS
	public int CalculateWIS(int playerNdx){
		int tWIS;
		if (playerNdx == 0) {
			tWIS = (int)(1.5f * LVL[playerNdx]); // Blob: Lvl 1 = 1
        } else {
			tWIS = (int)(2 * LVL[playerNdx]); // Chani: Lvl 1 = 2
		}
		return tWIS;
	}
	// AGI
	public int CalculateAGI(int playerNdx){
		int tAGI;
		if (playerNdx == 0) {
			tAGI = (int)(1.5f * LVL[playerNdx]); // Blob: Lvl 1 = 1
		} else {
			tAGI = (int)(2 * LVL[playerNdx]); // Chani: Lvl 1 = 2
		}
		return tAGI;
	}

	// SpellNdx (Mathf.Min used to prevent spellNdx from exceeding
	// the amount of spells each member is capable of learning)
	public int CalculateSpellNdx(int playerNdx){
		int tSpellNdx;
		if (playerNdx == 0) {
			tSpellNdx = Mathf.Min((int)(0.5f * LVL[playerNdx]), spells[playerNdx].Count); // Blob: Lvl 1 = 0
		} else {
			tSpellNdx = Mathf.Min((int)(1.0f * LVL[playerNdx] + 1), spells[playerNdx].Count); // Chani: Lvl 1 = 2
		}
		return tSpellNdx;
	}

	public void CheckForLevelUp () {
		if (EXP[0] > 2000 && !p1Lvl[10]) { LevelUp (10, 0);
		} else if (EXP[0] > 1300  && !p1Lvl[9]) { LevelUp (9, 0);
		} else if (EXP[0] > 800  && !p1Lvl[8]) { LevelUp (8, 0);
		} else if (EXP[0] > 450 && !p1Lvl[7]) { LevelUp (7, 0);
		} else if (EXP[0] > 220 && !p1Lvl[6]) { LevelUp (6, 0);
		} else if (EXP[0] > 110 && !p1Lvl[5]) { LevelUp (5, 0);
		} else if (EXP[0] > 47 &&  !p1Lvl[4]) { LevelUp (4, 0);
		} else if (EXP[0] > 23 && !p1Lvl[3]) { LevelUp (3, 0);
		} else if (EXP[0] > 7 && !p1Lvl[2]) { LevelUp (2, 0);
		}
	
		if (partyNdx >= 1) {
			if (EXP[1] > 2100 && !p2Lvl [10]) { LevelUp (10, 1);
			} else if (EXP[1] > 1300  && !p2Lvl [9]) { LevelUp (9, 1);
			} else if (EXP[1] > 850  && !p2Lvl [8]) { LevelUp (8, 1);
			} else if (EXP[1] > 450 && !p2Lvl [7]) { LevelUp (7, 1);
			} else if (EXP[1] > 250 && !p2Lvl [6]) { LevelUp (6, 1);
			} else if (EXP[1] > 110 && !p2Lvl [5]) { LevelUp (5, 1);
			} else if (EXP[1] > 55 && !p2Lvl [4]) { LevelUp (4, 1);
			} else if (EXP[1] > 23 && !p2Lvl [3]) { LevelUp (3, 1);
			} else if (EXP[1] > 9 && !p2Lvl [2]) { LevelUp (2, 1);
			}
		}
	}
		
	void LevelUp (int newLVL, int playerNdx) {
		hasLevelledUp[playerNdx] = true;

		LVL[playerNdx] = newLVL;

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
        for (int i = 0; i < EquipScreen.S.playerEquipment[0].Count; i++) {
            if (EquipScreen.S.playerEquipment[playerNdx][i] != null) {
                EquipStatsEffect.S.AddItemEffect(playerNdx, EquipScreen.S.playerEquipment[playerNdx][i]);
            }
        }

        // Set HP & MP to new max value
        HP[playerNdx] = maxHP[playerNdx];
		MP[playerNdx] = maxMP[playerNdx];

        // Mark that this Level has been reached (and all previous levels)
		for (int i = 0; i < newLVL + 1; i++) {
			if (playerNdx == 0) { p1Lvl[i] = true; }
			if (playerNdx == 1) { p2Lvl[i] = true; }
		}
	}
}

public class Stats {
	public string name;
	public int HP;
	public int maxHP;
	public int baseMaxHP;
	public int MP;
	public int maxMP;
	public int baseMaxMP;
	
	public int STR;
	public int baseSTR;
	public int DEF;
	public int baseDEF;
	public int WIS;
	public int baseWIS;
	public int AGI;
	public int baseAGI;
	
	public int EXP;
	public int LVL;
	public int spellNdx;
	public List<Spell> spells;
	public List<bool> hasReachedThisLevel;

	public Stats(string name, int HP, int maxHP, int baseMaxHP, int MP, int maxMP, int baseMaxMP,
		int STR, int baseSTR, int DEF, int baseDEF, int WIS, int baseWIS, int AGI, int baseAGI,
		int EXP, int LVL, int spellNdx, List<Spell> spells, List<bool> hasReachedThisLevel) {
		this.name = name;
		this.HP = HP;
		this.maxHP = maxHP;
		this.baseMaxHP = baseMaxHP;
		this.MP = MP;
		this.maxMP = maxMP;
		this.baseMaxMP = baseMaxMP;

		this.STR = STR;
		this.baseSTR = baseSTR;
		this.DEF = DEF;
		this.baseDEF = baseDEF;
		this.WIS = WIS;
		this.baseWIS = baseWIS;
		this.AGI = AGI;
		this.baseAGI = baseAGI;
		
		this.EXP = EXP;
		this.LVL = LVL;
		this.spellNdx = spellNdx;
		this.spells = spells;
		this.hasReachedThisLevel = hasReachedThisLevel;
	}
}
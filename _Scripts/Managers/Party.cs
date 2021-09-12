using System.Collections;
using System.Collections.Generic;
using UnityEngine;
	
// AI, Status Effects (Poison, Blind, Confuse, etc.)

/// <summary>
/// Stores the party's stats
/// </summary>
public class Party : MonoBehaviour {
	[Header("Set in Inspector")] 
	public int						Gold;

	[Header("Set Dynamically")]
	// Singleton
	private static Party			_S;
	public static Party				S { get { return _S; } set { _S = value; } }

	public static List<PartyStats>	stats = new List<PartyStats>();  

	public int 						partyNdx;

	void Awake() {
		S = this;
	}

	void Start(){
        // Player 1
        stats.Add(new PartyStats("Blob", 40, 40, 40, 6, 6, 6,
            2, 2, 2, 2, 1, 1, 1, 1,
            0, 1, 0,
            new List<Spell> { SpellManager.S.spells[0], SpellManager.S.spells[2], SpellManager.S.spells[1], SpellManager.S.spells[3] },
            new List<bool>(new bool[30]), false)
        );

        // Player 2
        stats.Add(new PartyStats("Bill", 32, 32, 32, 15, 15, 15,
            1, 1, 1, 1, 2, 2, 2, 2,
            0, 1, 2,
            new List<Spell> { SpellManager.S.spells[1], SpellManager.S.spells[3], SpellManager.S.spells[0], SpellManager.S.spells[2] },
            new List<bool>(new bool[30]), false)
        );
    }

	// HP
	public void SetNewHP(int playerNdx) {
		if (playerNdx == 0) {
			stats[playerNdx].HP = ((10) * (3 + stats[playerNdx].LVL)); // Blob: Lvl 1 = 40
		} else {
			stats[playerNdx].HP = ((8) * (3 + stats[playerNdx].LVL)); // Chani: Lvl 1 = 32
		}
		stats[playerNdx].maxHP = stats[playerNdx].HP;
		stats[playerNdx].baseMaxHP = stats[playerNdx].HP;
	}
	// MP
	public void SetNewMP(int playerNdx) {
		if (playerNdx == 0) {
			stats[playerNdx].MP = (6 * stats[playerNdx].LVL); // Blob: Lvl 1 = 6
		} else {
			stats[playerNdx].MP = ((9 * stats[playerNdx].LVL) + 6); // Chani: Lvl 1 = 15
		}
		stats[playerNdx].maxMP = stats[playerNdx].MP;
		stats[playerNdx].baseMaxMP = stats[playerNdx].MP;
	}
	// STR
	public void SetNewSTR(int playerNdx) {
		if (playerNdx == 0) {
			stats[playerNdx].STR = (int)(2 * stats[playerNdx].LVL); // Blob: Lvl 1 = 2
		} else {
			stats[playerNdx].STR = (int)(1.5f * stats[playerNdx].LVL); // Chani: Lvl 1 = 1
		}
		stats[playerNdx].baseSTR = stats[playerNdx].STR;
	}
	// DEF
	public void SetNewDEF(int playerNdx) {
		if (playerNdx == 0) {
			stats[playerNdx].DEF = (int)(2 * stats[playerNdx].LVL); // Blob: Lvl 1 = 2
		} else {
			stats[playerNdx].DEF = (int)(1.5f * stats[playerNdx].LVL); // Chani: Lvl 1 = 1
		}
		stats[playerNdx].baseDEF = stats[playerNdx].DEF;
	}
	// WIS
	public void SetNewWIS(int playerNdx) {
		if (playerNdx == 0) {
			stats[playerNdx].WIS = (int)(1.5f * stats[playerNdx].LVL); // Blob: Lvl 1 = 1
		} else {
			stats[playerNdx].WIS = (int)(2 * stats[playerNdx].LVL); // Chani: Lvl 1 = 2
		}
		stats[playerNdx].baseWIS = stats[playerNdx].WIS;
	}
	// AGI
	public void SetNewAGI(int playerNdx) {
		if (playerNdx == 0) {
			stats[playerNdx].AGI = (int)(1.5f * stats[playerNdx].LVL); // Blob: Lvl 1 = 1
		} else {
			stats[playerNdx].AGI = (int)(2 * stats[playerNdx].LVL); // Chani: Lvl 1 = 2
		}
		stats[playerNdx].baseAGI = stats[playerNdx].AGI;
	}

	// SpellNdx (Mathf.Min used to prevent spellNdx from exceeding
	// the amount of spells each member is capable of learning)
	public void SetNewSpellNdx(int playerNdx) {
		if (playerNdx == 0) {
			stats[playerNdx].spellNdx = Mathf.Min((int)(0.5f * stats[playerNdx].LVL), stats[playerNdx].spells.Count); // Blob: Lvl 1 = 0
		} else {
			stats[playerNdx].spellNdx = Mathf.Min((int)(1.0f * stats[playerNdx].LVL + 1), stats[playerNdx].spells.Count); // Chani: Lvl 1 = 2
		}
	}

	public void CheckForLevelUp () {
		if (stats[0].EXP > 2000 && !stats[0].hasReachedThisLevel[10]) { LevelUp (10, 0);
		} else if (stats[0].EXP > 1300  && !stats[0].hasReachedThisLevel[9]) { LevelUp (9, 0);
		} else if (stats[0].EXP > 800  && !stats[0].hasReachedThisLevel[8]) { LevelUp (8, 0);
		} else if (stats[0].EXP > 450 && !stats[0].hasReachedThisLevel[7]) { LevelUp (7, 0);
		} else if (stats[0].EXP > 220 && !stats[0].hasReachedThisLevel[6]) { LevelUp (6, 0);
		} else if (stats[0].EXP > 110 && !stats[0].hasReachedThisLevel[5]) { LevelUp (5, 0);
		} else if (stats[0].EXP > 47 && !stats[0].hasReachedThisLevel[4]) { LevelUp (4, 0);
		} else if (stats[0].EXP > 23 && !stats[0].hasReachedThisLevel[3]) { LevelUp (3, 0);
		} else if (stats[0].EXP > 7 && !stats[0].hasReachedThisLevel[2]) { LevelUp (2, 0);
		}
	
		if (partyNdx >= 1) {
			if (stats[1].EXP > 2100 && !stats[1].hasReachedThisLevel[10]) { LevelUp (10, 1);
			} else if (stats[1].EXP > 1300  && !stats[1].hasReachedThisLevel[9]) { LevelUp (9, 1);
			} else if (stats[1].EXP > 850  && !stats[1].hasReachedThisLevel[8]) { LevelUp (8, 1);
			} else if (stats[1].EXP > 450 && !stats[1].hasReachedThisLevel[7]) { LevelUp (7, 1);
			} else if (stats[1].EXP > 250 && !stats[1].hasReachedThisLevel[6]) { LevelUp (6, 1);
			} else if (stats[1].EXP > 110 && !stats[1].hasReachedThisLevel[5]) { LevelUp (5, 1);
			} else if (stats[1].EXP > 55 && !stats[1].hasReachedThisLevel[4]) { LevelUp (4, 1);
			} else if (stats[1].EXP > 23 && !stats[1].hasReachedThisLevel[3]) { LevelUp (3, 1);
			} else if (stats[1].EXP > 9 && !stats[1].hasReachedThisLevel[2]) { LevelUp (2, 1);
			}
		}
	}
		
	void LevelUp (int newLVL, int playerNdx) {
		stats[playerNdx].hasLeveledUp = true;

		stats[playerNdx].LVL = newLVL;
		SetNewSpellNdx(playerNdx);

		// Assign Stats
		SetNewHP(playerNdx);
		SetNewMP(playerNdx);
		SetNewSTR(playerNdx);
		SetNewAGI(playerNdx);
		SetNewDEF(playerNdx);
		SetNewWIS(playerNdx);

		// Add current equipment's stat effect(s) to party member's stats
		for (int i = 0; i < EquipScreen.S.playerEquipment[0].Count; i++) {
            if (EquipScreen.S.playerEquipment[playerNdx][i] != null) {
                EquipStatsEffect.S.AddItemEffect(playerNdx, EquipScreen.S.playerEquipment[playerNdx][i]);
            }
        }

		// Mark that this Level has been reached (and all previous levels)
		for (int i = 0; i < newLVL + 1; i++) {
			stats[playerNdx].hasReachedThisLevel[i] = true;
		}
	}
}

public class PartyStats {
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
	public bool hasLeveledUp;

	public PartyStats(string name, int HP, int maxHP, int baseMaxHP, int MP, int maxMP, int baseMaxMP,
		int STR, int baseSTR, int DEF, int baseDEF, int WIS, int baseWIS, int AGI, int baseAGI,
		int EXP, int LVL, int spellNdx, List<Spell> spells, List<bool> hasReachedThisLevel, bool hasLeveledUp) {
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eSpellType { healing, offensive, statusEffect, overWorld, singleTarget, AOE };
public enum eSpellStatEffect { HP, MP, STR, DEF, WIS, AGI, none };

public class SpellManager : MonoBehaviour {
	[Header("Set in Inspector")]
	public Sprite [] 			spellSprite;

	[Header("Set Dynamically")]
	public Spell[] 				_spells;

	void Awake() {
		_spells = new Spell[9];

		// Heal
		_spells [0] = new Spell ();
		_spells [0].spellName = "Heal";
		_spells [0].spellType = eSpellType.healing;
		_spells [0].spellStatEffect = eSpellStatEffect.HP;
		_spells [0].spellStatEffectValue = Random.Range(30, 45);
		_spells [0].spellCost = 3;
		_spells [0].spellDescription = "Replenishes at least 30 HP! What a heal spell!" + "\n Cost: 3 MP";
		_spells [0].spellSprite = spellSprite [0];

		// Warp
		_spells [1] = new Spell ();
		_spells [1].spellName = "Warp";
		_spells [1].spellType = eSpellType.overWorld;
		_spells [1].spellStatEffect = eSpellStatEffect.none;
		_spells [1].spellStatEffectValue = 0;
		_spells [1].spellCost = 1;
		_spells [1].spellDescription = "Warp to a previously visited land!" + "\n Cost: 1 MP";
		_spells [1].spellSprite = spellSprite [1];

		// Fireball
		_spells [2] = new Spell ();
		_spells [2].spellName = "Fireball";
		_spells [2].spellType = eSpellType.offensive;
		_spells [2].spellStatEffect = eSpellStatEffect.HP;
		_spells [2].spellStatEffectValue = Random.Range(8, 12); // +WIS
		_spells [2].spellCost = 2;
		_spells [2].spellDescription = "Blasts Enemy for at least 8 HP! What a damage spell!" + "\n Cost: 2 MP";
		_spells [2].spellSprite = spellSprite [2];

		// Fireblast
		_spells [3] = new Spell ();
		_spells [3].spellName = "Fireblast";
		_spells [3].spellType = eSpellType.offensive;
		_spells [3].spellStatEffect = eSpellStatEffect.HP;
		_spells [3].spellStatEffectValue = Random.Range (12, 20); // +WIS
		_spells [3].spellCost = 3;
		_spells [3].spellDescription = "Blasts ALL enemies for at least 12 HP! Hot damn!" + "\n Cost: 3 MP";
		_spells [3].spellSprite = spellSprite [3];
	}

	public Spell GetSpell(string spellName){
		Spell tSpell;

		switch (spellName) {
		case "Heal": tSpell = _spells [0]; break;
		case "Warp": tSpell = _spells [1]; break;
		case "Fireball": tSpell = _spells [2]; break;
		case "Fireblast": tSpell = _spells [3]; break;
		default: tSpell = _spells [0]; break;
		}

		return tSpell;
	}
}

public class Spell {
	public string 			spellName;
	public eSpellType 		spellType;
	public eSpellStatEffect spellStatEffect;
	public int 				spellStatEffectValue;
	public int 				spellCost;
	public string 			spellDescription;
	public Sprite 			spellSprite;
}

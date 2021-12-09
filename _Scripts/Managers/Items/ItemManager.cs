using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public enum eItem { hpPotion, mpPotion, paperSword, crap, nothing, paperArmor, paperHelmet, paperOther, woodenSword,
                    paperWand, berry, smallKey, bug1, bug2, bug3, bug4, bug5, bug6,
                    defaultWeapon, defaultArmor, defaultHelmet, defaultAccessory,
                    healAllPotion, warpPotion, revivePotion
};
public enum eItemType { weapon, armor, helmet, accessory, consumable, ingredient, important, nothing };
public enum eItemStatEffect { HP, MP, STR, DEF, WIS, AGI, nothing };

public class ItemManager : MonoBehaviour {
	[Header("Set in Inspector")]
    // TBR: Has yet to be implemented; will be used to display image of item on ItemScreen
	public Sprite [] 			itemSprite = new Sprite[30];

	[Header("Set Dynamically")]
	// Singleton
	private static ItemManager  _S;
	public static ItemManager   S { get { return _S; } set { _S = value; } }

	public Item[] 				items;

	void Awake() {
		S = this;

        // Initialize array of Items
		items = new Item[30];

        // Health Potion
        items[0] = new Item("Health Potion", eItemType.consumable, eItemStatEffect.HP, 30, 45, 8,
        "Heals a single party member for at least 30 HP." + "\n Value: 8 Gold", itemSprite[0]);

        // Magic Potion
        items[1] = new Item("Magic Potion", eItemType.consumable, eItemStatEffect.MP, 12, 20, 24,
        "Replenishes at least 12 MP for a single party member." + "\n Value: 24 Gold", itemSprite[1]);

        // Paper Sword
        items[2] = new Item("Paper Sword", eItemType.weapon, eItemStatEffect.STR, 10, 10, 5,
        "A paper sword capable of inflicting light physical damage upon an enemy. Adds +10 to Strength." + "\n Value: 5 Gold", itemSprite[2]);

        // Crap 
        items[3] = new Item("Crap", eItemType.nothing, eItemStatEffect.nothing, 0, 0, 0,
        "It's crap. Literal crap... yuck." + "\n Value: 0 Gold", itemSprite[3]);

        // Nothing 
        items[4] = new Item("Nothing", eItemType.nothing, eItemStatEffect.nothing, 0, 0, 0,
        "It's nothing. Absolutely nothing. Really? What a disappointing game!" + "\n Value: 0 Gold", itemSprite[4]);

        // Paper Armor
        items[5] = new Item("Paper Armor", eItemType.armor, eItemStatEffect.DEF, 6, 6, 7,
        "A set of paper armor capable of reducing the amount of damage taken from an enemy. Adds +6 to Defense." + "\n Value: 7 Gold", itemSprite[5]);

        // Paper Helmet
        items[6] = new Item("Paper Helmet", eItemType.helmet, eItemStatEffect.DEF, 9, 9, 9,
        "A paper helmet capable of reducing the amount of damage taken from an enemy. Adds +9 to Defense." + "\n Value: 9 Gold", itemSprite[6]);

        // Paper Accessory
        items[7] = new Item("Paper Accessory", eItemType.accessory, eItemStatEffect.AGI, 9, 9, 9,
        "A paper accessory that slightly increases the speed of a party member. Adds +9 to Agility." + "\n Value: 9 Gold", itemSprite[7]);

        // Wooden Sword
        items[8] = new Item("Wooden Sword", eItemType.weapon, eItemStatEffect.STR, 12, 12, 6,
        "A wooden sword capable of inflicting moderate physical damage upon an enemy. Adds +12 to Strength." + "\n Value: 6 Gold", itemSprite[8]);

        // Ass Wand
        items[9] = new Item("Ass Wand", eItemType.weapon, eItemStatEffect.WIS, 10, 10, 1,
        "A paper wand capable of inflicting light magic damage upon an enemy. Adds +10 to Wisdom." + "\n Value: 1 Gold", itemSprite[9]);

        // Berry
        items[10] = new Item("Berry", eItemType.ingredient, eItemStatEffect.nothing, 0, 0, 5,
        "Aside from its resale value, the party has no current use for this tasty berry." + "\n Value: 5 Gold", itemSprite[10]);

        // Small Key
        items[11] = new Item("Small Key", eItemType.important, eItemStatEffect.nothing, 0, 0, 0,
        "A small key that can unlock any small lock found on any small door. It can only be used on a single door." + "\n Value: 0 Gold", itemSprite[11]);

        // Bug_1
        items[12] = new Item("Blue Bat", eItemType.ingredient, eItemStatEffect.nothing, 0, 0, 15,
        "Aside from its resale value, the party has no current use for this creature." + "\n Value: 15 Gold", itemSprite[12]);

        // Bug_2
        items[13] = new Item("Violet Butterfly", eItemType.ingredient, eItemStatEffect.nothing, 0, 0, 15,
        "Aside from its resale value, the party has no current use for this creature." + "\n Value: 15 Gold", itemSprite[13]);

        // Bug_3
        items[14] = new Item("Vampire Bat", eItemType.ingredient, eItemStatEffect.nothing, 0, 0, 15,
        "Aside from its resale value, the party has no current use for this creature." + "\n Value: 15 Gold", itemSprite[14]);

        // Bug_4
        items[15] = new Item("Orange Butterfly", eItemType.ingredient, eItemStatEffect.nothing, 0, 0, 15,
        "Aside from its resale value, the party has no current use for this creature." + "\n Value: 15 Gold", itemSprite[15]);

        // Bug_5
        items[16] = new Item("Blue Butterfly", eItemType.ingredient, eItemStatEffect.nothing, 0, 0, 15,
        "Aside from its resale value, the party has no current use for this creature." + "\n Value: 15 Gold", itemSprite[16]);

        // Bug_6
        items[17] = new Item("Bumble Bee", eItemType.ingredient, eItemStatEffect.nothing, 0, 0, 15,
        "Aside from its resale value, the party has no current use for this creature." + "\n Value: 15 Gold", itemSprite[17]);

        // Default Weapon
        items[18] = new Item("Default Weapon", eItemType.weapon, eItemStatEffect.STR, 1, 1, 5,
        "The weapon that each party member starts out with. Adds +1 to Strength." + "\n Value: 5 Gold", itemSprite[18], true);

        // Default Armor
        items[19] = new Item("Default Armor", eItemType.armor, eItemStatEffect.DEF, 1, 1, 5,
        "The armor that each party member starts out with. Adds +1 to Defense." + "\n Value: 5 Gold", itemSprite[19], true);

        // Default Helmet
        items[20] = new Item("Default Helmet", eItemType.helmet, eItemStatEffect.DEF, 1, 1, 5,
        "The helmet that each party member starts out with. Adds +1 to Defense." + "\n Value: 5 Gold", itemSprite[20], true);

        // Default Accessory
        items[21] = new Item("Default Accessory", eItemType.accessory, eItemStatEffect.AGI, 1, 1, 5,
        "The accessory that each party member starts out with. Adds +1 to Agility." + "\n Value: 5 Gold", itemSprite[21], true);

        // Heal All Potion
        items[22] = new Item("Heal All Potion", eItemType.consumable, eItemStatEffect.HP, 12, 20, 20,
        "Heals ALL party members for at least 12 HP!" + "\n Value: 20 Gold", itemSprite[22], false, true);

        // Warp Potion
        items[23] = new Item("Warp Potion", eItemType.consumable, eItemStatEffect.nothing, 0, 0, 15,
        "Instantaneously transports the party to a previously visited location." + "\n Value: 15 Gold", itemSprite[23]);

        // Revive Potion
        items[24] = new Item("Revive Potion", eItemType.consumable, eItemStatEffect.HP, 12, 20, 25,
        "Revives a fallen party member and restores a small amount of their HP." + "\n Value: 25 Gold", itemSprite[24]);
    }

    public Item GetItem(eItem itemNdx){
		Item tItem = items[(int)itemNdx];
		return tItem;
	}
}

public class Item {
	public string 			name;
	public eItemType 		type;
	public eItemStatEffect 	statEffect;
    public int              statEffectMinValue;
    public int              statEffectMaxValue;
    public int 				value;
	public string 			description;
	public Sprite 			sprite;
    public bool             isEquipped;
    public bool             multipleTargets;

    public Item(string itemName, 
                eItemType itemType, eItemStatEffect itemStatEffect, 
                int itemStatEffectMinValue, int itemStatEffectMaxValue, int itemValue, string itemDescription, 
                Sprite itemSprite, bool itemIsEquipped = false, bool itemMultipleTargets = false) {
	    name = itemName;
	    type = itemType;
	    statEffect = itemStatEffect;
        statEffectMinValue = itemStatEffectMinValue;
        statEffectMaxValue = itemStatEffectMaxValue;
        value = itemValue;
	    description = itemDescription;
	    sprite = itemSprite;
        isEquipped = itemIsEquipped;
        multipleTargets = itemMultipleTargets;
    }
}
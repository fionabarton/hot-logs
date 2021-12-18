using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores and manages the party's inventory
/// </summary>
public class Inventory : MonoBehaviour
{
	[Header("Set Dynamically")]
	// Singleton
	private static Inventory		_S;
	public static Inventory			S { get { return _S; } set { _S = value; } }

	public Dictionary<Item, int>	items = new Dictionary<Item, int>();

	void Awake() {
		S = this;
	}

	void Start() {
        AddItemToInventory(ItemManager.S.items[0]);
        AddItemToInventory(ItemManager.S.items[0]);
        AddItemToInventory(ItemManager.S.items[1]);

        // Add items to inventory
        AddItemToInventory(ItemManager.S.items[23]);
        AddItemToInventory(ItemManager.S.items[23]);
        AddItemToInventory(ItemManager.S.items[2]);
        AddItemToInventory(ItemManager.S.items[3]);
        AddItemToInventory(ItemManager.S.items[4]);
        AddItemToInventory(ItemManager.S.items[5]);
        AddItemToInventory(ItemManager.S.items[6]);
        AddItemToInventory(ItemManager.S.items[7]);
        AddItemToInventory(ItemManager.S.items[8]);
        AddItemToInventory(ItemManager.S.items[9]);

        AddItemToInventory(ItemManager.S.items[10]);
        AddItemToInventory(ItemManager.S.items[0]);
        AddItemToInventory(ItemManager.S.items[11]);
        AddItemToInventory(ItemManager.S.items[12]);
        AddItemToInventory(ItemManager.S.items[13]);
        AddItemToInventory(ItemManager.S.items[14]);
        AddItemToInventory(ItemManager.S.items[15]);
        AddItemToInventory(ItemManager.S.items[16]);
        AddItemToInventory(ItemManager.S.items[17]);
        AddItemToInventory(ItemManager.S.items[18]);
        AddItemToInventory(ItemManager.S.items[19]);
        AddItemToInventory(ItemManager.S.items[20]);

        AddItemToInventory(ItemManager.S.items[22]);
        AddItemToInventory(ItemManager.S.items[22]);
        AddItemToInventory(ItemManager.S.items[24]);
        AddItemToInventory(ItemManager.S.items[24]);
        AddItemToInventory(ItemManager.S.items[24]);
    }

    public void AddItemToInventory(Item name) {
		if (items.ContainsKey(name)) {
			items[name] += 1;
		} else {
			items[name] = 1;
		}
	}

    public void RemoveItemFromInventory(Item name) {
        items[name]--;

        // Update Pause & Overworld GUI
        ItemScreen.S.AssignItemNames();
        PauseScreen.S.UpdateGUI();

        // Remove the entry if the count goes to 0.
        if (items[name] == 0) {
            items.Remove(name);
        }
    }

    // Return a List of all the Dictionary keys
    public List<Item> GetItemList() {
		List<Item> list = new List<Item>(items.Keys);
		return list;
	}

	// Return how many of that item are in inventory
	public int GetItemCount(Item name) {
		if (items.ContainsKey(name)) {
			return items[name];
		}
		return 0;
	}
}
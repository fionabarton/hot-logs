using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 0:
/// 1:
/// 2:
/// 3: Defeat Toilet
/// </summary>
///

public class QuestManager : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static QuestManager _S;
	public static QuestManager S { get { return _S; } set { _S = value; } }

	public List<bool> activated = new List<bool>() { false }; // Yet to be used
	public List<bool> completed = new List<bool>() { false };

	void Awake() {
		// Singleton
		S = this;
	}
}

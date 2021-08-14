using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Starts RandomEncounter.FixedUpdateCoroutine() when Player enters this trigger
/// </summary>
public class BattlePatch : MonoBehaviour {
	[Header("Set in Inspector")]
	public List<Enemy> enemies = new List<Enemy> ();

	// Activate randomEncounter.cs
	void OnTriggerEnter2D (Collider2D coll) {
		if (coll.gameObject.tag == "PlayerTrigger") {
			// Player RigidBody
			Player.S.rigid.sleepMode = RigidbodySleepMode2D.NeverSleep;

			RandomEncounter.S.StartStopCoroutine(true);
			//RandomEncounter.S.ImportEnemies (enemies);
			Debug.Log("Start Coroutine!!!!");
		}
	}
	// Deactivate randomEncounter.cs
	void OnTriggerExit2D (Collider2D coll) {
		if (coll.gameObject.tag == "PlayerTrigger") {
			// Player RigidBody
			Player.S.rigid.sleepMode = RigidbodySleepMode2D.StartAwake;

			RandomEncounter.S.StartStopCoroutine(false);
			Debug.Log("Stop Coroutine!!!!");
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sets a player or enemy's isOnSwapLayerTrigger isOnTrigger field
/// on collision
/// </summary>
public class IsOnSwapLayerTrigger : MonoBehaviour {
	// Set player/enemy isOnSwapLayerTrigger to true
	void OnTriggerEnter2D(Collider2D coll) {
		SetIsOnSwapLayerTrigger(coll, true);
	}

	// Set player/enemy isOnSwapLayerTrigger to false
	void OnTriggerExit2D(Collider2D coll) {
		SetIsOnSwapLayerTrigger(coll, false);
	}

	void SetIsOnSwapLayerTrigger(Collider2D coll, bool isOnTrigger) {
		// Set player's isOnSwapLayerTrigger
		if (coll.gameObject.tag == "Player") {
			Player.S.isOnSwapLayerTrigger = isOnTrigger;
		}

		// Set enemy's isOnSwapLayerTrigger
		if (coll.gameObject.tag == "Enemy") {
			Enemy enemy = coll.gameObject.GetComponent<Enemy>();
			if (enemy != null) {
				enemy.isOnSwapLayerTrigger = isOnTrigger;
			}
		}
	}
}
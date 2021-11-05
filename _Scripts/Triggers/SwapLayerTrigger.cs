using System.Collections.Generic;
using UnityEngine;

public class SwapLayerTrigger : MonoBehaviour{
    [Header("Set in Inspector")]
	// Colliders that enable/disable when Player exits trigger
	public List<GameObject>		enableColliders = new List<GameObject>();
	public List<GameObject>		disableColliders = new List<GameObject>();

	public List<SpriteRenderer> sRends = new List<SpriteRenderer>();

	// Variables that are set when Player exits trigger
	public string				exitLayerName; // Player or Foreground

	public int					level;

    void OnEnable() {
        if (Player.S.level == level) {
            ActivateColliders();
        }
    }

	// Set player/enemy ground level
	void OnTriggerEnter2D(Collider2D coll) {
		// Set player's level
		if (coll.gameObject.tag == "PlayerTrigger") {
			Player.S.level = level;

			ActivateColliders();
		}

		// Set enemy's level
		if (coll.gameObject.tag == "Enemy") {
			Enemy enemy = coll.gameObject.GetComponent<Enemy>();
			if (enemy != null) {
				enemy.level = level;
			}
		}
	}

	// Set player/enemy isOnSwapLayerTrigger to true
	void OnTriggerStay2D(Collider2D coll) {
		SetIsOnSwapLayerTrigger(coll, true);
	}

	// Set player/enemy isOnSwapLayerTrigger to false
	void OnTriggerExit2D(Collider2D coll) {
		SetIsOnSwapLayerTrigger(coll, false);
	}

	void SetIsOnSwapLayerTrigger(Collider2D coll, bool trueOrFalse) {
		// Set player's isOnSwapLayerTrigger
		if (coll.gameObject.tag == "PlayerTrigger") {
			Player.S.isOnSwapLayerTrigger = trueOrFalse;
		}

		// Set enemy's isOnSwapLayerTrigger
		if (coll.gameObject.tag == "Enemy") {
			Enemy enemy = coll.gameObject.GetComponent<Enemy>();
			if (enemy != null) {
				enemy.isOnSwapLayerTrigger = trueOrFalse;
			}
		}
	}

	void ActivateColliders() {
		if (enableColliders != null) {
			for (int i = 0; i < enableColliders.Count; i++) {
				enableColliders[i].SetActive(true);
			}
		}

		if (disableColliders != null) {
			for (int i = 0; i < disableColliders.Count; i++) {
				disableColliders[i].SetActive(false);
			}
		}

		// Set sorting layer name for each sprite renderer
		SetSortingLayerName();
	}

	void SetSortingLayerName() {
		if (sRends != null) {
			for (int i = 0; i < sRends.Count; i++) {
				sRends[i].sortingLayerName = exitLayerName;
			}
		}
	}
}
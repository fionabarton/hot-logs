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
            DoIt(true, exitLayerName);
        }
    }

	void OnTriggerEnter2D(Collider2D coll) {
		// Set player's level
		if (coll.gameObject.tag == "PlayerTrigger") {
			Player.S.level = level;
		}

		// Set enemy's level
		if (coll.gameObject.tag == "Enemy") {
			Enemy enemy = coll.gameObject.GetComponent<Enemy>();
			if (enemy != null) {
				enemy.level = level;
			}
		}
	}

	void OnTriggerExit2D(Collider2D coll) {
        // Set player's level
		if (coll.gameObject.tag == "PlayerTrigger") {
			Player.S.level = level;

			DoIt(true, exitLayerName);
		}

        // Set enemy's level
        if (coll.gameObject.tag == "Enemy") {
			Enemy enemy = coll.gameObject.GetComponent<Enemy>();
			if(enemy != null) {
				enemy.level = level;
			}
		}
	}

	void DoIt(bool activateColliders, string layerName) {
		if (enableColliders != null) {
			for (int i = 0; i < enableColliders.Count; i++) {
				enableColliders[i].SetActive(activateColliders);
			}
		}

		if (disableColliders != null) {
			for (int i = 0; i < disableColliders.Count; i++) {
				disableColliders[i].SetActive(!activateColliders);
			}
		}

		// Set Sorting Layer 
		if (sRends != null) {
			for (int i = 0; i < sRends.Count; i++) {
				sRends[i].sortingLayerName = layerName;
			}
		}
    }
}
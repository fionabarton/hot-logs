using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Swaps colliders and sorting layers when the player moves up or down a ground level
/// </summary>
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
		if (coll.gameObject.tag == "Player") {
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
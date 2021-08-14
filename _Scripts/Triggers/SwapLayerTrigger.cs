using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapLayerTrigger : MonoBehaviour{
    [Header("Set in Inspector")]
	// Colliders that enable/disable when Player exits trigger
	public List<GameObject> enableColliders = new List<GameObject>();
	public List<GameObject> disableColliders = new List<GameObject>();

	public List<SpriteRenderer> sRends = new List<SpriteRenderer>();

	// Variables that are set when Player exits trigger
	public bool		levelIsEven;
	public string	exitLayerName; // Player or Foreground
	public string	enterLayerName; // Player or Foreground

	void OnEnable() {
        if (Player.S.isOnEvenLevel == levelIsEven) {
            if (levelIsEven) {
				DoIt(true, exitLayerName);
			} else {
				DoIt(false, enterLayerName);
			}
		} else {
			if (levelIsEven) {
				DoIt(false, enterLayerName);
			} else {
				DoIt(true, exitLayerName);
			}
		}
	}

	void OnTriggerExit2D(Collider2D coll) {
		if (coll.gameObject.tag == "PlayerTrigger") {
			// Indicate Player whether is on even or level
			Player.S.isOnEvenLevel = levelIsEven;

			DoIt(true, exitLayerName);
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
		for (int i = 0; i < sRends.Count; i++) {
			sRends[i].sortingLayerName = layerName;
		}
	}
}
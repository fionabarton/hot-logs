using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Ex. Parallax Background
public class SpriteSwap : MonoBehaviour {
	[Header ("Set in Inspector")]
	public List<Sprite> 	sprites = new List<Sprite> ();

	public bool 			swapOnCollision;

	[Header ("Set Dynamically")]
	public SpriteRenderer 	sRend;

	void OnEnable () {
		// Activate on LoadLevel
		if (!swapOnCollision) {
			// Change Parallax Background
			if (RainSpawner.S.isRaining) {
				sRend.sprite = sprites [0];
			}else{
				sRend.sprite = sprites [1];
			}
		}
	}
	
	// Activate on Collision
	void OnTriggerEnter2D(Collider2D coll){
		if (swapOnCollision) {
			if (coll.gameObject.CompareTag("Player")) {
				sRend.sprite = sprites [0];
			}
		}
	}
}

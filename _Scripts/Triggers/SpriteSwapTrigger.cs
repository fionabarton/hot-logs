using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSwapTrigger : MonoBehaviour {
	[Header("Set in Inspector")]
	public List<Sprite>		sprites = new List<Sprite>();

	public bool				swapOnlyOnFirstCollision = true;

	public eSoundName		soundName;

	public SpriteRenderer	sRend;

	void OnTriggerEnter2D(Collider2D coll) {
		if (swapOnlyOnFirstCollision) {
			if (coll.gameObject.CompareTag("Player")) {
				sRend.sprite = sprites[0];

				AudioManager.S.PlaySFX(soundName);

				swapOnlyOnFirstCollision = false;
			}
		}
	}
}
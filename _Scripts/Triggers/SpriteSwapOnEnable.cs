using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used for changing background sprites if raining
/// </summary>
public class SpriteSwapOnEnable : MonoBehaviour {
	[Header("Set in Inspector")]
	public List<Sprite>		sprites = new List<Sprite>();

	public SpriteRenderer	sRend;

	void OnEnable() {
		// Change Parallax Background
		if (RainSpawner.S.isRaining) {
			sRend.sprite = sprites[0];
		} else {
			sRend.sprite = sprites[1];
		}
	}
}
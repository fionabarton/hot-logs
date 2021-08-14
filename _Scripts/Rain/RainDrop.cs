using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainDrop : MonoBehaviour {
	[Header("Set in Inspector")]
	public Animator			anim;
	public SpriteRenderer	sRend;

	public bool 			isFalling;

	[Header("Set Dynamically")]
	private float 			speed;

	private float			lifeTime;
	private float			timeToDie;

	void OnEnable(){
		// Randomly Set Lifetime
		lifeTime = Random.Range (0.1f, 0.75f);

		// Set TimeToDie
		timeToDie = Time.time + lifeTime;

		// Set Parent
		transform.SetParent (ObjectPool.S.poolAnchor);

		// Randomly Set Sprite
		int tNdx = Random.Range(0,19);
		sRend.sprite = RainSpawner.S.rainSprites [tNdx];

		// Randomly Set Opacity
		Color tColor = sRend.color;
		tColor.a = Random.Range (0.5f, 1f);
		sRend.color = tColor;

		if (isFalling) {
			// Randomly Set Speed
			speed = Random.Range (5, 15);
		} else {
			// Set Anim
			anim.CrossFade ("Rain_Drop_Fallen", 0);
		}

		StartCoroutine ("FixedUpdateCoroutine");
	}

	void OnDisable(){
		StopCoroutine ("FixedUpdateCoroutine");
	}


	public IEnumerator FixedUpdateCoroutine () {
		// if not Paused
		if (!RPG.S.paused) {
			if (isFalling) {
				// Move Down
				Vector2 tPos = transform.position;
				tPos.y -= speed * Time.fixedDeltaTime;
				transform.position = tPos;
			} 

			// Deactivate
			if (Time.time >= timeToDie) {
				// Deactivate GameObject
				gameObject.SetActive (false);

				// Set Parent
				transform.SetParent (RainSpawner.S.rainAnchorGO);
			}
		}

		yield return new WaitForFixedUpdate ();
		StartCoroutine ("FixedUpdateCoroutine");
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordBubble : MonoBehaviour {

	[Header("Set in Inspector")]
	public Sprite 			bubbleSprite;

	[Header("Set Dynamically")]
	private bool 			isVisible;
	private SpriteRenderer 	sRend;

	void Start () {
		sRend = GetComponent<SpriteRenderer> ();
	}

	public IEnumerator FixedUpdateCoroutine () {
		if (isVisible) {
			if (Mathf.Abs (Player.S.gameObject.transform.position.x - transform.parent.position.x) <= 2f &&
				Mathf.Abs (Player.S.gameObject.transform.position.y - transform.parent.position.y) <= 2f) {
				// Show Sprite if Player close enough
				sRend.sprite = bubbleSprite;
			} else {
				// No Sprite if Player too far
				sRend.sprite = null;
			}
		}

		yield return new WaitForFixedUpdate ();
		StartCoroutine ("FixedUpdateCoroutine");
	}

	void OnBecameVisible () { 
		isVisible = true; 

		StartCoroutine ("FixedUpdateCoroutine");
	}
	void OnBecameInvisible () { 
		isVisible = false; 

		StopCoroutine ("FixedUpdateCoroutine");
	}
}

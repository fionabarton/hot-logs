using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSwitch : MonoBehaviour {
	[Header("Set in Inspector")]
	public Sprite 			switchUpSprite, switchDownSprite;
	

	[Header("Set Dynamically")]
	SpriteRenderer			sRend;
	BoxCollider2D			boxColl;

	public bool 			switchIsDown;

	void Start () {
		sRend = GetComponent<SpriteRenderer>();
		boxColl = GetComponent<BoxCollider2D>();
	}
		
	void OnTriggerEnter2D(Collider2D coll){
		// Switch if stomped by Player or Boulder
		if (coll.gameObject.tag == "Player") {
			// Player RigidBody
			//Find.S.playerCS.rigid.sleepMode = RigidbodySleepMode2D.NeverSleep;

			if (switchIsDown) {
				sRend.sprite = switchUpSprite;
				switchIsDown = false;

				if (boxColl != null) { // needed to run in RPG
					Vector2 tBox = boxColl.size;
					tBox.y = 0.5f;
					boxColl.size = tBox;
				}
			} else {
				sRend.sprite = switchDownSprite;
				switchIsDown = true;

				if (boxColl != null) { // needed to run in RPG
					Vector2 tBox = boxColl.size;
					tBox.y = 0.2f;
					boxColl.size = tBox;
				}
			}
		}
	}

	void OnTriggerExit2D(Collider2D coll){
		if (coll.gameObject.tag == "Player") {
			// Player RigidBody
			//Find.S.playerCS.rigid.sleepMode = RigidbodySleepMode2D.StartAwake;
		}
	}
}

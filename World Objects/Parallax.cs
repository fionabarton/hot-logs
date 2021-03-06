using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour {
	[Header("Set in Inspector")]
	public eParallax	mode;
	public float		speedModifier = -1.5f;

	[Header ("Set Dynamically")]
	private Vector3 	pos; // THIS object's position

	// Player
	public Vector3		playerPos;
	public float		tPosMin;
	public float		tPosMax;

	// For CLAMPING Pos.Y
	public Vector2		startingPos;

	void Awake(){
		startingPos = transform.position;
	}

	void OnEnable(){
		StartCoroutine ("FixedUpdateCoroutine");
	}
	void OnDisable(){
		StopCoroutine ("FixedUpdateCoroutine");
	}
		
	public IEnumerator FixedUpdateCoroutine () {
		//if (!RPG.S.paused) { 

			// Get this.position
			pos = transform.position;

			switch (mode) {
			case eParallax.autoScroll:
				pos.x += speedModifier * Time.fixedDeltaTime;
				break;

			case eParallax.scrollWithPlayer:
				// Get Player Position
				playerPos = Player.S.gameObject.transform.position;

				// Scroll Horizontally
				if (playerPos.x < tPosMin || playerPos.x > tPosMax) {

					tPosMin = playerPos.x -= 0.01f;
					tPosMax = playerPos.x += 0.01f;

					switch (Player.S.mode) {
					case ePlayerMode.walkLeft:
					case ePlayerMode.walkDownLeft:
					case ePlayerMode.walkUpLeft:
						pos.x += speedModifier * Time.fixedDeltaTime;
						break;
					case ePlayerMode.runLeft:
					case ePlayerMode.runDownLeft:
					case ePlayerMode.runUpLeft:
						pos.x += speedModifier * Time.fixedDeltaTime * 2;
						break;
					case ePlayerMode.walkRight:
					case ePlayerMode.walkDownRight:
					case ePlayerMode.walkUpRight:
						pos.x -= speedModifier * Time.fixedDeltaTime;
						break;
					case ePlayerMode.runRight:
					case ePlayerMode.runDownRight:
					case ePlayerMode.runUpRight:
						pos.x -= speedModifier * Time.fixedDeltaTime * 2;
						break;
					}
				}

				// Clamp Y Position
				pos.y = Mathf.Clamp (transform.position.y, startingPos.y - 0.2f, startingPos.y + 0.2f);

				// Scroll Vertically
				switch (Player.S.mode) {
				case ePlayerMode.walkUp:
					pos.y -= 0.2f * Time.fixedDeltaTime;
					break;
				case ePlayerMode.runUp:
					pos.y -= 0.2f * Time.fixedDeltaTime * 2;
					break;
				case ePlayerMode.walkDown:
					pos.y += 0.2f * Time.fixedDeltaTime;
					break;
				case ePlayerMode.runDown:
					pos.y += 0.2f * Time.fixedDeltaTime * 2;
					break;
				}
				break;
			}

			// Recycle X Position if too far offscreen
			if (transform.position.x < -20) {
				pos.x = 20;
			} else if (transform.position.x > 20) {
				pos.x = -20;
			}

			// Set this.position
			transform.position = pos;


			yield return new WaitForFixedUpdate ();
			StartCoroutine ("FixedUpdateCoroutine");
		//}
	}
}

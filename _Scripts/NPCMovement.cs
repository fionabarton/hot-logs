using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eMovement { random, patrol, pursueWalk, pursueRun, flee, idle, reverse, auto };

/// <summary>
/// - Movement AI for NPCs 
/// - After dialogue deactivated, WaitSettings() is called in RPGDiaglogueTrigger
/// </summary>
public class NPCMovement : MonoBehaviour {

	[Header("Set in Inspector")]
	public eMovement 		mode = eMovement.random;

	public float			speed = 2;

	public Vector2			walkZone = new Vector2(2.5f, 2.5f);

	public Vector2 			waitDuration = new Vector2(0.75f, 1.25f);

	[Header("Set Dynamically")]
	private Rigidbody2D 	rigid;
	private Animator		anim;

	//public bool 			canMove;

	// Walk Zone
	Vector2					minWalkPoint;
	Vector2					maxWalkPoint;

	private bool 			isWalking;
	private int 			walkDirection;
	private int				nextWalkDirection = 999;
	// 0 = right, 1 = up, 2 = left, 3 = down
	// 4 = Down Right, 5 = Up Right, 6 = Up Left, 7 = Down Left

	private float 			walkCounter = 0;
	private float 			waitCounter = 0;

	// Flip
	private bool 			facingRight;

	public eMovement 		defaultMovementMode;

	void Start () {
		rigid = GetComponent<Rigidbody2D> ();
		anim = GetComponent<Animator> ();

		minWalkPoint = new Vector2(transform.position.x - walkZone.x, 
								   transform.position.y - walkZone.y);
		maxWalkPoint = new Vector2(transform.position.x + walkZone.x,
								   transform.position.y + walkZone.y); 

		StartCoroutine("FixedUpdateCoroutine");
	}
	//void OnBecameVisible(){
	//	canMove = true;
	//	StartCoroutine ("FixedUpdateCoroutine");
	//}
	//void OnBecameInvisible(){
	//	canMove = false;
	//	StopCoroutine ("FixedUpdateCoroutine");
	//}

    // Walk or Wait Loop
	public IEnumerator FixedUpdateCoroutine () {
		// if Visible
		//if (canMove) {
			// if not Paused, no Dialogue Text Box
			if (!RPG.S.paused && !DialogueManager.S.TextBoxSpriteGO.activeInHierarchy) {
				// Flip
				if (rigid.velocity.x < 0 && !facingRight) {
					Flip ();
				} else if (rigid.velocity.x > 0 && facingRight) {
					Flip ();
				}

				switch (mode) {
				case eMovement.random:
                    // Move GameObject
					if (isWalking) {
						// If outside of Walk Zone: stop moving, wait, & move in OPPOSITE direction
						switch (walkDirection) {
						case 0: //right
							rigid.velocity = new Vector2 (speed, 0);
							if (transform.position.x >= maxWalkPoint.x) {
								WaitSettings ();
								nextWalkDirection = 2;
							}
							break;
						case 1: //up
							rigid.velocity = new Vector2 (0, speed);
							if (transform.position.y >= maxWalkPoint.y) {
								WaitSettings ();
								nextWalkDirection = 3;
							}
							break;
						case 2: //left
							rigid.velocity = new Vector2 (-speed, 0);
							if (transform.position.x <= minWalkPoint.x) {
								WaitSettings ();
								nextWalkDirection = 0;
							}
							break;
						case 3: //down
							rigid.velocity = new Vector2 (0, -speed);
							if (transform.position.y <= minWalkPoint.y) {
								WaitSettings ();
								nextWalkDirection = 1;
							}
							break;
						case 4: //right down
							rigid.velocity = new Vector2 (speed / 2, -speed / 2);
							if (transform.position.x >= maxWalkPoint.x || transform.position.y <= minWalkPoint.y) {
								WaitSettings ();
								nextWalkDirection = 6;
							}
							break;
						case 5: //right up
							rigid.velocity = new Vector2 (speed / 2, speed / 2);
							if (transform.position.x >= maxWalkPoint.x || transform.position.y >= maxWalkPoint.y) {
								WaitSettings ();
								nextWalkDirection = 7;
							}
							break;
						case 6: //left up
							rigid.velocity = new Vector2 (-speed / 2, speed / 2);
							if (transform.position.x <= minWalkPoint.x || transform.position.y >= maxWalkPoint.y) {
								WaitSettings ();
								nextWalkDirection = 4;
							}
							break;
						case 7: //left down
							rigid.velocity = new Vector2 (-speed / 2, -speed / 2);
							if (transform.position.x <= minWalkPoint.x || transform.position.y <= minWalkPoint.y) {
								WaitSettings ();
								nextWalkDirection = 5;
							}
							break;
						}

						// Count down walkCounter
						walkCounter -= Time.deltaTime;

						// If walkCounter < 0, stop moving
						if (walkCounter < 0) {
							WaitSettings ();
						}
					} else {
						// Count down waitCounter
						waitCounter -= Time.deltaTime;
						rigid.velocity = Vector2.zero;

						// If waitCounter < 0, start moving
						if (waitCounter < 0) {
							WalkSettings ();
						}
					}
					break;
				}
			} else { // Freeze if RPG.S.paused
				rigid.velocity = Vector2.zero;
			}

			yield return new WaitForFixedUpdate ();
			StartCoroutine ("FixedUpdateCoroutine");
  //      } else { // Freeze if !canMove
		//	rigid.velocity = Vector2.zero;
		//}
	}

	// Walk
	public void WalkSettings() {
		anim.speed = 1;
		isWalking = true;
		walkCounter = Random.Range(waitDuration.x, waitDuration.y);

		// Set walkDirection
		if (nextWalkDirection == 999) {
			// Select random direction
			walkDirection = Random.Range(0, 8);
		} else {
			walkDirection = nextWalkDirection;
			// Reset nextWalkDirection 
			nextWalkDirection = 999;
		}
		
		// Animation
		switch (walkDirection) {
			case 1:
                anim.CrossFade("Walk_Up", 0);
				break;
			case 3:
				anim.CrossFade("Walk_Down", 0);
				break;
			case 0:
			case 2:
			case 4:
			case 7:
				anim.CrossFade("Walk_Down_Diagonal", 0);
				break;
			case 5:
			case 6:
				anim.CrossFade("Walk_Up_Diagonal", 0);
				break;	
		}
	}

	// Wait
	public void WaitSettings() {
        anim.speed = 0;
        isWalking = false;
		waitCounter = Random.Range(waitDuration.x, waitDuration.y);

		// Animation
		switch (walkDirection) {
			case 1:
				anim.Play("Walk_Up", 0, 1);
				break;
			case 3:
				anim.Play("Walk_Down", 0, 1);
				break;
			case 0:
			case 2:
			case 4:
			case 7:
				anim.Play("Walk_Down_Diagonal", 0, 1);
				break;
			case 5:
			case 6:
				anim.Play("Walk_Up_Diagonal", 0, 1);
				break;
		}
	}
	
	// X Distance between Player & Enemy is more than Y Distance (used for eMovement.pursueWalk)
	bool XDeltaY(){
		float tX = Mathf.Abs(transform.position.x - Player.S.gameObject.transform.position.x);
		float tY = Mathf.Abs(transform.position.y - Player.S.gameObject.transform.position.y);

		if (tX > tY){
			return (true);
		}else{
			return (false);
		}
	}

	// Flip sprite scale
	void Flip() {
		facingRight = !facingRight;
		Vector3 tScale = transform.localScale;
		tScale.x *= -1;
		transform.localScale = tScale;
	}

	// Called in: RPGDialogueTrigger InitializeDialogue()
	public void FacePlayer() {
		// When dialogue is over, the NPC will wait
		WaitSettings();

		// Face direction of Player
		anim.speed = 0;
		if (Player.S.gameObject.transform.position.x < transform.position.x && XDeltaY()) { // Left
			if (transform.localScale.x > 0) {
				Flip();
			}
			anim.Play("Walk_Down_Diagonal", 0, 1);
		} else if (Player.S.gameObject.transform.position.x > transform.position.x && XDeltaY()) { // Right
			if (transform.localScale.x < 0) {
				Flip();
			}
			anim.Play("Walk_Down_Diagonal", 0, 1);
		} else if (Player.S.gameObject.transform.position.y < transform.position.y && !XDeltaY()) { // Down
			anim.Play("Walk_Down", 0, 1);
		} else if (Player.S.gameObject.transform.position.y > transform.position.y && !XDeltaY()) { // Up
			anim.Play("Walk_Up", 0, 1);
		}
	}
}
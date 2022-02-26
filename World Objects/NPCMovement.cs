using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Movement AI for NPCs 
/// </summary>
public class NPCMovement : MonoBehaviour {
	[Header("Set in Inspector")]
	public eMovement 		mode = eMovement.randomWalk;
	public float			speed = 2;

	public Vector2			walkZone = new Vector2(2.5f, 2.5f);
	public Vector2 			waitDuration = new Vector2(0.75f, 1.25f);

	[Header("Set Dynamically")]
	private Rigidbody2D 	rigid;
	private Animator		anim;

	// Bounds of Walk Zone
	Vector2					minWalkPoint;
	Vector2					maxWalkPoint;

	private bool 			isWalking;
	private int 			walkDirection;
	private int				nextWalkDirection = 999;
	// 0 = right, 1 = up, 2 = left, 3 = down, 4 = Down/Right, 5 = Up/Right, 6 = Up/Left, 7 = Down/Left

	private float			timer = 0;

	// Flip
	private bool 			facingRight;

	void Start () {
		rigid = GetComponent<Rigidbody2D> ();
		anim = GetComponent<Animator> ();

		// Set bounds of walk zone
		minWalkPoint = new Vector2(transform.position.x - walkZone.x, transform.position.y - walkZone.y);
		maxWalkPoint = new Vector2(transform.position.x + walkZone.x, transform.position.y + walkZone.y); 

		StartCoroutine("FixedUpdateCoroutine");
	}

	public IEnumerator FixedUpdateCoroutine () {
		// If not paused, and there isn't any dialogue being displayed...
		if (!GameManager.S.paused && !DialogueManager.S.textBoxSpriteGO.activeInHierarchy) {
			// Flip
			if ((rigid.velocity.x < 0 && !facingRight) || (rigid.velocity.x > 0 && facingRight)) {
				Utilities.S.Flip(gameObject, ref facingRight);
			} 

			switch (mode) {
			case eMovement.randomWalk:
				// Decrement timer
				timer -= Time.deltaTime;

                // If the GameObject is walking...
				if (isWalking) {
					// If outside of Walk Zone: stop moving, wait, & move in OPPOSITE direction
					switch (walkDirection) {
					case 0: // right
						rigid.velocity = new Vector2 (speed, 0);
						if (transform.position.x >= maxWalkPoint.x) {
							Wait(2);
						}
						break;
					case 1: // up
						rigid.velocity = new Vector2 (0, speed);
						if (transform.position.y >= maxWalkPoint.y) {
							Wait(3);
						}
						break;
					case 2: // left
						rigid.velocity = new Vector2 (-speed, 0);
						if (transform.position.x <= minWalkPoint.x) {
							Wait(0);
						}
						break;
					case 3: // down
						rigid.velocity = new Vector2 (0, -speed);
						if (transform.position.y <= minWalkPoint.y) {
							Wait(1);
						}
						break;
					case 4: // right down
						rigid.velocity = new Vector2 (speed / 2, -speed / 2);
						if (transform.position.x >= maxWalkPoint.x || transform.position.y <= minWalkPoint.y) {
							Wait(6);
						}
						break;
					case 5: // right up
						rigid.velocity = new Vector2 (speed / 2, speed / 2);
						if (transform.position.x >= maxWalkPoint.x || transform.position.y >= maxWalkPoint.y) {
							Wait(7);
						}
						break;
					case 6: // left up
						rigid.velocity = new Vector2 (-speed / 2, speed / 2);
						if (transform.position.x <= minWalkPoint.x || transform.position.y >= maxWalkPoint.y) {
							Wait(4);
						}
						break;
					case 7: // left down
						rigid.velocity = new Vector2 (-speed / 2, -speed / 2);
						if (transform.position.x <= minWalkPoint.x || transform.position.y <= minWalkPoint.y) {
							Wait(5);
						}
						break;
					}

					// If timer < 0, stop walking 
					if (timer < 0) {
						Wait();
					}
				} else {
					// If not walking, freeze position
					rigid.velocity = Vector2.zero;

					// If timer < 0, start walking 
					if (timer < 0) {
						Walk();
					}
				}
				break;
			}
		} else { // If paused or dialogue is being displayed, freeze position
			rigid.velocity = Vector2.zero;
		}

		yield return new WaitForFixedUpdate();
		StartCoroutine ("FixedUpdateCoroutine");
	}

	public void Walk() {
		isWalking = true;
		anim.speed = 1;

		// Reset timer
		timer = Random.Range(waitDuration.x, waitDuration.y);

		if (this.nextWalkDirection == 999) {
			// Select random direction
			walkDirection = Random.Range(0, 8);
		} else {
			walkDirection = this.nextWalkDirection;
			// Reset nextWalkDirection 
			this.nextWalkDirection = 999;
		}

		// Set animation
		SetAnimation(walkDirection);
	}

	public void Wait(int nextWalkDirection = 999) {
		isWalking = false;
		anim.speed = 0;

		// Reset timer
		timer = Random.Range(waitDuration.x, waitDuration.y);

		// Set walkDirection
		this.nextWalkDirection = nextWalkDirection;
		
		// Set animation
		SetAnimation(walkDirection);
	}

	void SetAnimation(int walkDirection) {
		switch (walkDirection) {
            case 1: anim.Play("Walk_Up", 0, 1); break;
            case 3: anim.Play("Walk_Down", 0, 1); break;
            case 0:
            case 2:
            case 4:
            case 7: anim.Play("Walk_Down_Diagonal", 0, 1); break;
            case 5:
            case 6: anim.Play("Walk_Up_Diagonal", 0, 1); break;
        }
    }

	public void StopAndFacePlayer() {
		// Ensure the NPC waits when dialogue is over
		Wait();

		// Face direction of Player
		if (Player.S.gameObject.transform.position.x < transform.position.x &&
			!Utilities.S.isCloserHorizontally(gameObject, Player.S.gameObject)) { // Left
			// If facing right, flip
			if (transform.localScale.x > 0) {
				Utilities.S.Flip(gameObject, ref facingRight);
			}

			if (Player.S.gameObject.transform.position.y > transform.position.y) {
				anim.Play("Walk_Up_Diagonal", 0, 1);
			} else {
				anim.Play("Walk_Down_Diagonal", 0, 1);
			}

		} else if (Player.S.gameObject.transform.position.x > transform.position.x &&
			!Utilities.S.isCloserHorizontally(gameObject, Player.S.gameObject)) { // Right
			// If facing left, flip
			if (transform.localScale.x < 0) {
				Utilities.S.Flip(gameObject, ref facingRight);
			}

			if (Player.S.gameObject.transform.position.y > transform.position.y) {
				anim.Play("Walk_Up_Diagonal", 0, 1);
			} else {
				anim.Play("Walk_Down_Diagonal", 0, 1);
			}

		} else if (Player.S.gameObject.transform.position.y < transform.position.y &&
			Utilities.S.isCloserHorizontally(gameObject, Player.S.gameObject)) { // Down
			anim.Play("Walk_Down", 0, 1);
		} else if (Player.S.gameObject.transform.position.y > transform.position.y &&
			Utilities.S.isCloserHorizontally(gameObject, Player.S.gameObject)) { // Up
			anim.Play("Walk_Up", 0, 1);
		}
	}
}
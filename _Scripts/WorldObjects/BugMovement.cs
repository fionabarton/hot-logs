using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// In RPG, instantiated by ObjectPool.SpawnObjects()
/// </summary>
public class BugMovement : MonoBehaviour {
	[Header ("Set in Inspector")]
	public bool 			spriteInForeground;

	public float 			moveSpeed = 5;

	public Vector2			randomMoveLength = new Vector2(0.05f, 0.25f);
	public Vector2			randomWaitLength = new Vector2(0.05f, 0.25f);

	public Vector2			flyZone = new Vector2(2.5f, 2.5f);

	public int				animationNdx;

	public bool				isConstrainedToFlyZone;

	public bool				facingRight;

	[Header ("Set Dynamically")]
	private Rigidbody2D		rigid;
	private SpriteRenderer	sRend;
	private Animator		anim;

	private float 			moveLengthCounter;
	private float 			waitLengthCounter;

	private bool 			isMoving;
	private bool 			canMove;

	private Vector3 		moveDirection;

	// Bounds of fly zone
	Vector2					minFlyPoint;
	Vector2					maxFlyPoint;

	void OnBecameVisible() {
		canMove = true;

		StartCoroutine("FixedUpdateCoroutine");
	}
	void OnBecameInvisible() {
		canMove = false;
		rigid.velocity = Vector2.zero;

		StopCoroutine("FixedUpdateCoroutine");
	}

	void Start () {
		// Set up Movement
		rigid = GetComponent<Rigidbody2D> ();

		moveLengthCounter = Random.Range (randomMoveLength.x, randomMoveLength.y);
		waitLengthCounter = Random.Range (randomWaitLength.x, randomWaitLength.y);

		// Set bounds of fly zone
		minFlyPoint = new Vector2(transform.position.x - flyZone.x, transform.position.y - flyZone.y);
		maxFlyPoint = new Vector2(transform.position.x + flyZone.x, transform.position.y + flyZone.y);

		// Assign Sorting Layer
		sRend = GetComponent<SpriteRenderer> ();
		if (spriteInForeground) {
			sRend.sortingLayerName = "0";
		} else {
			sRend.sortingLayerName = "Foreground";
		}
	}

	void OnEnable(){
		// Set Animation
		anim = GetComponent<Animator> ();
		anim.CrossFade ("Bug_" + animationNdx + "_Flap", 0);
	}
		
	public IEnumerator FixedUpdateCoroutine () {
		// if Visible
		if (canMove) {
			// if not Paused, no Dialogue Text Box
			if (!RPG.S.paused && !DialogueManager.S.textBoxSpriteGO.activeInHierarchy) {
				Move();
			} else {
				rigid.velocity = Vector2.zero;
			}
		}

		yield return new WaitForFixedUpdate ();
		StartCoroutine ("FixedUpdateCoroutine");
	}

	void Move(){
		if (isMoving) {
            // If bug has flown out of fly zone...
            if (isConstrainedToFlyZone) {
				if (transform.position.x >= maxFlyPoint.x) {
					isMoving = false;
					waitLengthCounter = Random.Range(randomWaitLength.x, randomWaitLength.y);
					moveDirection = Vector3.left * moveSpeed;
				} else if (transform.position.y >= maxFlyPoint.y) {
					isMoving = false;
					waitLengthCounter = Random.Range(randomWaitLength.x, randomWaitLength.y);
					moveDirection = Vector3.down * moveSpeed;
				} else if (transform.position.x <= minFlyPoint.x) {
					isMoving = false;
					waitLengthCounter = Random.Range(randomWaitLength.x, randomWaitLength.y);
					moveDirection = Vector3.right * moveSpeed;
				} else if (transform.position.y <= minFlyPoint.y) {
					isMoving = false;
					waitLengthCounter = Random.Range(randomWaitLength.x, randomWaitLength.y);
					moveDirection = Vector3.up * moveSpeed;
				}
			}

			// If time is up...
			if (moveLengthCounter <= 0) {
				isMoving = false;
				waitLengthCounter = Random.Range(randomWaitLength.x, randomWaitLength.y);
				moveDirection = Random.onUnitSphere * moveSpeed;
			}

			// Flip
			if ((rigid.velocity.x < 0 && facingRight) || (rigid.velocity.x > 0 && !facingRight)) {
				Utilities.S.Flip(gameObject, ref facingRight);
			}

			// Move bug and decrement timer
			rigid.velocity = moveDirection;
			moveLengthCounter -= Time.deltaTime;
		} else {
			// Freeze bug position and decrement timer
			rigid.velocity = Vector2.zero;
			waitLengthCounter -= Time.deltaTime;

			// If time is up...
			if (waitLengthCounter <= 0) {
				isMoving = true;
				moveLengthCounter = Random.Range (randomMoveLength.x, randomMoveLength.y);
				moveDirection = Random.onUnitSphere * moveSpeed;
			}
		}
	}
}

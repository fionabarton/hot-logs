using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// - Movement AI for Enemies
/// - this.mode changes if DetectPlayerZone.cs or ForgetPlayerZone.cs attached to child gameObject
/// </summary>
public class EnemyMovement : MonoBehaviour {
	[Header("Set in Inspector")]
	public eMovement	mode = eMovement.random;

	public float		speed = 2;

	public Vector2		walkZone = new Vector2(2.5f, 2.5f);

	public Vector2		waitDuration = new Vector2(0.75f, 1.25f);

    [Header("Set Dynamically")]
    private Rigidbody2D rigid;
	public Animator		anim;

	// Walk Zone
	Vector2				minWalkPoint;
	Vector2				maxWalkPoint;

	public bool			canMove = true;

	private bool		isWalking;
	private int			walkDirection;
	private int			nextWalkDirection = 999;
	// 0 = right, 1 = up, 2 = left, 3 = down
	// 4 = Down Right, 5 = Up Right, 6 = Up Left, 7 = Down Left

	private float		walkCounter = 0;
	private float		waitCounter = 0;

	// Flip
	private bool		facingRight;

	public eMovement	defaultMovementMode;

	void Start() {
		rigid = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();

		minWalkPoint = new Vector2(transform.position.x - walkZone.x,
								   transform.position.y - walkZone.y);
		maxWalkPoint = new Vector2(transform.position.x + walkZone.x,
								   transform.position.y + walkZone.y);

		StartCoroutine("FixedUpdateCoroutine");
	}
	//void OnBecameVisible() {
	//	canMove = true;
	//	StartCoroutine("FixedUpdateCoroutine");
	//}
	//void OnBecameInvisible() {
	//	canMove = false;
	//	StopCoroutine("FixedUpdateCoroutine");
	//}

	// Walk or Wait Loop
	public IEnumerator FixedUpdateCoroutine() {
		// if Visible
		if (canMove) {
			// if not Paused, no Dialogue Text Box
			if (!RPG.S.paused && !DialogueManager.S.TextBoxSpriteGO.activeInHierarchy) {
				// Flip
				if (rigid.velocity.x < 0 && !facingRight) {
					Flip();
				} else if (rigid.velocity.x > 0 && facingRight) {
					Flip();
				}

				switch (mode) {
					case eMovement.random:
						// Move GameObject
						if (isWalking) {
							switch (walkDirection) {
								case 0: //right
									rigid.velocity = new Vector2(speed, 0);
									if (transform.position.x >= maxWalkPoint.x) {
										WaitSettings();
										nextWalkDirection = 2;
									}
									break;
								case 1: //up
									rigid.velocity = new Vector2(0, speed);
									if (transform.position.y >= maxWalkPoint.y) {
										WaitSettings();
										nextWalkDirection = 3;
									}
									break;
								case 2: //left
									rigid.velocity = new Vector2(-speed, 0);
									if (transform.position.x <= minWalkPoint.x) {
										WaitSettings();
										nextWalkDirection = 0;
									}
									break;
								case 3: //down
									rigid.velocity = new Vector2(0, -speed);
									if (transform.position.y <= minWalkPoint.y) {
										WaitSettings();
										nextWalkDirection = 1;
									}
									break;
								case 4: //right down
									rigid.velocity = new Vector2(speed / 2, -speed / 2);
									if (transform.position.x >= maxWalkPoint.x || transform.position.y <= minWalkPoint.y) {
										WaitSettings();
										nextWalkDirection = 6;
									}
									break;
								case 5: //right up
									rigid.velocity = new Vector2(speed / 2, speed / 2);
									if (transform.position.x >= maxWalkPoint.x || transform.position.y >= maxWalkPoint.y) {
										WaitSettings();
										nextWalkDirection = 7;
									}
									break;
								case 6: //left up
									rigid.velocity = new Vector2(-speed / 2, speed / 2);
									if (transform.position.x <= minWalkPoint.x || transform.position.y >= maxWalkPoint.y) {
										WaitSettings();
										nextWalkDirection = 4;
									}
									break;
								case 7: //left down
									rigid.velocity = new Vector2(-speed / 2, -speed / 2);
									if (transform.position.x <= minWalkPoint.x || transform.position.y <= minWalkPoint.y) {
										WaitSettings();
										nextWalkDirection = 5;
									}
									break;
							}

							// Count down Walk Counter
							walkCounter -= Time.deltaTime;

							if (walkCounter < 0) {
								WaitSettings();
							}
						} else {
							// Count down Wait Counter
							waitCounter -= Time.deltaTime;
							rigid.velocity = Vector2.zero;

							if (waitCounter < 0) {
								WalkSettings();
							}
						}
						break;
					case eMovement.pursueWalk:
						if (Player.S.gameObject.transform.position.x < transform.position.x && XDeltaY()) { // Left
							rigid.velocity = new Vector2(-speed, 0);
						} else if (Player.S.gameObject.transform.position.x > transform.position.x && XDeltaY()) { // Right
							rigid.velocity = new Vector2(speed, 0);
						} else if (Player.S.gameObject.transform.position.y < transform.position.y && !XDeltaY()) { // Down
							rigid.velocity = new Vector2(0, -speed);
						} else if (Player.S.gameObject.transform.position.y > transform.position.y && !XDeltaY()) { // Up
							rigid.velocity = new Vector2(0, speed);
						}
						break;
					case eMovement.pursueRun:
						if (Player.S.gameObject.transform.position.x < transform.position.x && Player.S.gameObject.transform.position.y < transform.position.y) { // Left/Down
							rigid.velocity = new Vector2(-speed, -speed);
						} else if (Player.S.gameObject.transform.position.x < transform.position.x && Player.S.gameObject.transform.position.y > transform.position.y) { // Left/Up
							rigid.velocity = new Vector2(-speed, speed);
						} else if (Player.S.gameObject.transform.position.x > transform.position.x && Player.S.gameObject.transform.position.y < transform.position.y) { // Right/Down
							rigid.velocity = new Vector2(speed, -speed);
						} else if (Player.S.gameObject.transform.position.x > transform.position.x && Player.S.gameObject.transform.position.y > transform.position.y) { // Right/Up
							rigid.velocity = new Vector2(speed, speed);
						}
						break;
					case eMovement.flee:
						if (Player.S.gameObject.transform.position.x > transform.position.x && Player.S.gameObject.transform.position.y > transform.position.y) { // Left/Down
							rigid.velocity = new Vector2(-speed, -speed);
						} else if (Player.S.gameObject.transform.position.x > transform.position.x && Player.S.gameObject.transform.position.y < transform.position.y) { // Left/Up
							rigid.velocity = new Vector2(-speed, speed);
						} else if (Player.S.gameObject.transform.position.x < transform.position.x && Player.S.gameObject.transform.position.y > transform.position.y) { // Right/Down
							rigid.velocity = new Vector2(speed, -speed);
						} else if (Player.S.gameObject.transform.position.x < transform.position.x && Player.S.gameObject.transform.position.y < transform.position.y) { // Right/Up
							rigid.velocity = new Vector2(speed, speed);
						}
						break;
					case eMovement.reverse:
						// Count Down back to defaultMovementMode
						waitCounter -= Time.deltaTime;
						if (waitCounter < 0) {
							mode = defaultMovementMode;
						}
						break;
				}
			} else { // Freeze if Paused
				rigid.velocity = Vector2.zero;
			}

			yield return new WaitForFixedUpdate();
			StartCoroutine("FixedUpdateCoroutine");
		} else {
			rigid.velocity = Vector2.zero;
		}
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
	}

	// Wait
	public void WaitSettings() {
		anim.speed = 0;

		isWalking = false;
		waitCounter = Random.Range(waitDuration.x, waitDuration.y);
	}

	// Bounce back in opposite direction (ex. collisions w/ walls, etc.)
	void OnCollisionEnter2D(Collision2D coll) {
		if (mode == eMovement.pursueWalk || mode == eMovement.pursueRun || mode == eMovement.flee) {
			// if not Paused, no Dialogue Text Box
			if (!RPG.S.paused && !DialogueManager.S.TextBoxSpriteGO.activeInHierarchy) {
				// Set Counter back to defaultMovementMode
				waitCounter = 0.25f;
				// Set mode to Reverse
				mode = eMovement.reverse;
				// Reverse Velocity
				int randomNdx = Random.Range(0, 4);
				switch (randomNdx) {
					case 0:
						rigid.velocity = new Vector2(-speed, -speed);
						break;
					case 1:
						rigid.velocity = new Vector2(-speed, speed);
						break;
					case 2:
						rigid.velocity = new Vector2(speed, -speed);
						break;
					case 3:
						rigid.velocity = new Vector2(speed, speed);
						break;
				}
			}
		}
	}

	// X Distance between Player & Enemy is more than Y Distance (used for eMovement.pursueWalk)
	bool XDeltaY() {
		float tX = Mathf.Abs(transform.position.x - Player.S.gameObject.transform.position.x);
		float tY = Mathf.Abs(transform.position.y - Player.S.gameObject.transform.position.y);

		if (tX > tY) {
			return (true);
		} else {
			return (false);
		}
	}

    // Flip sprite scale
    void Flip() {
        facingRight = !facingRight;
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}
}
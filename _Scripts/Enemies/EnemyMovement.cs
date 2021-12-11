using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// - Movement AI for Enemies
/// - this.mode changes if DetectPlayerZone.cs or ForgetPlayerZone.cs attached to child gameObject
/// </summary>
public class EnemyMovement : MonoBehaviour {
	[Header("Set in Inspector")]
	public eMovement	currentMode = eMovement.randomWalk;
	public eMovement	onForgetPlayerMode = eMovement.randomWalk;
	public eMovement	onDetectPlayerMode;

	public float		speed = 2;

	public Vector2		walkZone = new Vector2(2.5f, 2.5f);
	public Vector2		waitDuration = new Vector2(0.75f, 1.25f);

    [Header("Set Dynamically")]
	// Components
	public Rigidbody2D	rigid;
	public Animator		anim;
	private Enemy		enemy;

	// Walk Zone
	Vector2				minWalkPoint;
	Vector2				maxWalkPoint;

	public bool			canMove = true;

	private bool		isWalking;
	private int			walkDirection;
	private int			nextWalkDirection = -1;
	// 0 = right, 1 = up, 2 = left, 3 = down, 4 = Down Right, 5 = Up Right, 6 = Up Left, 7 = Down Left

	private float		timer = 0;

	// Flip
	private bool		facingRight;

	public eMovement	cachedMode;

	void Start() {
		rigid = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();
		enemy = GetComponent<Enemy>();

		minWalkPoint = new Vector2(transform.position.x - walkZone.x,
								   transform.position.y - walkZone.y);
		maxWalkPoint = new Vector2(transform.position.x + walkZone.x,
								   transform.position.y + walkZone.y);

		// Flee if the enemy's level is only 33% or less of the player's level 
		if (Utilities.S.GetPercentage(enemy.stats[0].LVL, Party.S.stats[0].LVL) <= 0.33f) {
			onDetectPlayerMode = eMovement.flee;
		}

		StartCoroutine("FixedUpdateCoroutine");
	}

	// Walk or Wait Loop
	public IEnumerator FixedUpdateCoroutine() {
		// if Visible
		if (canMove) {
			// if not Paused, no Dialogue Text Box
			if (!RPG.S.paused && !DialogueManager.S.textBoxSpriteGO.activeInHierarchy) {
				// Flip
				if ((rigid.velocity.x < 0 && !facingRight) || (rigid.velocity.x > 0 && facingRight)) {
					Utilities.S.Flip(gameObject, ref facingRight);
				}

				switch (currentMode) {
					case eMovement.randomWalk:
						// Move GameObject
						if (isWalking) {
							switch (walkDirection) {
								case 0: Walk(speed, 0, 2); break; //right
								case 1: Walk(0, speed, 3); break; //up
								case 2: Walk(-speed, 0, 0); break; //left
								case 3: Walk(0, -speed, 1); break; //down
								case 4: Walk(speed / 2, -speed / 2, 6); break; //right down
								case 5: Walk(speed / 2, speed / 2, 7); break; //right up
								case 6: Walk(-speed / 2, speed / 2, 4); break; //left up
								case 7: Walk(-speed / 2, -speed / 2, 5); break; //left down
							}

							// Count down Walk Counter
							timer -= Time.deltaTime;

							if (timer < 0) {
								StartWaiting();
							}
						} else {
							// Count down Wait Counter
							timer -= Time.deltaTime;
							rigid.velocity = Vector2.zero;

							if (timer < 0) {
								StartWalking();
							}
						}
						break;
					case eMovement.pursueWalk:
						if (Player.S.gameObject.transform.position.x < transform.position.x &&
							!Utilities.S.isCloserHorizontally(gameObject, Player.S.gameObject)) { // Left
							rigid.velocity = new Vector2(-speed, 0);
						} else if (Player.S.gameObject.transform.position.x > transform.position.x &&
							!Utilities.S.isCloserHorizontally(gameObject, Player.S.gameObject)) { // Right
							rigid.velocity = new Vector2(speed, 0);
						} else if (Player.S.gameObject.transform.position.y < transform.position.y &&
							Utilities.S.isCloserHorizontally(gameObject, Player.S.gameObject)) { // Down
							rigid.velocity = new Vector2(0, -speed);
						} else if (Player.S.gameObject.transform.position.y > transform.position.y &&
							Utilities.S.isCloserHorizontally(gameObject, Player.S.gameObject)) { // Up
							rigid.velocity = new Vector2(0, speed);
						}
						break;
					case eMovement.pursueRun:
						PursueOrFlee(gameObject, Player.S.gameObject);
						//PursueOrFlee(gameObject, Player.S.gameObject, speed);
						break;
					case eMovement.flee:
						PursueOrFlee(Player.S.gameObject, gameObject);
						//PursueOrFlee(Player.S.gameObject, gameObject, speed * -1);
						break;
					case eMovement.reverse:
						// Countdown back to defaultMovementMode
						timer -= Time.deltaTime;
						if (timer < 0) {
							currentMode = cachedMode;
							timer = Random.Range(waitDuration.x, waitDuration.y);
							StartWalking();
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
	public void StartWalking() {
		anim.speed = 1;
		isWalking = true;
		timer = Random.Range(waitDuration.x, waitDuration.y);

		// Set walkDirection
		if (nextWalkDirection == -1) {
			// Select random direction
			walkDirection = Random.Range(0, 8);
		} else {
			walkDirection = nextWalkDirection;
			// Reset nextWalkDirection 
			nextWalkDirection = -1;
		}
	}

	// Wait
	public void StartWaiting() {
		anim.speed = 0;
		isWalking = false;
		timer = Random.Range(waitDuration.x, waitDuration.y);
	}

	void Walk(float xVelocity, float yVelocity, int nextDirection) {
		rigid.velocity = new Vector2(xVelocity, yVelocity);
		if (transform.position.x <= minWalkPoint.x || transform.position.x >= maxWalkPoint.x ||
			transform.position.y <= minWalkPoint.y || transform.position.y >= maxWalkPoint.y) {
			StartWaiting();
			nextWalkDirection = nextDirection;
		}
	}

	// My implementation
	void PursueOrFlee(GameObject hunter, GameObject chase) {
		if (hunter.transform.position.x >= chase.transform.position.x) {
			if (hunter.transform.position.y >= chase.transform.position.y) {
				rigid.velocity = new Vector2(-speed, -speed); // Left/Down
			} else {
				rigid.velocity = new Vector2(-speed, speed); // Left/Up
			}
		} else {
			if (hunter.transform.position.y >= chase.transform.position.y) {
				rigid.velocity = new Vector2(speed, -speed); // Right/Down
			} else {
				rigid.velocity = new Vector2(speed, speed); // Right/Up
			}
		}
	}

	// Uses Unity's MoveTowards function
	void PursueOrFlee(GameObject hunter, GameObject chase, float speed) {
		// Flip
		if ((hunter.transform.position.x > chase.transform.position.x && !facingRight) ||
			(hunter.transform.position.x < chase.transform.position.x && facingRight)) {
			Utilities.S.Flip(gameObject, ref facingRight);
		}

		transform.position = Vector3.MoveTowards(transform.position, Player.S.transform.position, speed * Time.fixedDeltaTime);
	}

	// Bounce back in opposite direction 
	void OnCollisionEnter2D(Collision2D coll) {
		if (currentMode == eMovement.randomWalk) {
			ChangeDirection(coll, false);
		}
	}

	// Bounce back in random direction
	void OnCollisionStay2D(Collision2D coll) {
		if (currentMode == eMovement.pursueWalk || currentMode == eMovement.pursueRun || currentMode == eMovement.flee) {
			ChangeDirection(coll, true);
		}
	}

	void ChangeDirection(Collision2D coll, bool selectRandomDirection) {
		// If enemy is colliding with ONLY_PLAYER_ENEMY layer
		if (coll.gameObject.layer == 28) {
			// If not Paused, no Dialogue Text Box
			if (!RPG.S.paused && !DialogueManager.S.textBoxSpriteGO.activeInHierarchy) {
				// Set timer duration
				timer = 0.1f;

				// Cache current mode
				cachedMode = currentMode;

				// Set mode to reverse
				currentMode = eMovement.reverse;

				// Randomly select direction
				if (selectRandomDirection) {
					walkDirection = Random.Range(0, 8);
				}

				// Go in opposite direction
				switch (walkDirection) {
					case 0: rigid.velocity = new Vector2(-speed, 0); break;
					case 1: rigid.velocity = new Vector2(0, -speed); break;
					case 2: rigid.velocity = new Vector2(speed, 0); break;
					case 3: rigid.velocity = new Vector2(0, speed); break;
					case 4: rigid.velocity = new Vector2(-speed / 2, speed / 2); break;
					case 5: rigid.velocity = new Vector2(-speed / 2, -speed / 2); break;
					case 6: rigid.velocity = new Vector2(speed / 2, -speed / 2); break;
					case 7: rigid.velocity = new Vector2(speed / 2, speed / 2); break;
				}
			}
		}
	}
}
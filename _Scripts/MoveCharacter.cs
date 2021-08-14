using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveCharacter : MonoBehaviour
{
	[Header("Set in Inspector")]
	public List<int>	directions;
	public List<float>	distances;

	public int speed;

	[Header("Set Dynamically")]
	private Rigidbody2D rigid;
	private Animator	anim;

	private int			moveNdx;

	private bool		facingRight;

	private void Start() {
		rigid = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();
	}

	void OnDisable() {
		// Remove Delgate
		UpdateManager.fixedUpdateDelegate -= FixedLoop;
	}

	public void StartMovement() {
		// Animation
		if(moveNdx < directions.Count) {
			SetAnimation(directions[moveNdx]);
		}

		GetDesination();

		// Add FixedLoop() to UpdateManager
		UpdateManager.fixedUpdateDelegate += FixedLoop;
	}

	public void FixedLoop() {
		if (!RPG.S.paused) {
			if (moveNdx < directions.Count) {
				switch (directions[moveNdx]) {
					case 0: //right
						rigid.velocity = new Vector2(speed, 0);
						if (transform.position.x >= distances[moveNdx]) {
							NextMove();
						}
						break;
					case 1: //up
						rigid.velocity = new Vector2(0, speed);
						if (transform.position.y >= distances[moveNdx]) {
							NextMove();
						}
						break;
					case 2: //left
						rigid.velocity = new Vector2(-speed, 0);
						if (transform.position.x <= distances[moveNdx]) {
							NextMove();
						}
						break;
					case 3: //down
						rigid.velocity = new Vector2(0, -speed);
						if (transform.position.y <= distances[moveNdx]) {
							NextMove();
						}
						break;
				}

				// Flip
				if (rigid.velocity.x < 0 && !facingRight) {
					Flip();
				} else if (rigid.velocity.x > 0 && facingRight) {
					Flip();
				}
			}
        } else {
			rigid.velocity = Vector2.zero;
		}

	}

	void NextMove() {
		moveNdx += 1;

        if (moveNdx < directions.Count) {
            GetDesination();
			SetAnimation(directions[moveNdx]);
		} else {
			// Done moving
			UpdateManager.fixedUpdateDelegate -= FixedLoop;
			rigid.velocity = Vector2.zero;

			// If this is a step in an event,
			// move to next step
			CutsceneManager.S.stepDone = true;
		}
	}

	// Translate the distance to move into destination location (Player Position + Distance)
	void GetDesination () {
		if(moveNdx < directions.Count) {
			switch (directions[moveNdx]) {
				case 0:
					distances[moveNdx] = transform.position.x + distances[moveNdx];
					break;
				case 1:
					distances[moveNdx] = transform.position.y + distances[moveNdx];
					break;
				case 2:
					distances[moveNdx] = transform.position.x - distances[moveNdx];
					break;
				case 3:
					distances[moveNdx] = transform.position.y - distances[moveNdx];
					break;
			}
		}
	}

	void SetAnimation(int ndx) {
        if (anim) {
			switch (ndx) {
				case 1:
					// Check if clip exists
					if (anim.GetLayerIndex("Walk_Up") != -1) {
						anim.CrossFade("Walk_Up", 0);
					}
					break;
				case 3:
					// Check if clip exists
					if (anim.GetLayerIndex("Walk_Down") != -1) {
						anim.CrossFade("Walk_Down", 0);
					}
					break;
				case 0:
				case 2:
					//case 4:
					//case 7:
					// Check if clip exists
					if (anim.GetLayerIndex("Walk_Down_Diagonal") != -1) {
						anim.CrossFade("Walk_Down_Diagonal", 0);
					}
					break;
					//case 5:
					//case 6:
					//	anim.CrossFade("Walk_Up_Diagonal", 0);
					//	break;
			}
		}
	}

	void Flip() {
		facingRight = !facingRight;
		Vector3 tScale = transform.localScale;
		tScale.x *= -1;
		transform.localScale = tScale;
	}
}

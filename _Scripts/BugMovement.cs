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

	public int				animationNdx;

	[Header ("Set Dynamically")]
	private Rigidbody2D		rigid;
	private SpriteRenderer	sRend;
	private Animator		anim;

	private float 			moveLengthCounter;
	private float 			waitLengthCounter;

	private bool 			isMoving;
	private bool 			canMove;

	private Vector3 		moveDirection;

	void Start () {
		// Set up Movement
		rigid = GetComponent<Rigidbody2D> ();

		moveLengthCounter = Random.Range (randomMoveLength.x, randomMoveLength.y);
		waitLengthCounter = Random.Range (randomWaitLength.x, randomWaitLength.y);

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
			if (!RPG.S.paused && !DialogueManager.S.TextBoxSpriteGO.activeInHierarchy) {
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
			rigid.velocity = moveDirection;
			moveLengthCounter -= Time.deltaTime;

			if (moveLengthCounter <= 0) {
				isMoving = false;
				waitLengthCounter = Random.Range (randomWaitLength.x, randomWaitLength.y);
				moveDirection = Random.onUnitSphere * moveSpeed;
			}
		} else {
			rigid.velocity = Vector2.zero;
			waitLengthCounter -= Time.deltaTime;

			if (waitLengthCounter <= 0) {
				isMoving = true;
				moveLengthCounter = Random.Range (randomMoveLength.x, randomMoveLength.y);
				moveDirection = Random.onUnitSphere * moveSpeed;
			}
		}
	}


	void OnBecameVisible(){
		canMove = true;

		StartCoroutine ("FixedUpdateCoroutine");
	}
	void OnBecameInvisible(){
		canMove = false;
		rigid.velocity = Vector2.zero;

		StopCoroutine ("FixedUpdateCoroutine");
	}
}
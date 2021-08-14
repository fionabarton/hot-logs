using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eDirection { up, down, left, right };

public class SlidingBlock : MonoBehaviour {
	[Header("Set in Inspector")]
	public float        speed = 2.5f;

	public float        distanceToMove = 0.5f;
	public float        destination;

    public eDirection   directionToMove = eDirection.down;

	[Header("Set Dynamically")]
	public Rigidbody2D  rigid;

	public bool         triggerActive;

	public bool         isMoving;

	void Start(){
		rigid = GetComponent<Rigidbody2D>();
	}

	void OnTriggerStay2D(Collider2D coll){
		float xDiff = Mathf.Abs(coll.transform.position.x - transform.position.x);
		float yDiff = Mathf.Abs(coll.transform.position.y - transform.position.y);

		if (coll.transform.position.y >= transform.position.y && xDiff < 0.5f && yDiff > 0.5f){
			directionToMove = eDirection.down;
		}

		if (coll.transform.position.y <= transform.position.y && xDiff < 0.5f && yDiff > 0.5f){
			directionToMove = eDirection.up;
		}

		if (coll.transform.position.x >= transform.position.x && yDiff < 0.5f && xDiff > 0.5f){
			directionToMove = eDirection.left;
		}

		if (coll.transform.position.x <= transform.position.x && yDiff < 0.5f && xDiff > 0.5f){
			directionToMove = eDirection.right;
		}

		//Debug.Log(directionToMove);

		if (!isMoving){
			if (!RPG.S.paused) {
				if (coll.gameObject.tag == "PlayerTrigger"){
					// Player RigidBody
					Player.S.rigid.sleepMode = RigidbodySleepMode2D.NeverSleep;

					if (!triggerActive){
						if (Input.GetButtonDown("SNES A Button")){
							triggerActive = true;

                            switch (directionToMove){
								case eDirection.up:
									destination = transform.position.y + distanceToMove;

									Engage();
								break;
								case eDirection.down:
									destination = transform.position.y - distanceToMove;

									Engage();
									break;
								case eDirection.left:
									destination = transform.position.x - distanceToMove;

									Engage();
									break;
								case eDirection.right:
									destination = transform.position.x + distanceToMove;

									Engage();
									break;
							}
						}
					}
				}
			}
		}
	}

	public IEnumerator FixedUpdateCoroutine(){
        if (isMoving){
			Move();
        }

		yield return new WaitForFixedUpdate();
		StartCoroutine("FixedUpdateCoroutine");
	}

    void Move(){
		switch (directionToMove){
			case eDirection.up:
			    rigid.velocity = new Vector2(0, speed);

                if (transform.position.y >= destination){
					Stop();
				}
			break;
			case eDirection.down:
				rigid.velocity = new Vector2(0, -speed);

				if (transform.position.y <= destination){
					Stop();
				}
				break;
			case eDirection.left:
				rigid.velocity = new Vector2(-speed, 0);

				if (transform.position.x <= destination){
					Stop();
				}
				break;
			case eDirection.right:
				rigid.velocity = new Vector2(speed, 0);

				if (transform.position.x >= destination){
					Stop();
				}
				break;
		}
	}

    void Engage(){
		isMoving = true;
		StartCoroutine("FixedUpdateCoroutine");
	}

    void Stop(){
		isMoving = false;
		rigid.velocity = Vector2.zero;
		StopCoroutine("FixedUpdateCoroutine");
		Debug.Log("STOP!");
	}

	void OnTriggerExit2D(Collider2D coll){
		if (coll.gameObject.tag == "PlayerTrigger"){
			// Player RigidBody
			Player.S.rigid.sleepMode = RigidbodySleepMode2D.StartAwake;
			// Deactivate Trigger
			triggerActive = false;

			StopCoroutine("FixedUpdateCoroutine");
		}
	}
}

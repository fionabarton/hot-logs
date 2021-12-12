using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eRPGMode { idle, walkLeft, walkRight, walkUp, walkDown, runLeft, runRight, runUp, runDown, 
						walkUpLeft, walkUpRight, walkDownLeft, walkDownRight, runUpLeft, runUpRight, runDownLeft, runDownRight,
						jumpFull, jumpHalf, attack, knockback };

/// <summary>
/// Player Input, Player Sprite/Animation, & Steps Count
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour {
	[Header("Set in Inspector")]
	public GameObject 			playerTriggerGO;

	// Components
	public Rigidbody2D			rigid;
	public SpriteRenderer		sRend;
	public Animator				anim;
	public BoxCollider2D		boxColl;

	[Header ("Set Dynamically")]
	// Singleton
	private static Player _S;
	public static Player S { get { return _S; } set { _S = value; } }

	private float 				speed = 4f;

	public eRPGMode				mode = eRPGMode.idle;

	// Respawn
	public Vector3				respawnPos;

	// DontDestroyOnLoad
	private static bool			exists;

	// Facing
	private bool				facingRight = true;

	// Last Direction Faced
	public int					lastDirection;
	// 0 = right, 1 = up, 2 = left, 3 = down, 4 = Down/Right, 5 = Up/Right, 6 = Up/Left, 7 = Down/Left

	public bool 				canMove = true;

	// Overworld Player Stats timer to appear on screen
	public float 				playerUITimer;

	// Steps
	private Vector3             currentPos;
	private Vector3             previousPos;
	public int                  stepsCount = 0;// Used in PauseScreen.cs to get/set Steps

	public bool					alreadyTriggered; // Prevents triggering multiple triggers
	public bool					isBattling;

	// The ground level that the player is on
	public int					level = 1;

	// If player/enemy are both on a swap layer trigger,
	// Call StartBattle() OnCollision even if they're not on the same ground level
	public bool					isOnSwapLayerTrigger;

	public Invincibility		invincibility;

	void Awake () {
		S = this;

		// DontDestroyOnLoad
		if (!exists) {
			exists = true;
			DontDestroyOnLoad (transform.gameObject);
		} else {
			Destroy (gameObject);
		}

		// Add Loop() and FixedLoop() to UpdateManager
		UpdateManager.updateDelegate += Loop;
		UpdateManager.fixedUpdateDelegate += FixedLoop;
	}

    void Start() {
		invincibility = GetComponent<Invincibility>();
    }

    #region Update Loop
    public void Loop () {
		if (!RPG.S.paused) {
			if (canMove) {
				// ************ INPUT ************ \\
				switch (mode) {
				case eRPGMode.idle:
					if (Input.GetAxisRaw ("Horizontal") > 0f) { // Walk/Run Right 
						if (Input.GetAxisRaw ("SNES B Button") > 0) {
							SwitchMode (eRPGMode.runRight);
						} else {
							SwitchMode (eRPGMode.walkRight);
						}
					} else if (Input.GetAxisRaw ("Horizontal") < 0f) { // Walk/Run Left 
							if (Input.GetAxisRaw ("SNES B Button") > 0) {
							SwitchMode (eRPGMode.runLeft);
						} else {
							SwitchMode (eRPGMode.walkLeft);
						}
					}
					if (Input.GetAxisRaw ("Vertical") > 0f) { // Walk/Run Up
						if (Input.GetAxisRaw ("SNES B Button") > 0) {
							SwitchMode (eRPGMode.runUp);
						} else {
							SwitchMode (eRPGMode.walkUp);
						}
					} else if (Input.GetAxisRaw ("Vertical") < 0f) { // Walk/Run Down
						if (Input.GetAxisRaw ("SNES B Button") > 0) {
							SwitchMode (eRPGMode.runDown);
						} else {
							SwitchMode (eRPGMode.walkDown);
						}
					}
					break;
				case eRPGMode.walkLeft:
					if (Input.GetAxisRaw ("Horizontal") >= 0) { // Idle
							SwitchMode (eRPGMode.idle);
					} 
					if (Input.GetAxisRaw ("SNES B Button") > 0) { // Run Left
						SwitchMode (eRPGMode.runLeft);
					}
					if (Input.GetAxisRaw ("Vertical") > 0f) { // Walk Up Left
						SwitchMode (eRPGMode.walkUpLeft);
					} else if (Input.GetAxisRaw ("Vertical") < 0f) { // Walk Down Left
						SwitchMode (eRPGMode.walkDownLeft);
					}
					break;
				case eRPGMode.walkRight:
					if (Input.GetAxisRaw ("Horizontal") <= 0) { // Idle
						SwitchMode (eRPGMode.idle);
					}
					if (Input.GetAxisRaw ("SNES B Button") > 0) { // Run Right 
						SwitchMode (eRPGMode.runRight);
					}
					if (Input.GetAxisRaw ("Vertical") > 0f) { // Walk Up Right
						SwitchMode (eRPGMode.walkUpRight);
					} else if (Input.GetAxisRaw ("Vertical") < 0f) { // Walk Down Right
						SwitchMode (eRPGMode.walkDownRight);
					}
					break;
				case eRPGMode.walkUp:
					if (Input.GetAxisRaw ("Vertical") <= 0) { // Idle
							SwitchMode (eRPGMode.idle);
					}
					if (Input.GetAxisRaw ("SNES B Button") > 0) { // Run Up
						SwitchMode (eRPGMode.runUp);
					}
					if (Input.GetAxisRaw ("Horizontal") < 0) { // Walk Up Left
						SwitchMode (eRPGMode.walkUpLeft);
					} else if (Input.GetAxisRaw ("Horizontal") > 0f) { // Walk Up Right
						SwitchMode (eRPGMode.walkUpRight);
					}
					break;
				case eRPGMode.walkDown:		
					if (Input.GetAxisRaw ("Vertical") >= 0) { // Idle
							SwitchMode (eRPGMode.idle);
					}
					if (Input.GetAxisRaw ("SNES B Button") > 0) { // Run Down
						SwitchMode (eRPGMode.runDown);
					}
					if (Input.GetAxisRaw ("Horizontal") < 0) { // Walk Down Left
						SwitchMode (eRPGMode.walkDownLeft);
					} else if (Input.GetAxisRaw ("Horizontal") > 0f) { // Walk Down Right
						SwitchMode (eRPGMode.walkDownRight);
					}
					break;
				case eRPGMode.runLeft:
					if (Input.GetAxisRaw ("Horizontal") >= 0) { // Idle 
						SwitchMode (eRPGMode.idle);
					}
					if (Input.GetAxisRaw ("SNES B Button") <= 0) { // Walk Left
						SwitchMode (eRPGMode.walkLeft);
					}
					if (Input.GetAxisRaw ("Vertical") > 0f) { // Walk Up Left
						SwitchMode (eRPGMode.walkUpLeft);
					} else if (Input.GetAxisRaw ("Vertical") < 0f) { // Walk Down Left
						SwitchMode (eRPGMode.walkDownLeft);
					}
					break;
				case eRPGMode.runRight:
					if (Input.GetAxisRaw ("Horizontal") <= 0) { // Idle
							SwitchMode (eRPGMode.idle); 
					}
					if (Input.GetAxisRaw ("SNES B Button") <= 0) { // Walk Right
						SwitchMode (eRPGMode.walkRight);
					}
					if (Input.GetAxisRaw ("Vertical") > 0f) { // Up Right 
						SwitchMode (eRPGMode.walkUpRight);
					} else if (Input.GetAxisRaw ("Vertical") < 0f) { // Down Right
						SwitchMode (eRPGMode.walkDownRight);
					}
					break;
				case eRPGMode.runUp:
					if (Input.GetAxisRaw ("Vertical") <= 0) { // Idle
						SwitchMode (eRPGMode.idle);
					}
					if (Input.GetAxisRaw ("SNES B Button") <= 0) { // Walk Up
						SwitchMode (eRPGMode.walkUp);
					}
					if (Input.GetAxisRaw ("Horizontal") < 0) { // Up Left
						SwitchMode (eRPGMode.walkUpLeft);
					} else if (Input.GetAxisRaw ("Horizontal") > 0f) { // Up Right
						SwitchMode (eRPGMode.walkUpRight);
					}
					break;
				case eRPGMode.runDown:
					if (Input.GetAxisRaw ("Vertical") >= 0) { // Idle
						SwitchMode (eRPGMode.idle);
					}
					if (Input.GetAxisRaw ("SNES B Button") <= 0) { // Walk Down
						SwitchMode (eRPGMode.walkDown);
					}
					if (Input.GetAxisRaw ("Horizontal") < 0) { // Walk Down Left
						SwitchMode (eRPGMode.walkDownLeft);
					} else if (Input.GetAxisRaw ("Horizontal") > 0f) { // Walk Down Right
						SwitchMode (eRPGMode.walkDownRight);
					}
					break;
				case eRPGMode.walkUpLeft:
				case eRPGMode.walkUpRight:
					if (Input.GetAxisRaw ("Vertical") <= 0) { // Idle
						SwitchMode (eRPGMode.idle);
					}
					if (Input.GetAxisRaw ("Vertical") > 0 && Input.GetAxisRaw ("Horizontal") == 0f) { // Walk Up
						SwitchMode (eRPGMode.walkUp);
					}
					if (Input.GetAxisRaw ("SNES B Button") > 0) { // Run Up Left
						SwitchMode (eRPGMode.runUpLeft);
					}
					break;
				case eRPGMode.walkDownLeft:
				case eRPGMode.walkDownRight:
					if (Input.GetAxisRaw ("Vertical") >= 0) { // Idle
						SwitchMode (eRPGMode.idle);
					}
					if (Input.GetAxisRaw ("Vertical") < 0 && Input.GetAxisRaw ("Horizontal") == 0f) { // Walk Down
						SwitchMode (eRPGMode.walkDown);
					}
					if (Input.GetAxisRaw ("SNES B Button") > 0) { // Run Down Left
						SwitchMode (eRPGMode.runDownLeft);
					}
					break;
				case eRPGMode.runUpLeft:
				case eRPGMode.runUpRight:
					if (Input.GetAxisRaw ("Vertical") <= 0) { // Idle
						SwitchMode (eRPGMode.idle);
					}
					if (Input.GetAxisRaw ("Vertical") > 0 && Input.GetAxisRaw ("Horizontal") == 0f) { // Walk Up
						SwitchMode (eRPGMode.walkUp);
					}
					if (Input.GetAxisRaw ("SNES B Button") <= 0) { // Walk Up Left
						SwitchMode (eRPGMode.walkUpLeft);
					}
					break;
				case eRPGMode.runDownLeft:
				case eRPGMode.runDownRight:
					if (Input.GetAxisRaw ("Vertical") >= 0) { // Idle
						SwitchMode (eRPGMode.idle);
					}
					if (Input.GetAxisRaw ("Vertical") < 0 && Input.GetAxisRaw ("Horizontal") == 0f) { // Walk Down
						SwitchMode (eRPGMode.walkDown);
					}
					if (Input.GetAxisRaw ("SNES B Button") <= 0) { // Walk Up Left
						SwitchMode (eRPGMode.walkUpLeft);
					}
					break;
				}
			}
		}
	}
    #endregion

    public void SwitchMode(eRPGMode tMode){
		// Set Mode
		mode = tMode;

		// Overworld Player Stats UI
		if (!RPG.S.paused && !DialogueManager.S.textBoxSpriteGO.activeInHierarchy) {
			if (RPG.S.currentScene != "Battle"){
				if (mode == eRPGMode.idle) {
					// Set amount of time for Player Stats to stay onscreen
					playerUITimer = Time.time + 1.5f;
                } else {
					// Deactivate Player Stats
					if (PlayerButtons.S.gameObject.activeInHierarchy) {
						PlayerButtons.S.gameObject.SetActive(false);
					}
				}
			}
		}
	}

	void ModeSettings(float triggerPosX, float triggerPosY, int animSpeed, string animName, int tLastDirection){
		// Set Player Trigger Position
		Utilities.S.SetLocalPosition (playerTriggerGO, triggerPosX, triggerPosY);

		// Set Anim
		if (gameObject.activeInHierarchy) {
			anim.speed = animSpeed;
			anim.CrossFade(animName, 0);
		}

		lastDirection = tLastDirection;

        // Measure amount of steps
        currentPos = transform.position;
		// If moved 1 unit
		if (currentPos.x > previousPos.x + 1 || currentPos.x < previousPos.x - 1 || currentPos.y > previousPos.y + 1 || currentPos.y < previousPos.y - 1)
		{
			// Used in PauseScreen.cs to measure & display Steps
			stepsCount += 1;
			previousPos = transform.position;
		}
	}

    #region Fixed Loop
    public void FixedLoop () {
		if (canMove) {
			if (!RPG.S.paused) {
				if (gameObject.activeInHierarchy) {
					// ************ FLIP ************ \\
					if (Input.GetAxisRaw("Horizontal") > 0 && !facingRight ||
						Input.GetAxisRaw("Horizontal") < 0 && facingRight) {
						Utilities.S.Flip(gameObject, ref facingRight);
					}

					// Overworld Player Stats UI
					switch (mode) {
						case eRPGMode.idle:
							if (!RPG.S.paused && !DialogueManager.S.textBoxSpriteGO.activeInHierarchy) {
								if (RPG.S.currentScene != "Battle") {
									if (!PlayerButtons.S.gameObject.activeInHierarchy) {
										if (Time.time >= playerUITimer) {
											PlayerButtons.S.gameObject.SetActive(true);
										}
									}
								}
							}
						break;
					}
				}
			}
		}
		// ************ MODES ************ \\ 
		switch (mode) {
			case eRPGMode.idle:
				// Prevents NPC from launching Player upon collision
				rigid.velocity = Vector2.zero;

				switch (lastDirection) {
					case 0:
					case 2:
						ModeSettings(0.375f, 0, 1, "RPG_Idle_Side", lastDirection);
						break;
					case 1:
						ModeSettings(0, 0.375f, 1, "RPG_Idle_Up", lastDirection);
						break;
					case 3:
						ModeSettings(0, -0.375f, 1, "RPG_Idle_Down", lastDirection);
						break;
					case 5:
					case 6:
						ModeSettings(0.25f, 0.25f, 1, "RPG_Idle_Walk_Up_Diagonal", lastDirection);
						break;
					case 4:
					case 7:
						ModeSettings(0.25f, -0.25f, 1, "RPG_Idle_Walk_Down_Diagonal", lastDirection);
						break;
				}
				break;
			case eRPGMode.walkLeft:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * speed, 0);
				ModeSettings(0.375f, 0, 1, "RPG_Walk_Side", 2);
				break;
			case eRPGMode.walkRight:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * speed, 0);
				ModeSettings(0.375f, 0, 1, "RPG_Walk_Side", 0);
				break;
			case eRPGMode.walkUp:
				rigid.velocity = new Vector2(0, Input.GetAxisRaw("Vertical") * speed);
				ModeSettings(0, 0.375f, 1, "RPG_Walk_Up", 1);
				break;
			case eRPGMode.walkDown:
				rigid.velocity = new Vector2(0, Input.GetAxisRaw("Vertical") * speed);
				ModeSettings(0, -0.375f, 1, "RPG_Walk_Down", 3);
				break;
			case eRPGMode.runLeft:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * (speed * 2), 0);
				ModeSettings(0.375f, 0, 2, "RPG_Walk_Side", 2);
				break;
			case eRPGMode.runRight:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * (speed * 2), 0);
				ModeSettings(0.375f, 0, 2, "RPG_Walk_Side", 0);
				break;
			case eRPGMode.runUp:
				rigid.velocity = new Vector2(0, Input.GetAxisRaw("Vertical") * (speed * 2));
				ModeSettings(0, 0.375f, 2, "RPG_Walk_Up", 1);
				break;
			case eRPGMode.runDown:
				rigid.velocity = new Vector2(0, Input.GetAxisRaw("Vertical") * (speed * 2));
				ModeSettings(0, -0.375f, 2, "RPG_Walk_Down", 3);
				break;
			case eRPGMode.walkUpLeft:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * speed * .75f, Input.GetAxisRaw("Vertical") * speed * .75f);
				ModeSettings(0.25f, 0.25f, 1, "RPG_Walk_Up_Diagonal", 6);
				break;
			case eRPGMode.walkUpRight:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * speed * .75f, Input.GetAxisRaw("Vertical") * speed * .75f);
				ModeSettings(0.25f, 0.25f, 1, "RPG_Walk_Up_Diagonal", 5);
				break;
			case eRPGMode.walkDownLeft:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * speed * .75f, Input.GetAxisRaw("Vertical") * speed * .75f);
				ModeSettings(0.25f, -0.25f, 1, "RPG_Walk_Down_Diagonal", 7);
				break;
			case eRPGMode.walkDownRight:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * speed * .75f, Input.GetAxisRaw("Vertical") * speed * .75f);
				ModeSettings(0.25f, -0.25f, 1, "RPG_Walk_Down_Diagonal", 4);
				break;
			case eRPGMode.runUpLeft:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * (speed * 2) * .75f, Input.GetAxisRaw("Vertical") * (speed * 2) * .75f);
				ModeSettings(0.25f, 0.25f, 2, "RPG_Walk_Up_Diagonal", 6);
				break;
			case eRPGMode.runUpRight:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * (speed * 2) * .75f, Input.GetAxisRaw("Vertical") * (speed * 2) * .75f);
				ModeSettings(0.25f, 0.25f, 2, "RPG_Walk_Up_Diagonal", 5);
				break;
			case eRPGMode.runDownLeft:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * (speed * 2) * .75f, Input.GetAxisRaw("Vertical") * (speed * 2) * .75f);
				ModeSettings(0.25f, -0.25f, 2, "RPG_Walk_Down_Diagonal", 7);
				break;
			case eRPGMode.runDownRight:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * (speed * 2) * .75f, Input.GetAxisRaw("Vertical") * (speed * 2) * .75f);
				ModeSettings(0.25f, -0.25f, 2, "RPG_Walk_Down_Diagonal", 4);
				break;
		}
	}
    #endregion
}
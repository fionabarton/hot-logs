using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	public ePlayerMode			mode = ePlayerMode.idle;

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
				case ePlayerMode.idle:
					if (Input.GetAxisRaw ("Horizontal") > 0f) { // Walk/Run Right 
						if (Input.GetAxisRaw ("SNES Y Button") > 0) {
							SwitchMode (ePlayerMode.runRight);
						} else {
							SwitchMode (ePlayerMode.walkRight);
						}
					} else if (Input.GetAxisRaw ("Horizontal") < 0f) { // Walk/Run Left 
							if (Input.GetAxisRaw ("SNES Y Button") > 0) {
							SwitchMode (ePlayerMode.runLeft);
						} else {
							SwitchMode (ePlayerMode.walkLeft);
						}
					}
					if (Input.GetAxisRaw ("Vertical") > 0f) { // Walk/Run Up
						if (Input.GetAxisRaw ("SNES Y Button") > 0) {
							SwitchMode (ePlayerMode.runUp);
						} else {
							SwitchMode (ePlayerMode.walkUp);
						}
					} else if (Input.GetAxisRaw ("Vertical") < 0f) { // Walk/Run Down
						if (Input.GetAxisRaw ("SNES Y Button") > 0) {
							SwitchMode (ePlayerMode.runDown);
						} else {
							SwitchMode (ePlayerMode.walkDown);
						}
					}
					break;
				case ePlayerMode.walkLeft:
					if (Input.GetAxisRaw ("Horizontal") >= 0) { // Idle
							SwitchMode (ePlayerMode.idle);
					} 
					if (Input.GetAxisRaw ("SNES Y Button") > 0) { // Run Left
						SwitchMode (ePlayerMode.runLeft);
					}
					if (Input.GetAxisRaw ("Vertical") > 0f) { // Walk Up Left
						SwitchMode (ePlayerMode.walkUpLeft);
					} else if (Input.GetAxisRaw ("Vertical") < 0f) { // Walk Down Left
						SwitchMode (ePlayerMode.walkDownLeft);
					}
					break;
				case ePlayerMode.walkRight:
					if (Input.GetAxisRaw ("Horizontal") <= 0) { // Idle
						SwitchMode (ePlayerMode.idle);
					}
					if (Input.GetAxisRaw ("SNES Y Button") > 0) { // Run Right 
						SwitchMode (ePlayerMode.runRight);
					}
					if (Input.GetAxisRaw ("Vertical") > 0f) { // Walk Up Right
						SwitchMode (ePlayerMode.walkUpRight);
					} else if (Input.GetAxisRaw ("Vertical") < 0f) { // Walk Down Right
						SwitchMode (ePlayerMode.walkDownRight);
					}
					break;
				case ePlayerMode.walkUp:
					if (Input.GetAxisRaw ("Vertical") <= 0) { // Idle
							SwitchMode (ePlayerMode.idle);
					}
					if (Input.GetAxisRaw ("SNES Y Button") > 0) { // Run Up
						SwitchMode (ePlayerMode.runUp);
					}
					if (Input.GetAxisRaw ("Horizontal") < 0) { // Walk Up Left
						SwitchMode (ePlayerMode.walkUpLeft);
					} else if (Input.GetAxisRaw ("Horizontal") > 0f) { // Walk Up Right
						SwitchMode (ePlayerMode.walkUpRight);
					}
					break;
				case ePlayerMode.walkDown:		
					if (Input.GetAxisRaw ("Vertical") >= 0) { // Idle
							SwitchMode (ePlayerMode.idle);
					}
					if (Input.GetAxisRaw ("SNES Y Button") > 0) { // Run Down
						SwitchMode (ePlayerMode.runDown);
					}
					if (Input.GetAxisRaw ("Horizontal") < 0) { // Walk Down Left
						SwitchMode (ePlayerMode.walkDownLeft);
					} else if (Input.GetAxisRaw ("Horizontal") > 0f) { // Walk Down Right
						SwitchMode (ePlayerMode.walkDownRight);
					}
					break;
				case ePlayerMode.runLeft:
					if (Input.GetAxisRaw ("Horizontal") >= 0) { // Idle 
						SwitchMode (ePlayerMode.idle);
					}
					if (Input.GetAxisRaw ("SNES Y Button") <= 0) { // Walk Left
						SwitchMode (ePlayerMode.walkLeft);
					}
					if (Input.GetAxisRaw ("Vertical") > 0f) { // Walk Up Left
						SwitchMode (ePlayerMode.walkUpLeft);
					} else if (Input.GetAxisRaw ("Vertical") < 0f) { // Walk Down Left
						SwitchMode (ePlayerMode.walkDownLeft);
					}
					break;
				case ePlayerMode.runRight:
					if (Input.GetAxisRaw ("Horizontal") <= 0) { // Idle
							SwitchMode (ePlayerMode.idle); 
					}
					if (Input.GetAxisRaw ("SNES Y Button") <= 0) { // Walk Right
						SwitchMode (ePlayerMode.walkRight);
					}
					if (Input.GetAxisRaw ("Vertical") > 0f) { // Up Right 
						SwitchMode (ePlayerMode.walkUpRight);
					} else if (Input.GetAxisRaw ("Vertical") < 0f) { // Down Right
						SwitchMode (ePlayerMode.walkDownRight);
					}
					break;
				case ePlayerMode.runUp:
					if (Input.GetAxisRaw ("Vertical") <= 0) { // Idle
						SwitchMode (ePlayerMode.idle);
					}
					if (Input.GetAxisRaw ("SNES Y Button") <= 0) { // Walk Up
						SwitchMode (ePlayerMode.walkUp);
					}
					if (Input.GetAxisRaw ("Horizontal") < 0) { // Up Left
						SwitchMode (ePlayerMode.walkUpLeft);
					} else if (Input.GetAxisRaw ("Horizontal") > 0f) { // Up Right
						SwitchMode (ePlayerMode.walkUpRight);
					}
					break;
				case ePlayerMode.runDown:
					if (Input.GetAxisRaw ("Vertical") >= 0) { // Idle
						SwitchMode (ePlayerMode.idle);
					}
					if (Input.GetAxisRaw ("SNES Y Button") <= 0) { // Walk Down
						SwitchMode (ePlayerMode.walkDown);
					}
					if (Input.GetAxisRaw ("Horizontal") < 0) { // Walk Down Left
						SwitchMode (ePlayerMode.walkDownLeft);
					} else if (Input.GetAxisRaw ("Horizontal") > 0f) { // Walk Down Right
						SwitchMode (ePlayerMode.walkDownRight);
					}
					break;
				case ePlayerMode.walkUpLeft:
				case ePlayerMode.walkUpRight:
					if (Input.GetAxisRaw ("Vertical") <= 0) { // Idle
						SwitchMode (ePlayerMode.idle);
					}
					if (Input.GetAxisRaw ("Vertical") > 0 && Input.GetAxisRaw ("Horizontal") == 0f) { // Walk Up
						SwitchMode (ePlayerMode.walkUp);
					}
					if (Input.GetAxisRaw ("SNES Y Button") > 0) { // Run Up Left
						SwitchMode (ePlayerMode.runUpLeft);
					}
					break;
				case ePlayerMode.walkDownLeft:
				case ePlayerMode.walkDownRight:
					if (Input.GetAxisRaw ("Vertical") >= 0) { // Idle
						SwitchMode (ePlayerMode.idle);
					}
					if (Input.GetAxisRaw ("Vertical") < 0 && Input.GetAxisRaw ("Horizontal") == 0f) { // Walk Down
						SwitchMode (ePlayerMode.walkDown);
					}
					if (Input.GetAxisRaw ("SNES Y Button") > 0) { // Run Down Left
						SwitchMode (ePlayerMode.runDownLeft);
					}
					break;
				case ePlayerMode.runUpLeft:
				case ePlayerMode.runUpRight:
					if (Input.GetAxisRaw ("Vertical") <= 0) { // Idle
						SwitchMode (ePlayerMode.idle);
					}
					if (Input.GetAxisRaw ("Vertical") > 0 && Input.GetAxisRaw ("Horizontal") == 0f) { // Walk Up
						SwitchMode (ePlayerMode.walkUp);
					}
					if (Input.GetAxisRaw ("SNES Y Button") <= 0) { // Walk Up Left
						SwitchMode (ePlayerMode.walkUpLeft);
					}
					break;
				case ePlayerMode.runDownLeft:
				case ePlayerMode.runDownRight:
					if (Input.GetAxisRaw ("Vertical") >= 0) { // Idle
						SwitchMode (ePlayerMode.idle);
					}
					if (Input.GetAxisRaw ("Vertical") < 0 && Input.GetAxisRaw ("Horizontal") == 0f) { // Walk Down
						SwitchMode (ePlayerMode.walkDown);
					}
					if (Input.GetAxisRaw ("SNES Y Button") <= 0) { // Walk Up Left
						SwitchMode (ePlayerMode.walkUpLeft);
					}
					break;
				}
			}
		}
	}
    #endregion

    public void SwitchMode(ePlayerMode tMode){
		// Set Mode
		mode = tMode;

		// Overworld Player Stats UI
		if (!RPG.S.paused && !DialogueManager.S.textBoxSpriteGO.activeInHierarchy) {
			if (RPG.S.currentScene != "Battle"){
				if (mode == ePlayerMode.idle) {
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
		if (currentPos.x > previousPos.x + 1 || currentPos.x < previousPos.x - 1 || currentPos.y > previousPos.y + 1 || currentPos.y < previousPos.y - 1) {
			// Used in PauseScreen.cs to measure & display Steps
			stepsCount += 1;
			previousPos = transform.position;

			// Every 4 steps...
			if(stepsCount % 4 == 0) {
				// For each party member...
				for (int i = 0; i <= Party.S.partyNdx; i++) {
					// If poisoned...
					if (BattleStatusEffects.S.CheckIfPoisoned(Party.S.stats[i].name)) {
						Debug.Log(Party.S.stats[i].name + "is poisoned!");

						// ...decrement HP by 1
						if (Party.S.stats[i].HP > 1) {
							Party.S.stats[i].HP -= 1;
						}

						// Audio: Damage
						AudioManager.S.PlayRandomDamageSFX();

						// Start flickering
						invincibility.StartInvincibility(0.5f, 0.1f, false);

						// Display Floating Score
						RPG.S.InstantiateFloatingScore(gameObject, "-1", Color.red);
					}
				}
			}
			
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
						case ePlayerMode.idle:
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
			case ePlayerMode.idle:
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
			case ePlayerMode.walkLeft:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * speed, 0);
				ModeSettings(0.375f, 0, 1, "RPG_Walk_Side", 2);
				break;
			case ePlayerMode.walkRight:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * speed, 0);
				ModeSettings(0.375f, 0, 1, "RPG_Walk_Side", 0);
				break;
			case ePlayerMode.walkUp:
				rigid.velocity = new Vector2(0, Input.GetAxisRaw("Vertical") * speed);
				ModeSettings(0, 0.375f, 1, "RPG_Walk_Up", 1);
				break;
			case ePlayerMode.walkDown:
				rigid.velocity = new Vector2(0, Input.GetAxisRaw("Vertical") * speed);
				ModeSettings(0, -0.375f, 1, "RPG_Walk_Down", 3);
				break;
			case ePlayerMode.runLeft:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * (speed * 2), 0);
				ModeSettings(0.375f, 0, 2, "RPG_Walk_Side", 2);
				break;
			case ePlayerMode.runRight:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * (speed * 2), 0);
				ModeSettings(0.375f, 0, 2, "RPG_Walk_Side", 0);
				break;
			case ePlayerMode.runUp:
				rigid.velocity = new Vector2(0, Input.GetAxisRaw("Vertical") * (speed * 2));
				ModeSettings(0, 0.375f, 2, "RPG_Walk_Up", 1);
				break;
			case ePlayerMode.runDown:
				rigid.velocity = new Vector2(0, Input.GetAxisRaw("Vertical") * (speed * 2));
				ModeSettings(0, -0.375f, 2, "RPG_Walk_Down", 3);
				break;
			case ePlayerMode.walkUpLeft:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * speed * .75f, Input.GetAxisRaw("Vertical") * speed * .75f);
				ModeSettings(0.25f, 0.25f, 1, "RPG_Walk_Up_Diagonal", 6);
				break;
			case ePlayerMode.walkUpRight:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * speed * .75f, Input.GetAxisRaw("Vertical") * speed * .75f);
				ModeSettings(0.25f, 0.25f, 1, "RPG_Walk_Up_Diagonal", 5);
				break;
			case ePlayerMode.walkDownLeft:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * speed * .75f, Input.GetAxisRaw("Vertical") * speed * .75f);
				ModeSettings(0.25f, -0.25f, 1, "RPG_Walk_Down_Diagonal", 7);
				break;
			case ePlayerMode.walkDownRight:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * speed * .75f, Input.GetAxisRaw("Vertical") * speed * .75f);
				ModeSettings(0.25f, -0.25f, 1, "RPG_Walk_Down_Diagonal", 4);
				break;
			case ePlayerMode.runUpLeft:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * (speed * 2) * .75f, Input.GetAxisRaw("Vertical") * (speed * 2) * .75f);
				ModeSettings(0.25f, 0.25f, 2, "RPG_Walk_Up_Diagonal", 6);
				break;
			case ePlayerMode.runUpRight:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * (speed * 2) * .75f, Input.GetAxisRaw("Vertical") * (speed * 2) * .75f);
				ModeSettings(0.25f, 0.25f, 2, "RPG_Walk_Up_Diagonal", 5);
				break;
			case ePlayerMode.runDownLeft:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * (speed * 2) * .75f, Input.GetAxisRaw("Vertical") * (speed * 2) * .75f);
				ModeSettings(0.25f, -0.25f, 2, "RPG_Walk_Down_Diagonal", 7);
				break;
			case ePlayerMode.runDownRight:
				rigid.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * (speed * 2) * .75f, Input.GetAxisRaw("Vertical") * (speed * 2) * .75f);
				ModeSettings(0.25f, -0.25f, 2, "RPG_Walk_Down_Diagonal", 4);
				break;
		}
	}
    #endregion
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour {
	[Header("Set in Inspector")]
	// Cursors
	public GameObject		turnCursor;
	public SpriteRenderer	turnCursorSRend;

	public List<GameObject> targetCursors = new List<GameObject>();
	public List<Animator>	targetCursorAnims = new List<Animator>();

	// Turn Order UI
	public Text				turnOrderTxt;

	[Header("Set Dynamically")]
	// Singleton
	private static BattleUI _S;
	public static BattleUI S { get { return _S; } set { _S = value; } }

	private Battle _;

	void Awake() {
		S = this;
	}

	void Start() {
		_ = Battle.S;
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.T)) {
			TargetAllEnemies();
		}
	}

	public void TurnCursorPosition(GameObject go) {
		// Activate Cursor
		if (!turnCursor.activeSelf) {
			turnCursor.SetActive(true);
		}

		// Position Cursor
		// - dynamically set for Enemies, but explicitly set for Party Members: 
		//	 in case function is called when Party is still running to their positions
		
		if (_.turnNdx == _.turnOrder.IndexOf(Party.stats[0].name)) { // Player 1
			Vector2 t = new Vector2(-4.5f, 2.75f);
			turnCursor.transform.localPosition = t;
		} else if (_.turnNdx == _.turnOrder.IndexOf(Party.stats[1].name)) { // Player 2
			Vector2 t = new Vector2(-6, 0.75f);
			turnCursor.transform.localPosition = t;
		} else { // Enemies
			float tPosX = go.transform.localPosition.x;
			float tPosY = go.transform.localPosition.y;

			float tParentX = go.transform.parent.localPosition.x;
			float tParentY = go.transform.parent.localPosition.y;

			turnCursor.transform.localPosition = new Vector2((tPosX + tParentX), (tPosY + tParentY + 1.25f));
		}
	}

	public void TargetCursorPosition(GameObject go, float y) {
		// Activate Cursor
		if (!targetCursors[0].activeInHierarchy) {
			targetCursors[0].SetActive(true);
		}

		// Position Cursor & Set Anim
		if (targetCursors[0].activeInHierarchy) {
			if (go == BattlePlayerActions.S.playerButtonGO[0]) {
				targetCursors[0].transform.localPosition = new Vector2((_.playerSprite[0].transform.position.x + 1), (_.playerSprite[0].transform.position.y + y));
				targetCursorAnims[0].CrossFade("Target_Cursor_Flash_Left", 0);
			} else if (go == BattlePlayerActions.S.playerButtonGO[1]) {
				targetCursors[0].transform.localPosition = new Vector2((_.playerSprite[1].transform.position.x + 1), (_.playerSprite[1].transform.position.y + y));
				targetCursorAnims[0].CrossFade("Target_Cursor_Flash_Left", 0);
			} else if (go == BattlePlayerActions.S.enemyButtonGO[0]) {
				targetCursors[0].transform.localPosition = new Vector2((_.enemySprite[0].transform.position.x + -1), (_.enemySprite[0].transform.position.y + y));
				targetCursorAnims[0].CrossFade("Target_Cursor_Flash_Right", 0);
			} else if (go == BattlePlayerActions.S.enemyButtonGO[1]) {
				targetCursors[0].transform.localPosition = new Vector2((_.enemySprite[1].transform.position.x + -1), (_.enemySprite[1].transform.position.y + y));
				targetCursorAnims[0].CrossFade("Target_Cursor_Flash_Right", 0);
			} else if (go == BattlePlayerActions.S.enemyButtonGO[2]) {
				targetCursors[0].transform.localPosition = new Vector2((_.enemySprite[2].transform.position.x + -1), (_.enemySprite[2].transform.position.y + y));
				targetCursorAnims[0].CrossFade("Target_Cursor_Flash_Right", 0);
			} else {
				RectTransform rectTrans = go.GetComponent<RectTransform>();
				if (rectTrans != null) {
					targetCursors[0].transform.localPosition = new Vector2((rectTrans.position.x + 1), (rectTrans.position.y + y));
					targetCursorAnims[0].CrossFade("Target_Cursor_Flash_Left", 0);
				}
			}
		}
	}

	public void TargetAllEnemies() {
		for (int i = _.enemyStats.Count - 1; i >= 0; i--) {
			if (!_.enemyStats[i].isDead) {
				// Activate and set target cursor position
				targetCursors[i].SetActive(true);
				targetCursors[i].transform.localPosition = new Vector2((_.enemySprite[i].transform.position.x + -1), (_.enemySprite[i].transform.position.y));
				targetCursorAnims[i].CrossFade("Target_Cursor_Flash_Right", 0);
			} else {
				//Deactivate target cursor
				targetCursors[i].SetActive(false);
			}
		}
	}

	public void TargetAllPartyMembers() {
		for (int i = _.playerDead.Count - 1; i >= 0; i--) {
			if (!_.playerDead[i]) {
				// Activate and set target cursor position
				targetCursors[i].SetActive(true);
				targetCursors[i].transform.localPosition = new Vector2((_.playerSprite[i].transform.position.x + 1), (_.playerSprite[i].transform.position.y));
				targetCursorAnims[i].CrossFade("Target_Cursor_Flash_Left", 0);
			} else {
				//Deactivate target cursor
				targetCursors[i].SetActive(false);
			}
		}
	}

	public void DisplayTurnOrder() {
		// Get index of character whose turn it is
		int tInt = _.turnNdx;

		// To be populated with an ordered string of character names
		string tText = null;

		// For all party member's and enemies currently engaged in battle...
		for (int i = 0; i < _.turnOrder.Count; i++) {
			// Add a character to the string
			if (i <= 0) {
				// Start string with name of character whose turn it is
				tText += _.turnOrder[tInt];
			} else {
				// For each subsequent character, add new line followed by their name
				tText += "\n" + _.turnOrder[tInt];
			}

			// Change index of which character name to add to string on next iteration
			if (tInt < (_.enemyAmount + _.partyQty)) {
				tInt += 1;
			} else {
				tInt = 0;
			}
		}

		// Display ordered string of character names
		turnOrderTxt.text = tText;
	}
}
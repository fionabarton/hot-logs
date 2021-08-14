using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMove : MonoBehaviour {
	[Header("Set in Inspector")]
	public float 		speed = 4;
	public bool 		isActive = true;

	[Header ("Set Dynamically")]
	public Vector3 		currentPos;

	void Start () {
		// Prevents Player from being pulled to currentPos on Play
		if (isActive) {
			currentPos = this.transform.position;
		}
	}

	void Update () {
		if (isActive) {
			currentPos = this.transform.position;
		}
	}

	void FixedUpdate (){
		if (isActive) {
			Vector2 pos = currentPos;
			Vector2 posGrid = GetPosOnGrid ();

			float delta = 0;

			// Find delta of Player Pos to Grid Pos
			if (Player.S.lastDirection == 2 || Player.S.lastDirection == 0) {
				delta = posGrid.y - pos.y;
			} else {
				delta = posGrid.x - pos.x;
			}

			// If already on Grid, return
			if (delta == 0) return;

			float move = speed * Time.deltaTime;
			move = Mathf.Min (move, Mathf.Abs (delta));
			if (delta < 0)
				move = -move;

			// Add Grid Pos to Player Pos
			if (Player.S.lastDirection == 2 || Player.S.lastDirection == 0) {
				pos.y += move;
			} else {
				pos.x += move;
			}

			this.transform.position = pos;
		}
	}

	// Round Player Pos to Grid Pos
	public Vector2 GetPosOnGrid (float mult = 0.5f){
		Vector2 pos = currentPos;
		pos /= mult;
		pos.x = Mathf.Round (pos.x);
		pos.y = Mathf.Round (pos.y);
		pos *= mult;
		return pos;
	}
}

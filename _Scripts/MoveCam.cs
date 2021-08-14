using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveCam : MonoBehaviour {
	[Header("Set in Inspector")]
	public List<int>	directions;
	public List<float>	distances;

	public float		speed = 5;

	[Header("Set Dynamically")]
	private int			moveNdx;

	void OnDisable() {
		// Remove Delgate
		UpdateManager.fixedUpdateDelegate -= FixedLoop;
	}

	public void StartMovement() {
		// Prevent cam from following any gameObject
		CamManager.S.camMode = eCamMode.noTarget;

		GetDesination();

		// Add FixedLoop() to UpdateManager
		UpdateManager.fixedUpdateDelegate += FixedLoop;
	}

	public void FixedLoop() {
		if (!RPG.S.paused) {
			if (moveNdx < directions.Count) {
				switch (directions[moveNdx]) {
					case 0: //right
						Utilities.S.MoveXPosition(gameObject, speed);

						if (transform.position.x >= distances[moveNdx]) {
							NextMove();
						}
						break;
					case 1: //up
						Utilities.S.MoveYPosition(gameObject, speed);

						if (transform.position.y >= distances[moveNdx]) {
							NextMove();
						}
						break;
					case 2: //left
						Utilities.S.MoveXPosition(gameObject, -speed);

						if (transform.position.x <= distances[moveNdx]) {
							NextMove();
						}
						break;
					case 3: //down
						Utilities.S.MoveYPosition(gameObject, -speed);

						if (transform.position.y <= distances[moveNdx]) {
							NextMove();
						}
						break;
				}
			}
		} 
	}

	void NextMove() {
		moveNdx += 1;

		if (moveNdx < directions.Count) {
			GetDesination();
		} else {
			// Done moving
			UpdateManager.fixedUpdateDelegate -= FixedLoop;

			// Reset lists
            directions.Clear();
            distances.Clear();
        }
	}

	// Translate the distance to move into destination location (Player Position + Distance)
	void GetDesination() {
		if (moveNdx < directions.Count) {
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
}
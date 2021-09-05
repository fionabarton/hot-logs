using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainSpawner : MonoBehaviour {
	[Header("Set in Inspector")]
	public List<GameObject>	rainDrops = new List<GameObject>();

	public List<Sprite>		rainSprites = new List<Sprite>();

	public GameObject		darkFilter;

	public Transform		rainAnchorGO;

	[Header("Set Dynamically")]
	// Singleton
	private static RainSpawner _S;
	public static RainSpawner S { get { return _S; } set { _S = value; } }

	private float 			xPos, yPos;

	public bool 			isRaining;

	private float 			timeToWaitToSpawn;
	private float 			timeToSpawn;

	private int 			dropNdx;

	void Awake() {
		// Singleton
		S = this;
	}

	public IEnumerator FixedUpdateCoroutine () {
		// if not Paused
		if (!RPG.S.paused) {
			if (isRaining) {
				if (Time.time >= timeToSpawn) {
					SpawnRain ();
				}
			}
		}

		yield return new WaitForFixedUpdate ();
		StartCoroutine ("FixedUpdateCoroutine");
	}

	void SpawnRain(){
		if (!rainDrops [dropNdx].activeInHierarchy) {
			// Randomly Set X & Y
			xPos = Random.Range(-12f, 12f);
			yPos = Random.Range(-5f, 10f);

			// Set Position
			Utilities.S.SetLocalPosition (rainDrops [dropNdx], xPos, yPos);

			// Randomly timeToWaitToSpawn
			//timeToWaitToSpawn = Random.Range(0.001f, 0.005f);
			timeToWaitToSpawn = 0.001f;

			// Set timeToSpawn
			timeToSpawn = Time.time + timeToWaitToSpawn;

			// Activate
			rainDrops [dropNdx].SetActive (true);

			// Next RainDrop to Drop
			if (dropNdx >= rainDrops.Count - 1) {
				dropNdx = 0;
			} else {
				dropNdx += 1;
			}
		}
	}
}

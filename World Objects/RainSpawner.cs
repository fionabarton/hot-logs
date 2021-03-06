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

	public bool 			isRaining;
	private int 			dropNdx;
	private int				amountOfDropsToSpawnPerFrame = 2;

	void Awake() {
		S = this;
	}

	public void StartRaining(int amountOfDropsToSpawnPerFrame = 2) {
		isRaining = true;
		darkFilter.SetActive(true);
		this.amountOfDropsToSpawnPerFrame = amountOfDropsToSpawnPerFrame;

		// Add FixedLoop() to Fixed Update Delgate
		UpdateManager.fixedUpdateDelegate += FixedLoop;
	}

	public void StopRaining() {
		isRaining = false;
		darkFilter.SetActive(false);

		// Remove FixedLoop() from Fixed Update Delgate
		UpdateManager.fixedUpdateDelegate -= FixedLoop;
	}

	private void FixedLoop() {
		if (!GameManager.S.paused) {
            if (isRaining) {
				SpawnRain();
			}
        }
	}

	void SpawnRain(){
		for(int i = 0; i < amountOfDropsToSpawnPerFrame; i++) {
			if (!rainDrops[dropNdx].activeInHierarchy) {
				// Randomly Set X & Y
				float xPos = Random.Range(-12f, 12f);
				float yPos = Random.Range(-5f, 10f);

				// Set Position
				Utilities.S.SetLocalPosition(rainDrops[dropNdx], xPos, yPos);

				// Activate gameObject
				rainDrops[dropNdx].SetActive(true);

				// Next RainDrop to Drop
				if (dropNdx >= rainDrops.Count - 1) {
					dropNdx = 0;
				} else {
					dropNdx += 1;
				}
			}
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Is activated by Player entering a BattlePatch
/// </summary>
public class RandomEncounter : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static RandomEncounter _S;
	public static RandomEncounter S { get { return _S; } set { _S = value; } }

	public Vector3 				currentPos;
	public Vector3 				playerPos;

	public List<Enemy> 	enemyList = new List <Enemy> ();

	void Awake() {
		// Singleton
		S = this;
	}

    // Import Enemies from Battle Patch
    public void ImportEnemies(List<Enemy> tEnemies) {
        // Reset Enemy List
        enemyList.Clear();

        // Add Enemies from Battle Patch to Enemy List
        for (int i = 0; i < tEnemies.Count; i++) {
            enemyList.Add(tEnemies[i]);
        }
    }

    // Called in BattlePatch.CS
    public void StartStopCoroutine(bool startOrStop){
        if (startOrStop){
			StartCoroutine("FixedUpdateCoroutine");
		}
        else{
			StopCoroutine("FixedUpdateCoroutine");
		}
    }
	public IEnumerator FixedUpdateCoroutine()
	{
		// Player's Current Position
		currentPos = transform.position;

		// If moved 1 unit
		if (currentPos.x > playerPos.x + 1 || currentPos.x < playerPos.x - 1 || currentPos.y > playerPos.y + 1 || currentPos.y < playerPos.y - 1) {
			// 1/48 (2%) chance of Random Encounter each Step
			if (!Player.S.invincible) {
				if (Random.value < 0.05f) {
					// Randomly Select Enemy Group dropped into Inspector
					int enemyIndex = Random.Range (0, enemyList.Count - 1);

					// Get Random Enemy's Stats
					Enemy eStats = enemyList [enemyIndex].GetComponent<Enemy> ();

					// Start Battle
					//RPG.S.StartBattle (eStats);

					Debug.Log("RANDOM BATTLE!!!!");
				}
			}

			// Document Player's Current Position
			playerPos = transform.position;
		}
		yield return new WaitForFixedUpdate();
		StartCoroutine("FixedUpdateCoroutine");
	}
}

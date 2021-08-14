using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////////////////////////////////////////////////
// Respawn Objects (Fruit, etc.)
///////////////////////////////////////////////////////////////////////////////////////////

public class RespawnManager : MonoBehaviour {
	[Header("Set in Inspector")]

	public List<GameObject> 		GO = new List<GameObject> ();		

	public List<bool>    			isEnabled = new List<bool> ();
	public List<int> 				duration = new List<int> ();
	public List<Vector2>    		location = new List<Vector2> ();

	public List<string> 			sceneName = new List<string> ();

	[Header("Set Dynamically")]
	// Singleton
	private static RespawnManager _S;
	public static RespawnManager S { get { return _S; } set { _S = value; } }

	public System.DateTime[] 		rebirthDay = new System.DateTime[15];

	// DontDestroyOnLoad
	public static bool				respawnManExists;

	void Awake() {
		// Singleton
		S = this;

		// DontDestroyOnLoad
		if (!respawnManExists) {
			respawnManExists = true;
			DontDestroyOnLoad (gameObject);
		} else {
			Destroy (gameObject);
		}
	}
		
	// Called RPGLevelMan(134)
	public void SpawnObjects (string currentScene){

		// Activate/Deactivate
		for(int i = 0; i< GO.Count;i++){
			if (currentScene == sceneName [i]) {
				if (isEnabled [i]) {

					// Set Position
					GO [i].transform.position = location [i];
					// Reactivate
					GO [i].SetActive (true);
				} else {
					if (rebirthDay [i] <= System.DateTime.Now) {

						// Set Position
						GO [i].transform.position = location [i];
						// Reactivate
						GO [i].SetActive (true);
						isEnabled [i] = true;

						// Reset Rebirth Date
						rebirthDay [i] = System.DateTime.Now;
					} 
				}
			} else {
				// Deactivate
				GO[i].SetActive (false);
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////
	// Called in ItemTrigger /////////////////////////////////////////////////////////////////////
	public void DeactivateObject(int ndx){
		// Deactivate
		GO[ndx].SetActive (false);

		isEnabled [ndx] = false;

		// Set Position
		GO[ndx].transform.position = location[ndx];

		// Get Duration
		System.TimeSpan length = new System.TimeSpan (0, duration [ndx], 0);
		// Set Rebirth Day/Time
		rebirthDay[ndx] = System.DateTime.Now.Add(length);  
		Debug.Log(rebirthDay[ndx]); 
	}
	//////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////
}

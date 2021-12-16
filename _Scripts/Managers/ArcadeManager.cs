using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scene MUST be added to switch statement in this script
/// AND also in RPGLevelManager.cs at line 190.
/// </summary>
public class ArcadeManager : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static ArcadeManager _S;
	public static ArcadeManager S { get { return _S; } set { _S = value; } }

	void Awake() {
		// Singleton
		S = this;
	}

	public void Loop() {
		if (Input.GetKeyDown(KeyCode.X) || Input.GetAxisRaw("SNES Start Button") > 0) {
			// Activate Black Screen
			ColorScreen.S.ActivateBlackScreen();

			// Load Level
			Debug.Log(RPG.S.previousScene);
			RPG.S.LoadLevel(RPG.S.previousScene);

			// Update Delegate
			UpdateManager.updateDelegate -= Loop;
		}
	}
}

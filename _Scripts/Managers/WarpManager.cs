using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpManager : MonoBehaviour
{
    [Header("Set Dynamically")]
    // Singleton
    private static WarpManager _S;
    public static WarpManager S { get { return _S; } set { _S = value; } }

    void Awake() {
        // Singleton
        S = this;
    }

	public IEnumerator Warp(Vector3 tWarpPos, 
							bool tWarpToNewScene, 
							string tSceneName = "zTown", 
							bool tCamFollows = true, 
							Vector3 tCamWarpPos = default(Vector3)) {
		// Enable Black Screen
		RPG.S.blackScreen.enabled = true;
		// Deactivate Player
		Player.S.gameObject.SetActive(false);

		if (!tWarpToNewScene) {
			// Set PlayerPos to PlayerWarpPos
			Player.S.gameObject.transform.position = tWarpPos;
		} else {
			// Set PlayerRespawnPos to PlayerWarpPos
			Player.S.respawnPos = tWarpPos;
		}

		// Wait for 0.5f seconds
		yield return new WaitForSeconds(0.5f);

		if (!tWarpToNewScene) {
			// Disable Black Screen
			RPG.S.blackScreen.enabled = false;
			// Enable Player
			Player.S.gameObject.SetActive(true);
		} else {
			// Load Scene
			RPG.S.LoadLevel(tSceneName);
		}

		// Camera Settings
		if (tCamFollows) {
			// Camera Follows Player
			CamManager.S.camMode = eCamMode.followAll;
		} else {
			// Set Freeze Camera Position
			CamManager.S.camMode = eCamMode.freezeCam;
			CamManager.S.transform.position = tCamWarpPos;
		}
	}
}
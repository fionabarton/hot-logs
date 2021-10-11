using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarpManager : MonoBehaviour {
    [Header("Set Dynamically")]
    // Singleton
    private static WarpManager	_S;
    public static WarpManager	S { get { return _S; } set { _S = value; } }

	// All warp locations 
	public List<WarpLocation>	locations = new List<WarpLocation>();

	// Locations the party has already visited and can warp to
	public List<WarpLocation>	visitedLocations = new List<WarpLocation>();

	// Ensures audio is only played once when button is selected
	public GameObject			previousSelectedLocationGO;

	void Awake() {
        S = this;
    }

    void Start() {
		locations.Add(new WarpLocation("Home", "Area_1", new Vector3(0, 2, 0), "My town, Chi town. The windy city, stooge.", 1));
		locations.Add(new WarpLocation("Brown Valley", "Area_2", new Vector3(0, -2, 0), "Connects Home to the overworld. Watch out for cretins.", 3));
		locations.Add(new WarpLocation("Downtown", "Town_1", new Vector3(0, 2, 0), "Buy stuff.", 3));
	}

	// Record that the player has visited this location
	public void HasVisited(string sceneName) {
		// Return if the party has already visited this location
		for (int i = 0; i < visitedLocations.Count; i++) {
			if (sceneName == visitedLocations[i].sceneName) {
				return;
			}
		}

		// Add this location to the list of locations the party can warp to
		for (int i = 0; i < locations.Count; i++) {
			if (sceneName == locations[i].sceneName) {
				visitedLocations.Add(locations[i]);
				return;
			}
		}
	}

	public void DisplayButtonDescriptions(List<Button> buttons, int cursorDistanceFromCenter) {
		for (int i = 0; i < visitedLocations.Count; i++) {
            if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == buttons[i].gameObject) {
				PauseMessage.S.SetText("<color=#FFFFFF>Warp to:</color> " + 
				visitedLocations[i].name + "?\n" + "<color=#FFFFFF>Description:</color> " + visitedLocations[i].description, true);

				// Cursor Position set to Selected Button
				Utilities.S.PositionCursor(buttons[i].gameObject, cursorDistanceFromCenter, 0, 0);

				// Set selected button text color	
				buttons[i].gameObject.GetComponentInChildren<Text>().color = new Color32(205, 208, 0, 255);

				// Audio: Selection (when a new gameObject is selected)
				Utilities.S.PlayButtonSelectedSFX(ref previousSelectedLocationGO);		
			} else {
                // Set non-selected button text color
                buttons[i].gameObject.GetComponentInChildren<Text>().color = new Color32(255, 255, 255, 255);
            }
        }
	}

	public void DeactivateUnusedButtonSlots(List<Button> buttons) {
        for (int i = 0; i < buttons.Count; i++) {
			buttons[i].gameObject.SetActive(false);
        }

		for (int i = 0; i < visitedLocations.Count; i++) {
			buttons[i].gameObject.SetActive(true);
		}
	}

	public void AssignButtonEffect(List<Button> buttons) {
		Utilities.S.RemoveListeners(buttons);

		for (int i = 0; i < visitedLocations.Count; i++) {
			// Add listener to Button
			int copy = i;
			buttons[copy].onClick.AddListener(delegate {
                StartCoroutine(Warp(
                    visitedLocations[copy].position,
                    true,
                    visitedLocations[copy].sceneName,
					visitedLocations[copy].playerFacingDirection));
            });
		}
	}

    public void AssignButtonNames(List<Text> buttonsText) {
		for (int i = 0; i < visitedLocations.Count; i++) {
			// Assign Button Name Text
			buttonsText[i].text = visitedLocations[i].name;
		}
    }

	// "Warps" the player to a new scene and/or position
	public IEnumerator Warp(Vector3 destinationPos,
							bool warpToNewScene,
							string sceneName = "zTown",
							int facingDirection = 99,
							bool camFollows = true, 
							Vector3 camWarpPos = default(Vector3)) {
		// Enable Black Screen
		RPG.S.blackScreen.enabled = true;
		// Deactivate Player
		Player.S.gameObject.SetActive(false);

		if (!warpToNewScene) {
			// Set PlayerPos to destinationPos
			Player.S.gameObject.transform.position = destinationPos;
		} else {
			// Set PlayerRespawnPos to destinationPos
			Player.S.respawnPos = destinationPos;
		}

		// Set Player facing direction
		if(facingDirection != 99) {
			Player.S.lastDirection = facingDirection;

			// Audio: Buff 2
			AudioManager.S.PlaySFX(eSoundName.buff2);
		}

		// Camera Settings
		if (camFollows) {
			// Camera Follows Player
			CamManager.S.camMode = eCamMode.followAll;
		} else {
			// Set Freeze Camera Position
			CamManager.S.camMode = eCamMode.freezeCam;
			CamManager.S.transform.position = camWarpPos;
		}

		// Wait for 0.5f seconds
		yield return new WaitForSeconds(0.5f);

		if (!warpToNewScene) {
			// Disable Black Screen
			RPG.S.blackScreen.enabled = false;
			// Enable Player
			Player.S.gameObject.SetActive(true);
		} else {
			// Load Scene
			RPG.S.LoadLevel(sceneName);
		}
	}
}

public class WarpLocation {
	public string name;
	public string sceneName;
	public Vector3 position;
	public string description;
	public int playerFacingDirection;

	public WarpLocation(string name, string sceneName, Vector3 position, string description, int playerFacingDirection) {
		this.name = name;
		this.sceneName = sceneName;
		this.position = position;
		this.description = description;
		this.playerFacingDirection = playerFacingDirection;
	}
}
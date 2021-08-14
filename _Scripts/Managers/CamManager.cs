using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eCamMode { freezeCam, followAll, followUp, followDown, followLR, noTarget };

public class CamManager : MonoBehaviour {
	[Header("Set in Inspector")]
	public Transform		targetTrans;

	[Header("Set Dynamically")]
	// Singleton
	private static CamManager _S;
	public static CamManager S { get { return _S; } set { _S = value; } }

	private static bool		exists;

	public float 			camPosX;
	public float 			camPosY;  
	private float			camPosZ = -10;

	// Smooth Lerp
	private float			easing = 0.15f; 
	private Vector3 		velocity = Vector3.zero;

	private Vector3			destination;

	public eCamMode 		camMode;

	// Clamps CamPosX
	public bool 			hasMinMaxPosX; // This is deactivated every scene change in LevelMan.LoadSettings()
	public float 			minPosX;
	public float 			maxPosX;

	public bool				canLerp;

	void Awake () {
		// Singleton
		S = this;

		// DontDestroyOnLoad
		if (!exists) {
			exists = true;
			DontDestroyOnLoad (transform.gameObject);
		} else {
			Destroy (gameObject);
		}
	}

	void Start () {
        if (!targetTrans) {
			Debug.LogError("targetTrans has NOT been assigned a transform in the Inspector.");
		}
	}

	void LateUpdate() {
		if(camMode != eCamMode.noTarget) {
			if (targetTrans) {
				switch (camMode) {
					case eCamMode.freezeCam:
						destination.x = camPosX;
						destination.y = camPosY;
						break;
					case eCamMode.followAll:
						destination = targetTrans.localPosition;
						break;
					case eCamMode.followLR:
						destination = targetTrans.position;

						// Locks Camera Pos.Y
						destination.y = camPosY;
						break;
					case eCamMode.followUp:
						destination = targetTrans.localPosition;

						// Prevents Camera from going too far up or down
						destination.y = Mathf.Max(destination.y, camPosY);

						// Locks Camera Pos.X
						destination.x = camPosX;
						break;
					case eCamMode.followDown:
						destination = targetTrans.localPosition;

						// Prevents Camera from going too far up or down
						destination.y = Mathf.Max(destination.y, camPosY);

						// Locks Camera Pos.X
						destination.x = camPosX;
						break;
				}

				// Clamps CamPosX
				if (hasMinMaxPosX) {
					destination.x = Mathf.Max(destination.x, minPosX);
					destination.x = Mathf.Min(destination.x, maxPosX);
				}

				// Interpolate from the current Camera position towards Destination
				if (canLerp) {
					destination = Vector3.SmoothDamp(transform.localPosition, destination, ref velocity, easing);
				}

				// Keeps Pos.Z at -10
				destination.z = camPosZ;

				// Set the Camera Pos to destination
				transform.localPosition = destination;
			}
		}
	}

	public void ChangeTarget(GameObject tGO, bool smoothLerpToTarget) {
		if (camMode != eCamMode.freezeCam) {
			// Smoothly transition to new cam target
			canLerp = smoothLerpToTarget;

			// Change Target
			if (tGO) {
				targetTrans = tGO.transform;
			}

			// If this is a step in an cutscene, move to the next step
			CutsceneManager.S.stepDone = true;
		}
	}
}
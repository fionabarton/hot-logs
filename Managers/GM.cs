using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GM : MonoBehaviour {
    [Header("Set Dynamically")]
    private static GM _S;
    public static GM S { get { return _S; } set { _S = value; } }

    // DontDestroyOnLoad
    private static bool exists;

	public GameManager gameManager;
	public UpdateManager updateManager;

	void Awake() {
		S = this;

		// DontDestroyOnLoad
		if (!exists) {
			exists = true;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
		}

		gameManager = GetComponent<GameManager>();
		updateManager = GetComponent<UpdateManager>();
	}
}
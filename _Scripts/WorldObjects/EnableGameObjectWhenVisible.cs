using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enables a "Group of GameObjects" when Visible.
public class EnableGameObjectWhenVisible : MonoBehaviour {
	[Header ("Set in Inspector")]
	public List<GameObject> GOs = new List<GameObject>();

	void OnEnable(){
		// Disable GO's
		for (int i = 0; i < GOs.Count; i++) {
			GOs [i].SetActive (false);
		}
	}
		
	void OnBecameVisible () {
		// Enable GO's
		for (int i = 0; i < GOs.Count; i++) {
			GOs [i].SetActive (true);
		}
	}

	void OnBecameInvisible () {
		// Disable GO's
		for (int i = 0; i < GOs.Count; i++) {
			GOs [i].SetActive (false);
		}
	}
}

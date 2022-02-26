using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCurtain : MonoBehaviour {
	[Header("Set in Inspector")]
	public Animator anim;

	[Header("Set Dynamically")]
	// Singleton
	private static BattleCurtain _S;
	public static BattleCurtain S { get { return _S; } set { _S = value; } }

	void Awake() {
		// Singleton
		S = this;
	}

	public void Open() {
		if(Random.value > 0.5f) {
			anim.Play("Horizontal_Open");
		} else {
			anim.Play("Vertical_Open");
		}
	}

	public void Close() {
		if (Random.value > 0.5f) {
			anim.Play("Horizontal_Close");
		} else {
			anim.Play("Vertical_Close");
		}
	}
}

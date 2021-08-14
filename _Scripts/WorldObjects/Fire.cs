using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour {
	[Header("Set in Inspector")]
	public float speed = 1;

	[Header("Set Dynamically")]
	Animator anim;

	void Start () {
		anim = GetComponent<Animator>();
		anim.speed = speed;
	}
}

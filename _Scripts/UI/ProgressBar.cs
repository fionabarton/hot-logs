﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GameObject))]
public class ProgressBar : MonoBehaviour {
	[Header("Set in Inspector")]
	public GameObject	currentBar;

	[Header("Set Dynamically")]
	float				progressBarMaxWidth;
	SpriteRenderer		sRend;

	void Awake()
	{
		progressBarMaxWidth = currentBar.transform.localScale.x;

		sRend = GetComponent<SpriteRenderer>();
	}

	public void UpdateBar(float currentHP, float maxHP, bool colorChanges = true) {
		// Set scale
		Vector3 scale = currentBar.transform.localScale;
		scale.x = Utilities.S.Map(0, maxHP, 0, progressBarMaxWidth, currentHP);
		currentBar.transform.localScale = scale;

		// Set position
		Vector3 pos = currentBar.transform.localPosition;
		pos.x = (scale.x / 2) - 0.5f;
		currentBar.transform.localPosition = pos;

        // Change color based off of current value
        if (colorChanges) {
			if (currentHP >= ((maxHP / 3) * 2)) {
				sRend.color = Color.green;
			} else if (currentHP >= (maxHP / 3)) {
				sRend.color = Color.yellow;
			} else {
				sRend.color = Color.red;
			}
		}
	}
}

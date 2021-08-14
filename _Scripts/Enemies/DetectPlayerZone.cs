using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemyMovement.mode changes if EnemyMovement.cs attached to parent gameObject
/// </summary>
public class DetectPlayerZone : MonoBehaviour {
	[Header("Set Dynamically")]
	private EnemyMovement	enemyMovementCS;

	void OnEnable(){
		enemyMovementCS = GetComponentInParent<EnemyMovement> ();
	}

	void OnTriggerEnter2D(Collider2D coll){
		if (coll.gameObject.tag == "Player") {
			enemyMovementCS.anim.speed = 1;
			enemyMovementCS.mode = enemyMovementCS.defaultMovementMode;
		}
	}
}

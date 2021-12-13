using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemyMovement.mode changes if EnemyMovement.cs attached to parent gameObject
/// </summary>
public class ForgetPlayerZone : MonoBehaviour {
	[Header("Set Dynamically")]
	private EnemyMovement	enemyMovementCS;

	void OnEnable(){
		enemyMovementCS = GetComponentInParent<EnemyMovement> ();
	}

	void OnTriggerExit2D(Collider2D coll){
		if (coll.gameObject.tag == "Player") {
			enemyMovementCS.currentMode = enemyMovementCS.onForgetPlayerMode;
		}
	}
}

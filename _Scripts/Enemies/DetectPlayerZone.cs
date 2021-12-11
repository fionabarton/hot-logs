using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemyMovement.mode changes if EnemyMovement.cs attached to parent gameObject
/// </summary>
public class DetectPlayerZone : MonoBehaviour {
	[Header("Set Dynamically")]
	private EnemyMovement	enemyMovementCS;
	private Enemy			enemyCS;

	void OnEnable(){
		enemyMovementCS = GetComponentInParent<EnemyMovement>();
		enemyCS = GetComponentInParent<Enemy>();
	}

	void OnTriggerEnter2D(Collider2D coll){
		if (coll.gameObject.tag == "Player") {
			enemyCS.exclamationBubble.SetActive(true);

			// Audio: Flicker
			AudioManager.S.PlaySFX(eSoundName.flicker);

			Invoke("ActivateDetectPlayerMode", 0.5f);
		}
	}

	void ActivateDetectPlayerMode() {
		enemyMovementCS.rigid.velocity = Vector2.zero;
		enemyMovementCS.anim.speed = 1;
		enemyMovementCS.currentMode = enemyMovementCS.onDetectPlayerMode;
	}
}

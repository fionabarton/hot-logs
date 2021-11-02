using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eEnemyAI { Random, FocusOnAttack, FocusOnHeal, FocusOnDefend, FightWisely, DontUseMP, RunAway, CallForBackup };

/// <summary>
/// - Start Battle OnCollision w/ Player
/// </summary>
[ExecuteInEditMode]
public class Enemy : MonoBehaviour {
	[Header("Set in Inspector")]
	public List<EnemyStats> stats;

	// The ground level that the enemy is on
	public int				level = 1;

	[Header("Set Dynamically")]
	// Used to track enemy in order to handle what happens if it's dead,
	// defaultMovementMode has been changed, etc.
	public int 				ndx;

    private void Start() {
		ndx = transform.GetSiblingIndex();
	}

    // Start Battle
    void OnCollisionEnter2D(Collision2D coll){
		if (coll.gameObject.CompareTag("Player")) {
			if (level == Player.S.level) {
				if (!RPG.S.paused) {
					// If this Enemy does not StartBattle() on collision
					if (stats[0].questNdx == 0) {
						if (!Player.S.invincibility.isInvincible && !Player.S.isBattling) {
							// Set Camera to Enemy gameObject
							CamManager.S.ChangeTarget(gameObject, true);

							// Prevent collisions w/ multiple enemies
							Player.S.isBattling = true;

							// Start Battle ( Assign Enemy Stats)
							RPG.S.StartBattle(stats);

							// Inform EnemyManager this Enemy has battled and is the current enemy
							EnemyManager.S.enemiesBattled.Add(ndx);
							EnemyManager.S.currentEnemyNdx = ndx;

							// Store position of each enemy in currentScene
							EnemyManager.S.GetEnemyPositions();
							// Store level of each enemy in currentScene
							EnemyManager.S.GetEnemyLevels();
							// Store sorting layer name of each swap layer sprite in currentScene
							SwapLayerManager.S.GetLayerNames();

							EnemyMovement enemyMove = GetComponent<EnemyMovement>();
							if (enemyMove != null) {
								// Freeze Enemy
								enemyMove.canMove = false;
							}

							// Get and position Explosion game object
							GameObject explosion = ObjectPool.S.GetPooledObject("Explosion");
							explosion.SetActive(true);
							Utilities.S.SetPosition(explosion, coll.contacts[0].point.x, coll.contacts[0].point.y);
						}
					}
				}
			}
		}
	}
}
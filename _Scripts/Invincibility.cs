using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invincibility : MonoBehaviour {
    [Header("Set Dynamically")]
	public bool		isInvincible; // enables StartBattle() in RPGEnemy.cs upon collision w/ Enemies
	private float	timeToFlash;
	private float	flashRate;
	private float	timeToEndInvincibility;

	public void FixedLoop() {
        if (!RPG.S.paused) {
            if (isInvincible) {
                if (Time.time >= timeToEndInvincibility) {
                    EndInvincibility();
                } else {
					if (Time.time >= timeToFlash) {
						// "Flash" the sprite by enabling its SpriteRenderer
						Player.S.sRend.enabled = !Player.S.sRend.enabled;

						// Increase the rate at which the sprite will flash
						flashRate -= 0.01f;

						// Reset the timer
						timeToFlash = Time.time + flashRate;
					}
                }
            }
        } else {
			// Effectively "pauses" both timers when the game is paused
			timeToEndInvincibility += Time.fixedDeltaTime;
			timeToFlash += Time.fixedDeltaTime;
		}
    }

	public void StartInvincibility() {
		isInvincible = true;

		// Set timers
		flashRate = 0.25f;
		timeToFlash = Time.time + flashRate;
		timeToEndInvincibility = Time.time + 3f;

		// Add FixedLoop() to UpdateManager
		UpdateManager.fixedUpdateDelegate += FixedLoop;
	}

	public void EndInvincibility() {
		isInvincible = false;

		// Enable SpriteRenderer
		Player.S.sRend.enabled = true;

		// Disable then enable the player’s box collider to call OnCollisionEnter2D
		// on an Enemy script if it has already collided with an enemy
		Player.S.boxColl.enabled = false;
		Player.S.boxColl.enabled = true;

		// Remove FixedLoop() from UpdateManager
		UpdateManager.fixedUpdateDelegate -= FixedLoop;
	}
}
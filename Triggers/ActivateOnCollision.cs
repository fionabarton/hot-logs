using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateOnCollision : MonoBehaviour {
    protected virtual void OnTriggerEnter2D(Collider2D coll) {
        if (!GameManager.S.paused) {
			if (coll.gameObject.CompareTag("PlayerTrigger")) {
				// Player RigidBody
				Player.S.rigid.sleepMode = RigidbodySleepMode2D.NeverSleep;

                // Update Delgate
                UpdateManager.updateDelegate += Loop;

				Action();
			}
		}
    }

    protected virtual void OnTriggerExit2D(Collider2D coll) {
		// Player RigidBody
		if (coll.gameObject.CompareTag("PlayerTrigger")) {
			Player.S.rigid.sleepMode = RigidbodySleepMode2D.StartAwake;

			// Update Delgate
			UpdateManager.updateDelegate -= Loop;
        }
    }

    public void Loop() {

	}

	protected virtual void Action() {

    }
}

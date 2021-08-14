using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateOnButtonPress : MonoBehaviour {
    [Header("Set in Inspector")]
    public bool activateInteractableCursor = true;

    public bool canBeReset = true;

    [Header("Set Dynamically")]
    public bool firstButtonPressed;

    void OnDisable() {
        // Remove Delgate
        UpdateManager.updateDelegate -= Loop;
    }

    protected virtual void OnTriggerEnter2D(Collider2D coll) {
        if (!Player.S.alreadyTriggered) {
            if (!RPG.S.paused) {
                if (coll.gameObject.CompareTag("PlayerTrigger")) {
                    Player.S.alreadyTriggered = true;

                    // Player RigidBody
                    Player.S.rigid.sleepMode = RigidbodySleepMode2D.NeverSleep;

                    // Interactable Trigger
                    if (activateInteractableCursor) {
                        InteractableCursor.S.Activate(true, gameObject);
                    }

                    // Update Delgate
                    UpdateManager.updateDelegate += Loop;
                }
            }
        }
	}

	protected virtual void OnTriggerExit2D(Collider2D coll) {
        if (coll.gameObject.CompareTag("PlayerTrigger")) {
            // Player RigidBody
            Player.S.rigid.sleepMode = RigidbodySleepMode2D.StartAwake;

            firstButtonPressed = false;

            // Interactable Trigger
            if (InteractableCursor.S.cursorGO) {
				InteractableCursor.S.Activate(false);
            }
			
            // Update Delgate
            UpdateManager.updateDelegate -= Loop;

            Player.S.alreadyTriggered = false;
        }
    }

    public void Loop() {
        if (!RPG.S.paused) {
            if (RPG.S.canInput) {
                // If there hasn't been any input yet
                if (!firstButtonPressed) {
                    // Activate on button press
                    if (Input.GetButtonDown("SNES A Button")) {
                        Action();
                        firstButtonPressed = true;
                        InteractableCursor.S.Activate(false);
                    }
                }

                // Reset trigger
                if (canBeReset) {
                    if (DialogueManager.S.dialogueFinished && DialogueManager.S.ndx <= 0) {
                        if (Input.GetButtonDown("SNES A Button")) {
                            ResetTrigger();
                        }
                    }
                }
            }
        }
	}

    void ResetTrigger() {
        if (activateInteractableCursor) {
            InteractableCursor.S.Activate(true, gameObject);
        }

        firstButtonPressed = false;

        DialogueTrigger t = GetComponent<DialogueTrigger>();
        if (t) {
            if (t.enabled) {
                // Reset messages
                t.ResetMessages();
            }
        }
    }

    protected virtual void Action() {

    }
}

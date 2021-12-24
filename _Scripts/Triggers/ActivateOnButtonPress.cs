using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Trigger that performs an action implemented its child class OnButtonPress
/// </summary>
public class ActivateOnButtonPress : MonoBehaviour {
    [Header("Set in Inspector")]
    // Activate Interactable Cursor OnTriggerEnter2D
    public bool activateInteractableCursor = true;

    // Prevent trigger from being reset (used on "Toiletron" cutscene trigger)
    public bool canBeReset = true;

    [Header("Set Dynamically")]
    // Used in DialogueTrigger to change trigger's action after it's been pressed once
    public bool firstButtonPressed;

    void OnDisable() {
        // Remove Update Delgate
        UpdateManager.updateDelegate -= Loop;
    }

    protected virtual void OnTriggerEnter2D(Collider2D coll) {
        if (!Player.S.alreadyTriggered) {
            if (!RPG.S.paused) {
                if (coll.gameObject.CompareTag("PlayerTrigger")) {
                    // Prevents triggering multiple triggers
                    Player.S.alreadyTriggered = true;

                    // Player RigidBody
                    Player.S.rigid.sleepMode = RigidbodySleepMode2D.NeverSleep;

                    // Activate Interactable Trigger
                    if (activateInteractableCursor) {
                        InteractableCursor.S.Activate(gameObject);
                    }

                    // Add Update Delgate
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

            // Deactivate Interactable Trigger
            if (InteractableCursor.S.cursorGO) {
				InteractableCursor.S.Deactivate();
            }

            // Remove Update Delgate
            UpdateManager.updateDelegate -= Loop;

            // Unsubscribe ResetTrigger() from the OnShopScreenDeactivated event
            EventManager.OnShopScreenDeactivated -= ResetTrigger;

            // Prevents triggering multiple triggers
            Player.S.alreadyTriggered = false;
        }
    }

    public void Loop() {
        if (!RPG.S.paused) {
            if (RPG.S.canInput) {
                // If there hasn't been any input yet...
                if (!firstButtonPressed) {
                    // ...Activate on button press
                    if (Input.GetButtonDown("SNES B Button")) {
                        Action();
                        firstButtonPressed = true;
                        InteractableCursor.S.Deactivate();
                    }
                }

                // Reset trigger
                if (canBeReset) {
                    if (DialogueManager.S.dialogueFinished && DialogueManager.S.ndx <= 0) {
                        if (Input.GetButtonDown("SNES B Button")) {
                            ResetTrigger();
                        }
                    }
                }
            }
        }
	}

    public void ResetTrigger() {
        firstButtonPressed = false;

        // Activate Interactable Trigger
        if (activateInteractableCursor) {
            InteractableCursor.S.Activate(gameObject);
        }

        // Reset DialogueTrigger messages
        DialogueTrigger t = GetComponent<DialogueTrigger>();
        if (t) {
            if (t.enabled) {
                t.ResetMessages();
            }
        }
    }

    // Function implemented in child class that's called OnButtonPress
    protected virtual void Action() {

    }
}
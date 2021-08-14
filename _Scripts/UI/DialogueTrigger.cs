using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : ActivateOnButtonPress {
	[Header("Set in Inspector")]
	// Four different sets of dialogue, one of which is displayed depending on quests completed
	public List<string> 	messages0;
	public List<string> 	messages1;
	public List<string>     messages2;
	public List<string>     messages3;

    // Change Dialogue based on Quests Completed
    public List<int>		questNdx;

	[Header ("Set Dynamically")]
	// Necessary because with multiple lines the the list of strings is reduced to one element after dialogue finished
	public List<string>		messagesCLONE0;
	public List<string>		messagesCLONE1;
	public List<string>		messagesCLONE2;
	public List<string>		messagesCLONE3;

	int						highestQuestCompletedNdx = 0;

    void OnEnable() {
		// Initialize cloned messages; otherwise if Player enters trigger, 
		// doesn't call DisplayText(), and then exits trigger, messages will be set to messagesCLONE which is NULL
		CacheMessages();
	}

    void CacheMessages() {
		messagesCLONE0 = new List<string>(messages0);
		messagesCLONE1 = new List<string>(messages1);
		messagesCLONE2 = new List<string>(messages2);
		messagesCLONE3 = new List<string>(messages3);
	}

	public void ResetMessages() {
		messages0 = new List<string>(messagesCLONE0);
		messages1 = new List<string>(messagesCLONE1);
		messages2 = new List<string>(messagesCLONE2);
		messages3 = new List<string>(messagesCLONE3);
	}

	protected override void Action() {
		// Set Camera to DialogueTrigger gameObject
		CamManager.S.ChangeTarget(gameObject, true);

		// Cache messages
		CacheMessages();

		// Find index of highest completed quest
		for (int i = 0; i < questNdx.Count; i++) {
			if (QuestManager.S.completed[questNdx[i]]) {
				if (questNdx[i] > highestQuestCompletedNdx) {
					highestQuestCompletedNdx = i;
				}
			}
		}

		// Display correct message depending on which quests completed
		switch (highestQuestCompletedNdx) {
			case 0:
				DialogueManager.S.DisplayText(messages0);
				break;
			case 1:
				DialogueManager.S.DisplayText(messages1);
				break;
			case 2:
				DialogueManager.S.DisplayText(messages2);
				break;
			case 3:
				DialogueManager.S.DisplayText(messages3);
				break;
		}

		// Interactable Trigger
		InteractableCursor.S.Activate(false);

		// Switch Player mode
        Player.S.SwitchMode(eRPGMode.idle);

		// If NPC, face direction of Player
		NPCMovement npc = GetComponent<NPCMovement>();
        if (npc) {
			npc.FacePlayer();
        }
    }

	public void ThisLoop () {
        // Would prefer better solution... 
        // This prevents an occasional bug when the Player is within this trigger on scene change
        // by removing ThisLoop from updateDelegate on scene change
        if (!RPG.S.canInput) {
            UpdateManager.updateDelegate -= ThisLoop;
        }

        if (Input.GetButtonDown("SNES A Button")) {
			if (firstButtonPressed) {
				if (!RPG.S.paused) {
					// For Multiple Lines
					if (DialogueManager.S.dialogueFinished && DialogueManager.S.ndx > 0) {
						// Reset Text & Cursor
						DialogueManager.S.ClearForNextLine();

						List<string> tMessage = new List<string>();

						switch (highestQuestCompletedNdx) {
							case 0:
								tMessage = messages0;
								break;
							case 1:
								tMessage = messages1;
								break;
							case 2:
								tMessage = messages2;
								break;
							case 3:
								tMessage = messages3;
								break;
						}

						tMessage.RemoveAt(0);

						// Call DisplayText() with one less line of "messages"
						DialogueManager.S.DisplayText(tMessage);
					}
                }
			}
		}
	}

	protected override void OnTriggerEnter2D(Collider2D coll) {
        if (enabled) {
			if (coll.gameObject.CompareTag("PlayerTrigger")) {
				if (!Player.S.alreadyTriggered) {
					base.OnTriggerEnter2D(coll);

					// Update Delgate
					UpdateManager.updateDelegate += ThisLoop;
				}
			}
		}
	}

	protected override void OnTriggerExit2D(Collider2D coll) {
		if (enabled) {
			if (coll.gameObject.CompareTag("PlayerTrigger")) {
				base.OnTriggerExit2D(coll);

				// Reset messages
				ResetMessages();

				// Update Delgate
				UpdateManager.updateDelegate -= ThisLoop;
			}
		}
	}
}
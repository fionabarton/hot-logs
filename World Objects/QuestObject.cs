using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// If Quest Completed:
/// 1. (De)activate GameObject
/// 2. Change Sprite/Animation
/// 3. Change Position
/// 4. Change Dialogue? (This is already taken care of in DialogueTrigger)
/// 
/// </summary>
///
public class QuestObject : MonoBehaviour {
	[Header("Set in Inspector")]
	public int questNumber;

	public eQuestAction questAction;

	// Deactivate GameObject if associated Quest has been Completed
	void OnEnable() {
		if (QuestManager.S.completed[questNumber]) {
			switch (questAction) {
				case eQuestAction.deactivateGo:
					gameObject.SetActive(false);
					break;
				case eQuestAction.activateGo:
					gameObject.SetActive(true);
					break;
			}
		} else {
			switch (questAction) {
				case eQuestAction.deactivateGo:
					gameObject.SetActive(true);
					break;
				case eQuestAction.activateGo:
					gameObject.SetActive(false);
					break;
			}
		}
	}
}
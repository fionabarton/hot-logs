using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerButtons : MonoBehaviour {
	[Header("Set Dynamically")]
	// Singleton
	private static PlayerButtons _S;
	public static PlayerButtons S { get { return _S; } set { _S = value; } }

	public RectTransform rectTrans;

	[Header ("Set in Inspector")]
	public Text[] 		statName = new Text[2];
	public Text[] 		statValue = new Text[2];

	public List<GameObject> buttonsGO;
	public List<Button> 	buttonsCS;
	public List<Animator>	anim;

	[Header("Set Dynamically")]
	public Text			goldValue;

	void Awake() {
		// Singleton
		S = this;
	}

    void OnEnable () {
		UpdateGUI ();

        // Set Position
        if (ScreenManager.S) {
			if (RPG.S.paused) {
				rectTrans.anchoredPosition = new Vector2(0, 0);
			} else {
				rectTrans.anchoredPosition = new Vector2(0, 650);
			}
		}
	}

    public void UpdateGUI(){
		statName [0].text = "HP\nMP";
		statName [1].text = "HP\nMP";
		// Weapon: STR, Armor: DEF

		try{
		statValue [0].text = Stats.S.HP [0] + "/" + Stats.S.maxHP [0] + "\n" + Stats.S.MP [0] + "/" + Stats.S.maxMP [0] + "\n";
		statValue [1].text = Stats.S.HP [1] + "/" + Stats.S.maxHP [1] + "\n" + Stats.S.MP [1] + "/" + Stats.S.maxMP [1] + "\n";
		
		goldValue.text = "" + Stats.S.Gold; 

		}catch(NullReferenceException){}
			
		// Current Stat, Arrow Sprite, Potential Stat
	}

	public void PositionCursor(){
		for (int i = 0; i < buttonsCS.Count; i++) {
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == buttonsCS [i].gameObject) {
				// Cursor Position set to Selected Button
				float tPosX = buttonsCS [i].gameObject.GetComponent<RectTransform> ().anchoredPosition.x;
				float tPosY = buttonsCS [i].gameObject.GetComponent<RectTransform> ().anchoredPosition.y;

				float tParentX = buttonsCS [i].gameObject.transform.parent.GetComponent<RectTransform> ().anchoredPosition.x;
				float tParentY = buttonsCS [i].gameObject.transform.parent.GetComponent<RectTransform> ().anchoredPosition.y;

				ScreenCursor.S.rectTrans.anchoredPosition = new Vector2 ((tPosX + tParentX + 60), (tPosY + tParentY));
			}
		}
	}
}

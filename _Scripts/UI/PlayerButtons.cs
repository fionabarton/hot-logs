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

	public RectTransform	rectTrans;

	[Header ("Set in Inspector")]
	public Text[] 			statName = new Text[2];
	public Text[] 			statValue = new Text[2];

	public List<GameObject> buttonsGO;
	public List<Button> 	buttonsCS;
	public List<Animator>	anim;

	[Header("Set Dynamically")]
	public Text				goldValue;

	void Awake() {
		S = this;
	}

    void OnEnable () {
		UpdateGUI ();

        // Set position 
		if (RPG.S.paused) {
			rectTrans.anchoredPosition = new Vector2(0, 0);
		} else {
			rectTrans.anchoredPosition = new Vector2(0, 650);
		}
	}

	// Display the party's current HP, MP, and Gold
    public void UpdateGUI(){
		statName [0].text = "HP\nMP";
		statName [1].text = "HP\nMP";
		// Weapon: STR, Armor: DEF

		try{
			statValue [0].text = 
				PartyStats.S.HP [0] + "/" + 
				PartyStats.S.maxHP [0] + "\n" + 
				PartyStats.S.MP [0] + "/" + 
				PartyStats.S.maxMP [0] + "\n";

			statValue [1].text = 
				PartyStats.S.HP [1] + "/" + 
				PartyStats.S.maxHP [1] + "\n" + 
				PartyStats.S.MP [1] + "/" + 
				PartyStats.S.maxMP [1] + "\n";
		
			goldValue.text = "" + PartyStats.S.Gold; 

		}catch(NullReferenceException){}
	}

	public void SetAnim(string animName) {
		for(int i = 0; i < buttonsCS.Count; i++) {
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == buttonsCS[i].gameObject) {
				anim[i].CrossFade(animName, 0);
			} else {
                try {
					anim[i].CrossFade("Idle", 0);
				}
				catch (NullReferenceException) { }
			}
		}
    }
}
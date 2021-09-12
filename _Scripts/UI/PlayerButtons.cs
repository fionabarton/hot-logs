using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerButtons : MonoBehaviour {
	[Header("Set in Inspector")]
	public Text[] statName;
	public Text[] statValue;

	public List<GameObject> buttonsGO;
	public List<Button> buttonsCS;
	public List<Animator> anim;

	public Text goldValue;

	[Header("Set Dynamically")]
	// Singleton
	private static PlayerButtons _S;
	public static PlayerButtons S { get { return _S; } set { _S = value; } }

	public RectTransform	rectTrans;

	void Awake() {
		S = this;
	}

    void OnEnable() {
        try {
            UpdateGUI();
        }
        catch (NullReferenceException) { }

        // Set position 
        if (RPG.S.paused) {
            rectTrans.anchoredPosition = new Vector2(0, 0);
        } else {
            rectTrans.anchoredPosition = new Vector2(0, 650);
        }
    }

    // Display the party's current HP, MP, and Gold
    public void UpdateGUI(){
		statName[0].text = "HP\nMP";
		statName[1].text = "HP\nMP";
		// Weapon: STR, Armor: DEF

		try{
			if(Party.stats.Count > 0) {
				statValue[0].text =
					Party.stats[0].HP + "/" +
					Party.stats[0].maxHP + "\n" +
					Party.stats[0].MP + "/" +
					Party.stats[0].maxMP + "\n";

				statValue[1].text =
					Party.stats[1].HP + "/" +
					Party.stats[1].maxHP + "\n" +
					Party.stats[1].MP + "/" +
					Party.stats[1].maxMP + "\n";
			}

			goldValue.text = "" + Party.S.Gold; 

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
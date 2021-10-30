﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerButtons : MonoBehaviour {
	[Header("Set in Inspector")]
	public Text[]			statName;
	public Text[]			statValue;

	public List<GameObject> buttonsGO;
	public List<Button>		buttonsCS;
	public List<Animator>	anim;

	public Text				goldValue;

	[Header("Set Dynamically")]
	// Singleton
	private static PlayerButtons	_S;
	public static PlayerButtons		S { get { return _S; } set { _S = value; } }

	public RectTransform			rectTrans;

	void Awake() {
		S = this;
	}

    void OnEnable() {
        try {
            UpdateGUI();

			// Set position 
			if (RPG.S.paused) {
				rectTrans.anchoredPosition = new Vector2(0, 0);
			} else {
				rectTrans.anchoredPosition = new Vector2(0, 650);
			}

			// Deactivate all player buttons 
			for (int i = 0; i < buttonsGO.Count; i++) {
				buttonsGO[i].SetActive(false);
			}

			// Activate player buttons depending on party amount
			for (int i = 0; i <= Party.S.partyNdx; i++) {
				buttonsGO[i].SetActive(true);
			}
		}
        catch (NullReferenceException) { }
    }

    // Display the party's current HP, MP, and Gold
    public void UpdateGUI(){
		statName[0].text = "HP\nMP";
		statName[1].text = "HP\nMP";
		statName[2].text = "HP\nMP";
		// Weapon: STR, Armor: DEF

		try {
            if (Party.stats.Count > 0) {
				for (int i = 0; i <= Party.S.partyNdx; i++) {
					statValue[i].text =
						Party.stats[i].HP + "/" +
						Party.stats[i].maxHP + "\n" +
						Party.stats[i].MP + "/" +
						Party.stats[i].maxMP;
				}
			}

			goldValue.text = "" + Party.S.gold; 

		}catch(NullReferenceException){}
	}

	public void SetSelectedAnim(string animName) {
        for (int i = 0; i < buttonsCS.Count; i++) {
			if (buttonsCS[i].gameObject.activeInHierarchy) {
				if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == buttonsCS[i].gameObject) {
					anim[i].CrossFade(animName, 0);
				} else {
					anim[i].CrossFade("Idle", 0);
				}
			}
		}	
	}

	public void SetButtonsColor(List<Button> buttons, Color32 color) {
		for(int i = 0; i < buttons.Count; i++) {
			ColorBlock colorBlock = buttons[i].colors;
			colorBlock.normalColor = color;
			buttons[i].colors = colorBlock;
		}
	} 
}
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ColorScreen : MonoBehaviour{
	[Header("Set in Inspector")]
	public Animator             anim;

    [Header("Set Dynamically")]
    public List<AnimationClip>  clips;

    private static ColorScreen  _S;
	public static ColorScreen   S { get { return _S; } set { _S = value; } }

	// DontDestroyOnLoad
	private static bool         exists;

    public int                  targetNdx;
    public Spell                spell;

    void Awake() {
		S = this;

		// DontDestroyOnLoad
		if (!exists) {
			exists = true;
			DontDestroyOnLoad(transform.parent.gameObject);
		} else {
			Destroy(transform.parent.gameObject);
		}
	}

    private void Start() {
        // Store animator's clips: (0: Swell, 1: Flicker)
        clips.Add(anim.runtimeAnimatorController.animationClips[0]);
        clips.Add(anim.runtimeAnimatorController.animationClips[1]);
    }

    public void ActivateBlackScreen() {
        anim.Play("Black Screen", 0, 0);
    }

    public void PlayClip(string functionName, int actionNdx) {
        // Remove all animation events
        RemoveEvents();

        // Prevent battle input
        Battle.S.mode = eBattleMode.noInputPermitted;

        // Remove all listeners
        Utilities.S.RemoveListeners(BattlePlayerActions.S.playerButtonCS);

        anim.Play("Clear Screen", 0, 0);

        // Create new animation event
        AnimationEvent evt = new AnimationEvent();

        // Set animation event parameters
        evt.functionName = functionName;
        evt.intParameter = actionNdx;
        
        // Add event to clip
        switch (functionName) {
            case "Swell":
                // Audio: Swell
                AudioManager.S.PlaySFX(eSoundName.swell);

                evt.time = 1.325f;
                clips[0].AddEvent(evt);
                break;
            case "Flicker":
                // Audio: Flicker
                AudioManager.S.PlaySFX(eSoundName.flicker);

                evt.time = 0.4f; // 2 flicks
                //evt.time = 0.65f; // 3 flicks
                clips[1].AddEvent(evt);
                break;
        }

        // Play animation from first frame
        anim.Play(functionName, 0, 0);
    }

    /// <summary>
    /// - HP, MP, Heal All, Revive, Warp Potions?
    /// </summary>

    public void Swell(int actionNdx = 0) {
        // Function to call after animation is played
        switch (actionNdx) {
            case 0: // Party: Heal Spell
                SpellScreen.S.battleSpells.HealSinglePartyMember(targetNdx, spell);
                break;
            case 1: // Enemy: Heal Spell
                BattleEnemyActions.S.HealSpell(targetNdx);
                break;
            case 2: // Party: Heal All Spell
                SpellScreen.S.battleSpells.HealAll(targetNdx, spell);
                break;
            case 3: // Party: Revive Spell
                SpellScreen.S.battleSpells.ReviveSelectedPartyMember(targetNdx, spell);
                break;
            case 4: // Party: Detoxify Spell
                SpellScreen.S.battleSpells.DetoxifySinglePartyMember(targetNdx, spell);
                break;
            case 5: // Party: Mobilize Spell
                SpellScreen.S.battleSpells.MobilizeSinglePartyMember(targetNdx, spell);
                break;
            case 6: // Party: Wake Spell
                SpellScreen.S.battleSpells.WakeSinglePartyMember(targetNdx, spell);
                break;
        }

        // Remove all animation events
        RemoveEvents();
    }
	public void Flicker(int actionNdx = 0) {
        // Function to call after animation is played
        switch (actionNdx) {
            case 0: // Party: Fireball Spell
                SpellScreen.S.battleSpells.AttackSelectedEnemy(targetNdx, spell);
                break;
            case 1: // Party: Fireblast Spell
                SpellScreen.S.battleSpells.AttackAllEnemies(targetNdx, spell);
                break;
            case 2: // Enemy: Attack All Spell
                BattleEnemyActions.S.AttackAll();
                break;
            case 3: // Enemy: Attack Single Spell
                BattleEnemyActions.S.AttackSingle();
                break;
            case 4: // Party: Poison Single Spell
                SpellScreen.S.battleSpells.PoisonSingle(targetNdx, spell);
                break;
            case 5: // Party: Paralyze Single Spell
                SpellScreen.S.battleSpells.ParalyzeSingle(targetNdx, spell);
                break;
            case 6: // Party: Sleep Single Spell
                SpellScreen.S.battleSpells.SleepSingle(targetNdx, spell);
                break;
        }

        // Remove all animation events
        RemoveEvents();
    }

    public void RemoveEvents() {
        AnimatorClipInfo[] myClipInfos = anim.GetCurrentAnimatorClipInfo(0);
        AnimationClip myCurrentClip = myClipInfos[0].clip;
        AnimationEvent[] myEvents = myCurrentClip.events;

        if (myEvents.Length > 0) {
            var list = myEvents.ToList();
            list.RemoveAt(0);
            myCurrentClip.events = list.ToArray();
        }
    }
}
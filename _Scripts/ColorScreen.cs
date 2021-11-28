using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ColorScreen : MonoBehaviour{
	[Header("Set in Inspector")]
	public Animator             anim;

    [Header("Set Dynamically")]
    public List<AnimationClip>  clips;

    // Singleton
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
        // Prevent battle input
        Battle.S.battleMode = eBattleMode.noInputPermitted;

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
                evt.time = 1.325f;
                clips[0].AddEvent(evt);
                break;
            case "Flicker":
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
        // Remove all animation events
        RemoveEvents();

        // Function to call after animation is played
        switch (actionNdx) {
            case 0: // Party: Heal Spell
                BattleSpells.S.HealSelectedPartyMember(targetNdx, spell);
                break;
            case 1: // Enemy: Heal Spell
                BattleEnemyActions.S.HealSpell();
                break;
            case 2: // Party: Heal All Spell
                BattleSpells.S.HealAll(targetNdx, spell);
                break;
        }
    }
	public void Flicker(int actionNdx = 0) {
        // Remove all animation events
        RemoveEvents();

        Debug.Log("Called Flicker" + " Ndx: " + actionNdx);

        // Function to call after animation is played
        switch (actionNdx) {
            case 0: // Party: Fireball Spell
                BattleSpells.S.AttackSelectedEnemy(targetNdx, spell);
                break;
            case 1: // Party: Fireblast Spell
                BattleSpells.S.AttackAllEnemies(targetNdx, spell);
                break;
            case 2: // Enemy: Attack All Spell
                BattleEnemyActions.S.AttackAll();
                break;
        }
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneTriggerOnButtonPress : ActivateOnButtonPress {
    [Header("Set in Inspector")]
    // GameObjects participating in the cutscene
    public List<GameObject> actors;

    // Index that informs CutsceneManager which cutscene to trigger
    public int              ndx = 0;

    // If this cutscene has already happened,
    // set up GameObjects as they were at the end of the cutscene
    private void OnEnable() {
        if (CutsceneManager.S.sceneDone[ndx]) {
            CutsceneManager.S.SceneHasAlreadyHappened(ndx, actors);
        }
    }

    protected override void Action() {
        CutsceneManager.S.StartScene(ndx, actors);
    }
}
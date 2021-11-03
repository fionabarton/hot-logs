using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapLayerManager : MonoBehaviour {
    [Header("Set in Inspector")]
    public List<SpriteRenderer>     layerSRend = new List<SpriteRenderer>();
    public List<string>             layerName = new List<string>();

    // Singleton
    private static SwapLayerManager _S;
    public static SwapLayerManager S { get { return _S; } set { _S = value; } }

    void Awake() {
        S = this;
    }

    // Get all swap layer sprite renderers in current scene
    public void GetSpriteRenderers() {
        // Clear/reset list
        layerSRend.Clear();

        GameObject layerSprites = GameObject.Find("SwapLayerSprites");
        if (layerSprites != null) {
            foreach (Transform child in layerSprites.transform) {
                SpriteRenderer sRend = child.GetComponent<SpriteRenderer>();

                if (sRend != null) {
                    layerSRend.Add(sRend);
                }
            }
        }
    }


    // Get list of the current sorting layer name for each swap layer sprites in current scene
    public void GetLayerNames() {
        // Clear/reset list
        layerName.Clear();

        GameObject layerSprites = GameObject.Find("SwapLayerSprites");
        if (layerSprites != null) {
            foreach (Transform child in layerSprites.transform) {
                SpriteRenderer sRend = child.GetComponent<SpriteRenderer>();

                if(sRend != null) {
                    layerName.Add(sRend.sortingLayerName);
                }
            }
        }
    }

    // After battle, set each sprite renderer's sorting layer name to what they were right before the battle started
    public void SetLayerNames() {
        for (int i = 0; i < layerSRend.Count - 1; i++) {
            layerSRend[i].sortingLayerName = layerName[i];
        }
    }
}
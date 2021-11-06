using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeSwapLayerTrigger : MonoBehaviour {
	[Header("Set in Inspector")]
    // Colliders that enable/disable when Player enters trigger
    public GameObject       horizontalCollider;
    public GameObject       verticalCollider;

    // Sprites that change sorting layer when Player enters trigger
	public SpriteRenderer   frontSprite;
    public SpriteRenderer   backSprite;

    public bool             isHorizontalTrigger;

    public int              level;

    private void OnEnable() {
        if(level == Player.S.level) {
            // Set the sorting layer of each sprite
            frontSprite.sortingLayerName = "Player";
            backSprite.sortingLayerName = "Foreground";

            //Enable/Disable colliders
            horizontalCollider.SetActive(true);
            verticalCollider.SetActive(false);
        } else {
            // Set the sorting layer of each sprite
            frontSprite.sortingLayerName = "Player";
            backSprite.sortingLayerName = "Player";

            //Enable/Disable colliders
            horizontalCollider.SetActive(false);
            verticalCollider.SetActive(true);
        }
    }

    void OnTriggerEnter2D(Collider2D coll) {
        if (coll.gameObject.CompareTag("PlayerTrigger")) {
			if (isHorizontalTrigger) {
                // Set the sorting layer of each sprite
                frontSprite.sortingLayerName = "Player";
                backSprite.sortingLayerName = "Foreground";

                //Enable/Disable colliders
                horizontalCollider.SetActive(true);
                verticalCollider.SetActive(false);
            } else {
                // Set the sorting layer of each sprite
                frontSprite.sortingLayerName = "Player";
                backSprite.sortingLayerName = "Player";

                //Enable/Disable colliders
                horizontalCollider.SetActive(false);
                verticalCollider.SetActive(true);
            }
        }
	}
}
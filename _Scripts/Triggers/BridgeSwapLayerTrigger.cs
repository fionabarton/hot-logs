using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeSwapLayerTrigger : MonoBehaviour {
	[Header("Set in Inspector")]
    // Colliders that enable/disable when Player enters trigger
    public GameObject enableCollider;
    public GameObject disableCollider;

    // Sprites that change sorting layer when Player enters trigger
	public SpriteRenderer frontSprite;
    public SpriteRenderer backSprite;

    public bool isHorizontalTrigger;

    void OnTriggerEnter2D(Collider2D coll) {
        if (coll.gameObject.CompareTag("PlayerTrigger")) {
            //Enable/Disable colliders
            enableCollider.SetActive(true);
            disableCollider.SetActive(false);

            // Change the sorting layer of the sprites
			if (isHorizontalTrigger) {
				frontSprite.sortingLayerName = "Player";
                backSprite.sortingLayerName = "Foreground";
			} else {
                frontSprite.sortingLayerName = "Player";
                backSprite.sortingLayerName = "Player";
            }
        }
	}
}

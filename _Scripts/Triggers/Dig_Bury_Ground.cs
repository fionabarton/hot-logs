using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eGroundType { desert, dirt, grass, sand, snow };

public class Dig_Bury_Ground : ActivateOnButtonPress {
    [Header("Set in Inspector")]
    public eGroundType      groundType = eGroundType.dirt;

    public List<Sprite>     moundSprites = new List<Sprite>();
	public List<Sprite>		holeSprites = new List<Sprite>();

	public SpriteRenderer	sRend;
    public BoxCollider2D    boxColl;

    [Header("Set Dynamically")]
    public bool             isDugUp;

    void Start() {
        switch (groundType) {
            case eGroundType.desert:
                sRend.sprite = moundSprites[0];
                break;
            case eGroundType.dirt:
                sRend.sprite = moundSprites[1];
                break;
            case eGroundType.grass:
                sRend.sprite = moundSprites[2];
                break;
            case eGroundType.sand:
                sRend.sprite = moundSprites[3];
                break;
            case eGroundType.snow:
                sRend.sprite = moundSprites[4];
                break;
        }
    }

    protected override void Action() {
        //Set Sprites and Collider
        SetSpritesAndCollider(isDugUp);
    }

    public void SetSpritesAndCollider(bool tBool) {
        // Set Sprite
        if (tBool) {
            DialogueManager.S.DisplayText("And now you've buried it...");

            // Audio: Deny
            AudioManager.S.PlaySFX(eSoundName.deny);

            switch (groundType) {
                case eGroundType.desert:
                    sRend.sprite = moundSprites[0];
                    break;
                case eGroundType.dirt:
                    sRend.sprite = moundSprites[1];
                    break;
                case eGroundType.grass:
                    sRend.sprite = moundSprites[2];
                    break;
                case eGroundType.sand:
                    sRend.sprite = moundSprites[3];
                    break;
                case eGroundType.snow:
                    sRend.sprite = moundSprites[4];
                    break;
            }
        } else {
            DialogueManager.S.DisplayText("You have dug up the ground!");

            // Audio: Confirm
            AudioManager.S.PlaySFX(eSoundName.confirm);

            switch (groundType) {
                case eGroundType.desert:
                    sRend.sprite = holeSprites[0];
                    break;
                case eGroundType.dirt:
                    sRend.sprite = holeSprites[1];
                    break;
                case eGroundType.grass:
                    sRend.sprite = holeSprites[2];
                    break;
                case eGroundType.sand:
                    sRend.sprite = holeSprites[3];
                    break;
                case eGroundType.snow:
                    sRend.sprite = holeSprites[4];
                    break;
            }
        }

        // Box Collider
        boxColl.enabled = !tBool;

        // Reset Bool
        isDugUp = !tBool;
	}
}

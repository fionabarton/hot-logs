using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPlayer : MonoBehaviour {
    [Header("Set in Inspector")]
    public float    lowerAmountToBlock = 0.625f;
    public float    upperAmountToBlock = 0.625f;

    public bool     isHorizontalOrVertical;

    [Header("Set Dynamically")]
    public bool     isChecking;

    public float    startingPosition;
    public float    lowerPosition;
    public float    upperPosition;

    private void Start() {
        // Set starting position
        if (isHorizontalOrVertical) {
            startingPosition = transform.position.x;
        } else {
            startingPosition = transform.position.y;
        }
     
        // Set upper and lower bounds 
        upperPosition = startingPosition + upperAmountToBlock;
        lowerPosition = startingPosition - lowerAmountToBlock;
    }

    void OnBecameVisible() {
        isChecking = true;

        StartCoroutine("FixedUpdateCoroutine");
    }

    void OnBecameInvisible() {
        isChecking = false;

        StopCoroutine("FixedUpdateCoroutine");
    }

    public IEnumerator FixedUpdateCoroutine() {
        if (isChecking) {
            // Follow player horizontally
            if (isHorizontalOrVertical) {
                if (Player.S.gameObject.transform.position.x < upperPosition &&
                    Player.S.gameObject.transform.position.x > lowerPosition) {
                    Utilities.S.SetPosition(gameObject, Player.S.gameObject.transform.position.x, gameObject.transform.position.y);
                }
            // Follow player vertically
            } else {
                if (Player.S.gameObject.transform.position.y < upperPosition &&
                    Player.S.gameObject.transform.position.y > lowerPosition) {
                    Utilities.S.SetPosition(gameObject, gameObject.transform.position.x, Player.S.gameObject.transform.position.y);
                }
            }
        }

        yield return new WaitForFixedUpdate();
        StartCoroutine("FixedUpdateCoroutine");
    }
}

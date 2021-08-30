using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatDetection : MonoBehaviour
{
    [Header("Set in Inspector")]
    // Song to play
    public int songNdx = 0;

    public List<AudioSource>    bgmCS = new List<AudioSource>();
    public List<int>            tempos = new List<int>();
    public List<float>          songDurations = new List<float>();

    // Amount of time the player is able to correctly input before and after the beat
    float threshold = 0.3f; 
   
    // Testing: sprite color change
    public SpriteRenderer sprite;

    [Header("Set Dynamically")]
    // Length (time) of one beat
    float beatDuration;

    // Prevents detecting the beat if 
    float waitToDetectBeat = 0;
    float amountToWait;

    // Testing: sprite color change 
    public bool isColored;

    private void Start() {
        // Map beats from (0s to 60s) to (0% to 100%)
        float mappedBeats = (Utilities.S.Map(0, 60, 0, 100, songDurations[songNdx])) / 100;

        // Get total amount of beats
        float amountOfBeats = mappedBeats * tempos[songNdx];

        // Get duration of single beat
        beatDuration = songDurations[songNdx] / amountOfBeats;

        // Set duration of how long to wait to detect beat
        amountToWait = beatDuration / 2;

        // Play song
        bgmCS[songNdx].Play();

        //threshold = beatDuration / 2;

        Debug.Log("Beat duration: " + beatDuration);
        Debug.Log("Threshold: " + threshold);
    }

    void Update() {
        ////////////////////////////////////////////////////////
        // Does something visually to represent current tempo //
        ////////////////////////////////////////////////////////

        // Time is up, so we're able to check for the next beat
        if (waitToDetectBeat > amountToWait) {
            // Is close enough to where a beat should be, so a beat is detected
            if (bgmCS[songNdx].time % beatDuration > -0.1f && bgmCS[songNdx].time % beatDuration < 0.1f) {
                isColored = !isColored;

                // Switch sprite color
                if (isColored) {
                    sprite.color = Color.white;
                } else {
                    sprite.color = Color.black;
                }

                // Reset wait timer
                waitToDetectBeat = 0;
            }
        } else {
            // Once time is up, we're able to check for the next beat
            waitToDetectBeat += Time.deltaTime;
        }

        ///////////////////////////////////////////////////////////
        // Indicates whether player has tapped along to the beat //
        ///////////////////////////////////////////////////////////

        if (Input.GetButtonDown("SNES A Button")) {
            if (bgmCS[songNdx].time % beatDuration > -threshold && bgmCS[songNdx].time % beatDuration < threshold) {
                Debug.Log("GOOD");
            } else {
                Debug.Log("BAD");
            }
        }
    }
}
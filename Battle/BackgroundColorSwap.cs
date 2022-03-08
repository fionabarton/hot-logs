using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundColorSwap : MonoBehaviour{
    [Header("Set in Inspector")]
    public int  colorNdx = 0;

    [Header("Set Dynamically")]
    List<Vector4> colors = new List<Vector4> 
    { 
        new Vector4(0, 0, 0, 1),         // Black
        new Vector4(0, 0, 1, 1),         // Blue
        new Vector4(0, 1, 0, 1),         // Green
        new Vector4(1, 0.64f, 0, 1),     // Orange
        new Vector4(0.5f, 0, 0.5f, 1),   // Purple
        new Vector4(1, 0, 0, 1),         // Red
        new Vector4(1, 1, 1, 1),         // White
        new Vector4(1, 0.92f, 0.016f, 1) // Yellow
    };

    SpriteRenderer sRend;

    void Start() {
        sRend = GetComponent<SpriteRenderer>();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.X)) {
            if(colorNdx < colors.Count - 1) {
                colorNdx += 1;
            } else {
                colorNdx = 0;
            }

            sRend.color = colors[colorNdx];
        }
    }
}
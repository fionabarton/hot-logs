using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleParticleSystems : MonoBehaviour{
    [Header("Set in Inspector")]
    public List<GameObject> particleSystems = new List<GameObject>();

    public int ndx = 0;

    [Header("Set Dynamically")]
    List<Vector4> colors = new List<Vector4>
    {
        new Vector4(0, 0, 0, 1),         // Black
        new Vector4(1, 0.92f, 0.016f, 1),// Yellow
        new Vector4(1, 1, 1, 1),         // White
        new Vector4(0, 0, 1, 1),         // Blue
        new Vector4(0, 1, 0, 1),         // Green
        new Vector4(1, 0.64f, 0, 1),     // Orange
        new Vector4(0.5f, 0, 0.5f, 1),   // Purple
        new Vector4(1, 0, 0, 1)          // Red
    };

    SpriteRenderer sRend;

    private void OnEnable() {
        sRend = GetComponent<SpriteRenderer>();

        // Deactivate all particle systems
        for (int i = 0; i < particleSystems.Count; i++) {
            particleSystems[i].SetActive(false);
        }

        // Activate particle system
        particleSystems[ndx].SetActive(true);

        // Set background sprite color
        switch (ndx) {
            case 0:
                sRend.color = colors[0];
                break;
            case 1:
                sRend.color = colors[5];
                break;
            case 2:
                sRend.color = colors[0];
                break;
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Z)) {
            if (ndx < particleSystems.Count - 1) {
                ndx += 1;
            } else {
                ndx = 0;
            }

            // Deactivate all particle systems
            for (int i = 0; i < particleSystems.Count; i++) {
                particleSystems[i].SetActive(false);
            }

            // Activate particle system
            particleSystems[ndx].SetActive(true);
        }
    }
}

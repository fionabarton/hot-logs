using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSwap : MonoBehaviour{
    [Header("Set in Inspector")]
    public List<GameObject> maps = new List<GameObject>();

    public int currentMapNdx = 0;

    void Start(){
        // Activate first map
        Map(currentMapNdx);
    }

    void Update(){
        // Go to next map
        if (Input.GetKeyDown(KeyCode.X)) {
            if(currentMapNdx + 1 < maps.Count) {
                currentMapNdx += 1;
            } else {
                currentMapNdx = 0;
            }

            Map(currentMapNdx);
        }

        // Go to previous map
        if (Input.GetKeyDown(KeyCode.Z)) {
            if (currentMapNdx > 0) {
                currentMapNdx -= 1;
            } else {
                currentMapNdx = maps.Count - 1;
            }

            Map(currentMapNdx);
        }

        // Reset player position to (0, 0)
        if (Input.GetKeyDown(KeyCode.Space)) {
            Player.S.transform.position = new Vector2(0, 1);
        }
    }

    void Map(int ndx) {
        // Deactivate all maps
        for(int i = 0; i < maps.Count; i++) {
            maps[i].SetActive(false);
        }

        // Activate selected map
        maps[ndx].SetActive(true);
        Debug.Log("Map: " + maps[ndx]);
    }
}

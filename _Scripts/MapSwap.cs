using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSwap : MonoBehaviour{
    [Header("Set in Inspector")]
    public List<GameObject> maps = new List<GameObject>();

    public int currentMapNdx = 0;

    void Start(){
        // activate selected map
        Map(currentMapNdx);
    }

    void Update(){
        // change index
        if (Input.GetKeyDown(KeyCode.Z)) {

            if(currentMapNdx + 1 < maps.Count) {
                currentMapNdx += 1;
            } else {
                currentMapNdx = 0;
            }

            Map(currentMapNdx);
        }

        if (Input.GetKeyDown(KeyCode.X)) {
            Player.S.transform.position = new Vector2(0, 1);
        }
    }

    void Map(int ndx) {
        // deactivate all maps
        for(int i = 0; i < maps.Count; i++) {
            maps[i].SetActive(false);
        }

        // activate selected map
        maps[ndx].SetActive(true);
    }
}

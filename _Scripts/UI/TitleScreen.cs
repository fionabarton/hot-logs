using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour{
    [Header("Set in Inspector")]
    public List<Button> buttons;

    void Start(){
        // New Game Button
        buttons[0].onClick.AddListener(delegate { RPG.S.LoadLevel("Town_1"); });

        // Load Game Button
        //buttons[1].onClick.AddListener(delegate { RPG.S.LoadLevel("Area_1"); });

        // Options Button
        //buttons[2].onClick.AddListener(delegate { RPG.S.LoadLevel("Area_2"); });

        // Set Selected GameObject (New Game Button)
        Utilities.S.SetSelectedGO(buttons[0].gameObject);
    }
}

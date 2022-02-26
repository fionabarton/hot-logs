using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores and returns commonly used words and phrases
/// </summary>
public class WordManager : MonoBehaviour {
    [Header("Set Dynamically")]
    public List<string> exclamations = new List<string>();

    private static WordManager _S;
    public static WordManager S { get { return _S; } set { _S = value; } }

    void Awake() {
        S = this;
    }

    void Start() {
        exclamations = new List<string>() { "Oh yeah", "Heck yeah", "Hoorah", "Whoopee", "Yahoo", "Wahoo", "Hot diggity dog", 
            "Huzzah", "Yippee", "Woo hoo", "Whoop dee doo", "", "Hooray",  "Gee whiz", "Right on", "Far out", 
            "Groovy", "Awesome", "Excellent", "Cool", "Incredible", "Unreal", "Fabulous", "Terrific", "Yay", 
            "Fantastic", "Great", "Gnarly", "Sweet", "Nice", "Splendid", "Wicked", "Wow", "Dude", "Cool beans",
            "Booyah", "Cowabunga", "Tubular" };
    }

    public string GetRandomExclamation() {
        // Get random index
        int randomNdx = Random.Range(0, exclamations.Count);

        // Return random exclamation
        return exclamations[randomNdx];
    }
}
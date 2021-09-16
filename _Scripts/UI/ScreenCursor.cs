using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attached to RPGMainCamera; used to get ScreenCursor gameObject
/// </summary>
public class ScreenCursor : MonoBehaviour
{
    [Header("Set Dynamically")]
    // Singleton
    private static ScreenCursor _S;
    public static ScreenCursor S { get { return _S; } set { _S = value; } }
    
    [Header("Set in Inspector")]
    public List<GameObject>     cursorGO = new List<GameObject>();
    public List<RectTransform>  rectTrans = new List<RectTransform>();

    void Awake() {
        S = this;
    }
}

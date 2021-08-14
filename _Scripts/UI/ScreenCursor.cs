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
    // Cursor GameObject
    public GameObject cursorGO;
    public RectTransform rectTrans;

    void Awake() {
        // Singleton
        S = this;
    }
}

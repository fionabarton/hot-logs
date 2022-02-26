using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public delegate void ReactivateShopkeeperTriggerAction();
    public static event ReactivateShopkeeperTriggerAction OnShopScreenDeactivated;

    public static void ShopScreenDeactivated()
    {
        OnShopScreenDeactivated?.Invoke();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuItemState : MonoBehaviour {
    public bool disabled = false;
    
    [HideInInspector]
    public MenuItems parentMenu = null;
}

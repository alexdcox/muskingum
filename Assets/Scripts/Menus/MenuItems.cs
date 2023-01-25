using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class MenuItems : MonoBehaviour {
    public MenuInputActions menuInputActions;
    public GameObject[] menuItems;
    public UnityEvent[] menuEvents;

    private Color defaultColor = new Color32(226, 226, 226, 255);
    private Color selectedColor = new Color32(128, 199, 85, 255);
    private Color disabledColor = new Color32(108, 108, 108, 255);

    public int selectedIndex = 0;

    void OnStart() {
        // SetSelectedMenuItem(selectedIndex);
        foreach(GameObject menuItem in menuItems) {
            MenuItemState state = menuItem.GetComponent<MenuItemState>();
            if (state != null) {
                state.parentMenu = this;
                // TODO: Make sure the disabled ones are styled that way. Maybe this is best done within the actual component??
            } 
        }
        SetSelectedMenuItem(selectedIndex);
    }

    void OnAwake() {
        // foreach(GameObject menuItem in menuItems) {
        //     MenuItemState state = menuItem.GetComponent<MenuItemState>();
        //     if (state != null) {
        //         state.parentMenu = this;
        //         // TODO: Make sure the disabled ones are styled that way. Maybe this is best done within the actual component??
        //     } 
        // }
        // SetSelectedMenuItem(selectedIndex);
    }

    void OnEnable() {
        menuInputActions = new MenuInputActions();
        menuInputActions.Enable();
        menuInputActions.Main.Up.performed += context => {
            SetSelectedMenuItem(selectedIndex - 1);
        };
        menuInputActions.Main.Down.performed += context => {
            SetSelectedMenuItem(selectedIndex + 1);
        };
        menuInputActions.Main.Select.performed += context => {
            menuEvents[selectedIndex].Invoke();
        };
    }

    void OnDisable() {
        menuInputActions.Disable();
    }

    void OnMenuItemDisabledChange(GameObject menuItem) {
        // TODO: if it is now 
    }

    void SetSelectedMenuItem(int index) {
        if (index < 0) {
            index = menuItems.Length - 1;
        }
        if (index > menuItems.Length - 1) {
            index = 0;
        }
        MenuItemState selectedItemState = menuItems[index].GetComponent<MenuItemState>();
        if (selectedItemState && selectedItemState.disabled) {
            if (menuItems.Length <= 2) {
                return;
            }
            int offset = index - selectedIndex;
            SetSelectedMenuItem(index + offset);
            return;
        }
        foreach(GameObject menuItem in menuItems) {
            Color color = defaultColor;
            MenuItemState state = menuItem.GetComponent<MenuItemState>();
            if (state != null && state.disabled) {
                color = disabledColor;
            } 
            menuItem.GetComponent<TMP_Text>().color = color;
        }
        menuItems[index].GetComponent<TMP_Text>().color = selectedColor;
        selectedIndex = index;
    }
}

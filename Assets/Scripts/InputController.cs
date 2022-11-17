using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using HexGame;

public class InputController : MonoBehaviour {
    public GameController gameController;

    InputActions _inputActions;

    HexLayout2 _lastMouseInLayout;
    Hex _lastMouseInHex;

    void Start() {
        _inputActions = new InputActions();
        _inputActions.Mouse.Enable();
        _inputActions.Mouse.MousePosition.performed += OnMouseMove;
        _inputActions.Mouse.MouseClick.performed += OnMouseClick;

    }

    // void OnEnable() {
    //     _inputActions.Mouse.Enable();
    // }

    // void OnDisable() {
    //     _inputActions.Mouse.Disable();
    // }

    void OnMouseMove (InputAction.CallbackContext ctx) {
        (HexLayout2 hexLayout, Hex hex) = RaycastForHex(ctx);

        if (_lastMouseInHex != null && _lastMouseInHex != hex) {
            OnHexOut(hexLayout, hex);
        }
        
        if (hex != null) {
            OnHexIn(hexLayout, hex);
        }
    }

    void OnMouseClick(InputAction.CallbackContext ctx) {
        (HexLayout2 hexLayout, Hex hex) = RaycastForHex(ctx);
        if (hex != null) {
            gameController.OnHexClicked(hexLayout, hex);
        }
    }

    void OnHexOut(HexLayout2 hexLayout, Hex hex) {
        gameController.OnHexMouseOut(_lastMouseInLayout, _lastMouseInHex);
        _lastMouseInHex = null;
        _lastMouseInLayout = null;
    }

    void OnHexIn(HexLayout2 hexLayout, Hex hex) {
        gameController.OnHexMouseIn(hexLayout, hex);
        _lastMouseInLayout = hexLayout;
        _lastMouseInHex = hex;
    }

    (HexLayout2 hexLayout, Hex hex) RaycastForHex(InputAction.CallbackContext ctx) {
        HexLayout2 hexLayout = null;
        Hex hex = null;
        
        var pos = Mouse.current.position.ReadValue();
        var ray = Camera.main.ScreenPointToRay(pos);
        
        if (Physics.Raycast(ray, out RaycastHit rayHitInfo)) {
            var go = rayHitInfo.collider.gameObject;
            if (go.name.Contains("Bg")) {
                hexLayout = go.transform.parent.GetComponent<HexLayout2>();
                var worldPoint = Camera.main.ScreenToWorldPoint(pos);
                var layoutPoint = worldPoint - hexLayout.transform.position - hexLayout.centerTranslate;
                hex = hexLayout.layout.PixelToHex(layoutPoint);
            }
        }

        return (hexLayout, hex);
    }
}

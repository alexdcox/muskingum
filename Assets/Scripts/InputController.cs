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

    GameInputActions _inputActions;

    HexLayout2 _lastMouseInLayout = null;
    Hex _lastMouseInHex = null;

    void Start() {
        _inputActions = new GameInputActions();
        _inputActions.Mouse.Enable();
        _inputActions.Mouse.MousePosition.performed += OnMouseMove;
        _inputActions.Mouse.MouseClick.performed += OnMouseClick;
        _inputActions.Mouse.EnterPress.performed += OnEnterPress;

    }

    void OnEnable() {
        _inputActions.Mouse.Enable();
    }

    void OnDisable() {
        _inputActions.Mouse.Disable();
    }

    void OnMouseMove (InputAction.CallbackContext ctx) {
        (HexLayout2 hexLayout, Hex hex) = RaycastForHex(ctx);
       
        if (_lastMouseInHex != null && hex == null) {
            gameController.OnHexMouseOut(_lastMouseInLayout, _lastMouseInHex);
            _lastMouseInHex = null;
            _lastMouseInLayout = null;
        }
        
        if (hex != null) {
            // skip mouse in if we're already in that hex
            if (_lastMouseInHex != null && _lastMouseInHex.Equals(hex)) {
                return;
            }
            // mouse out of previous hex if we've moved the mouse too quickly into another hex
            if (_lastMouseInHex != null && !_lastMouseInHex.Equals(hex)) {
                gameController.OnHexMouseOut(_lastMouseInLayout, _lastMouseInHex);
                _lastMouseInHex = null;
                _lastMouseInLayout = null;
            }
            gameController.OnHexMouseIn(hexLayout, hex);
            _lastMouseInHex = hex;
            _lastMouseInLayout = hexLayout;
        }
    }

    void OnMouseClick(InputAction.CallbackContext ctx) {
        (HexLayout2 hexLayout, Hex hex) = RaycastForHex(ctx);
        if (hexLayout == null || hex == null) {
            return;
        }
        if (hexLayout != null && hex != null) {
            gameController.OnHexClicked(hexLayout, hex);
        }
    }

    void OnEnterPress(InputAction.CallbackContext ctx) {
      gameController.SkipTurn();
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

using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class InputController : MonoBehaviour {
    InputActions _inputActions;
    TMP_Text _coords;
    GameController _gameController;

    void Start() {
        _gameController = GameObject.Find("Controllers").GetComponent<GameController>();
        _coords = GameObject.Find("Coords").GetComponent<TMP_Text>();

        _inputActions = new InputActions();
        _inputActions.Mouse.Enable();
        _inputActions.Mouse.MousePosition.performed += OnMouseMove;
        _inputActions.Mouse.MouseClick.performed += OnMouseClick;

    }

    void OnEnable() {
    }

    // void OnDisable() {
    //     _inputActions.Mouse.Disable();
    // }

    void Update() {
        // Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        // if (!Physics.Raycast(ray, out RaycastHit rayHitInfo))
        //     return;
        //
        // GameObject currentHex = rayHitInfo.collider.gameObject;
        //
        // var meshRenderer = currentHex.GetComponent<MeshRenderer>();
        // meshRenderer.material.color = Color.white;
        //
        // if (_previousHex != null && currentHex != _previousHex) {
        //     _previousHex.GetComponent<MeshRenderer>().material.color = Color.black;
        // }
        // _previousHex = currentHex;
        //
        // if (Input.GetMouseButtonDown(0)) {
        //     Debug.Log(currentHex.transform.parent.name);
        // }
    }

    void OnMouseMove (InputAction.CallbackContext ctx) {
        var pos = ctx.ReadValue<Vector2>();
        Vector2 cellPos = _gameController.MouseToCell(pos);
        _coords.text = cellPos.ToString();
    }

    void OnMouseClick(InputAction.CallbackContext ctx) {
        // GameObject summoner = GameObject.Find("Summoner");
        // Vector3 to = _gameController.CellToMouse(new Vector2(0, 0));
        // summoner.transform.DOMove(to, .6f).SetEase(Ease.InOutCubic);

        var definitions = Resources.FindObjectsOfTypeAll<UnitDefinition>();

        UnitDefinition summoner = null;
        foreach (UnitDefinition definition in definitions) {
            if (definition.name == "Summoner") {
                summoner = definition;
            }
        }

        _gameController.SpawnUnit(summoner, 2, new Vector2(0, 0));
    }
}

using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;
using HexGame;
using System.Collections.Generic;
using System;
using System.Linq;
using TMPro;

public class GameController : MonoBehaviour {
    public Camera mainCamera;
    public TMP_Text coords;
    public HexLayout2 board;
    public HexLayout2 p1Hand;
    public HexLayout2 p1Draw;
    public HexLayout2 p2Hand;
    public HexLayout2 p2Draw;

    [HideInInspector] Game _game;

    Dictionary<UnitDefinition, Unit> _unitDefinitions = new();

    void Start() {       
        int layoutsReady = 0;
        void handleLayoutComplete(HexLayout2 layout) {
            layout.LayoutCompleted += () => {
                layoutsReady++;
                if(layoutsReady == 5) {
                    _game = new Game(LoadAvailableUnits(), board.layout);
                    _game.StateChanged += Refresh;
                    _game.Start();
                }
            };
        }
        handleLayoutComplete(board);
        handleLayoutComplete(p1Hand);
        handleLayoutComplete(p1Draw);
        handleLayoutComplete(p2Hand);
        handleLayoutComplete(p2Draw);
    }

    public void OnHexMouseIn(HexLayout2 hexLayout, Hex hex) {
        var doubled = hexLayout.layout.HexToDoubledCoord(hex);
        coords.text = String.Format("Axial({0},{1}) Doubled({2},{3})", (int)hex.q, (int)hex.r, (int)doubled.col, (int)doubled.row);
    }

    public void OnHexMouseOut(HexLayout2 hexLayout, Hex hex) {
        coords.text = "";
    }
    
    public void OnHexClicked(HexLayout2 hexLayout, Hex hex) {
        var doubled = hexLayout.layout.HexToDoubledCoord(hex);
        coords.text = String.Format("clickity: Axial({0},{1}) Doubled({2},{3})", (int)hex.q, (int)hex.r, (int)doubled.col, (int)doubled.row);
    }
    
    public UnitDefinition GetUnitDefinition(Unit unit) {
        foreach (var (definition, unit2) in _unitDefinitions) {
            if (unit.name == unit2.name) {
                return definition;
            }
        }
        return null;
    }

    void Refresh(GameState state) {
        board.RemoveAllUnits();
        foreach (UnitState unitState in state.units) {
            board.SpawnUnit(unitState);
        }

        var availableUnits = _unitDefinitions.Values
            .ToList()
            .Where(unit => !unit.HasId(UnitId.Summoner))
            .OrderBy(unit => String.Format("{0}{1}", unit.cost, unit.name))
            .ToArray();

        void refreshDraw(int playerNum, HexLayout2 hexLayout) {
            hexLayout.RemoveAllUnits();
            var i = 0;
            foreach(var coord in hexLayout.GetHexCoords()) {
                hexLayout.SpawnUnit(new UnitState() {
                    unit = availableUnits[i++],
                    hex = Hex.Axial(coord.x, coord.y),
                    playerNum = playerNum,
                });
            }
        }
        refreshDraw(0, p1Draw);
        refreshDraw(1, p2Draw);
        
        void refreshHand(int playerNum, HexLayout2 hexLayout) {
            var player = state.players[playerNum];
            var handCoords = hexLayout.GetHexCoords();
            hexLayout.RemoveAllUnits();
            var i = 0;
            foreach (var unit in player.hand.GetUnits()) {
                var coord = handCoords[i++];
                hexLayout.SpawnUnit(new UnitState() {
                    unit = unit,
                    hex = Hex.Axial(coord.x, coord.y),
                    playerNum = playerNum,
                }); 
            }
        }

        refreshHand(0, p1Hand);
        refreshHand(1, p2Hand);
    }

    UnitCollection LoadAvailableUnits() {
        UnitCollection _availableUnits = new();
        foreach (UnitDefinition unitDefinition in Util.GetAllInstances<UnitDefinition>()) {
            var unit = new HexGame.Unit();
            unit.name = unitDefinition.name;
            unit.cost = unitDefinition.cost;
            unit.damage = unitDefinition.damage;
            unit.health = unitDefinition.health;
            unit.speed = unitDefinition.speed;
            _unitDefinitions.Add(unitDefinition, unit);
            _availableUnits.Add(unit);
        }
        return _availableUnits;
    }

    [MenuItem("HexGame/Testing")]
    public static void Test() {
        Debug.Log("IT DID IT");
    }

}

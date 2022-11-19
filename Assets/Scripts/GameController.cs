using UnityEditor;
using UnityEngine;
using HexGame;
using System.Collections.Generic;
using System;
using System.Linq;
using TMPro;

public class GameController : MonoBehaviour {
    public Camera mainCamera;
    public TMP_Text info;
    public TMP_Text coords;
    public HexLayout2 board;
    public HexLayout2 p1Hand;
    public HexLayout2 p1Draw;
    public HexLayout2 p2Hand;
    public HexLayout2 p2Draw;

    public Color summonHighlight = new Color32(0x5D, 0x3F, 0xD3, 0xFF);
    public Color moveHighlight = Color.green;

    private Game _game;
    private GameState _lastState;
    private ActionState _actionState = ActionState.None;
    private Dictionary<UnitDefinition, Unit> _unitDefinitions = new();
    private UnitState _selectedUnit = null;

    private enum ActionState {
        None,
        Summoning,
        MoveAttack
    }

    void Start() {       
        int layoutsReady = 0;
        void handleLayoutComplete(HexLayout2 layout) {
            layout.LayoutCompleted += () => {
                layoutsReady++;
                if(layoutsReady == 5) {
                    _game = new Game(LoadAvailableUnits(), board.layout);
                    _game.StateChanged += Refresh;
                    _game.Start();
                    PlaceEnergyIndicators();
                }
            };
        }
        handleLayoutComplete(board);
        handleLayoutComplete(p1Hand);
        handleLayoutComplete(p1Draw);
        handleLayoutComplete(p2Hand);
        handleLayoutComplete(p2Draw);
    }

    void InstantiateEnergyIndicators() {
        foreach((Hex energyHex, int energy) in Game.EnergyCoords) {

        }
    }

    public void OnHexMouseIn(HexLayout2 hexLayout, Hex hex) {
        var doubled = hexLayout.layout.HexToDoubledCoord(hex);
        coords.text = String.Format("Layout {0} Axial({1},{2}) Doubled({3},{4})", hexLayout.name, (int)hex.q, (int)hex.r, (int)doubled.col, (int)doubled.row);
        
        var playerNum = hexLayout.GetPlayerNumber();
        if (hexLayout.type == HexLayout2.Type.Hand && playerNum > -1 && playerNum == _lastState.turn.playerNum) {
            var unit = hexLayout.GetUnitRenderer(hex);
            if (unit == null) {
                // TODO: Work out why we sometimes get no unit.
                return;
            }
            unit.SetSelected(true);
        }
    }

    public void OnHexMouseOut(HexLayout2 hexLayout, Hex hex) {
        coords.text = "";
        
        var playerNum = hexLayout.GetPlayerNumber();
        if (hexLayout.type == HexLayout2.Type.Hand && playerNum > -1 && playerNum == _lastState.turn.playerNum) {
            UnitRenderer unitRenderer = hexLayout.GetUnitRenderer(hex);
            if (unitRenderer == null) {
                // TODO: Work out why we sometimes get no unit.
                return;
            }
            if (unitRenderer.unitState == _selectedUnit) {
                return;
            }
            unitRenderer.SetSelected(false);
        }
    }
    
    public void OnHexClicked(HexLayout2 hexLayout, Hex hex) {
        UnitRenderer unitRenderer = hexLayout.GetUnitRenderer(hex);
        var layoutPlayerNum = hexLayout.GetPlayerNumber();
        
        // ----- Handle select hand unit -----

        void hideSummonArea() {
            board.ResetBackgroundHexColor();
            _selectedUnit = null;
            _actionState = ActionState.None;
        }

        void deselectBoardUnit() {
            Debug.Log("Deselect board unit");
            board.ResetBackgroundHexColor();
            board.GetUnitRenderer(_selectedUnit.hex).SetSelected(false);
            _selectedUnit = null;
            _actionState = ActionState.None;
        }

        if (hexLayout.type == HexLayout2.Type.Hand) {
            if (unitRenderer != null && layoutPlayerNum == _lastState.turn.playerNum) {
                Debug.Log("@hande select hand unit " + unitRenderer.unitState.unit.name);
                if (_actionState != ActionState.None && _actionState != ActionState.Summoning) {
                    Debug.Log("Move/attack in progress, can't summon");
                    return;
                }
                if (_selectedUnit == unitRenderer.unitState) {
                    hideSummonArea();
                    return;
                }
                if (_selectedUnit != null) {
                    var previousUnitRenderer = hexLayout.GetUnitRenderer(_selectedUnit.hex);
                    previousUnitRenderer.SetSelected(false);
                }
                unitRenderer.SetSelected(true);
                _selectedUnit = unitRenderer.unitState;
                ShowSummonArea(_lastState.turn.playerNum);
                _actionState = ActionState.Summoning;
                return;
            } else {
                hideSummonArea();
                return;
            }
        }

        if (hexLayout.type == HexLayout2.Type.Board) {
            // ----- Handle summon -----

            if (_actionState == ActionState.Summoning) {
                Debug.Log("@hande summon");
                var summoner = _lastState.GetSummoner(_lastState.turn.playerNum);
                if (!summoner.hex.Neighbors().Any(h => h.Equals(hex))) {
                    Debug.Log("Can only summon next to summoner");
                    return;
                }
                _game.Summon(_selectedUnit.unit.Id(), hex);
                return;
            }


            // ----- Handle select board unit -----
            
            if (_actionState == ActionState.None &&
                unitRenderer != null &&
                unitRenderer.unitState.playerNum == _lastState.turn.playerNum
            ) {
                Debug.Log("@select board unit");
                if (_actionState == ActionState.Summoning) {
                    Debug.Log("Can't summon on top of another unit");
                    return;
                }
                if (unitRenderer.unitState == _selectedUnit) {
                    deselectBoardUnit();
                    return;
                }
                unitRenderer.SetSelected(true);
                _selectedUnit = unitRenderer.unitState;
                _actionState = ActionState.MoveAttack;
                HighlightAreaFromUnit(_selectedUnit.hex, _selectedUnit.unit.speed, moveHighlight);
                return;
            }

            // ----- Handle move/attack -----

            if (_actionState == ActionState.MoveAttack) {
                Debug.Log("@hande move/attack");
                if (_selectedUnit.hex.Equals(hex)) {
                    deselectBoardUnit();
                    return;
                }
                if (unitRenderer != null) {
                    if (unitRenderer.unitState.playerNum == _lastState.turn.playerNum) {
                        Debug.Log("Can't attack your own");
                        return;
                    }
                    var targetUnit = unitRenderer.unitState;
                    _game.Attack(_selectedUnit.hex, targetUnit.hex);
                } else {
                    _game.Move(_selectedUnit.hex, hex);
                }
            }
        }
    }

    public void HighlightAreaFromUnit(Hex hex, int distance, Color color) {
        foreach(Hex h in board.GetHexes()) {
            if (h.Distance(hex) <= distance && !board.HasUnit(h)) {
                board.SetBackgroundHexColor(h, color);
            }
        }
    }
    
    public void ShowSummonArea(int playerNum) {
        var summoner = _lastState.GetSummoner(playerNum);
        HighlightAreaFromUnit(summoner.hex, summoner.unit.speed, summonHighlight);
        // var neighbours = summoner.hex.Neighbors().FindAll(n => board.HasHex(n));
        // foreach(var neighbour in neighbours) {
        //     board.SetBackgroundHexColor(neighbour, Color.yellow);
        // }
    }

    public UnitDefinition GetUnitDefinition(Unit unit) {
        foreach (var (definition, unit2) in _unitDefinitions) {
            if (unit.name == unit2.name) {
                return definition;
            }
        }
        return null;
    }

    public void SkipTurn() {
        _game.SkipTurn(_lastState.turn.playerNum);
    }

    void Refresh(GameState state) {
        _lastState = state;
        _actionState = ActionState.None;
        _selectedUnit = null;

        info.text = String.Format(
            "Current player is {0}. You have {1} energy (+{2} per turn)",
            state.turn.playerNum + 1,
            state.Player().energy,
            state.Player().energyGain
        );
        
        board.ResetBackgroundHexColor();
        p1Hand.ResetBackgroundHexColor();
        p2Hand.ResetBackgroundHexColor();
        
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
            var player = state.players[playerNum];
            var i = 0;
            foreach(var hex in hexLayout.GetHexes()) {
                var unit = availableUnits[i++];
                UnitRenderer unitRenderer = hexLayout.SpawnUnit(new UnitState() {
                    unit = unit,
                    hex = hex,
                    playerNum = playerNum,
                });
                unitRenderer.showOverlay = player.draw.Find(u => u.name == unit.name) == null;
            }
        }
        refreshDraw(0, p1Draw);
        refreshDraw(1, p2Draw);
        
        void refreshHand(int playerNum, HexLayout2 hexLayout) {
            var player = state.players[playerNum];
            var handCoords = hexLayout.GetHexCoords();
            hexLayout.RemoveAllUnits();
            if (handCoords.Count() != 5) {
                return;
            }
            Debug.Log(String.Format(
                "refreshing hand, we have {0} hexes/positions and {1} in hand",
                handCoords.Count(),
                player.hand.Count()
            ));
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

    [MenuItem("HexGame/Place Energy Indicators")]
    public static void PlaceEnergyIndicators() {
        var board = GameObject.Find("Board").GetComponent<HexLayout2>();
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EnergyIndicator.prefab");
        foreach((Hex energy, int amount) in Game.EnergyCoords) {
            var pixel = (Vector3)board.layout.HexToPixel(energy) + board.centerTranslate + board.transform.position + new Vector3(0, 0, -2);
            var name = "Energy Indicator " + energy.ToString();
            var go = GameObject.Find(name);
            var pos = new Vector3(pixel.x, pixel.y, pixel.z);
            if (go == null) {
                go = Instantiate(prefab, pos, Quaternion.Euler(-180, 0, 0));
                go.name = "Energy Indicator " + energy.ToString();
            }
            go.transform.position = pos;
        }
    }

    [MenuItem("HexGame/Test")]
    public static void Test() {
        var gameController = GameObject.Find("Controllers").GetComponent<GameController>();
        var board = GameObject.Find("Board").GetComponent<HexLayout2>();
        var availableUnits = gameController.LoadAvailableUnits();
        var state = new UnitState(){
            unit = availableUnits.GetUnits()[0],
            hex = Hex.Axial(0, 0),
        };
        board.SpawnUnit(state);
    }
}

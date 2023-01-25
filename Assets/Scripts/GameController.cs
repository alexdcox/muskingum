using UnityEditor;
using UnityEngine;
using HexGame;
using System.Collections.Generic;
using System;
using System.Linq;
using TMPro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GameController : MonoBehaviour {
  public Camera mainCamera;
  public TMP_Text info;
  public TMP_Text coords;
  public HexLayout2 board;
  public HexLayout2 p1Hand;
  public HexLayout2 p1Draw;
  public HexLayout2 p2Hand;
  public HexLayout2 p2Draw;
  public MenuController menuController;
  public NetworkController networkController;
  public GameObject gameGameObject;
  public GameObject unitPrefab;
  public GameObject energyIndicatorPrefab;
  public List<UnitDefinition> unitDefinitions;
  public Shader blockColorShader;
  public Shader newTryShader;
  public GameOver gameOverScript;

  public Color summonHighlight = new Color32(0x5D, 0x3F, 0xD3, 0xFF);
  public Color moveHighlight = Color.green;

  private string _playerId;
  private int _currentPlayerIndex;
  private Game _game;
  private GameState _lastState;
  private ActionState _actionState = ActionState.None;
  private UnitState _selectedUnit = null;

  private bool _layoutsComplete = false;
  private bool _startPending = true;

  private enum ActionState {
    None,
    Summoning,
    MoveAttack
  }

  void Update() {
    if (
      !_layoutsComplete &&
      board.HasInit() &&
      p1Hand.HasInit() &&
      p1Draw.HasInit() &&
      p2Hand.HasInit() &&
      p2Draw.HasInit()
    ) {
      _layoutsComplete = true;
    }

    if (_layoutsComplete && _startPending) {
      _startPending = false;
      PlaceEnergyIndicators();
      Refresh(_lastState);
    }
  }

  public void OnHexMouseIn(HexLayout2 hexLayout, Hex hex) {
    var doubled = hexLayout.layout.HexToDoubledCoord(hex);
    coords.text = String.Format("D {0}, {1}", (int)doubled.col, (int)doubled.row);

    var playerIndex = hexLayout.GetPlayerIndex();
    if (hexLayout.type == HexLayout2.Type.Hand && playerIndex > -1 && playerIndex == _lastState.turn.playerIndex) {
      var unit = hexLayout.GetUnitRenderer(hex);
      if (unit == null) {
        return;
      }
      unit.SetSelected(true);
    }
  }

  public void OnHexMouseOut(HexLayout2 hexLayout, Hex hex) {
    coords.text = "";

    var playerIndex = hexLayout.GetPlayerIndex();
    if (hexLayout.type == HexLayout2.Type.Hand && playerIndex > -1 && playerIndex == _lastState.turn.playerIndex) {
      UnitRenderer unitRenderer = hexLayout.GetUnitRenderer(hex);
      if (unitRenderer == null) {
        return;
      }
      if (unitRenderer.unitState == _selectedUnit) {
        return;
      }
      unitRenderer.SetSelected(false);
    }
  }

  public void OnHexClicked(HexLayout2 hexLayout, Hex hex) {
    if (_lastState.turn.playerIndex != _currentPlayerIndex) {
      return;
    }

    UnitRenderer unitRenderer = hexLayout.GetUnitRenderer(hex);
    var layoutPlayerIndex = hexLayout.GetPlayerIndex();

    // ----- Handle select hand unit -----

    void hideSummonArea() {
      board.ResetBackgroundHexColor();
      _selectedUnit = null;
      _actionState = ActionState.None;
    }

    void deselectBoardUnit() {
      board.ResetBackgroundHexColor();
      board.GetUnitRenderer(_selectedUnit.coord).SetSelected(false);
      _selectedUnit = null;
      _actionState = ActionState.None;
    }

    if (hexLayout.type == HexLayout2.Type.Hand) {
      if (unitRenderer != null && layoutPlayerIndex == _lastState.turn.playerIndex) {
        if (_actionState != ActionState.None && _actionState != ActionState.Summoning) {
          return;
        }
        if (_selectedUnit == unitRenderer.unitState) {
          hideSummonArea();
          return;
        }
        if (_selectedUnit != null) {
          var previousUnitRenderer = hexLayout.GetUnitRenderer(_selectedUnit.coord);
          previousUnitRenderer.SetSelected(false);
        }
        unitRenderer.SetSelected(true);
        _selectedUnit = unitRenderer.unitState;
        ShowSummonArea(_lastState.turn.playerIndex);
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
        networkController.SendMessageSummon(_selectedUnit.id, hexLayout.layout.HexToDoubledCoord(hex));
        return;
      }


      // ----- Handle select board unit -----

      if (_actionState == ActionState.None &&
          unitRenderer != null &&
          unitRenderer.unitState.playerIndex == _currentPlayerIndex
      ) {
        if (_actionState == ActionState.Summoning) {
          return;
        }
        if (unitRenderer.unitState == _selectedUnit) {
          deselectBoardUnit();
          return;
        }
        unitRenderer.SetSelected(true);
        Hex selectedUnitHex = hexLayout.layout.CoordToHex(unitRenderer.unitState.coord);
        _actionState = ActionState.MoveAttack;
        _selectedUnit = unitRenderer.unitState;
        HighlightMoveAreaFromUnit(selectedUnitHex, unitRenderer.unitDefinition.speed, moveHighlight);
        return;
      }

      // ----- Handle move/attack -----

      if (_actionState == ActionState.MoveAttack) {
        Hex selectedUnitHex = hexLayout.layout.CoordToHex(_selectedUnit.coord);
        if (selectedUnitHex.Equals(hex)) {
          deselectBoardUnit();
          return;
        }
        if (unitRenderer != null) {
          if (unitRenderer.unitState.playerIndex == _currentPlayerIndex) {
            return;
          }
          var targetUnit = unitRenderer.unitState;
          Hex targetUnitHex = hexLayout.layout.CoordToHex(targetUnit.coord);
          networkController.SendMessageAttack(_selectedUnit.coord, hexLayout.layout.HexToDoubledCoord(hex));
        } else {
          networkController.SendMessageMove(_selectedUnit.coord, hexLayout.layout.HexToDoubledCoord(hex));
        }
      }
    }
  }

  public void HighlightAreaFromUnit(Hex hex, int distance, Color color) {
    foreach (Hex h in board.GetHexes()) {
      if (h.Distance(hex) <= distance && !board.HasUnit(h)) {
        board.SetBackgroundHexColor(h, color);
      }
    }
  }

  public void HighlightMoveAreaFromUnit(Hex hex, int distance, Color color) {
    UnitRenderer UnitAt(Hex hex) {
      var r = board.GetUnitRenderer(hex);
      if (r == null || r.unitState == null) {
        return null;
      }
      return r;
    }

    bool IsEnemy(UnitRenderer unit) {
      return unit != null && unit.unitState.playerIndex != _currentPlayerIndex;
    }

    UnitRenderer unit = UnitAt(hex);

    List<Hex> engaged = new();
    foreach (var next in hex.Neighbors()) {
      if (IsEnemy(UnitAt(next))) {
        engaged.Add(next);
      }
    }

    List<Hex> frontier = new();
    frontier.Add(hex);
    List<Hex> reached = new();
    reached.Add(hex);

    while (frontier.Count() > 0) {
      Hex current = frontier.Take(1).ElementAt(0);
      frontier.RemoveAt(0);
      foreach (var next in current.Neighbors()) {
        if (UnitAt(next) != null) {
          continue;
        }
        if (next.Distance(hex) > unit.unitDefinition.speed) {
          continue;
        }
        if (next.Distance(hex) > 10) {
          continue;
        }
        if (engaged.Any(e => e.Distance(next) < 2)) {
          continue;
        }
        if (!reached.Any(r => r.Equals(next))) {
          frontier.Add(next);
          reached.Add(next);
        }
      }
    }

    foreach (Hex h in reached) {
      board.SetBackgroundHexColor(h, color);
    }
  }

  public void ShowSummonArea(int playerIndex) {
    UnitState summoner = _lastState.GetSummoner(playerIndex);
    Hex summonerHex = board.layout.CoordToHex(summoner.coord);
    UnitDefinition summonerDefinition = GetUnitDefinition(summoner.id);
    HighlightAreaFromUnit(summonerHex, summonerDefinition.speed, summonHighlight);
  }

  public UnitDefinition GetUnitDefinition(UnitId id) {
    foreach (var unitDefinition in unitDefinitions) {
      if (unitDefinition.HasId(id)) {
        return unitDefinition;
      }
    }
    return null;
  }

  public void SkipTurn() {
    networkController.SendMessageSkipTurn();
  }

  public void Refresh(GameState state) {
    if (state == null) {
      return;
    }
    if (state.turn == null) {
      return;
    }
    if (state.players[_currentPlayerIndex] == null) {
      return;
    }

    _lastState = state;
    _actionState = ActionState.None;
    _selectedUnit = null;

    info.text = String.Format(
        "You are player {0}. Current player is {1}. You have {2} energy (+{3} per turn)",
        _currentPlayerIndex,
        state.turn.playerIndex,
        state.players[_currentPlayerIndex].energy,
        state.players[_currentPlayerIndex].energyGain
    );

    board.ResetBackgroundHexColor();
    p1Hand.ResetBackgroundHexColor();
    p2Hand.ResetBackgroundHexColor();

    board.RemoveAllUnits();
    foreach (UnitState unitState in state.units) {
      board.SpawnUnit(unitState);
    }

    var availableUnits = unitDefinitions
        .Where(unit => !unit.HasId(UnitId.Summoner))
        .OrderBy(unit => String.Format("{0}{1}", unit.cost, unit.name))
        .ToArray();

    void refreshDraw(int playerIndex, HexLayout2 hexLayout) {
      hexLayout.RemoveAllUnits();
      var player = state.players[playerIndex];
      var i = 0;
      foreach (var hex in hexLayout.GetHexes()) {
        UnitDefinition unitDefinition = availableUnits[i++];
        UnitRenderer unitRenderer = hexLayout.SpawnUnit(new UnitState() {
          id = unitDefinition.id,
          coord = hexLayout.layout.HexToDoubledCoord(hex),
          playerIndex = playerIndex,
          remainingHealth = unitDefinition.health,
        });
        unitRenderer.showOverlay = !player.draw.Any(uid => uid.Equals(unitDefinition.id));
      }
    }
    refreshDraw(0, p1Draw);
    refreshDraw(1, p2Draw);

    void refreshHand(int playerIndex, HexLayout2 hexLayout) {
      var player = state.players[playerIndex];
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
      foreach (UnitId unitId in player.hand) {
        Vector2Int coord = handCoords[i++];
        UnitDefinition unitDefinition = unitDefinitions.First(u => u.id == unitId);
        hexLayout.SpawnUnit(new UnitState() {
          id = unitDefinition.id,
          coord = hexLayout.layout.HexToDoubledCoord(Hex.Axial(coord.x, coord.y)),
          playerIndex = playerIndex,
          remainingHealth = unitDefinition.health,
        });
      }
    }

    refreshHand(0, p1Hand);
    refreshHand(1, p2Hand);
  }

  public void NewGame(GameState gameState) {
    menuController.HideAll();
    gameGameObject.SetActive(true);
    _startPending = true;
    _lastState = gameState;
  }

  public void EndGame(GameStats gameStats) {
    gameGameObject.SetActive(false);
    menuController.LoadScreen(MenuController.MenuScreen.GameOver);
    gameOverScript.RenderGameStats(gameStats);
  }

  public void SetPlayerId(string playerId) {
    _playerId = playerId;
  }

  public void SetPlayerIndex(int playerIndex) {
    _currentPlayerIndex = playerIndex;
  }

  public void PlaceEnergyIndicators() {
    var board = GameObject.Find("Board").GetComponent<HexLayout2>();
    foreach ((Hex energy, int amount) in Game.EnergyCoords) {
      var pixel = (Vector3)board.layout.HexToPixel(energy) + board.centerTranslate + board.transform.position + new Vector3(0, 0, -2);
      var name = "Energy Indicator " + energy.ToString();
      var go = GameObject.Find(name);
      var pos = new Vector3(pixel.x, pixel.y, pixel.z);
      if (go == null) {
        go = Instantiate(energyIndicatorPrefab, pos, Quaternion.Euler(-180, 0, 0));
        go.name = "Energy Indicator " + energy.ToString();
      }
      go.transform.position = pos;
    }
  }
}

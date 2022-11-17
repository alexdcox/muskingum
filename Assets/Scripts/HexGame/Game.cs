using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace HexGame {
  public delegate void StateChanged(GameState gameState);
  
  public class Game {
    // TODO: update for center being 0,0
    public static Dictionary<DoubledCoord, int> EnergyCoords = new Dictionary<DoubledCoord, int>(){
      {new DoubledCoord(3, 1), 1},
      {new DoubledCoord(3, 3), 1},
      {new DoubledCoord(9, 1), 1},
      {new DoubledCoord(9, 3), 1},
      {new DoubledCoord(6, 2), 2},
    };
    
    UnitCollection _availableUnits;
    Layout _layout;

    List<PlayerState> _players = new();
    List<UnitState> _boardUnits = new();
    TurnState _turn = new();

    public event StateChanged StateChanged;

    protected virtual void EmitStateChanged() {
        StateChanged?.Invoke(GetState());
    }
    
    public Game(UnitCollection availableUnits, Layout layout) {
      _availableUnits = availableUnits;
      _layout = layout;
      
      Unit summoner = availableUnits.FindById(UnitId.Summoner);
      if (summoner == null) {
        throw new Exception("Summoner unit not available");
      }

      UnitCollection summonable = availableUnits.Filter(unit => unit.name != summoner.name);
      
      for (int playerNum = 0; playerNum <= 1; playerNum++) {
        PlayerState state = new PlayerState();
        state.draw = new UnitCollection(summonable);
        state.draw.Shuffle();
        state.hand.Add(state.draw.Draw(5));
        _players.Add(state);
      }
      
      _boardUnits.Add(new UnitState() {
        unit = summoner,
        hex =  Hex.Axial(-3, 0),
        playerNum = 0,
      });

      _boardUnits.Add(new UnitState() {
        unit = summoner,
        hex = Hex.Axial(3, 0),
        playerNum = 1,
      });

      EmitStateChanged();
    }

    public void Start() {
      EmitStateChanged();
    }

    void NextTurn() {
      _turn.playerNum++;
      if (_turn.playerNum > 2) {
        _turn.playerNum = 1;
      }

      _turn.stage = TurnStage.Summon;
      Debug.Log(String.Format("Handling player {0} turn", _turn.playerNum + 1));
      CalculateEnergyGain();
      var player = _players[_turn.playerNum];
      Debug.Log(String.Format(
        "Increasing player {0} energy by {1} from {2} to {3}",
        _turn.playerNum,
        player.energyGain,
        player.energy,
        player.energy + player.energyGain
      ));
      player.energy += player.energyGain;

      EmitStateChanged();
    }

    void CalculateEnergyGain() {
      for (var playerIndex = 0; playerIndex < _players.Count; playerIndex++) {
        var player = _players[playerIndex];
        var energyGain = 1;
        
        foreach (var (coord, energy) in EnergyCoords) {
          foreach (var unit in _boardUnits) {
            if ((int)unit.playerNum != playerIndex) {
              continue;
            }
            // TODO: Need to get the layout doubled convertion setup cleanly somewhere.
            if (unit.hex == _layout.CoordToHex(coord)) {
              energyGain += energy;
            }
          }
        }

        player.energyGain = energyGain;
      }
    }

    void Summon(UnitId unitId, Hex to) {
      var playerIndex = _turn.playerNum - 1;
      var playerNum = _turn.playerNum;
      var player = _players[playerIndex];
      var unit = _availableUnits.FindById(unitId);
      String.Format("Player {0} summoning {1} to {2}", playerNum, unit.name, to);
      var unitState = new UnitState{
        unit = unit,
        hex = to,
        playerNum = _turn.playerNum,
      };
      if (player.energy < unit.cost) {
        Debug.Log(String.Format(
          "Not enough muhlah to summon {0} have {1}/{2}, nice try",
          unit.name,
          player.energy,
          unit.cost
        ));
        return;
      }
      player.energy -= unit.cost;
      var summoner = _boardUnits.Find(bu => bu.playerNum == playerNum && bu.unit.HasId(UnitId.Summoner));
      if (to.Distance(summoner.hex) != 1) {
        Debug.Log("Cannae summon there ya nonse");
        return;
      }
      _turn.unitsSummoned.Add(to);
      _boardUnits.Add(unitState);
      player.hand.RemoveById(unitId);
      player.hand.Add(player.draw.Draw(1));

      CalculateEnergyGain();
      EmitStateChanged();
    }

    void SkipSummon(int playerNum) {
      if (playerNum != _turn.playerNum) {
        Debug.Log("You cant skip another player summon");
        return;
      }
      Debug.Log(String.Format("Player {0} skipping summon", _turn.playerNum));
      _turn.stage = TurnStage.Move;
      EmitStateChanged();
    }

    void Move(Hex from, Hex to) {
      Debug.Log(String.Format("Player {0} attempting to move from {1} to {2}", _turn.playerNum, from, to));
      var unitState = _boardUnits.Find(unit => unit.hex == from);
      if (unitState is null) {
        Debug.Log("No unit");
        return;
      }
      var distance = from.Distance(to);
      var hasSummoningSickness = false;
      foreach (var summoned in _turn.unitsSummoned) {
        if (summoned == from) {
          hasSummoningSickness = true;
          break;
        }
      }
      if (hasSummoningSickness) {
        Debug.Log("Them theres got the summoning sickness");
        return;
      }
      if (distance > unitState.unit.speed) {
        Debug.Log("You aint got long enough legs for that pal");
        return;
      }
      var existingUnit = _boardUnits.Find(unit => unit.hex == to);
      if (existingUnit == null) {
        Debug.Log("Trying to move on top of another unit failed, they werent feeling frisky");
        return;
      }
      var alreadyMoved = _turn.unitsMoved.Find(coord => coord == from);
      if (alreadyMoved != null) {
        Debug.Log("The poor devils already done their duty, give em a rest");
        return;
      }
      unitState.hex = to;
      _turn.unitsMoved.Add(to);
      var unitCount = _boardUnits.Count(unit => unit.playerNum == _turn.playerNum);
      if (_turn.unitsSummoned.Count > 0) {
        unitCount--;
      }
      if (_turn.unitsMoved.Count == unitCount) {
        Debug.Log("All units moved, progressing to attack phase");
        _turn.stage = TurnStage.Attack;
      }
      CalculateEnergyGain();
      EmitStateChanged();
    }

    void SkipMove(int player) {
      if (player != _turn.playerNum) {
        Debug.Log("You cant skip another player move");
        return;
      }
      Debug.Log(String.Format("Player {0} skipping move", _turn.playerNum));
      _turn.stage = TurnStage.Attack;
      EmitStateChanged();
    }

    void Attack(Hex from, Hex to) {
      var playerNum = _turn.playerNum;
      Debug.Log(String.Format("Player {0} attacking from {1} to {2} to", playerNum, from, to));
      var hasSummoningSickness = false;
      foreach (var summoned in _turn.unitsSummoned) {
        if (summoned == from) {
          hasSummoningSickness = true;
          break;
        }
      }
      if (hasSummoningSickness) {
        Debug.Log("Them theres got the summoning sickness");
        return;
      }
      var fromUnitState = _boardUnits.Find(u => u.hex == from);
      if (fromUnitState == null) {
        Debug.Log("Theres... nothing there. You cant attack with nothing.");
        return;
      }
      var toUnitState = _boardUnits.Find(u => u.hex == to);
      if (toUnitState == null) {
        Debug.Log("You cant attack thin air");
        return;
      }
      var alreadyAttacked = _turn.unitsAttacked.Find(coord => coord == from);
      if (alreadyAttacked != null) {
        Debug.Log("This unit thinks one battle is enough for the time being");
        return;
      }
      var isFriendly = _boardUnits.Find(bu => bu.hex == to && bu.playerNum == playerNum);
      if (isFriendly != null) {
        Debug.Log("Whoa there laddy, that ones on our side");
        return;
      }
      toUnitState.remainingHealth -= fromUnitState.unit.damage;
      if (toUnitState.remainingHealth <= 0) {
        Debug.Log("Ohhh shiiit, unit down!");
        
        _boardUnits.Remove(toUnitState);
        
        if (toUnitState.unit.HasId(UnitId.Summoner)) {
          Debug.Log("💥💣🔥🧨🎇🚨 SUMMONER DOWN!!!");
          Debug.Log(String.Format("Player {0} wins!!!!!", _turn.playerNum));
          var winningSummonerState = _boardUnits.Find(bu => bu.playerNum == _turn.playerNum && bu.unit.HasId(UnitId.Summoner));
          _boardUnits = new(){winningSummonerState};
        }
      }
      _turn.unitsAttacked.Add(from);
      CalculateEnergyGain();
      EmitStateChanged();

      var unitCount = _boardUnits.Count(bu => bu.playerNum == _turn.playerNum);
      if (_turn.unitsAttacked.Count == unitCount) {
        Debug.Log("All units attacked, progressing to next turn");
        NextTurn();
      }
    }

    void SkipAttack(int player) {
      if (player != _turn.playerNum) {
        Debug.Log("You cant skip another player attack");
        return;
      }
      Debug.Log(String.Format("Player {0} skipping attack", _turn.playerNum));
      NextTurn();
    }

    void Log(string log) {
      Debug.Log(log);
    }

    public GameState GetState() {
      var gameState = new GameState() {
        units = _boardUnits,
        players = _players,
        turn = _turn,
      };

      // _playerState.forEach((playerState, id) => {
      //   gameState.players.push({
      //     id: id + 1,
      //     energy: playerState.energy,
      //     energyGain: playerState.energyGain,
      //     draw: playerState.draw.getUnitIds(),
      //     hand: playerState.hand.getUnitIds(),
      //     discard: playerState.discard.getUnitIds(),
      //   })
      // })

      return gameState;
    }
  }
}
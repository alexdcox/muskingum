using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace HexGame {
  public delegate void StateChanged(GameState gameState);
  
  public class Game {
    public static Dictionary<Hex, int> EnergyCoords = new(){
      {Hex.Axial(0, 0), 2},
      {Hex.Axial(-2, 1), 1},
      {Hex.Axial(-1, -1), 1},
      {Hex.Axial(1, 1), 1},
      {Hex.Axial(2, -1), 1},
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
        PlayerState player = new PlayerState();
        player.draw = new UnitCollection(summonable);
        player.draw.Shuffle();
        player.hand.Add(player.draw.Draw(5));
        _players.Add(player);
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
      NextTurn();
    }

    void NextTurn() {
      _turn = new TurnState() {
        playerNum = (_turn.playerNum + 1) % 2
      };
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
            if (unit.hex.Equals(coord)) {
              energyGain += energy;
            }
          }
        }

        player.energyGain = energyGain;
      }
    }

    public void Summon(UnitId unitId, Hex to) {
      var player = _players[_turn.playerNum];
      var unit = player.hand.FindById(unitId);
      if (unit == null) {
        Debug.Log(String.Format("Cannot summon {0}, not in player hand", Unit.IdToString(unitId)));
        return;
      }
      String.Format("Player {0} summoning {1} to {2}", _turn.playerNum, unit.name, to.ToString());
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
      var summoner = _boardUnits.Find(bu => bu.playerNum == _turn.playerNum && bu.unit.HasId(UnitId.Summoner));
      if (to.Distance(summoner.hex) != 1) {
        Debug.Log("Cannae summon there ya nonse");
        return;
      }
      _turn.unitsSummoned.Add(to);
      _boardUnits.Add(unitState);
      player.draw.RemoveById(unitId);
      player.hand.RemoveById(unitId);
      player.hand.Add(player.draw.Draw(1));

      CalculateEnergyGain();
      EmitStateChanged();
    }

    public void Move(Hex from, Hex to) {
      Debug.Log(String.Format("Player {0} attempting to move from {1} to {2}", _turn.playerNum, from.ToString(), to.ToString()));
      var unitState = _boardUnits.Find(unit => unit.hex.Equals(from));
      if (unitState is null) {
        Debug.Log("No unit");
        return;
      }
      var distance = from.Distance(to);
      var hasSummoningSickness = false;
      foreach (var summoned in _turn.unitsSummoned) {
        if (summoned.Equals(from)) {
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
      var existingUnit = _boardUnits.Find(unit => unit.hex.Equals(to));
      if (existingUnit != null) {
        Debug.Log("Trying to move on top of another unit failed, they werent feeling frisky");
        return;
      }
      var alreadyMoved = _turn.unitsMoved.Find(coord => coord.Equals(from));
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
      CalculateEnergyGain();
      EmitStateChanged();
    }

    public void SkipTurn(int player) {
      if (player != _turn.playerNum) {
        Debug.Log("You cant skip another player turn");
        return;
      }
      Debug.Log(String.Format("Player {0} skipping turn", _turn.playerNum));
      NextTurn();
    }

    public void Attack(Hex from, Hex to) {
      Debug.Log(String.Format("Player {0} attacking from {1} to {2} to", _turn.playerNum, from.ToString(), to.ToString()));
      var hasSummoningSickness = false;
      foreach (var summoned in _turn.unitsSummoned) {
        if (summoned.Equals(from)) {
          hasSummoningSickness = true;
          break;
        }
      }
      if (hasSummoningSickness) {
        Debug.Log("Them theres got the summoning sickness");
        return;
      }
      var fromUnitState = _boardUnits.Find(u => u.hex.Equals(from));
      if (fromUnitState == null) {
        Debug.Log("Theres... nothing there. You cant attack with nothing.");
        return;
      }
      var toUnitState = _boardUnits.Find(u => u.hex.Equals(to));
      if (toUnitState == null) {
        Debug.Log("You cant attack thin air");
        return;
      }
      var alreadyAttacked = _turn.unitsAttacked.Find(coord => coord.Equals(from));
      if (alreadyAttacked != null) {
        Debug.Log("This unit thinks one battle is enough for the time being");
        return;
      }
      var isFriendly = _boardUnits.Find(bu => bu.hex.Equals(to) && bu.playerNum == _turn.playerNum);
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

    public void Log(string log) {
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
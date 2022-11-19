using System;
using System.Collections.Generic;

namespace HexGame {
  public class GameState {
    public List<UnitState> units;
    public List<PlayerState> players;
    public TurnState turn;

    public UnitState GetSummoner(int playerNum) {
      return units.Find(unitState => unitState.playerNum == playerNum && unitState.unit.HasId(UnitId.Summoner));
    }

    public PlayerState Player() {
      return players[turn.playerNum];
    }
  }
}
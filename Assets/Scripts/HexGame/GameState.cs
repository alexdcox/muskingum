using System;
using System.Collections.Generic;

namespace HexGame {
  public class GameState {
    public List<UnitState> units;
    public List<PlayerState> players;
    public TurnState turn;

    public UnitState GetSummoner(int playerIndex) {
      return units.Find(unitState => unitState.playerIndex == playerIndex && unitState.id == UnitId.Summoner);
    }

    public PlayerState Player() {
      if (turn.playerIndex < 0 || turn.playerIndex > players.Count - 1) {
        return null;
      }
      return players[turn.playerIndex];
    }
  }
}
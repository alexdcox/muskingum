using System;
using System.Collections.Generic;

namespace HexGame {
  public class GameState {
    public List<UnitState> units;
    public List<PlayerState> players;
    public TurnState turn;
  }
}
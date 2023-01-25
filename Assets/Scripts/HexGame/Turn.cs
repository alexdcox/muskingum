using System.Collections.Generic;

namespace HexGame {
  public class TurnState {
    public List<UnitState> summoned = new();
    public int playerIndex = -1;
    public List<DoubledCoord> unitsMoved = new();
    public List<DoubledCoord> unitsAttacked = new();
    public List<DoubledCoord> unitsSummoned = new();
  }
}
using System.Collections.Generic;

namespace HexGame {
  public class TurnState {
    public int playerNum = -1;
    public List<Hex> unitsMoved = new();
    public List<Hex> unitsAttacked = new();
    public List<Hex> unitsSummoned = new();
  }
}
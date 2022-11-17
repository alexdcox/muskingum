using System.Collections.Generic;

namespace HexGame {
  public class TurnState {
    public int playerNum = -1;
    public TurnStage stage = TurnStage.Summon;
    public List<Hex> unitsMoved = new();
    public List<Hex> unitsAttacked = new();
    public List<Hex> unitsSummoned = new();
  }

  public enum TurnStage {
    Summon,
    Move,
    Attack
  }
}
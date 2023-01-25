using System.Collections.Generic;

namespace HexGame {
  public class PlayerState {
    public int index;
    public int energy;
    public int energyGain;
    public List<UnitId> draw = new();
    public List<UnitId> discard = new();
    public List<UnitId> hand = new();
  }
}
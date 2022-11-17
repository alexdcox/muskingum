namespace HexGame {
  public class PlayerState {
    public UnitCollection draw = new();
    public UnitCollection discard = new();
    public UnitCollection hand = new();
    public int energy;
    public int energyGain;
  }
}
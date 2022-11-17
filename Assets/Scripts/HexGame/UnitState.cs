namespace HexGame {
  public class UnitState {
    Unit _unit;
    public Hex hex;
    public int playerNum;
    public int remainingHealth;

    public Unit unit {
      get => _unit;
      set {
        _unit = value;
        remainingHealth = _unit.health;
      }
    }
  }
}
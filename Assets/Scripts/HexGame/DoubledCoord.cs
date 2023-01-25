using System;

namespace HexGame {
  public class DoubledCoord {
    public int col;
    public int row;

    public DoubledCoord(int col, int row) {
      this.col = col;
      this.row = row;
    }

    public static DoubledCoord QDoubledFromCube(Hex h) {
      int col = h.q;
      int row = 2 * h.r + h.q;
      return new DoubledCoord(col, row);
    }

    public Hex ToHexUsingLayout(Layout layout) {
      return layout.CoordToHex(this);
    }

    public Hex QDoubledToCube() {
      int q = col;
      int r = (row - col) / 2;
      int s = -q - r;
      return Hex.Cube(q, r, s);
    }

    public static DoubledCoord RDoubledFromCube(Hex h) {
      int col = 2 * h.q + h.r;
      int row = h.r;
      return new DoubledCoord(col, row);
    }

    public Hex RDoubledToCube() {
      int q = (col - row) / 2;
      int r = row;
      int s = -q - r;
      return Hex.Cube(q, r, s);
    }

    public override bool Equals(object obj) => this.Equals(obj as DoubledCoord);

    public bool Equals(DoubledCoord o) {
        if (o is null) {
            return false;
        }

        if (Object.ReferenceEquals(this, o)) {
            return true;
        }

        if (this.GetType() != o.GetType()) {
            return false;
        }

        return (row == o.row) && (col == o.col);
    }
    
    public static bool operator ==(DoubledCoord a, DoubledCoord b) {
        return a.Equals(b);
    }

    public static bool operator !=(DoubledCoord a, DoubledCoord b) {
        return !(a == b);
    }

    public override int GetHashCode() => (col, row).GetHashCode();
  }
}
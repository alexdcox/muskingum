using UnityEngine;

namespace HexGame {
  public class OffsetCoord {
    int col;
    int row;

    static int EVEN = 1;
    static int ODD = -1;

    OffsetCoord(int col, int row) {
      this.col = col;
      this.row = row;
    }

    public static OffsetCoord QOffsetFromCube(int offset, Hex h) {
      int col = h.q;
      int row = h.r + (h.q + offset * (h.q & 1)) / 2;
      if (offset != EVEN && offset != ODD) {
        Debug.LogError("offset must be EVEN (+1) or ODD (-1)");
        return null;
      }
      return new OffsetCoord(col, row);
    }

    public static Hex QOffsetToCube(int offset, OffsetCoord h) {
      int q = h.col;
      int r = h.row - (h.col + offset * (h.col & 1)) / 2;
      int s = -q - r;
      if (offset != EVEN && offset != ODD) {
        Debug.LogError("offset must be EVEN (+1) or ODD (-1)");
        return null;
      }
      return Hex.Cube(q, r, s);
    }

    public static OffsetCoord ROffsetFromCube(int offset, Hex h) {
      int col = h.q + (h.r + offset * (h.r & 1)) / 2;
      int row = h.r;
      if (offset != EVEN && offset != ODD) {
        Debug.LogError("offset must be EVEN (+1) or ODD (-1)");
        return null;
      }
      return new OffsetCoord(col, row);
    }

    public static Hex ROffsetToCube(int offset, OffsetCoord h) {
      var q = h.col - (h.row + offset * (h.row & 1)) / 2;
      var r = h.row;
      var s = -q - r;
      if (offset != EVEN && offset != ODD) {
        Debug.LogError("offset must be EVEN (+1) or ODD (-1)");
      }
      return Hex.Cube(q, r, s);
    }
  }
}
using System.Collections.Generic;
using System;
using UnityEngine;

namespace HexGame {
  public class Layout {
    public Orientation orientation;
    public Vector2 size;
    public Vector2 origin;
    public float spacing = .05f;

    public Layout(
        Orientation orientation,
        Vector2 size,
        Vector2 origin
    ) {
      this.orientation = orientation;
      this.size = size;
      this.origin = origin;
    }

    public Vector2 HexToPixel(Hex h) {
      var M = orientation;
      var x = (M.f0 * h.q + M.f1 * h.r) * size.x;
      var y = (M.f2 * h.q + M.f3 * h.r) * size.y;
      return new Vector2(x + origin.x, y + origin.y);
    }

    public Hex PixelToHex(Vector2 p) {
      Orientation M = orientation;
      var pt = new Vector2((p.x - origin.x) / size.x, (p.y - origin.y) / size.y);
      int q = Mathf.RoundToInt(M.b0 * pt.x + M.b1 * pt.y);
      int r = Mathf.RoundToInt(M.b2 * pt.x + M.b3 * pt.y);
      return Hex.Cube(q, r, -q - r);
    }

    public Vector2 HexCornerOffset(int corner) {
      var M = orientation;
      var angle = 2f * Mathf.PI * (M.start_angle - corner) / 6f;
      return new Vector2(size.x * Mathf.Cos(angle) * (1 - spacing), size.y * Mathf.Sin(angle) * (1 - spacing));
    }

    public Vector3[] PolygonCorners(Hex h) {
      var corners = new List<Vector3>();
      Vector2 center = HexToPixel(h);
      for (int i = 0; i < 6; i++) {
        Vector2 offset = HexCornerOffset(i);
        corners.Add(
            new Vector3(
                center.x + offset.x,
                center.y + offset.y,
                0
            )
        );
      }
      return corners.ToArray();
    }

    public Hex CoordToHex(DoubledCoord doubled) {
      return orientation == Orientation.Pointy ? 
        doubled.RDoubledToCube() :
        doubled.QDoubledToCube();
    }

    public Hex CoordToHex(OffsetCoord offset) {
      // TODO: FIX
      // return _layout.orientation == Orientation.Pointy ? 
      //   offset. :
      //   doubled.QDoubledToCube();
      return null;
    }

    public DoubledCoord HexToDoubledCoord(Hex hex) {
      return orientation == Orientation.Pointy ? 
        DoubledCoord.RDoubledFromCube(hex) :
        DoubledCoord.QDoubledFromCube(hex);
    }

    public OffsetCoord HexToOffsetCoord(Hex hex) {
      // return _layout.orientation == Orientation.Pointy ? 
      //   OffsetCoord.RDoubledFromCube(hex) :
      //   OffsetCoord.QDoubledFromCube(hex);
      // TODO: Fix me!
      return null;
    }
  }
}
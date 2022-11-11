using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace HT {

    public class HexDimensions {
        float _derivedSideLength = 0;
        public float Apothem {
            get => Mathf.Sqrt(3) / 2 * _derivedSideLength;
            set => _derivedSideLength = 2 * value / 3 * Mathf.Sqrt(3);
        }
        public float Area {
            get => 3 * Mathf.Sqrt(3) / 2 * Mathf.Pow(_derivedSideLength, 2);
            set => _derivedSideLength = Mathf.Sqrt(2 * value) * Mathf.Sqrt(Mathf.Sqrt(3)) / 3;
        }
        public float Perimeter {
            get => _derivedSideLength * 6;
            set => _derivedSideLength = value / 6;
        }
        public float VertexToVertex {
            get => _derivedSideLength * 2;
            set => _derivedSideLength = value / 2;
        }
        public float CenterToVertex {
            get => _derivedSideLength;
            set => _derivedSideLength = value;
        }
        public float Side {
            get => _derivedSideLength;
            set => _derivedSideLength = value;
        }
        public float SideToSide {
            get => Apothem * 2;
            set => Apothem = value / 2;
        }
    }

    public class Hex {
        public int q;
        public int r;
        public int s;

        public static Vector3[] Directions = {
            new(1, 0, -1),
            new(1, -1, 0),
            new(0, -1, 1),
            new(-1, 0, 1),
            new(-1, 1, 0),
            new(0, 1, -1),
        };

        public static Vector3[] Diagonals = {
            new(2, -1, -1),
            new(1, -2, 1),
            new(-1, -1, 2),
            new(-2, 1, 1),
            new(-1, 2, -1),
            new(1, 1, -2),
        };

        Hex (int q, int r, int s) {
            if (Mathf.Round(q + r + s) != 0) {
                Debug.LogError("q + r + s must be 0");
                return;
            }
            this.q = q;
            this.r = r;
            this.s = s;
        }

        public static Hex Cube (int q, int r, int s) {
            return new Hex(q, r, s);
        }

        public static Hex Axial (int q, int r) {
            int s = -q - r;
            return new Hex(q, r, s);
        }

        public new string ToString() {
            return q + "," + r + "," + s;
        }

        public bool Equals (Hex other) {
            if (other == null) {
                return false;
            }
            return q == other.q && r == other.r && s == other.s;
        }

        public static Hex operator + (Hex a, Hex b) {
            return new Hex(a.q + b.q, a.r + b.r, a.s + b.s);
        }

        public static Hex operator + (Hex a, Vector3 b) {
            return new Hex(
                a.q + Mathf.RoundToInt(b.x),
                a.r + Mathf.RoundToInt(b.y),
                a.s + Mathf.RoundToInt(b.z)
            );
        }

        public Hex Add (Hex b) {
            return this + b;
        }

        public Hex Add (Vector3 b) {
            return this + b;
        }

        public static Hex operator - (Hex a, Hex b) {
            return new Hex(a.q - b.q, a.r - b.r, a.s - b.s);
        }

        public Hex Subtract (Hex b) {
            return this - b;
        }

        public Hex Scale (int k) {
            return new Hex(q * k, r * k, s * k);
        }

        public Hex RotateLeft() {
            return new Hex(-s, -q, -r);
        }

        public Hex RotateRight() {
            return new Hex(-r, -s, -q);
        }

        public Hex Neighbor (int direction) {
            return Add(Directions[direction]);
        }

        public Hex DiagonalNeighbor (int direction) {
            return Add(Hex.Diagonals[direction]);
        }

        public int Len() {
            return (Mathf.Abs(q) + Mathf.Abs(r) + Mathf.Abs(s)) / 2;
        }

        public int Distance (Hex b) {
            return Subtract(b).Len();
        }

        // TODO: Not sure what this was originally for?!
        // public Hex Round() {
        //   var qi = Math.round(q);
        //   var ri = Math.round(r);
        //   var si = Math.round(s);
        //   var q_diff = Math.abs(qi - _q);
        //   var r_diff = Math.abs(ri - _r);
        //   var s_diff = Math.abs(si - _s);
        //   if (q_diff > r_diff && q_diff > s_diff) {
        //     qi = -ri - si;
        //   } else if (r_diff > s_diff) {
        //     ri = -qi - si;
        //   } else {
        //     si = -qi - ri;
        //   }
        //   return new Hex(qi, ri, si);
        // }

        public Hex Lerp (Hex b, float t) {
            return new Hex(
                Mathf.RoundToInt(q * (1f - t) + b.q * t),
                Mathf.RoundToInt(r * (1f - t) + b.r * t),
                Mathf.RoundToInt(s * (1f - t) + b.s * t)
            );
        }

        // public Hex[] Linedraw (Hex b) {
        //     var N = Distance(b);
        //     var a_nudge = new Hex(_q + 1e-06, _r + 1e-06, _s - 2e-06);
        //     var b_nudge = new Hex(b.q + 1e-06, b.r + 1e-06, b.s - 2e-06);
        //     var results = Array.Empty<Hex>();
        //     var step = 1.0 / Mathf.Max(N, 1);
        //     for (var i = 0; i <= N; i++) {
        //         results += a_nudge.Lerp(b_nudge, step * i).Round();
        //     }
        //     return results;
        // }
    }

    public class OffsetCoord {
        int col;
        int row;

        static int EVEN = 1;
        static int ODD = -1;

        OffsetCoord (int col, int row) {
            this.col = col;
            this.row = row;
        }

        static OffsetCoord QOffsetFromCube (int offset, Hex h) {
            int col = h.q;
            int row = h.r + (h.q + offset * (h.q & 1)) / 2;
            if (offset != EVEN && offset != ODD) {
                Debug.LogError("offset must be EVEN (+1) or ODD (-1)");
                return null;
            }
            return new OffsetCoord(col, row);
        }

        static Hex QOffsetToCube (int offset, OffsetCoord h) {
            int q = h.col;
            int r = h.row - (h.col + offset * (h.col & 1)) / 2;
            int s = -q - r;
            if (offset != EVEN && offset != ODD) {
                Debug.LogError("offset must be EVEN (+1) or ODD (-1)");
                return null;
            }
            return Hex.Cube(q, r, s);
        }

        static OffsetCoord ROffsetFromCube (int offset, Hex h) {
            int col = h.q + (h.r + offset * (h.r & 1)) / 2;
            int row = h.r;
            if (offset != EVEN && offset != ODD) {
                Debug.LogError("offset must be EVEN (+1) or ODD (-1)");
                return null;
            }
            return new OffsetCoord(col, row);
        }

        static Hex ROffsetToCube (int offset, OffsetCoord h) {
            var q = h.col - (h.row + offset * (h.row & 1)) / 2;
            var r = h.row;
            var s = -q - r;
            if (offset != EVEN && offset != ODD) {
                Debug.LogError("offset must be EVEN (+1) or ODD (-1)");
            }
            return Hex.Cube(q, r, s);
        }
    }

    class DoubledCoord {
        public int col;
        public int row;

        DoubledCoord (int col, int row) {
            this.col = col;
            this.row = row;
        }

        public bool Equals (DoubledCoord o) {
            if (o == null) {
                return false;
            }
            return col == o.col && row == o.row;
        }

        static DoubledCoord QDoubledFromCube (Hex h) {
            int col = h.q;
            int row = 2 * h.r + h.q;
            return new DoubledCoord(col, row);
        }

        public Hex QDoubledToCube() {
            int q = col;
            int r = (row - col) / 2;
            int s = -q - r;
            return Hex.Cube(q, r, s);
        }

        static DoubledCoord RDoubledFromCube (Hex h) {
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
    }

    public class Orientation {
        public static Orientation Pointy = new Orientation(
            Mathf.Sqrt(3f),
            Mathf.Sqrt(3f) / 2f,
            0f,
            3f / 2f,
            Mathf.Sqrt(3f) / 3f,
            -1f / 3f,
            0f,
            2f / 3f,
            0.5f
        );

        public static Orientation Flat = new Orientation(
            3f / 2,
            0f,
            Mathf.Sqrt(3f) / 2f,
            Mathf.Sqrt(3f),
            2f / 3f,
            0f,
            -1f / 3f,
            Mathf.Sqrt(3f) / 3f,
            0f
        );

        public float f0;
        public float f1;
        public float f2;
        public float f3;
        public float b0;
        public float b1;
        public float b2;
        public float b3;
        public float start_angle;

        Orientation (
            float f0,
            float f1,
            float f2,
            float f3,
            float b0,
            float b1,
            float b2,
            float b3,
            float start_angle
        ) {
            this.f0 = f0;
            this.f1 = f1;
            this.f2 = f2;
            this.f3 = f3;
            this.b0 = b0;
            this.b1 = b1;
            this.b2 = b2;
            this.b3 = b3;
            this.start_angle = start_angle;
        }
    }

    public class Layout {
        public Orientation orientation;
        public Vector2 size;
        public Vector2 origin;
        public float spacing = .05f;

        public Layout (
            Orientation orientation,
            Vector2 size,
            Vector2 origin
        ) {
            this.orientation = orientation;
            this.size = size;
            this.origin = origin;
        }

        public Vector2 HexToPixel (Hex h) {
            var M = orientation;
            var x = (M.f0 * h.q + M.f1 * h.r) * size.x;
            var y = (M.f2 * h.q + M.f3 * h.r) * size.y;
            return new Vector2(x + origin.x, y + origin.y);
        }

        public Hex PixelToHex (Vector2 p) {
            Orientation M = orientation;
            var pt = new Vector2((p.x - origin.x) / size.x, (p.y - origin.y) / size.y);
            float q = M.b0 * pt.x + M.b1 * pt.y;
            float r = M.b2 * pt.x + M.b3 * pt.y;
            return Hex.Cube(
                Mathf.RoundToInt(q),
                Mathf.RoundToInt(r),
                Mathf.RoundToInt(-q - r)
            );
        }

        public Vector2 HexCornerOffset (int corner) {
            var M = orientation;
            var angle = 2f * Mathf.PI * (M.start_angle - corner) / 6f;
            return new Vector2(size.x * Mathf.Cos(angle) * (1 - spacing), size.y * Mathf.Sin(angle) * (1 - spacing));
        }

        public Vector3[] PolygonCorners (Hex h) {
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
    }

}

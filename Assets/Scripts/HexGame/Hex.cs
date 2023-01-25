using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HexGame {
  public class Hex {
    public int q;
    public int r;
    public int s;

    public static Vector3Int[] Directions = {
      new(1, 0, -1),
      new(1, -1, 0),
      new(0, -1, 1),
      new(-1, 0, 1),
      new(-1, 1, 0),
      new(0, 1, -1),
    };

    public static Vector3Int[] Diagonals = {
      new(2, -1, -1),
      new(1, -2, 1),
      new(-1, -1, 2),
      new(-2, 1, 1),
      new(-1, 2, -1),
      new(1, 1, -2),
    };

    Hex(int q, int r, int s) {
      if (Mathf.Round(q + r + s) != 0) {
        Debug.LogError("q + r + s must be 0");
        return;
      }
      this.q = q;
      this.r = r;
      this.s = s;
    }

    public static Hex Cube(int q, int r, int s) {
      return new Hex(q, r, s);
    }

    public static Hex Axial(int q, int r) {
      int s = -q - r;
      return new Hex(q, r, s);
    }

    public new string ToString() {
      return q + "," + r;
    }

    public bool Equals(Hex o) {
        return (q == o.q) && (r == o.r);
    }
    
    public static Hex operator +(Hex a, Hex b) {
      return new Hex(a.q + b.q, a.r + b.r, a.s + b.s);
    }

    public static Hex operator +(Hex a, Vector3 b) {
      return new Hex(
          a.q + Mathf.RoundToInt(b.x),
          a.r + Mathf.RoundToInt(b.y),
          a.s + Mathf.RoundToInt(b.z)
      );
    }

    public Hex Add(Hex b) {
      return this + b;
    }

    public Hex Add(Vector3 b) {
      return this + b;
    }

    public static Hex operator -(Hex a, Hex b) {
      return new Hex(a.q - b.q, a.r - b.r, a.s - b.s);
    }

    public Hex Subtract(Hex b) {
      return this - b;
    }

    public Hex Scale(int k) {
      return new Hex(q * k, r * k, s * k);
    }

    public Hex RotateLeft() {
      return new Hex(-s, -q, -r);
    }

    public Hex RotateRight() {
      return new Hex(-r, -s, -q);
    }

    public Hex Neighbor(int direction) {
      return Add(Directions[direction]);
    }

    public List<Hex> Neighbors() {
      List<Hex> neighbours = new();
      foreach(var direction in Directions) {
        neighbours.Add(Add(new Hex(direction.x, direction.y, direction.z)));
      }
      return neighbours;
    }

    public Hex DiagonalNeighbor(int direction) {
      return Add(Hex.Diagonals[direction]);
    }

    public int Len() {
      return (Mathf.Abs(q) + Mathf.Abs(r) + Mathf.Abs(s)) / 2;
    }

    public int Distance(Hex b) {
      return Subtract(b).Len();
    }

    public Hex Lerp(Hex b, float t) {
      return new Hex(
          Mathf.RoundToInt(q * (1f - t) + b.q * t),
          Mathf.RoundToInt(r * (1f - t) + b.r * t),
          Mathf.RoundToInt(s * (1f - t) + b.s * t)
      );
    }
  }
}
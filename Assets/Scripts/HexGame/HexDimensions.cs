using UnityEngine;

namespace HexGame {
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
}
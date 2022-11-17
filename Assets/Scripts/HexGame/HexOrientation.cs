using UnityEngine;

namespace HexGame {
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

    Orientation(
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
}
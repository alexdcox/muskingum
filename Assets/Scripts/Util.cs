using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util : MonoBehaviour {
  public static Rect GetRectTransformRect(RectTransform rect) {
      Vector3[] corners = new Vector3[4];
      // 0 is bottom-left, it works around anti-clockwise.
      rect.GetWorldCorners(corners);
      return new Rect(
        corners[0].x,
        corners[0].y,
        corners[2].x - corners[1].x,
        corners[1].y - corners[0].y
      );
  }

  public static void GizmoDrawRect(RectTransform rt, Color color) {
    Rect rect = GetRectTransformRect(rt);
    GizmoDrawRect(rt.parent, rect, color);
  }

  public static void GizmoDrawRect(Transform parent, Rect rect, Color color) {
    Gizmos.color = color;
    var z = parent.position.z;
    Gizmos.DrawLine(new Vector3(rect.xMin, rect.yMax, z), new Vector3(rect.xMax, rect.yMax, z));
    Gizmos.DrawLine(new Vector3(rect.xMax, rect.yMax, z), new Vector3(rect.xMax, rect.yMin, z));
    Gizmos.DrawLine(new Vector3(rect.xMax, rect.yMin, z), new Vector3(rect.xMin, rect.yMin, z));
    Gizmos.DrawLine(new Vector3(rect.xMin, rect.yMin, z), new Vector3(rect.xMin, rect.yMax, z));
  }

}

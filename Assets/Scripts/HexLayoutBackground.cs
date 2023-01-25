using System.Collections.Generic;
using HexGame;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

public class HexLayoutBackground : MonoBehaviour {
  public Color colorLight = new Color32(11, 11, 11, 255);
  public Color colorDark = new Color32(6, 6, 6, 255);
  public Shader blockColorShader;

  private HexDimensions _hexDimensions;
  private Rect _hexRect;
  private bool _hasInit;

  private void Update() {
    if (transform == null)
      return;
    if (_hasInit) {
      return;
    }
    if (!_hasInit) {
      _hasInit = true;
      InstantiateBackgroundHexagons();
    }
  }

  void InstantiateBackgroundHexagons() {
    if (transform == null || transform.parent == null) {
      return;
    }

    var position = transform.position;

    var rectTransform = GetComponent<RectTransform>();
    if (rectTransform == null) {
      return;
    }

    var rect = Util.GetRectTransformRect(rectTransform);
    if (rect.width == 0 || rect.height == 0) {
      return;
    }

    Vector3 topLeft = new Vector3(rect.xMin, rect.yMax, position.z);
    Vector3 topRight = new Vector3(rect.xMax, rect.yMax, position.z);
    Vector3 bottomRight = new Vector3(rect.xMax, rect.yMin, position.z);
    Vector3 bottomLeft = new Vector3(rect.xMin, rect.yMin, position.z);

    float desiredVerticalHexagons = 5.6f;

    _hexDimensions = new HexDimensions() { SideToSide = rect.height / desiredVerticalHexagons };

    float size = size = _hexDimensions.CenterToVertex;
    Layout layout = new Layout(Orientation.Flat, new Vector2(size, size), topLeft);
    layout.spacing = 0.00f;

    Color color = Color.yellow;
    
    GameObject DrawHex(Hex hex) {
      var triangles = new List<int>();
      var triangleCount = 0;
      var verticies = new List<Vector3>();
      var uv = new List<Vector2>();
      var hexCorners = layout.PolygonCorners(hex);
      Vector3 midpoint = layout.HexToPixel(hex);
      midpoint += position;

      var hexHeight = _hexDimensions.VertexToVertex;
      var hexWidth = _hexDimensions.Side * 2;

      Vector2 uvBottomLeft = (Vector2)midpoint - new Vector2(hexWidth / 2, hexHeight / 2);
      Vector2 uvTopRight = (Vector2)midpoint + new Vector2(hexWidth / 2, hexHeight / 2);
      Vector2 uvSize = new Vector2(uvTopRight.x - uvBottomLeft.x, uvTopRight.y - uvBottomLeft.y);

      for (var i = 0; i < 6; i++) {
        Vector3 from = (i == 0 ? hexCorners[5] : hexCorners[i - 1]) + position;
        Vector3 to = hexCorners[i] + position;
        verticies.Add(midpoint);
        verticies.Add(from);
        verticies.Add(to);
        triangles.Add(triangleCount++);
        triangles.Add(triangleCount++);
        triangles.Add(triangleCount++);

        var uvMidpoint = ((Vector2)midpoint + (uvSize / 2)) / uvSize;
        var uvFrom = ((Vector2)from + (uvSize / 2)) / uvSize;
        var uvTo = ((Vector2)to + (uvSize / 2)) / uvSize;

        uv.Add(uvMidpoint);
        uv.Add(uvFrom);
        uv.Add(uvTo);
      }
      GameObject go = new GameObject("Hex " + hex.ToString()) {
        transform = {
          parent = transform,
        }
      };
      Mesh mesh = new Mesh() {
        vertices = verticies.ToArray(),
        triangles = triangles.ToArray(),
        uv = uv.ToArray()
      };
      mesh.RecalculateUVDistributionMetrics();
      mesh.RecalculateNormals();
      mesh.RecalculateTangents();
      go.AddComponent<MeshFilter>().mesh = mesh;

      var material = new Material(blockColorShader);
      material.SetColor("_Color", color);
      var meshRenderer = go.AddComponent<MeshRenderer>();
      meshRenderer.material = material;
      
      go.AddComponent<MeshCollider>();

      return go;
    }

    bool IsHexVisible(Hex hex) {
      Vector3[] corners = layout.PolygonCorners(hex);
      float xMin = corners[0].x, xMax = corners[0].x, yMin = corners[0].y, yMax = corners[0].y;
      foreach (Vector3 corner in corners) {
        xMin = Math.Min(xMin, corner.x);
        xMax = Math.Max(xMax, corner.x);
        yMin = Math.Min(yMin, corner.y);
        yMax = Math.Max(yMax, corner.y);
      }

      var width = xMax - xMin;
      var height = yMax - yMin;

      // TODO: Could refactor this to use a "get hex rect" function or something along those lines.
      Rect hexRect = new Rect(xMin, yMin, width, height);
      return rect.Overlaps(hexRect);
    }

    var bottomRightDirection = 1;
    var downDirection = 2;

    int[] directionPattern = new int[] {
      downDirection, // 0
      bottomRightDirection, // 1
      bottomRightDirection, // 2
      downDirection, // 3
      bottomRightDirection, // 4
    };
    var currentDirectionIndex = 0;

    var infiniteLoopEscaper = 0;
    var infiniteEscapeAt = 100;

    Hex[] starters = new Hex[] {
      Hex.Axial(0, 0),
      Hex.Axial(1, -1),
      Hex.Axial(2, -1),
      Hex.Axial(4, -2),
      Hex.Axial(6, -3),
      Hex.Axial(7, -4),
      Hex.Axial(8, -4),
      Hex.Axial(10, -5),
      Hex.Axial(11, -6),
      Hex.Axial(0, -5),
      Hex.Axial(0, -3),
      Hex.Axial(0, -2),
    };

    int[] patternage = new int[] {
      0,
      1,
      1,
      2,
      3,
      4,
      4,
      0,
      1,
      0,
      3,
      2,
    };

    var currentHexIndex = 0;
    foreach (Hex hex in starters) {
      color = colorDark;
      if (currentHexIndex % 2 == 0) {
        color = colorLight;
      }
      Hex currentHex = hex;
      currentDirectionIndex = patternage[currentHexIndex];
      currentHexIndex++;
      while(IsHexVisible(currentHex)) {
        DrawHex(currentHex);
        currentHex = currentHex.Neighbor(directionPattern[currentDirectionIndex]);
        currentDirectionIndex = (currentDirectionIndex + 1) % 5;
        infiniteLoopEscaper++;
        if (infiniteLoopEscaper > infiniteEscapeAt) {
          return;
        }
      }
    }
  }
}

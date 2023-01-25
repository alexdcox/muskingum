using System.Collections.Generic;
using HexGame;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

public delegate void LayoutCompleted();

public class HexLayout2 : MonoBehaviour {
  public enum Type {
    Board,
    Hand,
    LeftDeck,
    RightDeck,
  };

  public Type type = Type.Board;
  public Color backgroundHexColor = new Color32(36, 36, 36, 255);

  public bool debugShowHexes;
  public bool debugShowMidpoint;
  public bool debugShowMargins;
  public bool debugShowHexBorder;

  public GameController gameController;

  private GameObject unitPrefab;
  private Shader blockColorShader;
  private Shader newTryShader;

  [HideInInspector] public Layout layout;
  [HideInInspector] public Vector3 centerTranslate;

  private HexDimensions _hexDimensions;
  private Rect _hexRect;
  private Rect _innerRect;
  private bool _hasInit;

  private List<GameObject> _unitGameObjects = new();
  private List<GameObject> _backgroundHexes = new();

  public event LayoutCompleted LayoutCompleted;

  public class LayoutKind {
    public static readonly Vector2Int[] Board = {
      new(-3, 0), new(-2, 0), new(1, 0), new(2, 0), new(3, 0), new(0, 0),
      new(-3, 1), new(-2, 1), new(-1, 1), new(0, 1), new(1, 1), new(2, 1),
      new(-3, 2), new(-2, 2), new(-1, 2), new(0, 2), new(1, 2), new(0, -1),
      new(-1, -1), new(-2, -1), new(1, -1), new(2, -1), new(3, -1), new(0, -2),
      new(-1, -2), new(1, -2), new(2, -2), new(3, -2), new(-1, 0)
    };
    public static readonly Vector2Int[] Hand = {
      new(0, 0), new(0, 1), new(1, 0), new(1, 1), new(2, 0),
    };
    public static readonly Vector2Int[] LeftDeck = {
      new(0, 0),  new(1, 0),  new(1, -1),  new(2, -1),  new(1, -2),  new(2, -2),
      new(2, -3), new(3, -3), new(2, -4),  new(3, -4),  new(3, -5),  new(4, -5),
      new(3, -6), new(4, -6), new(4, -7),  new(5, -7),  new(4, -8),  new(5, -8),
      new(5, -9), new(6, -9), new(5, -10), new(6, -10), new(6, -11), new(7, -11),
    };
    public static readonly Vector2Int[] RightDeck = {
      new(0, 0),  new(1, 0),  new(0, -1),  new(1, -1),  new(1, -2),  new(2, -2),
      new(1, -3), new(2, -3), new(2, -4),  new(3, -4),  new(2, -5),  new(3, -5),
      new(3, -6), new(4, -6), new(3, -7),  new(4, -7),  new(4, -8),  new(5, -8),
      new(4, -9), new(5, -9), new(5, -10), new(6, -10), new(5, -11), new(6, -11),
    };
  }

  void Start() {
    blockColorShader = gameController.blockColorShader;
    newTryShader = gameController.newTryShader;
  }

  void Awake() {
    if (transform == null)
      return;
  }

  private void Update() {
    if (transform == null)
      return;
    if (_hasInit) {
      return;
    }
    bool ok = Layout();
    if (!_hasInit && ok) {
      _hasInit = true;
      InstantiateBackgroundHexagons();
      LayoutCompleted?.Invoke();
    }
  }

  public bool HasInit() {
    return _hasInit;
  }

  void InstantiateBackgroundHexagons() {
    foreach (Hex hex in GetHexes()) {
      GameObject hexGameObject = CreateHexMesh(hex);
      hexGameObject.name = String.Format("Bg Hex {0}", hex.ToString());
      var material = new Material(blockColorShader);
      material.SetColor("_Color", backgroundHexColor);
      hexGameObject.GetComponent<MeshRenderer>().material = material;
      _backgroundHexes.Add(hexGameObject);
    }
  }

  public UnitRenderer SpawnUnit(UnitState unitState) {
    if (!_hasInit) {
      return null;
    }

    GameObject unitGameObject = Instantiate(gameController.unitPrefab, transform);
    unitGameObject.transform.position = (Vector3)layout.CoordToPixel(unitState.coord) + transform.position + centerTranslate + new Vector3(0, 0, -5);

    Transform hexOutline = unitGameObject.transform.Find("HexOutline");
    var hexBounds = hexOutline.GetComponent<MeshRenderer>().bounds;

    var scale = _hexDimensions.SideToSide / hexBounds.size.x;
    unitGameObject.transform.localScale *= scale;
    unitGameObject.transform.localScale *= 0.94f;

    var unitRenderer = unitGameObject.GetComponent<UnitRenderer>();
    unitRenderer.unitState = unitState;
    unitRenderer.unitDefinition = gameController.GetUnitDefinition(unitState.id);
    unitRenderer.Render();

    _unitGameObjects.Add(unitGameObject);

    return unitRenderer;
  }

  public void RemoveAllUnits() {
    foreach (var unitGameObject in _unitGameObjects) {
      Destroy(unitGameObject);
    }
    _unitGameObjects = new();
  }

  void OnDrawGizmos() {
    if (transform == null)
      return;

    if (!_hasInit) {
      Layout();
    }

    if (debugShowMargins) {
      Util.GizmoDrawRect(transform, _innerRect, Color.magenta);
    }

    if (debugShowHexBorder) {
      Rect r = _hexRect;
      r.center += (Vector2)transform.position;
      Util.GizmoDrawRect(transform, r, Color.yellow);
    }

    if (debugShowHexes) {
      Gizmos.color = Color.green;
      var position = transform.position;
      foreach (Hex hex in GetHexes()) {
        var hexCorners = layout.PolygonCorners(hex);
        Vector3 midpoint = layout.HexToPixel(hex);
        midpoint += position + centerTranslate;
        for (var i = 0; i < 6; i++) {
          Vector3 from = (i == 0 ? hexCorners[5] : hexCorners[i - 1]) + position + centerTranslate;
          Vector3 to = hexCorners[i] + position + centerTranslate;
          Gizmos.DrawLine(from, to);
          if (debugShowMidpoint) {
            Gizmos.DrawLine(midpoint, to);
          }
        }
      }
    }
  }

  bool Layout() {
    var rectTransform = GetComponent<RectTransform>();
    if (rectTransform == null) {
      return false;
    }

    Vector3 position = rectTransform.position;

    var rect = Util.GetRectTransformRect(rectTransform);

    // NOTE: We can reach this point before the RectTransform has actually sized itself, so we need
    //       to return false and wait for the corners to exist.
    if (rect.width == 0 || rect.height == 0) {
      return false;
    }

    float marginPercent = 1.2f;
    float margin = (rect.width < rect.height ? rect.width : rect.height) * marginPercent / 100f;

    _innerRect = new Rect(
        rect.x + margin,
        rect.y + margin,
        rect.width - (2 * margin),
        rect.height - (2 * margin)
    );

    switch (type) {
      case Type.LeftDeck:
      case Type.RightDeck: {
          _hexDimensions = new HexDimensions() { VertexToVertex = 1f };
          float ratio = (_hexDimensions.VertexToVertex * 6) / ((_hexDimensions.VertexToVertex * 6) + (_hexDimensions.Side * 6.5f));
          var v2vAllocated = _innerRect.height * ratio;
          _hexDimensions.VertexToVertex = v2vAllocated / 6;
          break;
        }
      case Type.Board:
        _hexDimensions = new HexDimensions { Apothem = _innerRect.width / 7 / 2 };
        if ((_hexDimensions.VertexToVertex * 3) + (_hexDimensions.Side * 2) > _innerRect.height) {
          var ratio = (_hexDimensions.VertexToVertex * 3) / ((_hexDimensions.VertexToVertex * 3) + (_hexDimensions.Side * 2));
          var v2vAllocated = _innerRect.height * ratio;
          _hexDimensions.VertexToVertex = v2vAllocated / 3;

        }
        break;
      case Type.Hand:
        _hexDimensions = new HexDimensions { Apothem = _innerRect.width / 3 / 2 };
        if ((_hexDimensions.VertexToVertex * 1) + (_hexDimensions.Side * 1.5f) > _innerRect.height) {
          var ratio = (_hexDimensions.VertexToVertex * 1) / ((_hexDimensions.VertexToVertex * 1) + (_hexDimensions.Side * 1.5f));
          var v2vAllocated = _innerRect.height * ratio;
          _hexDimensions.VertexToVertex = v2vAllocated / 1;
        }
        break;
    }

    float size = size = _hexDimensions.CenterToVertex;
    layout = new Layout(Orientation.Pointy, new Vector2(size, size), new Vector2(0, 0));

    // Find Bounds and Center
    // The hexagons are already sized, we just need to translate from the 0,0 tile to the 0,0 local position.

    _hexRect = new Rect();

    foreach (Hex hex in GetHexes()) {
      var p = layout.HexToPixel(hex);
      if (p.x < _hexRect.xMin) {
        _hexRect.xMin = p.x;
      }
      if (p.x > _hexRect.xMax) {
        _hexRect.xMax = p.x;
      }
      if (p.y > _hexRect.yMax) {
        _hexRect.yMax = p.y;
      }
      if (p.y < _hexRect.yMin) {
        _hexRect.yMin = p.y;
      }
    }

    // Compensate for the distance between the midpoint of a hex to the edge of the bounding rect.
    _hexRect.yMax += _hexDimensions.CenterToVertex;
    _hexRect.yMin -= _hexDimensions.CenterToVertex;
    _hexRect.xMin -= _hexDimensions.Apothem;
    _hexRect.xMax += _hexDimensions.Apothem;

    centerTranslate = new Vector3();

    // Shift the bounding hex bottom left to match the parent rect midpoint.
    Vector3 hexBottomLeft = new Vector3(_hexRect.xMin, _hexRect.yMin, 0) + position;
    centerTranslate = position - hexBottomLeft;
    _hexRect.center += (Vector2)centerTranslate;

    // Shift the bounding hex from 0,0 to centered within the parent.
    Vector3 halfHeightWidth = new Vector3(_hexRect.width / 2, _hexRect.height / 2, 0);
    centerTranslate -= halfHeightWidth;
    _hexRect.center -= (Vector2)halfHeightWidth;

    return true;
  }

  public Vector2Int[] GetHexCoords() {
    Vector2Int[] hexCoords = { };

    switch (type) {
      case Type.Board:
        hexCoords = LayoutKind.Board;
        break;
      case Type.Hand:
        hexCoords = LayoutKind.Hand;
        break;
      case Type.LeftDeck:
        hexCoords = LayoutKind.LeftDeck;
        break;
      case Type.RightDeck:
        hexCoords = LayoutKind.RightDeck;
        break;
    }

    return hexCoords;
  }

  public List<Hex> GetHexes() {
    List<Hex> hexes = new();
    foreach (var coord in GetHexCoords()) {
      hexes.Add(Hex.Axial(coord.x, coord.y));
    }
    return hexes;
  }

  public bool HasHex(Hex hex) {
    return GetHexes().Any(h => h.Equals(hex));
  }

  GameObject CreateHexMesh(Hex hex) {
    if (layout == null) {
      return null;
    }
    var triangles = new List<int>();
    var triangleCount = 0;
    var verticies = new List<Vector3>();
    var uv = new List<Vector2>();
    var hexCorners = layout.PolygonCorners(Hex.Cube(0, 0, 0));
    Vector3 midpoint = layout.HexToPixel(Hex.Cube(0, 0, 0));

    var hexHeight = _hexDimensions.VertexToVertex;
    var hexWidth = _hexDimensions.Side * 2;

    Vector2 uvBottomLeft = (Vector2)midpoint - new Vector2(hexWidth / 2, hexHeight / 2);
    Vector2 uvTopRight = (Vector2)midpoint + new Vector2(hexWidth / 2, hexHeight / 2);
    Vector2 uvSize = new Vector2(uvTopRight.x - uvBottomLeft.x, uvTopRight.y - uvBottomLeft.y);

    for (var i = 0; i < 6; i++) {
      Vector3 from = (i == 0 ? hexCorners[5] : hexCorners[i - 1]);
      Vector3 to = hexCorners[i];
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
    GameObject go = new GameObject("SomeHexyLad") {
      transform = {
                parent = transform,
                position = transform.position + centerTranslate + (Vector3)layout.HexToPixel(hex),
                // rotation = Quaternion.Euler(0, 0, 0)
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
    go.AddComponent<MeshRenderer>().material = new Material(newTryShader);
    go.AddComponent<MeshCollider>();

    return go;
  }

  public int GetPlayerIndex() {
    if (name.Contains("P1")) {
      return 0;
    }
    if (name.Contains("P2")) {
      return 1;
    }
    return -1;
  }

  public UnitRenderer GetUnitRenderer(DoubledCoord coord) {
    return GetUnitRenderer(layout.CoordToHex(coord));
  }

  public UnitRenderer GetUnitRenderer(Hex hex) {
    foreach (var unitGO in _unitGameObjects) {
      var unitRenderer = unitGO.GetComponent<UnitRenderer>();
      var unitHex = layout.CoordToHex(unitRenderer.unitState.coord);
      if (unitHex.Equals(hex)) {
        return unitRenderer;
      }
    }
    return null;
  }

  public void SetBackgroundHexColor(Hex hex, Color color) {
    foreach (var bgHex in _backgroundHexes) {
      if (bgHex.name.Replace("Bg Hex ", "") == hex.ToString()) {
        bgHex.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
        return;
      }
    }
  }

  public void ResetBackgroundHexColor() {
    foreach (var bgHex in _backgroundHexes) {
      bgHex.GetComponent<MeshRenderer>().material.SetColor("_Color", backgroundHexColor);
    }
  }

  public bool HasUnit(Hex h) {
    return GetUnitRenderer(h) != null;
  }
}

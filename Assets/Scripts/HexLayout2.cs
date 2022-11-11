using System.Collections.Generic;
using HT;
using UnityEditor;
using UnityEngine;
using Color = UnityEngine.Color;

public class HexLayout2 : MonoBehaviour {
    Layout _layout;
    HexDimensions _hexDimensions;
    Vector3 _centerTranslate;
    Rect _hexRect;
    Rect _innerRect;

    public enum Type {
        Board,
        Hand,
        LeftDeck,
        RightDeck,
    };

    public Type type = Type.Board;

    public bool debugShowMidpoint;
    public bool debugShowMargins;
    public bool debugShowHexBorder;

    bool _hasInit;

    public class LayoutKind {
        public static readonly Vector2[] Board = {
            new(-3, 0), new(-2, 0), new(-1, 0), new(0, 0), new(1, 0),
            new(2, 0), new(3, 0), new(-2, 0), new(-1, 0), new(0, 0),
            new(1, 0), new(2, 0), new(3, 0), new(-3, 1), new(-2, 1),
            new(-1, 1), new(0, 1), new(1, 1), new(2, 1), new(-3, 2),
            new(-2, 2), new(-1, 2), new(0, 2), new(1, 2), new(0, -1),
            new(-1, -1), new(-2, -1), new(1, -1), new(2, -1), new(3, -1),
            new(0, -2), new(-1, -2), new(1, -2), new(2, -2), new(3, -2),
        };
        public static readonly Vector2[] Hand = {
            new(0, 0), new(0, 1), new(1, 0), new(1, 1), new(2, 0),
        };
        public static readonly Vector2[] LeftDeck = {
            new(0, 0),  new(1, 0),  new(1, -1),  new(2, -1),  new(1, -2),  new(2, -2),
            new(2, -3), new(3, -3), new(2, -4),  new(3, -4),  new(3, -5),  new(4, -5),
            new(3, -6), new(4, -6), new(4, -7),  new(5, -7),  new(4, -8),  new(5, -8),
            new(5, -9), new(6, -9), new(5, -10), new(6, -10), new(6, -11), new(7, -11),
        };
        public static readonly Vector2[] RightDeck = {
            new(0, 0),  new(1, 0),  new(0, -1),  new(1, -1),  new(1, -2),  new(2, -2),
            new(1, -3), new(2, -3), new(2, -4),  new(3, -4),  new(2, -5),  new(3, -5),
            new(3, -6), new(4, -6), new(3, -7),  new(4, -7),  new(4, -8),  new(5, -8),
            new(4, -9), new(5, -9), new(5, -10), new(6, -10), new(5, -11), new(6, -11),
        };
    }

    private void Awake() {
        if (transform == null)
            return; 
    }

    private void Update() {
        if (transform == null)
            return;
        if (_hasInit) {
            return;
        }
        if (Layout()) {
            _hasInit = true;
            CreateHexMesh(Hex.Axial(0, 0)); 
        }
    }

    void OnDrawGizmos() {
        if (transform == null)
            return;

        Layout();

        void drawRect(Rect rect, Color color) {
            Gizmos.color = color;
            var z = transform.position.z;
            Gizmos.DrawLine(new Vector3(rect.xMin, rect.yMax, z), new Vector3(rect.xMax, rect.yMax, z));
            Gizmos.DrawLine(new Vector3(rect.xMax, rect.yMax, z), new Vector3(rect.xMax, rect.yMin, z));
            Gizmos.DrawLine(new Vector3(rect.xMax, rect.yMin, z), new Vector3(rect.xMin, rect.yMin, z));
            Gizmos.DrawLine(new Vector3(rect.xMin, rect.yMin, z), new Vector3(rect.xMin, rect.yMax, z));
        }
        
        if (debugShowMargins) {
            drawRect(_innerRect, Color.magenta);
        }

        if (debugShowHexBorder) {
            Rect r = _hexRect;
            r.center += (Vector2)transform.position;
            drawRect(r, Color.yellow);
        }

        // Draw Hexes

        Gizmos.color = Color.green;
        var position = transform.position;

        foreach (Vector2 coord in GetHexCoords()) {
            var hex = Hex.Axial((int)coord.x, (int)coord.y);
            var hexCorners = _layout.PolygonCorners(hex);
            Vector3 midpoint = _layout.HexToPixel(hex);
            midpoint += position + _centerTranslate;
            for (var i = 0; i < 6; i++) {
                Vector3 from = (i == 0 ? hexCorners[5] : hexCorners[i - 1]) + position + _centerTranslate;
                Vector3 to = hexCorners[i] + position + _centerTranslate;
                Gizmos.DrawLine(from, to);
                if (debugShowMidpoint) {
                    Gizmos.DrawLine(midpoint, to);
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

        var rectCorners = new Vector3[4];
        rectTransform.GetWorldCorners(rectCorners); // 0 is bottom-left, it works around anti-clockwise.

        var rect = new Rect(
            rectCorners[0].x,
            rectCorners[0].y,
            rectCorners[2].x - rectCorners[1].x,
            rectCorners[1].y - rectCorners[0].y
        );

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
                _hexDimensions = new HexDimensions() {VertexToVertex = 1f };
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
        _layout = new Layout(Orientation.Pointy, new Vector2(size, size), new Vector2(0, 0));

        // Find Bounds and Center
        // The hexagons are already sized, we just need to translate from the 0,0 tile to the 0,0 local position.

        _hexRect = new Rect();

        foreach (Vector2 coord in GetHexCoords()) {
            var hex = Hex.Axial((int)coord.x, (int)coord.y);
            var p = _layout.HexToPixel(hex);
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

        _centerTranslate = new Vector3();

        // Shift the bounding hex bottom left to match the parent rect midpoint.
        Vector3 hexBottomLeft = new Vector3(_hexRect.xMin, _hexRect.yMin, 0) + position;
        _centerTranslate = position - hexBottomLeft;
        _hexRect.center += (Vector2)_centerTranslate;

        // Shift the bounding hex from 0,0 to centered within the parent.
        Vector3 halfHeightWidth = new Vector3(_hexRect.width / 2, _hexRect.height / 2, 0);
        _centerTranslate -= halfHeightWidth;
        _hexRect.center -= (Vector2)halfHeightWidth;

        return true;
    }

    Vector2[] GetHexCoords() {
        Vector2[] hexCoords = { };

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

    void CreateHexMesh (Hex hex) {
        if (_layout == null) {
            return;
        }
        var triangles = new List<int>();
        var triangleCount = 0;
        var verticies = new List<Vector3>();
        var uv = new List<Vector2>();
        var hexCorners = _layout.PolygonCorners(Hex.Cube(0, 0, 0));
        Vector3 midpoint = _layout.HexToPixel(Hex.Cube(0, 0, 0));

        var hexHeight = _hexDimensions.VertexToVertex;
        var hexWidth = _hexDimensions.SideToSide;

        Vector2 uvBottomLeft = (Vector2)midpoint - new Vector2(hexWidth / 2, hexHeight / 2);
        Vector2 uvTopRight = (Vector2)midpoint + new Vector2(hexWidth / 2, hexHeight / 2);
        Vector2 uvSize = new Vector2(uvTopRight.x - uvBottomLeft.x, uvBottomLeft.y - uvTopRight.y);

        for (var i = 0; i < 6; i++) {
            Vector3 from = (i == 0 ? hexCorners[5] : hexCorners[i - 1]);
            Vector3 to = hexCorners[i];
            verticies.Add(midpoint);
            verticies.Add(from);
            verticies.Add(to);
            triangles.Add(triangleCount++);
            triangles.Add(triangleCount++);
            triangles.Add(triangleCount++);

            // uv.Add(new Vector2(0, 0));
            // uv.Add(new Vector2(from.x / _hexDimensions.Apothem, from.y / _hexDimensions.CenterToVertex));
            // uv.Add(new Vector2(to.x / (_hexDimensions.Apothem*2), to.y / _hexDimensions.CenterToVertex));

            var uvMidpoint = ((Vector2)midpoint + (uvSize / 2)) / uvSize;
            var uvFrom = ((Vector2)from + (uvSize / 2)) / uvSize;
            var uvTo = ((Vector2)to + (uvSize / 2)) / uvSize;

            uv.Add(uvMidpoint);
            uv.Add(uvFrom);
            uv.Add(uvTo);

            Debug.Log("uvmidpoint " + uvMidpoint + " uvfrom " + uvFrom + " uvto " + uvTo);
        }
        GameObject go = new GameObject("SomeHexyLad") {
            transform = {
                parent = transform,
                position = transform.position + (Vector3)_layout.HexToPixel(hex),
                rotation = Quaternion.Euler(0, 0, 180)
            }
        };
        Mesh mesh = new Mesh() {
            vertices = verticies.ToArray(),
            triangles = triangles.ToArray(),
            uv = uv.ToArray()
        };
        // mesh.RecalculateUVDistributionMetrics();
        // mesh.RecalculateNormals();
        // mesh.RecalculateTangents();
        go.AddComponent<MeshFilter>().mesh = mesh;
        go.AddComponent<MeshRenderer>().material =
            new Material(AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/NewTry.shadergraph"));
        ;
    }

    [ContextMenu("LetsGetHexy")]
    void DrawAHexyBoi() {
        CreateHexMesh(Hex.Axial(-3, 0));
        CreateHexMesh(Hex.Axial(3, 0));
    }


}

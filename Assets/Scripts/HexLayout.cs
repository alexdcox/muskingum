using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// public class HexLayout : MonoBehaviour {
//     const float OuterRadius = 10f;
//     const float InnerRadius = OuterRadius * 8.66025404f;
//     static Vector3[] _corners = {
//         new(0f, 0f, OuterRadius),
//         new(InnerRadius, 0f, 0.5f * OuterRadius),
//         new(InnerRadius, 0f, -0.5f * OuterRadius),
//         new(0f, 0f, -OuterRadius),
//         new(-InnerRadius, 0f, -0.5f * OuterRadius),
//         new(-InnerRadius, 0f, 0.5f * OuterRadius),
//         new(0f, 0f, OuterRadius)
//     };

//     void Start() {
//         MakeLayout();
//     }

//     public class HexMesh : MonoBehaviour {
//         Mesh _hexMesh;
//         MeshFilter _meshFilter;
//         List<Vector3> _vertices;
//         List<int> _triangles;

//         public void Awake() {
//             if (_hexMesh != null) {
//                 _hexMesh.name = "Hex Mesh";
//             }
//             _vertices = new List<Vector3>();
//             _triangles = new List<int>();

//             Triangulate();

//             if (_hexMesh == null) {
//                 _hexMesh = new Mesh();
//             }

//             _hexMesh.Clear();
//             _hexMesh.vertices = _vertices.ToArray();
//             _hexMesh.triangles = _triangles.ToArray();
//             _hexMesh.RecalculateNormals();

//             _meshFilter = gameObject.AddComponent<MeshFilter>();
//             _meshFilter.sharedMesh = _hexMesh;
//         }

//         void Triangulate () {
//             Vector3 center = transform.localPosition;
//             for (int i = 0; i < 6; i++) {
//                 AddTriangle(
//                     center,
//                     center + HexMetrics.corners[i],
//                     center + HexMetrics.corners[i + 1]
//                 );
//             }
//         }

//         void AddTriangle (Vector3 v1, Vector3 v2, Vector3 v3) {
//             int vertexIndex = _vertices.Count;
//             _vertices.Add(v1);
//             _vertices.Add(v2);
//             _vertices.Add(v3);
//             _triangles.Add(vertexIndex);
//             _triangles.Add(vertexIndex + 1);
//             _triangles.Add(vertexIndex + 2);
//         }

//     }


//     void MakeLayout() {
//         int width = 1;
//         int height = 1;

//         var cells = new GameObject[height * width];

//         for (int z = 0, i = 0; z < height; z++) {
//             for (int x = 0; x < width; x++) {
//                 GameObject cell = CreateCell(x, z, i);
//                 cells[i] = cell;
//             }
//         }
//     }

//     GameObject CreateCell (int x, int z, int i) {
//         Vector3 position;
//         // position.x = (x + z * 0.5f) * (HexMetrics.innerRadius * 2f);
//         position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
//         position.y = 0f;
//         position.z = z * (HexMetrics.outerRadius * 1.5f);

//         var cell = new GameObject("Hex (" + x + ", " + z + ")") {
//             transform = {
//                 parent = transform,
//                 localPosition = new Vector3(0, 1, 0),
//                 localRotation = Quaternion.Euler(0, 0, 0),
//                 localScale = new Vector3(.3f, 1, .1f),
//             }
//         };

//         cell.AddComponent<HexMesh>();
//         var meshRenderer = cell.AddComponent<MeshRenderer>();
//         // var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/BlockColor.shadergraph");
//         var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/HexImageStroke.shadergraph");
//         var mat = new Material(shader);
//         // mat.SetColor("_Color", Color.red);
//         meshRenderer.material = mat;

//         // var unitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Unit.prefab");
//         // GameObject unitGameObject = Instantiate(unitPrefab, transform);
//         // unitGameObject.transform.parent = transform;

//         // Quaternion rot = Quaternion.Euler(0, 0, 0);
//         // Instantiate(cell, position, Quaternion.identity, transform);
//         return cell;

//         // Transform transform1;
//         // (transform1 = cell.transform).SetParent(transform, false);
//         // transform1.localPosition = position;
//         // cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
//         // cell.color = defaultColor;

//         // TMP_Text label = Instantiate(cellLabelPrefab, gridCanvas.transform, false);
//         // label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
//         // label.text = cell.coordinates.ToStringOnSeparateLines();
//     }

//     void OnDrawGizmosSelected() {
//         if (transform == null)
//             return;

//         var rectTransform = GetComponent<RectTransform>();

//         // Gizmos.DrawLine(transform.position, new Vector3(0, 0, transform.position.z));
//         // Gizmos.DrawLine(transform.position, rectTransform.anchoredPosition);

//         Gizmos.color = Color.red;
//         var position = rectTransform.position;
//         Gizmos.DrawSphere(position, 30f);

//         var corners = new Vector3[4];
//         rectTransform.GetWorldCorners(corners);

//         var height = corners[1].y - corners[0].y;
//         var width = corners[2].x - corners[1].x;

//         Gizmos.color = Color.blue;
//         Vector3 leftMid = position;
//         leftMid.x -= width / 2;
//         Vector3 rightMid = position;
//         rightMid.x += width / 2;
//         Vector3 topMid = position;
//         topMid.y -= height / 2;
//         Vector3 bottomMid = position;
//         bottomMid.y += height / 2;
//         Gizmos.color = Color.white;
//         Gizmos.DrawLine(leftMid, rightMid);
//         Gizmos.DrawLine(topMid, bottomMid);

//         Vector3 topLeft = position + new Vector3(-width / 2, height / 2, 0);
//         Gizmos.DrawLine(topLeft, position);
//         Vector3 topRight = position + new Vector3(width / 2, height / 2, 0);
//         Gizmos.DrawLine(topRight, position);
//         Vector3 bottomLeft = position + new Vector3(-width / 2, -height / 2, 0);
//         Gizmos.DrawLine(bottomLeft, position);
//         Vector3 bottomRight = position + new Vector3(width / 2, -height / 2, 0);
//         Gizmos.DrawLine(bottomRight, position);

//         var margin = 1.2f;
//         var innerWidth = width * (1 - (margin * 2 / 100f));
//         var innerHeight = height * (1 - (margin * 2 / 100f));
//         var horizontalMargin = width - innerWidth;
//         var verticalMargin = height - innerHeight;

//         Vector3 innerTopLeft = topLeft + new Vector3(horizontalMargin, -verticalMargin, 0);
//         Vector3 innerTopRight = topRight + new Vector3(-horizontalMargin, -verticalMargin, 0);
//         Vector3 innerBottomLeft = bottomLeft + new Vector3(+horizontalMargin, +verticalMargin, 0);
//         Vector3 innerBottomRight = bottomRight + new Vector3(-horizontalMargin, +verticalMargin, 0);

//         Gizmos.color = Color.magenta;
//         Gizmos.DrawLine(innerTopLeft, innerTopRight);
//         Gizmos.DrawLine(innerTopRight, innerBottomRight);
//         Gizmos.DrawLine(innerBottomRight, innerBottomLeft);
//         Gizmos.DrawLine(innerBottomLeft, innerTopLeft);

//         var gridWidth = 3;
//         var gridHeight = 3;

//         var maxSideToSideLength = innerWidth / gridWidth;
//         var apothemH = maxSideToSideLength / 2;
//         var sideH = ((2 * apothemH) / 3) * Mathf.Sqrt(3);
//         var vertexToVertexH = sideH * 2;

//         var maxVertexToVertexHeight = innerHeight / gridHeight;
//         var sideV = maxVertexToVertexHeight / 2;
//         var apothemV = (Mathf.Sqrt(3) / 2) * sideV;
//         var sideToSideV = apothemV * 2;

//         var apothem = 0f;

//         if (vertexToVertexH > maxVertexToVertexHeight) {
//             // constrain using the horizontal (s2s) max v2v calc
//             apothem = apothemH;
//         } else {
//             // constrain using the vertical (v2v) max s2s calc
//             apothem = apothemV;
//         }



//         // Gizmos.color = Color.blue;
//         // Gizmos.DrawSphere(rectTransform.anchoredPosition, 30f);
//         // Gizmos.color = Color.yellow;
//         // Gizmos.DrawSphere(rectTransform.right, 30f);
//         // Gizmos.color = Color.green;
//         // Gizmos.DrawSphere(rectTransform.localPosition, 30f);
//         // Gizmos.color = Color.magenta;
//         // Gizmos.DrawSphere(rectTransform.offsetMin, 30f);
//         // Gizmos.color = Color.cyan;
//         // Gizmos.DrawSphere(rectTransform.offsetMax, 30f);
//     }


// }

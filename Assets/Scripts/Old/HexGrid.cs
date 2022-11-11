using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HexGrid : MonoBehaviour {
    void Start() {
        Debug.Log("HexGrid:Start");

        hexMesh.Triangulate(cells);

        var tilemap = GetComponentInChildren<Tilemap>();

        if (tilemap.HasTile(new Vector3Int(0, 0, 0))) {
            Debug.Log("YES, there's a tile at 0, 0, 0");
            var tile = tilemap.GetTile(new Vector3Int(0, 0, 0)) as Tile;
        }

        var cellBounds = tilemap.cellBounds;
        Debug.Log(cellBounds);
        for (var nx = cellBounds.xMin; nx <= cellBounds.xMax; nx++) {
            for (var ny = cellBounds.yMax; ny <= cellBounds.yMax; ny++) {
                Debug.Log("Have cell:" + nx + ":" + ny);
            }
        }

        for (var x = 0; x < 5; x++) {
            // tilemap.layoutGrid.cellSize
            // tilemap.layoutGrid.cellGap
            // tilemap.
        }

        foreach (var position in tilemap.cellBounds.allPositionsWithin) {
            Debug.Log(position);
            var tile = tilemap.GetTile(position) as Tile;
            Debug.Log(tile.gameObject.transform);
        }

        // tile.transform.GetPosition();
        // var prefab = new GameObject();
        // var child = Instantiate(prefab);
    }

    [ContextMenu("Make Me a Hex Mesh")]
    void Test() {
        Awake();
        hexMesh.Awake();
        hexMesh.Triangulate(cells);
        // string path = Path.Combine(Application.persistentDataPath, url);
        // byte [] bytes = MeshSerializer.WriteMesh(mesh, true);
        // File.WriteAllBytes(path, bytes);
    }

    public int width = 6;
    public int height = 6;

    public HexCell cellPrefab;

    public Color defaultColor = Color.white;
    public Color touchedColor = Color.magenta;

    HexCell[] cells;

    public TMP_Text cellLabelPrefab;
    Canvas gridCanvas;
    private HexMesh hexMesh;

    void Awake() {
        Debug.Log("HexGrid:Awake");
        gridCanvas = GetComponentInChildren<Canvas>();
        hexMesh = GetComponentInChildren<HexMesh>();

        cells = new HexCell[height * width];

        for (int z = 0, i = 0; z < height; z++) {
            for (int x = 0; x < width; x++) {
                CreateCell(x, z, i++);
            }
        }
    }

    void CreateCell (int x, int z, int i) {
        Vector3 position;
        // position.x = (x + z * 0.5f) * (HexMetrics.innerRadius * 2f);
        position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.outerRadius * 1.5f);

        HexCell cell = cells[i] = Instantiate(cellPrefab);
        Transform transform1;
        (transform1 = cell.transform).SetParent(transform, false);
        transform1.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.color = defaultColor;

        TMP_Text label = Instantiate(cellLabelPrefab, gridCanvas.transform, false);
        label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
        label.text = cell.coordinates.ToStringOnSeparateLines();
    }
}

using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

public class GameController : MonoBehaviour {
    [HideInInspector]
    public Camera mainCamera;
    [HideInInspector]
    public Tilemap tilemap;

    GameObject _unitPrefab;

    void Start() {
        // mainCamera = Camera.main;
        // tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
        // Assert.IsNotNull(tilemap, "Tilemap not found");
        // _unitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Unit.prefab");
        // Assert.IsNotNull(_unitPrefab, "Unit prefab not found");
    }

    public void SpawnUnit (UnitDefinition unitDefinition, int player, Vector2 coords) {
        Vector2 worldPos = CellToMouse(coords);
        var pos = new Vector3(worldPos.x, worldPos.y, tilemap.transform.position.z);
        GameObject unitGameObject = Instantiate(_unitPrefab, pos, Quaternion.Euler(90, 0, 0));
        var unitScript = unitGameObject.GetComponent<Unit>();
        unitScript.unitDefinition = unitDefinition;
        unitScript.player = player;
    }

    public Vector2 MouseToCell (Vector2 mousePosition) {
        Vector3 world = mainCamera.ScreenToWorldPoint(mousePosition);
        Vector3Int cellPos = tilemap.WorldToCell(world);
        return new Vector2(cellPos.x, cellPos.y);
    }

    public Vector2 CellToMouse (Vector2 cellCoords) {
        return tilemap.CellToWorld(
            new Vector3Int(
                (int)cellCoords.x,
                (int)cellCoords.y,
                (int)tilemap.transform.position.z
            )
        );
    }

}

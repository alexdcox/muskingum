using TMPro;
using Unity.VectorGraphics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using HexGame;

public class UnitRenderer : MonoBehaviour {
  public UnitState unitState;
  public UnitDefinition unitDefinition;
  public Shader hexShader;
  public Shader blockColorShader;

  static readonly int StrokeColor = Shader.PropertyToID("_StrokeColor");
  static readonly int Color1 = Shader.PropertyToID("_Color");
  static readonly int Image = Shader.PropertyToID("_Image");

  public bool showOverlay;

  private Color _border;
  private Color _fill;
  private Material _hexMaterial;
  private bool _selected;

  void Start() {
    Render();
  }

  public void OnValidate() {
    if (unitDefinition != null) {
      unitState = new(){
        unit = new Unit() {
          name = unitDefinition.name,
          cost = unitDefinition.cost,
          damage = unitDefinition.damage,
          health = unitDefinition.health,
          speed = unitDefinition.speed,
        },
      };
    }
    Render();
    Debug.Log("@OnValidate");
  }

  public void Render() {
    if (unitState == null) {
      Debug.Log("-unitState");
      return;
    };
    if (unitDefinition == null) {
      Debug.Log("-unitDefinition");
      return;
    };
    if (hexShader == null) {
      Debug.Log("-hexShader");
      return;
    };

    switch (unitState.playerNum) {
      case 1:
        _border = Color.blue;
        _fill = new Color(0f, 0f, 0.6f);
        break;
      default:
        _border = Color.red;
        _fill = new Color32(143, 5, 0, 255);
        break;
    }
    if (_selected) {
      _border = Color.yellow;
      _fill = new Color32(146, 150, 6, 255);
    }

    _hexMaterial = new Material(hexShader);
    _hexMaterial.SetColor(StrokeColor, _border);
    _hexMaterial.SetTexture(Image, unitDefinition.image);

    Transform hexOutline = transform.Find("HexOutline");
    hexOutline.GetComponent<MeshRenderer>().material = _hexMaterial;

    void SetCircleColors(Transform target) {
      Transform circleIn = target.Find("CircleIn");
      Transform circleOut = target.Find("CircleOut");
      if (circleIn != null && circleOut != null) {
        var circleInMaterial = new Material(blockColorShader);
        circleInMaterial.SetColor(Color1, _fill);
        circleIn.GetComponent<SpriteRenderer>().material = circleInMaterial;

        var circleOutMaterial = new Material(blockColorShader);
        circleOutMaterial.SetColor(Color1, _border);
        circleOut.GetComponent<SpriteRenderer>().material = circleOutMaterial;
      }
    }

    Transform cost = transform.Find("Cost");
    SetCircleColors(cost);
    if (unitState.unit.cost == 0) {
      cost.gameObject.SetActive(false);
    } else {
      cost.gameObject.SetActive(true);
      cost.Find("CostText").GetComponent<TMP_Text>().text = unitState.unit.cost.ToString();
    }

    Transform nameText = transform.Find("NameText");
    nameText.GetComponent<TMP_Text>().text = unitState.unit.name;

    Transform damageText = transform.Find("DamageText");
    damageText.GetComponent<TMP_Text>().text = unitState.unit.damage.ToString();

    Transform healthText = transform.Find("HealthText");
    healthText.GetComponent<TMP_Text>().text = unitState.unit.health.ToString();

    Transform healthRemaining = transform.Find("HealthRemaining");
    SetCircleColors(healthRemaining);
    if (unitState.remainingHealth < unitState.unit.health) {
      healthRemaining.gameObject.SetActive(true);
      healthRemaining.transform.Find("RemainingText").GetComponent<TMP_Text>().text = unitState.remainingHealth.ToString();
    } else {
      healthRemaining.gameObject.SetActive(false);
    }

    Transform speedText = transform.Find("SpeedText");
    speedText.GetComponent<TMP_Text>().text = unitState.unit.speed.ToString();

    var overlay = transform.Find("Overlay");
    if (showOverlay) {
      overlay.gameObject.SetActive(true);
      // GameObject combinedGO = overlay.Find("CombinedHexCircleMesh")?.gameObject;
      // if (combinedGO == null) {
      //   Debug.Log("Combining meshes");
      //   var hexMesh = overlay.Find("Hex").GetComponent<MeshFilter>();
      //   var circleMesh = overlay.Find("Circle").GetComponent<MeshFilter>();
      //   MeshFilter[] meshFilters = {hexMesh, circleMesh};
      //   CombineInstance[] combine = new CombineInstance[meshFilters.Length];
      //   for (var i = 0; i < meshFilters.Length; i++) {
      //       combine[i].mesh = meshFilters[i].sharedMesh;
      //       combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
      //       meshFilters[i].gameObject.SetActive(false);
      //   }
      //   var combinedMesh = new Mesh();
      //   combinedMesh.CombineMeshes(combine, true, true, false);
      //   combinedGO = new GameObject("CombinedHexCircleMesh");
      //   combinedGO.AddComponent<MeshFilter>().mesh = combinedMesh;
      //   combinedGO.transform.parent = overlay;
      //   var combinedMeshRenderer = combinedGO.AddComponent<MeshRenderer>();
      //   combinedMeshRenderer.material = hexMesh.GetComponent<MeshRenderer>().material;
      //   // Instantiate(combinedGO, transform.position, Quaternion.identity, transform);
      // }
    } else {
      overlay.gameObject.SetActive(false);
    }
  }

  public void SetSelected(bool selected) {
    _selected = selected;
    Render();
  }
}

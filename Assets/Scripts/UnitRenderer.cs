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

  Color _border;
  Color _fill;
  Material _hexMaterial;

  void Start() {
    Render();
  }

  public void OnValidate() {
    Render();
  }

  void Render() {
    if (unitState == null) {
      return;
    };
    if (hexShader == null) {
      return;
    };

    SetPlayerColors();
    ShowUnitDetails();
  }

  public void SetPlayerColors() {
    // Debug.Log("setting player to: " + player);

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

    _hexMaterial = new Material(hexShader);
    _hexMaterial.SetColor(StrokeColor, _border);

    Transform hexOutline = transform.Find("HexOutline");
    var meshRenderer = hexOutline.GetComponent<MeshRenderer>();
    meshRenderer.material = _hexMaterial;

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
    Transform healthRemaining = transform.Find("HealthRemaining");
    SetCircleColors(healthRemaining);
  }

  public void ShowUnitDetails() {
    Transform nameText = transform.Find("NameText");
    nameText.GetComponent<TMP_Text>().text = unitState.unit.name;

    Transform damageText = transform.Find("DamageText");
    damageText.GetComponent<TMP_Text>().text = unitState.unit.damage.ToString();

    Transform healthText = transform.Find("HealthText");
    healthText.GetComponent<TMP_Text>().text = unitState.unit.health.ToString();

    Transform healthRemaining = transform.Find("HealthRemaining");
    if (unitState.remainingHealth < unitState.unit.health) {
      healthRemaining.gameObject.SetActive(true);
      healthRemaining.transform.Find("HealthRemaining").GetComponent<TMP_Text>().text =
          unitState.remainingHealth.ToString();
    } else {
      healthRemaining.gameObject.SetActive(false);
    }

    Transform cost = transform.Find("Cost");
    if (unitState.unit.name == "Summoner") {
      cost.gameObject.SetActive(false);
    } else {
      cost.gameObject.SetActive(true);
      cost.Find("CostText").GetComponent<TMP_Text>().text = unitState.unit.cost.ToString();
    }

    _hexMaterial.SetTexture(Image, unitDefinition.image);
  }
}

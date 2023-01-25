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
    Render();
  }

  public void Render() {
    if (unitState == null) {
      return;
    };
    if (unitDefinition == null) {
      return;
    };
    if (hexShader == null) {
      return;
    };

    switch (unitState.playerIndex) {
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
    if (unitDefinition.cost == 0) {
      cost.gameObject.SetActive(false);
    } else {
      cost.gameObject.SetActive(true);
      cost.Find("CostText").GetComponent<TMP_Text>().text = unitDefinition.cost.ToString();
    }

    Transform nameText = transform.Find("NameText");
    nameText.GetComponent<TMP_Text>().text = unitDefinition.name;

    Transform damageText = transform.Find("DamageText");
    damageText.GetComponent<TMP_Text>().text = unitDefinition.damage.ToString();

    Transform healthText = transform.Find("HealthText");
    healthText.GetComponent<TMP_Text>().text = unitDefinition.health.ToString();

    Transform healthRemaining = transform.Find("HealthRemaining");
    SetCircleColors(healthRemaining);
    if (unitState.remainingHealth < unitDefinition.health) {
      healthRemaining.gameObject.SetActive(true);
      healthRemaining.transform.Find("RemainingText").GetComponent<TMP_Text>().text = unitState.remainingHealth.ToString();
    } else {
      healthRemaining.gameObject.SetActive(false);
    }

    Transform speedText = transform.Find("SpeedText");
    speedText.GetComponent<TMP_Text>().text = unitDefinition.speed.ToString();

    var overlay = transform.Find("Overlay");
    if (showOverlay) {
      overlay.gameObject.SetActive(true);
    } else {
      overlay.gameObject.SetActive(false);
    }
  }

  public void SetSelected(bool selected) {
    _selected = selected;
    Render();
  }
}

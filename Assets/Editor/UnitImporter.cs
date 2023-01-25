using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class UnitImporter : MonoBehaviour {
  [MenuItem("HexGame/Import Unit Definitions From JSON")]
  public static void ImportUnitsFromJson() {
    string path = Application.dataPath + "/Data/units.json";
    string jsonString = File.ReadAllText(path);
    var json = JsonConvert.DeserializeObject<List<ExpectedUnitSchema>>(jsonString);

    foreach (ExpectedUnitSchema unit in json) {
      var unitDefinition = ScriptableObject.CreateInstance<UnitDefinition>();
      unitDefinition.name = unit.Name;
      unitDefinition.cost = unit.Cost;
      unitDefinition.damage = unit.Damage;
      unitDefinition.health = unit.Health;
      unitDefinition.speed = unit.Movement;

      AssetDatabase.CreateAsset(unitDefinition, "Assets/Data/" + unit.Name + ".asset");
      print(unit.Name);
    }
  }
}
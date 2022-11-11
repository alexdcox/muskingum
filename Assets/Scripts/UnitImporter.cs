using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class ExpectedUnitSchema {
    public string Name { get; set; }
    public string Description { get; set; }
    public int Cost { get; set; }
    public int Damage { get; set; }
    public int Health { get; set; }
    public int Movement { get; set; }
}

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

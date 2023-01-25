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

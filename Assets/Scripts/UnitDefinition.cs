using UnityEngine;
using HexGame;

[CreateAssetMenu(fileName = "New Unit Definition", menuName = "Unit Definition", order = 0)]
public class UnitDefinition : ScriptableObject {
    [SerializeField]
    public UnitId id;
    [SerializeField]
    public new string name;
    [SerializeField]
    public Texture2D image;
    [SerializeField]
    public int cost;
    [SerializeField]
    public int damage;
    [SerializeField]
    public int health;
    [SerializeField]
    public int speed;

    public bool HasId(UnitId id) {
      return id.Equals(Unit.StringToId(name));
    }
}

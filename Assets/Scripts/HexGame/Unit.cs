using System.Collections.Generic;
using System.Linq;

namespace HexGame {
  public enum UnitId {
    Summoner,
    BoomBrothers,
    Nomad,
    Enforcer,
    Rageclaws,
    FireDragon,
    Emberstrike,
    Scavenger,
    Batariel,
    BurningSpears,
    FireStalker,
    FireWorm,
    FireDancer,
    Firesworn,
    GiantSlayer,
    Gladiatrix,
    Juggernaut,
    MagmaFiend,
    MagmaHurler,
    MagmaSpore,
    MagmaTurret,
    Moloch,
    SkyfireDrake,
    Sunreaver,
    Vulcan
  }

  public class Unit {
    static Dictionary<string, UnitId> _idMap = new Dictionary<string, UnitId> {
      {"Summoner", UnitId.Summoner},
      {"Boom Brothers", UnitId.BoomBrothers},
      {"Nomad", UnitId.Nomad},
      {"Enforcer", UnitId.Enforcer},
      {"Rageclaws", UnitId.Rageclaws},
      {"Fire Dragon", UnitId.FireDragon},
      {"Emberstrike", UnitId.Emberstrike},
      {"Scavenger", UnitId.Scavenger},
      {"Batariel", UnitId.Batariel},
      {"Burning Spears", UnitId.BurningSpears},
      {"Fire Stalker", UnitId.FireStalker},
      {"Fire Worm", UnitId.FireWorm},
      {"Fire Dancer", UnitId.FireDancer},
      {"Firesworn", UnitId.Firesworn},
      {"Giant Slayer", UnitId.GiantSlayer},
      {"Gladiatrix", UnitId.Gladiatrix},
      {"Juggernaut", UnitId.Juggernaut},
      {"Magma Fiend", UnitId.MagmaFiend},
      {"Magma Hurler", UnitId.MagmaHurler},
      {"Magma Spore", UnitId.MagmaSpore},
      {"Magma Turret", UnitId.MagmaTurret},
      {"Moloch", UnitId.Moloch},
      {"Skyfire Drake", UnitId.SkyfireDrake},
      {"Sunreaver", UnitId.Sunreaver},
      {"Vulca", UnitId.Vulcan},
    };

    public string name;
    public int cost;
    public int damage;
    public int health;
    public int speed;

    public bool HasId(UnitId id) {
      return Unit.IdToString(id) == name;
    }

    public static string IdToString(UnitId id) {
      return _idMap.FirstOrDefault(x => x.Value == id).Key;
    }

    public static UnitId StringToId(string name) {
        return _idMap.TryGetValue(name, out UnitId id) ? id : UnitId.Summoner;
    }
  }
}
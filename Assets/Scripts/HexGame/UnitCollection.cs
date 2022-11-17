using System;
using System.Linq;
using System.Collections.Generic;

namespace HexGame {
  public class UnitCollection {
    List<Unit> _units = new List<Unit>();

    public UnitCollection() { }

    public UnitCollection(List<Unit> units) {
      // TODO: Is this a shallow clone?
      _units = new List<Unit>(units);
    }

      public UnitCollection(UnitCollection unitCollection) {
      _units = unitCollection._units;
    }
    
    public void Shuffle() {
      _units.Shuffle();
    }

    public List<Unit> Draw(int x) {
      return _units.Take(x).ToList();
    }

    public UnitCollection Filter(Func<Unit, bool> cb) {
      UnitCollection filtered = new UnitCollection();
      foreach(Unit unit in _units) {
        if (cb(unit)) {
          filtered.Add(unit);
        }
      }
      return filtered;
    }

    public Unit Find(Func<Unit, bool> cb) {
      foreach(Unit unit in _units) {
        if (cb(unit)) {
          return unit;
        }
      }
      return null;
    }

    public Unit FindById(UnitId id) {
      return Find(unit => unit.name == Unit.IdToString(id));
    }

    public void RemoveById(UnitId id) {
      _units = Filter(unit => !unit.HasId(id))._units;
    }

    public UnitCollection Clone() {
      // TODO: Double check this works as intended or follow:
      //       https://stackoverflow.com/questions/222598/how-do-i-clone-a-generic-list-in-c
      return new UnitCollection(new List<Unit>(_units));
    }

    public List<Unit> GetUnits() {
      // TODO: Do I need to Clone first?
      return _units.ToList();
    }
    
    public List<UnitId> GetUnitIds() {
      List<UnitId> unitIds = new List<UnitId>();
      foreach(Unit unit in _units) {
        var id = Unit.StringToId(unit.name);
        if (unitIds.Contains(id)) {
          continue;
        }
        unitIds.Add(id);
      }
      return unitIds;
    }

    public void Add(Unit unit) {
      _units.Add(unit);
    }

    public void Add(List<Unit> units) {
      foreach(Unit unit in units) {
        Add(unit);
      }
    }
  }
}
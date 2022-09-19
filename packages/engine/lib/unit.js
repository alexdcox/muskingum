import units from './units.json' assert { type: "json" };
export var UnitId;
(function (UnitId) {
    UnitId[UnitId["Summoner"] = 0] = "Summoner";
    UnitId[UnitId["BoomBrothers"] = 1] = "BoomBrothers";
    UnitId[UnitId["Nomad"] = 2] = "Nomad";
    UnitId[UnitId["Enforcer"] = 3] = "Enforcer";
    UnitId[UnitId["Rageclaws"] = 4] = "Rageclaws";
    UnitId[UnitId["FireDragon"] = 5] = "FireDragon";
    UnitId[UnitId["Emberstrike"] = 6] = "Emberstrike";
    UnitId[UnitId["Scavenger"] = 7] = "Scavenger";
    UnitId[UnitId["Batariel"] = 8] = "Batariel";
    UnitId[UnitId["BurningSpears"] = 9] = "BurningSpears";
    UnitId[UnitId["FireStalker"] = 10] = "FireStalker";
    UnitId[UnitId["FireWorm"] = 11] = "FireWorm";
    UnitId[UnitId["FireDancer"] = 12] = "FireDancer";
    UnitId[UnitId["Firesworn"] = 13] = "Firesworn";
    UnitId[UnitId["GiantSlayer"] = 14] = "GiantSlayer";
    UnitId[UnitId["Gladiatrix"] = 15] = "Gladiatrix";
    UnitId[UnitId["Juggernaut"] = 16] = "Juggernaut";
    UnitId[UnitId["MagmaFiend"] = 17] = "MagmaFiend";
    UnitId[UnitId["MagmaHurler"] = 18] = "MagmaHurler";
    UnitId[UnitId["MagmaSpore"] = 19] = "MagmaSpore";
    UnitId[UnitId["MagmaTurret"] = 20] = "MagmaTurret";
    UnitId[UnitId["Moloch"] = 21] = "Moloch";
    UnitId[UnitId["SkyfireDrake"] = 22] = "SkyfireDrake";
    UnitId[UnitId["Sunreaver"] = 23] = "Sunreaver";
    UnitId[UnitId["Vulcan"] = 24] = "Vulcan";
})(UnitId || (UnitId = {}));
(function (UnitId) {
    function unit(id) {
        return UnitMap.get(id);
    }
    UnitId.unit = unit;
})(UnitId || (UnitId = {}));
export class Unit {
    id;
    name;
    cost;
    damage;
    health;
    movement;
    constructor(id, name, cost, damage, health, movement) {
        this.id = id;
        this.name = name;
        this.cost = cost;
        this.damage = damage;
        this.health = health;
        this.movement = movement;
    }
    get fileName() {
        return this.name.toLowerCase().replace(' ', '_');
    }
}
export const Units = units.map((definition, index) => {
    const id = index;
    return new Unit(id, definition.name, definition.cost, definition.damage, definition.health, definition.movement);
});
export const UnitMap = new Map();
Units.forEach(unit => UnitMap.set(unit.id, unit));
export class UnitCollection {
    units;
    constructor(units) {
        this.units = units;
    }
    shuffle() {
        let currentIndex = this.units.length;
        let randomIndex;
        while (currentIndex != 0) {
            randomIndex = Math.floor(Math.random() * currentIndex);
            currentIndex--;
            [this.units[currentIndex], this.units[randomIndex]] = [this.units[randomIndex], this.units[currentIndex]];
        }
    }
    draw(x) {
        return this.units.splice(0, x);
    }
    getUnits() {
        let units = [];
        for (let unit of this.units) {
            units = [...units];
        }
        return units;
    }
    getUnitIds() {
        let unitIds = [];
        for (let unit of this.units) {
            unitIds = [...unitIds, unit.id];
        }
        return unitIds;
    }
    add(units) {
        this.units = [...this.units, ...units];
    }
    removeUnit(id) {
        this.units = this.units.filter(unit => unit.id != id);
    }
}
//# sourceMappingURL=unit.js.map
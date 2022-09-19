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
export class Unit {
    id;
    damage;
    health;
    constructor(id, damage, health) {
        this.id = id;
        this.damage = damage;
        this.health = health;
    }
}
// export const Units: Map<number, Unit> = new Map([
//     [UnitId.Summoner, new Unit(3, 10)]
// ])
//
// Units.forEach((unit, id) => unit.id = id)

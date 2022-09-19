export declare enum UnitId {
    Summoner = 0,
    BoomBrothers = 1,
    Nomad = 2,
    Enforcer = 3,
    Rageclaws = 4,
    FireDragon = 5,
    Emberstrike = 6,
    Scavenger = 7,
    Batariel = 8,
    BurningSpears = 9,
    FireStalker = 10,
    FireWorm = 11,
    FireDancer = 12,
    Firesworn = 13,
    GiantSlayer = 14,
    Gladiatrix = 15,
    Juggernaut = 16,
    MagmaFiend = 17,
    MagmaHurler = 18,
    MagmaSpore = 19,
    MagmaTurret = 20,
    Moloch = 21,
    SkyfireDrake = 22,
    Sunreaver = 23,
    Vulcan = 24
}
export declare namespace UnitId {
    function unit(id: UnitId): Unit;
}
export declare class Unit {
    id: UnitId;
    name: string;
    cost: number;
    damage: number;
    health: number;
    movement: number;
    constructor(id: UnitId, name: string, cost: number, damage: number, health: number, movement: number);
    get fileName(): string;
}
export declare const Units: Unit[];
export declare const UnitMap: Map<UnitId, Unit>;
export declare class UnitCollection {
    private units;
    constructor(units: Unit[]);
    shuffle(): void;
    draw(x: number): Unit[];
    getUnits(): Unit[];
    getUnitIds(): UnitId[];
    add(units: Unit[]): void;
    removeUnit(id: UnitId): void;
}
//# sourceMappingURL=unit.d.ts.map
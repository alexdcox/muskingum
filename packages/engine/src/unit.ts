import units from './units.json' assert { type: "json" }

export enum UnitId {
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
    Vulcan,
}

export namespace UnitId {
    export function unit(id: UnitId): Unit {
        return UnitMap.get(id)!
    }
}

export class Unit {
    id: UnitId
    name: string
    cost: number
    damage: number
    health: number
    movement: number

    constructor(id: UnitId, name: string, cost: number, damage: number, health: number, movement: number) {
        this.id = id;
        this.name = name;
        this.cost = cost;
        this.damage = damage;
        this.health = health;
        this.movement = movement;
    }

    get fileName(): string {
        return this.name.toLowerCase().replace(' ', '_')
    }
}

export const Units: Unit[] = units.map((definition, index) => {
    const id: UnitId = index
    return new Unit(
      id,
      definition.name,
      definition.cost,
      definition.damage,
      definition.health,
      definition.movement,
    )
})

export const UnitMap = new Map<UnitId, Unit>()
Units.forEach(unit => UnitMap.set(unit.id, unit))

export class UnitCollection {
    private units: Unit[]

    constructor(units: Unit[]) {
        this.units = units
    }

    shuffle() {
        let currentIndex = this.units.length
        let randomIndex
        while (currentIndex != 0) {
            randomIndex = Math.floor(Math.random() * currentIndex)
            currentIndex--
            [this.units[currentIndex], this.units[randomIndex]] = [this.units[randomIndex], this.units[currentIndex]]
        }
    }

    draw(x: number): Unit[] {
        return this.units.splice(0, x)
    }

    getUnits(): Unit[] {
        let units: Unit[] = []
        for (let unit of this.units) {
            units = [...units]
        }
        return units
    }

    getUnitIds(): UnitId[] {
        let unitIds: UnitId[] = []
        for (let unit of this.units) {
            unitIds = [...unitIds, unit.id]
        }
        return unitIds
    }

    add(units: Unit[]) {
        this.units = [...this.units, ...units]
    }

    removeUnit(id: UnitId) {
        this.units = this.units.filter(unit => unit.id != id)
    }
}

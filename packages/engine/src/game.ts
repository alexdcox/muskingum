import {Unit, UnitCollection, UnitId, Units} from "./unit.js";
import {DoubledCoord} from "./hex.js"
import {Player, PlayerId} from "./player.js"
import {WebSocket} from 'ws'
import {EventEmitter} from "events"

declare module "ws" {
  interface WebSocket {
    player: number;
  }
}

export const EnergyCoords: [DoubledCoord, number][] = [
  [new DoubledCoord(3, 1), 1],
  [new DoubledCoord(3, 3), 1],
  [new DoubledCoord(9, 1), 1],
  [new DoubledCoord(9, 3), 1],
  [new DoubledCoord(6, 2), 2],
]

class Connection {
  public socket: WebSocket

  constructor(socket: WebSocket) {
    this.socket = socket
  }
}

export class GameServer {
  public connections: Connection[] = []
  public game?: Game

  constructor() {
    console.log('New game server 😊!')
  }

  handleConnectionOpened(socket: WebSocket) {
    if (this.connections.length == 2) {
      console.log('Player 3 is bringing down the house')
      for (const connection of this.connections) {
        connection.socket.close()
      }
      this.connections = []
    }
    const connection = new Connection(socket)
    connection.socket.player = this.connections.length + 1
    this.connections.push(connection)
    console.log('New websocket connection from player', this.connections.length)
    socket.send(JSON.stringify({type: 'playerid', id: this.connections.length}))
    if (this.connections.length == 2) {
      console.log('2 players connected, starting game')
      this.startGame()
    }
  }

  handleMessage(message: any, socket: WebSocket) {
    console.log('handle message', message)
    switch (message.type) {
      case 'login':
        console.log("Logging in user:", message.name)
        break
      case 'summon':
        const unitId = message.unitId
        const coord = new DoubledCoord(message.coord.col, message.coord.row)
        this.game?.summon(unitId, coord)
        break
      case 'skipsummon':
        this.game?.skipSummon(socket.player)
        break
      case 'move': {
        const from = new DoubledCoord(message.from.col, message.from.row)
        const to = new DoubledCoord(message.to.col, message.to.row)
        this.game?.move(from, to)
        break
      }
      case 'skipmove':
        this.game?.skipMove(socket.player)
        break
      case 'attack': {
        const from = new DoubledCoord(message.from.col, message.from.row)
        const to = new DoubledCoord(message.to.col, message.to.row)
        this.game?.attack(from, to)
        break
      }
      case 'skipattack':
        this.game?.skipAttack(socket.player)
        break
      case 'gamestate':
        if (this.game) {
          socket.send(JSON.stringify({type: 'gamestate', state: this.game.getState()}))
        }
        break
      default:
        console.log("Received message with invalid type:", message.type)
        break
    }
  }

  startGame() {
    this.game = new Game()
    this.game.eventEmitter.on('gamestate', state => {
      console.log('sending gamestate to', this.connections.length, 'connections')
      for (const connection of this.connections) {
        connection.socket.send(JSON.stringify({type: 'gamestate', state}))
      }
    })
    this.game.emitGameState()
    this.game.nextTurn()
  }
}

enum ZoneName {
  Draw,
  Hand,
  Grid,
  Discard
}

class Zone {
  units: Unit[] = [];
}

export interface TurnState {
  summoned: UnitState[]
  player: number
  stage: TurnStage
  unitsMoved: DoubledCoord[]
  unitsAttacked: DoubledCoord[]
}

export class Game {
  public eventEmitter: EventEmitter
  public players: Player[] = []
  public boardUnits: UnitState[] = []
  public playerState: GamePlayerState[] = []
  public turn: TurnState = {
    player: 0,
    stage: TurnStage.Summon,
    unitsMoved: [],
    unitsAttacked: [],
    summoned: [],
  }

  constructor() {
    console.log('New game started')

    this.eventEmitter = new EventEmitter()

    const player1 = new Player("Player 1")
    const player2 = new Player("Player 2")

    this.players.push(player1)
    this.players.push(player2)

    console.log("Shuffling deck for players and drawing hand...")
    this.players.forEach((player, index) => {
      const state = new GamePlayerState()
      state.draw.add(Units.filter(u => u.id != UnitId.Summoner))
      state.draw.shuffle()
      state.hand.add(state.draw.draw(5))
      state.energy = 0
      this.playerState.push(state)
    })

    console.log('Spawning in summoners')
    this.boardUnits.push({
      id: UnitId.Summoner,
      player: 1,
      coord: new DoubledCoord(0, 2)
    })
    this.boardUnits.push({
      id: UnitId.Summoner,
      player: 2,
      coord: new DoubledCoord(12, 2)
    })

    this.emitGameState()
  }

  getState(): GameState {
    const gameState: GameState = {
      units: this.boardUnits,
      players: [],
      turn: this.turn,
    }

    this.playerState.forEach((playerState, id) => {
      gameState.players.push({
        id: id + 1,
        energy: playerState.energy,
        energyGain: playerState.energyGain,
        draw: playerState.draw.getUnitIds(),
        hand: playerState.hand.getUnitIds(),
        discard: playerState.discard.getUnitIds(),
      })
    })

    return gameState
  }

  nextTurn() {
    this.turn.player++
    if (this.turn.player > 2) {
      this.turn.player = 1
    }

    this.turn.stage = TurnStage.Summon
    this.turn.summoned = []
    this.turn.unitsMoved = []
    this.turn.unitsAttacked = []

    const playerIndex = this.turn.player - 1
    console.log('Handling player', playerIndex + 1, 'turn')

    this.calculateEnergyGain()
    const playerState = this.playerState[playerIndex]
    console.log('Increasing player', playerIndex, 'energy from', playerState.energy, 'to', playerState.energy + playerState.energyGain)
    playerState.energy += playerState.energyGain

    this.emitGameState()
  }

  calculateEnergyGain() {
    for (let playerIndex = 0; playerIndex < this.players.length; playerIndex++) {
      const state = this.playerState[playerIndex]
      let energyGain = 1
      for (const [coord, energy] of EnergyCoords) {
        for (const unit of this.boardUnits) {
          if (unit.player != Number(playerIndex) + 1) {
            continue
          }
          if (unit.coord.equals(coord)) {
            energyGain += energy
          }
        }
      }
      state.energyGain = energyGain
    }
  }

  summon(unitId: UnitId, coord: DoubledCoord) {
    const playerIndex = this.turn.player - 1
    const playerNum = this.turn.player
    const playerState = this.playerState[playerIndex]
    const unit = UnitId.unit(unitId)
    console.log(`Player ${playerNum} summoning ${unit.name} to ${coord.toString()}`)
    const unitState = {
      id: unitId,
      coord,
      player: this.turn.player,
    }
    if (playerState.energy < unit.cost) {
      console.log(`Not enough muhlah to summon ${unit.name} have ${playerState.energy}/${unit.cost}, nice try`)
      return
    }
    playerState.energy -= unit.cost
    const summoner = this.boardUnits.find(unit => unit.player == playerNum && unit.id == UnitId.Summoner)!
    if (coord.rdoubledToCube().distance(summoner.coord.rdoubledToCube()) != 1) {
      console.log('Cannae summon there ya nonse')
      return
    }
    this.turn.summoned.push(unitState)
    this.boardUnits.push(unitState)
    playerState.hand.removeUnit(unitId)
    // TODO: do we draw now or at the end of the summon turn?
    playerState.hand.add(playerState.draw.draw(1))
    // this.turn.stage = TurnStage.Move
    this.calculateEnergyGain()
    this.emitGameState()
  }

  skipSummon(player: number) {
    if (player != this.turn.player) {
      console.log('You cant skip another player summon')
      return
    }
    console.log('Player', this.turn.player, 'skipping summon')
    this.turn.stage = TurnStage.Move
    this.emitGameState()
  }

  move(from: DoubledCoord, to: DoubledCoord) {
    console.log('Player', this.turn.player, 'attempting to move from', from, 'to', to)
    const unitState = this.boardUnits.find(unit => unit.coord.equals(from))!
    if (!unitState) {
      console.log('No unit')
      return
    }
    const unit = UnitId.unit(unitState.id)
    const distance = from.rdoubledToCube().distance(to.rdoubledToCube())
    let hasSummoningSickness = false
    for (const summoned of this.turn.summoned) {
      if (summoned.coord.equals(from)) {
        hasSummoningSickness = true
        break
      }
    }
    if (hasSummoningSickness) {
      console.log('Them theres got the summoning sickness')
      return
    }
    if (distance > unit.movement) {
      console.log('You aint got long enough legs for that pal')
      return
    }
    const existingUnit = this.boardUnits.find(unit => unit.coord.equals(to))
    if (existingUnit) {
      console.log('Trying to move on top of another unit failed, they werent feeling frisky')
      return
    }
    const alreadyMoved = this.turn.unitsMoved.find(coord => coord.equals(from))
    if (alreadyMoved) {
      console.log('The poor devils already done their duty, give em a rest')
      return
    }
    unitState.coord = to
    this.turn.unitsMoved.push(to)
    let unitCount = this.boardUnits.filter(unit => unit.player == this.turn.player).length
    if (this.turn.summoned.length) {
      unitCount--
    }
    if (this.turn.unitsMoved.length == unitCount) {
      console.log('All units moved, progressing to attack phase')
      this.turn.stage = TurnStage.Attack
    }
    this.calculateEnergyGain()
    this.emitGameState()
  }

  skipMove(player: number) {
    if (player != this.turn.player) {
      console.log('You cant skip another player move')
      return
    }
    console.log('Player', this.turn.player, 'skipping move')
    this.turn.stage = TurnStage.Attack
    this.emitGameState()
  }

  attack(from: DoubledCoord, to: DoubledCoord) {
    const playerNum = this.turn.player
    console.log('Player', playerNum, 'attacking from', from, 'to', to)
    let hasSummoningSickness = false
    for (const summoned of this.turn.summoned) {
      if (summoned.coord.equals(from)) {
        hasSummoningSickness = true
        break
      }
    }
    if (hasSummoningSickness) {
      console.log('Them theres got the summoning sickness')
      return
    }
    const fromUnitState = this.boardUnits.find(u => u.coord.equals(from))!
    if (!fromUnitState) {
      console.log('Theres... nothing there. You cant attack with nothing.')
      return
    }
    const fromUnit = UnitId.unit(fromUnitState.id)
    const toUnitState = this.boardUnits.find(u => u.coord.equals(to))!
    if (!toUnitState) {
      console.log('You cant attack thin air')
      return
    }
    const toUnit = UnitId.unit(toUnitState.id)
    const alreadyAttacked = this.turn.unitsAttacked.find(coord => coord.equals(from))
    if (alreadyAttacked) {
      console.log('This unit thinks one battle is enough for the time being')
      return
    }
    const isFriendly = this.boardUnits.find(u => u.coord.equals(to) && u.player == playerNum)
    if (isFriendly) {
      console.log('Whoa there laddy, that ones on our side')
      return
    }
    const healthRemaining = (toUnitState.remainingHealth || toUnit.health) - fromUnit.damage
    toUnitState.remainingHealth = healthRemaining
    if (healthRemaining <= 0) {
      console.log('Ohhh shiiit, unit down!')
      let filtered: UnitState[] = []
      for (const unit of this.boardUnits) {
        if (unit.coord.equals(to)) {
          continue
        }
        filtered = [...filtered, unit]
      }
      this.boardUnits = filtered

      if (toUnit.id == UnitId.Summoner) {
        console.log('💥💣🔥🧨🎇🚨 SUMMONER DOWN!!!')
        console.log('Player', this.turn.player, 'wins!!!!!')
        const winningSummonerState = this.boardUnits.find(unit => unit.player == this.turn.player && unit.id == UnitId.Summoner)!
        this.boardUnits = [winningSummonerState]
      }
    }
    this.turn.unitsAttacked.push(from)
    this.calculateEnergyGain()
    this.emitGameState()

    const unitCount = this.boardUnits.filter(unit => unit.player == this.turn.player).length
    if (this.turn.unitsAttacked.length == unitCount) {
      console.log('All units attacked, progressing to next turn')
      this.nextTurn()
    }
  }

  skipAttack(player: number) {
    if (player != this.turn.player) {
      console.log('You cant skip another player attack')
      return
    }
    console.log('Player', this.turn.player, 'skipping attack')
    this.nextTurn()
  }

  emitGameState() {
    console.log('emitting state')
    this.eventEmitter.emit('gamestate', this.getState())
  }
}

class GamePlayerState {
  public draw: UnitCollection = new UnitCollection([])
  public discard: UnitCollection = new UnitCollection([])
  public hand: UnitCollection = new UnitCollection([])
  public energy: number = 0
  public energyGain: number = 1
}

export interface UnitState {
  id: UnitId
  coord: DoubledCoord
  player: number
  remainingHealth?: number
}

export enum TurnStage {
  Summon,
  Move,
  Attack,
}

export interface GameState {
  units: UnitState[],
  turn: TurnState,
  players: {
    id: number,
    energy: number,
    energyGain: number,
    draw: UnitId[],
    hand: UnitId[],
    discard: UnitId[],
  }[]
}

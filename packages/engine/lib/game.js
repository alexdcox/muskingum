import { UnitCollection, UnitId, Units } from "./unit.js";
import { DoubledCoord } from "./hex.js";
import { Player } from "./player.js";
import { EventEmitter } from "events";
export const EnergyCoords = [
    [new DoubledCoord(-3, 1), 1],
    [new DoubledCoord(-3, -1), 1],
    [new DoubledCoord(3, 1), 1],
    [new DoubledCoord(3, -1), 1],
    [new DoubledCoord(0, 0), 2],
];
class Connection {
    socket;
    constructor(socket) {
        this.socket = socket;
    }
}
export class GameServer {
    connections = [];
    games = [];
    registeredPlayers = [];
    combatantQueue = [];
    constructor() {
        console.log('New game server 😊!');
    }
    handleConnectionOpened(socket) {
        const connection = new Connection(socket);
        const player = new Player("<unregistered>");
        connection.socket.player = player;
        this.connections.push(connection);
        console.log('New websocket connection', this.connections.length);
        socket.send(JSON.stringify({ type: 'playerid', id: player.id }));
    }
    handleConnectionClosed(socket) {
        const queueIndex = this.combatantQueue.findIndex(p => p.id === socket.player.id);
        if (queueIndex) {
            console.log('Queued player dropped out');
            console.log('queued players before: ', this.combatantQueue.length);
            this.combatantQueue.splice(queueIndex, 1);
            console.log('queued players after: ', this.combatantQueue.length);
        }
        console.log('server conns before: ', this.connections.length);
        this.connections = this.connections.filter(c => c.socket != socket);
        console.log('server conns after: ', this.connections.length);
    }
    handleMessage(message, socket) {
        console.log('handle message', message);
        switch (message.type) {
            case 'login':
                if (message.id) {
                    console.log('Received login message with id, selecting existing user');
                    const player = this.registeredPlayers.find(p => p.id = message.id && p.name == message.name);
                    if (!player) {
                        console.log('Unable to find registered player ', message.name, 'with id', message.id);
                        return;
                    }
                    socket.player = player;
                }
                else {
                    // TODO: Restore this existing name validation
                    // const existingPlayer = this.registeredPlayers.find(p => p.name.toLowerCase().trim() == message.name.toLowerCase().trim())
                    // if (existingPlayer) {
                    //   console.log('A player has already reserved this name')
                    //   return
                    // }
                    console.log("Received new login for user: ", message.name);
                    socket.player.name = message.name.trim();
                    this.registeredPlayers.push(socket.player);
                }
                console.log('combatants waiting: ' + this.combatantQueue.length);
                this.combatantQueue.push(socket.player);
                if (this.combatantQueue.length % 2 === 0) {
                    this.startGame();
                }
                return;
            case 'rename':
                // console.log("Renaming user:", message.name)
                // break
                return;
        }
        if (socket.player.gameId < 0) {
            console.log('No associated game to handle ', message.type, 'message');
            return;
        }
        switch (message.type) {
            case 'summon':
                const unitId = message.unitid;
                const coord = new DoubledCoord(message.coord.col, message.coord.row);
                this.games[socket.player.gameId]?.summon(socket.player, unitId, coord);
                break;
            case 'skipsummon':
                this.games[socket.player.gameId]?.skipSummon(socket.player);
                break;
            case 'move': {
                const from = new DoubledCoord(message.from.col, message.from.row);
                const to = new DoubledCoord(message.to.col, message.to.row);
                this.games[socket.player.gameId].move(socket.player, from, to);
                break;
            }
            case 'skipmove':
                this.games[socket.player.gameId].skipMove(socket.player);
                break;
            case 'attack': {
                const from = new DoubledCoord(message.from.col, message.from.row);
                const to = new DoubledCoord(message.to.col, message.to.row);
                this.games[socket.player.gameId].attack(socket.player, from, to);
                break;
            }
            case 'skipattack':
                this.games[socket.player.gameId].skipAttack(socket.player);
                break;
            case 'gamestate':
                const state = this.games[socket.player.gameId].getState();
                socket.send(JSON.stringify({ type: 'gamestate', state }));
                break;
            default:
                console.log("Received message with invalid type:", message.type);
                break;
        }
    }
    startGame() {
        const player1 = this.combatantQueue.splice(0, 1)?.[0];
        const p1c = this.connections.find(c => c.socket.player == player1);
        if (p1c) {
            console.log('Telling player', player1.id, 'to play as index', 0);
            p1c.socket.send(JSON.stringify({ type: 'playerindex', playerindex: 0 }));
        }
        else {
            console.log('No connection for player 1 found, now thats perplexing');
        }
        const player2 = this.combatantQueue.splice(0, 1)?.[0];
        const p2c = this.connections.find(c => c.socket.player == player2);
        if (p2c) {
            console.log('Telling player', player2.id, 'to play as index', 1);
            p2c.socket.send(JSON.stringify({ type: 'playerindex', playerindex: 1 }));
        }
        else {
            console.log('No connection for player 2 found, whoda thunk');
        }
        const game = new Game(player1, player2);
        const players = [player1, player2];
        game.id = this.games.length;
        player1.gameId = game.id;
        player2.gameId = game.id;
        game.eventEmitter.on('gamestate', (game) => {
            const state = game.getState();
            for (const connection of this.connections) {
                if (players.includes(connection.socket.player)) {
                    connection.socket.send(JSON.stringify({ type: 'gamestate', state }));
                }
            }
        });
        game.eventEmitter.on('gamestats', (game) => {
            const stats = game.getStats();
            for (const connection of this.connections) {
                if (players.includes(connection.socket.player)) {
                    connection.socket.send(JSON.stringify({ type: 'gamestats', stats }));
                }
            }
            this.combatantQueue = this.combatantQueue.filter(p => !game.players.includes(p));
            this.games = this.games.filter(g => g != game);
        });
        // this.game.emitGameState()
        game.nextTurn();
        this.games.push(game);
    }
}
var ZoneName;
(function (ZoneName) {
    ZoneName[ZoneName["Draw"] = 0] = "Draw";
    ZoneName[ZoneName["Hand"] = 1] = "Hand";
    ZoneName[ZoneName["Grid"] = 2] = "Grid";
    ZoneName[ZoneName["Discard"] = 3] = "Discard";
})(ZoneName || (ZoneName = {}));
class Zone {
    units = [];
}
export class Game {
    id = -1;
    started = new Date();
    ended;
    eventEmitter;
    players = [];
    boardUnits = [];
    playerState = [];
    turn = {
        index: 0,
        playerIndex: -1,
        // stage: TurnStage.Summon,
        unitsMoved: [],
        unitsAttacked: [],
        unitsSummoned: [],
    };
    constructor(player1, player2) {
        console.log('New game started');
        this.eventEmitter = new EventEmitter();
        this.players.push(player1);
        this.players.push(player2);
        player1.index = 0;
        player2.index = 1;
        this.turn.playerIndex = Math.round(Math.random());
        console.log("Shuffling deck for players and drawing hand...");
        this.players.forEach((player, index) => {
            const state = new GamePlayerState();
            state.draw.add(Units.filter(u => u.id != UnitId.Summoner));
            state.draw.shuffle();
            const drawn = state.draw.draw(5);
            state.hand.add(drawn);
            state.energy = 0;
            this.playerState.push(state);
            drawn.forEach(d => this.recordDrawnUnitCost(player, d.cost));
        });
        const summoner = UnitId.unit(UnitId.Summoner);
        this.boardUnits.push({
            id: summoner.id,
            playerIndex: 0,
            coord: new DoubledCoord(-6, 0),
            remainingHealth: summoner.health,
        });
        this.boardUnits.push({
            id: summoner.id,
            playerIndex: 1,
            coord: new DoubledCoord(6, 0),
            remainingHealth: summoner.health,
        });
        // this.stats = new GameStats();
        // this.emitGameState()
    }
    getState() {
        const gameState = {
            units: this.boardUnits,
            players: [],
            turn: this.turn,
        };
        this.playerState.forEach((playerState, index) => {
            // TODO: Can't we just return the actual players here?
            gameState.players.push({
                index: index,
                energy: playerState.energy,
                energyGain: playerState.energyGain,
                draw: playerState.draw.getUnitIds(),
                hand: playerState.hand.getUnitIds(),
                discard: playerState.discard.getUnitIds(),
            });
        });
        return gameState;
    }
    getPlayerIds() {
        return this.players.map(p => p.id);
    }
    nextTurn() {
        this.turn.index++;
        this.turn.playerIndex++;
        if (this.turn.playerIndex > 1) {
            this.turn.playerIndex = 0;
        }
        // this.turn.stage = TurnStage.Summon
        this.turn.unitsMoved = [];
        this.turn.unitsAttacked = [];
        this.turn.unitsSummoned = [];
        console.log('Handling player', this.turn.playerIndex, 'turn');
        this.calculateEnergyGain();
        const playerState = this.playerState[this.turn.playerIndex];
        console.log('Increasing player', this.turn.playerIndex, 'energy from', playerState.energy, 'to', playerState.energy + playerState.energyGain);
        playerState.energy += playerState.energyGain;
        playerState.energyGained += playerState.energyGain;
        this.emitGameState();
    }
    calculateEnergyGain() {
        for (let playerIndex = 0; playerIndex < this.players.length; playerIndex++) {
            const state = this.playerState[playerIndex];
            let energyGain = 1;
            for (const [coord, energy] of EnergyCoords) {
                for (const unit of this.boardUnits) {
                    if (unit.playerIndex != playerIndex) {
                        continue;
                    }
                    if (unit.coord.equals(coord)) {
                        energyGain += energy;
                    }
                }
            }
            state.energyGain = energyGain;
        }
    }
    summon(player, unitId, coord) {
        const playerState = this.playerState[this.turn.playerIndex];
        const unit = UnitId.unit(unitId);
        console.log(`Player ${this.turn.playerIndex} summoning ${unit.name} to ${coord.toString()}`);
        const unitState = {
            id: unitId,
            coord,
            playerIndex: this.turn.playerIndex,
            remainingHealth: unit.health,
        };
        if (playerState.energy < unit.cost) {
            console.log(`Not enough muhlah to summon ${unit.name} have ${playerState.energy}/${unit.cost}, nice try`);
            return;
        }
        playerState.energy -= unit.cost;
        const summoner = this.boardUnits.find(unit => unit.playerIndex == this.turn.playerIndex && unit.id == UnitId.Summoner);
        if (coord.rdoubledToCube().distance(summoner.coord.rdoubledToCube()) != 1) {
            console.log('Cannae summon there ya nonse');
            return;
        }
        this.turn.unitsSummoned.push(unitState.coord);
        this.boardUnits.push(unitState);
        playerState.hand.removeUnit(unitId);
        const drawn = playerState.draw.draw(1);
        this.recordDrawnUnitCost(player, drawn[0].cost);
        playerState.hand.add(drawn);
        playerState.energySpent += unit.cost;
        playerState.unitsSummoned++;
        // this.turn.stage = TurnStage.Move
        this.calculateEnergyGain();
        this.emitGameState();
    }
    skipSummon(player) {
        if (player.id != this.players[this.turn.playerIndex].id) {
            console.log('You cant skip another player summon');
            return;
        }
        console.log('Player', this.turn.playerIndex, 'skipping summon');
        // this.turn.stage = TurnStage.Move
        this.emitGameState();
    }
    move(player, from, to) {
        if (player.id != this.players[this.turn.playerIndex].id) {
            console.log('You cant make another player move');
            return;
        }
        console.log('Player', this.turn.playerIndex, 'attempting to move from', from, 'to', to);
        const unitState = this.boardUnits.find(unit => unit.coord.equals(from));
        if (!unitState) {
            console.log('No unit');
            return;
        }
        const unit = UnitId.unit(unitState.id);
        const unitAt = (hex) => {
            return this.boardUnits.find(unit => unit.coord.rdoubledToCube().equals(hex));
        };
        const isEnemy = (unit) => {
            return unit != undefined && unit.playerIndex != this.turn.playerIndex;
        };
        const start = unitState.coord.rdoubledToCube();
        const engaged = [];
        for (let next of start.neighbors()) {
            if (isEnemy(unitAt(next))) {
                engaged.push(next);
            }
        }
        const frontier = [];
        frontier.push(start);
        const reached = [];
        reached.push(start);
        while (frontier.length) {
            const current = frontier.shift();
            for (let next of current.neighbors()) {
                if (unitAt(next)) {
                    continue;
                }
                if (next.distance(start) > unit.movement) {
                    continue;
                }
                if (next.distance(start) > 10) {
                    continue;
                }
                if (engaged.find(e => e.distance(next) == 1)) {
                    continue;
                }
                if (!reached.find(r => r.equals(next))) {
                    frontier.push(next);
                    reached.push(next);
                }
            }
        }
        const toHex = to.rdoubledToCube();
        const reachable = reached.find(r => r.equals(toHex)) != null;
        const distance = from.rdoubledToCube().distance(to.rdoubledToCube());
        console.log('distance', distance);
        console.log('reachable hexagons', reached.length);
        console.log('within reachable area', reachable);
        console.log('reachable area', reached.map(r => DoubledCoord.rdoubledFromCube(r)));
        if (!reachable) {
            console.log('Cannae make it there, not in reachable area');
            return;
        }
        let hasSummoningSickness = false;
        for (const summoned of this.turn.unitsSummoned) {
            if (summoned.equals(from)) {
                hasSummoningSickness = true;
                break;
            }
        }
        if (hasSummoningSickness) {
            console.log('Them theres got the summoning sickness');
            return;
        }
        if (distance > unit.movement) {
            console.log('You aint got long enough legs for that pal');
            return;
        }
        const existingUnit = this.boardUnits.find(unit => unit.coord.equals(to));
        if (existingUnit) {
            console.log('Trying to move on top of another unit failed, they werent feeling frisky');
            return;
        }
        const alreadyMoved = this.turn.unitsMoved.find(coord => coord.equals(from));
        if (alreadyMoved) {
            console.log('The poor devils already done their duty, give em a rest');
            return;
        }
        unitState.coord = to;
        this.turn.unitsMoved.push(to);
        let unitCount = this.boardUnits.filter(unit => unit.playerIndex == this.turn.playerIndex).length;
        if (this.turn.unitsSummoned.length) {
            unitCount--;
        }
        if (this.turn.unitsMoved.length == unitCount) {
            // console.log('All units moved, progressing to attack phase')
            // this.turn.stage = TurnStage.Attack
        }
        this.calculateEnergyGain();
        this.emitGameState();
    }
    skipMove(player) {
        if (player.id != this.players[this.turn.playerIndex].id) {
            console.log('You cant skip another player move');
            return;
        }
        console.log('Player', this.turn.playerIndex, 'skipping move');
        // this.turn.stage = TurnStage.Attack
        console.log('next turn:');
        this.nextTurn();
        this.emitGameState();
    }
    attack(player, from, to) {
        if (player.id != this.players[this.turn.playerIndex].id) {
            console.log('You cant make another player attack');
            return;
        }
        console.log('Player', this.turn.playerIndex, 'attacking from', from, 'to', to);
        let hasSummoningSickness = false;
        for (const summoned of this.turn.unitsSummoned) {
            if (summoned.equals(from)) {
                hasSummoningSickness = true;
                break;
            }
        }
        if (hasSummoningSickness) {
            console.log('Them theres got the summoning sickness');
            return;
        }
        const fromUnitState = this.boardUnits.find(u => u.coord.equals(from));
        if (!fromUnitState) {
            console.log('Theres... nothing there. You cant attack with nothing.');
            return;
        }
        const fromUnit = UnitId.unit(fromUnitState.id);
        const toUnitState = this.boardUnits.find(u => u.coord.equals(to));
        if (!toUnitState) {
            console.log('You cant attack thin air');
            return;
        }
        const toUnit = UnitId.unit(toUnitState.id);
        const alreadyAttacked = this.turn.unitsAttacked.find(coord => coord.equals(from));
        if (alreadyAttacked) {
            console.log('This unit thinks one battle is enough for the time being');
            return;
        }
        const isFriendly = this.boardUnits.find(u => u.coord.equals(to) && u.playerIndex == this.turn.playerIndex);
        if (isFriendly) {
            console.log('Whoa there laddy, that ones on our side');
            return;
        }
        const healthRemaining = Math.max((toUnitState.remainingHealth || toUnit.health) - fromUnit.damage, 0);
        const damage = (toUnitState.remainingHealth || toUnit.health) - healthRemaining;
        console.log("Hit for", damage, 'damage');
        const attackingPlayerState = this.playerState[this.turn.playerIndex];
        const defendingPlayerState = this.playerState.filter(s => s != attackingPlayerState).at(0);
        attackingPlayerState.damageDealt += damage;
        defendingPlayerState.damageReceived += damage;
        toUnitState.remainingHealth = healthRemaining;
        if (healthRemaining == 0) {
            console.log('Ohhh shiiit, unit down!');
            defendingPlayerState.unitsLost++;
            let filtered = [];
            for (const unit of this.boardUnits) {
                if (unit.coord.equals(to)) {
                    continue;
                }
                filtered = [...filtered, unit];
            }
            this.boardUnits = filtered;
            if (toUnit.id == UnitId.Summoner) {
                console.log('💥💣🔥🧨🎇🚨 SUMMONER DOWN!!!');
                console.log('Player', this.turn.playerIndex, 'wins!!!!!');
                this.ended = new Date();
                this.players[0].gameId = -1;
                this.players[1].gameId = -1;
                const winningSummonerState = this.boardUnits.find(unit => unit.playerIndex == this.turn.playerIndex && unit.id == UnitId.Summoner);
                this.boardUnits = [winningSummonerState];
                this.emitGameStats();
                return;
            }
        }
        this.turn.unitsAttacked.push(from);
        this.calculateEnergyGain();
        this.emitGameState();
        const unitCount = this.boardUnits.filter(unit => unit.playerIndex == this.turn.playerIndex).length;
        if (this.turn.unitsAttacked.length == unitCount) {
            // console.log('All units attacked, progressing to next turn')
            // this.nextTurn()
        }
    }
    skipAttack(player) {
        if (player.id != this.players[this.turn.playerIndex].id) {
            console.log('You cant skip another player attack');
            return;
        }
        console.log('Player', this.turn.playerIndex, 'skipping attack');
        this.nextTurn();
    }
    emitGameState() {
        console.log('emitting state');
        this.eventEmitter.emit('gamestate', this);
    }
    recordDrawnUnitCost(player, cost) {
        const drawCostDistribution = this.playerState[player.index].drawCostDistribution;
        const previous = Number(drawCostDistribution.get(cost)) || 0;
        drawCostDistribution.set(cost, previous + 1);
    }
    emitGameStats() {
        console.log('emitting stats');
        this.eventEmitter.emit('gamestats', this);
    }
    getStats() {
        const gameStats = {
            id: this.id,
            turns: this.turn.index,
            started: this.started.valueOf(),
            ended: this.ended?.valueOf() || -1,
            winner: this.turn.playerIndex,
            player: [],
        };
        for (let i = 0; i < 2; i++) {
            let drawCostDistribution = {};
            for (let i2 = 1; i2 <= 6; i2++) {
                drawCostDistribution[i2] = this.playerState[i].drawCostDistribution.get(i2) || 0;
            }
            gameStats.player.push({
                unitsSummoned: this.playerState[i].unitsSummoned,
                unitsLost: this.playerState[i].unitsLost,
                damageDealt: this.playerState[i].damageDealt,
                damageReceived: this.playerState[i].damageReceived,
                energyGained: this.playerState[i].energyGained,
                energySpent: this.playerState[i].energySpent,
                drawCostDistribution,
            });
        }
        return gameStats;
    }
}
class GamePlayerState {
    draw = new UnitCollection([]);
    discard = new UnitCollection([]);
    hand = new UnitCollection([]);
    energy = 0;
    energyGain = 1;
    energyGained = 0;
    energySpent = 0;
    unitsSummoned = 0;
    unitsLost = 0;
    damageDealt = 0;
    damageReceived = 0;
    drawCostDistribution = new Map();
}
export var TurnStage;
(function (TurnStage) {
    TurnStage[TurnStage["Summon"] = 0] = "Summon";
    TurnStage[TurnStage["Move"] = 1] = "Move";
    TurnStage[TurnStage["Attack"] = 2] = "Attack";
})(TurnStage || (TurnStage = {}));
//# sourceMappingURL=game.js.map
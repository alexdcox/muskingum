/// <reference types="node" resolution-mode="require"/>
import { UnitCollection, UnitId } from "./unit.js";
import { DoubledCoord } from "./hex.js";
import { Player, PlayerId } from "./player.js";
import { WebSocket } from 'ws';
import { EventEmitter } from "events";
declare module "ws" {
    interface WebSocket {
        player: Player;
    }
}
export declare const EnergyCoords: [DoubledCoord, number][];
declare class Connection {
    socket: WebSocket;
    constructor(socket: WebSocket);
}
export declare class GameServer {
    connections: Connection[];
    games: Game[];
    registeredPlayers: Player[];
    combatantQueue: Player[];
    constructor();
    handleConnectionOpened(socket: WebSocket): void;
    handleConnectionClosed(socket: WebSocket): void;
    handleMessage(message: any, socket: WebSocket): void;
    startGame(): void;
}
export interface TurnState {
    index: number;
    playerIndex: number;
    unitsMoved: DoubledCoord[];
    unitsAttacked: DoubledCoord[];
    unitsSummoned: DoubledCoord[];
}
export declare class Game {
    id: number;
    started: Date;
    ended: Date | undefined;
    eventEmitter: EventEmitter;
    players: Player[];
    boardUnits: UnitState[];
    playerState: GamePlayerState[];
    turn: TurnState;
    constructor(player1: Player, player2: Player);
    getState(): GameState;
    getPlayerIds(): PlayerId[];
    nextTurn(): void;
    calculateEnergyGain(): void;
    summon(player: Player, unitId: UnitId, coord: DoubledCoord): void;
    skipSummon(player: Player): void;
    move(player: Player, from: DoubledCoord, to: DoubledCoord): void;
    skipMove(player: Player): void;
    attack(player: Player, from: DoubledCoord, to: DoubledCoord): void;
    skipAttack(player: Player): void;
    emitGameState(): void;
    recordDrawnUnitCost(player: Player, cost: number): void;
    emitGameStats(): void;
    getStats(): GameStats;
}
declare class GamePlayerState {
    draw: UnitCollection;
    discard: UnitCollection;
    hand: UnitCollection;
    energy: number;
    energyGain: number;
    energyGained: number;
    energySpent: number;
    unitsSummoned: number;
    unitsLost: number;
    damageDealt: number;
    damageReceived: number;
    drawCostDistribution: Map<number, number>;
}
export interface UnitState {
    id: UnitId;
    coord: DoubledCoord;
    playerIndex: number;
    remainingHealth?: number;
}
export declare enum TurnStage {
    Summon = 0,
    Move = 1,
    Attack = 2
}
export interface GameState {
    units: UnitState[];
    turn: TurnState;
    players: {
        index: number;
        energy: number;
        energyGain: number;
        draw: UnitId[];
        hand: UnitId[];
        discard: UnitId[];
    }[];
}
export interface GameStats {
    id: number;
    turns: number;
    started: number;
    ended: number;
    winner: number;
    player: {
        unitsSummoned: number;
        unitsLost: number;
        damageDealt: number;
        damageReceived: number;
        energyGained: number;
        energySpent: number;
        drawCostDistribution: Map<number, number>;
    }[];
}
export {};
//# sourceMappingURL=game.d.ts.map
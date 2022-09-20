/// <reference types="node" resolution-mode="require"/>
import { UnitCollection, UnitId } from "./unit.js";
import { DoubledCoord } from "./hex.js";
import { Player } from "./player.js";
import { WebSocket } from 'ws';
import { EventEmitter } from "events";
declare module "ws" {
    interface WebSocket {
        player: number;
    }
}
export declare const EnergyCoords: [DoubledCoord, number][];
declare class Connection {
    socket: WebSocket;
    constructor(socket: WebSocket);
}
export declare class GameServer {
    connections: Connection[];
    game?: Game;
    constructor();
    handleConnectionOpened(socket: WebSocket): void;
    handleMessage(message: any, socket: WebSocket): void;
    startGame(): void;
}
export interface TurnState {
    summoned: UnitState[];
    player: number;
    stage: TurnStage;
    unitsMoved: DoubledCoord[];
    unitsAttacked: DoubledCoord[];
}
export declare class Game {
    eventEmitter: EventEmitter;
    players: Player[];
    boardUnits: UnitState[];
    playerState: GamePlayerState[];
    turn: TurnState;
    constructor();
    getState(): GameState;
    nextTurn(): void;
    calculateEnergyGain(): void;
    summon(unitId: UnitId, coord: DoubledCoord): void;
    skipSummon(player: number): void;
    move(from: DoubledCoord, to: DoubledCoord): void;
    skipMove(player: number): void;
    attack(from: DoubledCoord, to: DoubledCoord): void;
    skipAttack(player: number): void;
    emitGameState(): void;
}
declare class GamePlayerState {
    draw: UnitCollection;
    discard: UnitCollection;
    hand: UnitCollection;
    energy: number;
    energyGain: number;
}
export interface UnitState {
    id: UnitId;
    coord: DoubledCoord;
    player: number;
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
        id: number;
        energy: number;
        energyGain: number;
        draw: UnitId[];
        hand: UnitId[];
        discard: UnitId[];
    }[];
}
export {};
//# sourceMappingURL=game.d.ts.map
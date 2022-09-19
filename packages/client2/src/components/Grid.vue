<script setup lang="ts">
import {
  Units,
  DoubledCoord,
  GameState,
  Hex,
  hexSetBounds,
  Layout,
  Orientation,
  Unit,
  UnitId,
  UnitMap,
  TurnStage
} from 'engine';
import Hand from './Hand.vue'
import Board from './Board.vue'
import Draw from './Draw.vue'

import {Ref, ref} from "vue";
import {Colors, makeGrid, Tile} from "../util"
import {UnitState} from 'engine'
import {EventEmitter} from "events"

const s = 50

const columns = 9
const rows = 9

let highlight = ref({}) as Ref<Tile>

const state = ref({}) as Ref<GameState>

const events = new EventEmitter()

const fakeHand: Unit[] = [
  UnitMap.get(UnitId.Enforcer)!,
  UnitMap.get(UnitId.Scavenger)!,
  UnitMap.get(UnitId.Batariel)!,
  UnitMap.get(UnitId.FireStalker)!,
  UnitMap.get(UnitId.MagmaSpore)!,
]

const onTileMouseover = (tile: Tile) => {
  highlight.value = tile
}

if (!window.game) {
  window.game = {}
}

const reconnect = (): WebSocket => {
  if (!window?.game?.ws) {
    const ws = new WebSocket("ws://10.144.73.158:3030")
    window.game.ws = ws
    return ws
  }
  return window.game.ws
}

const ws = reconnect()

const login = (name: string) => {
  console.log('sending login as', name)
  const loginMessage = {type: 'login', name}
  ws.send(JSON.stringify(loginMessage))
}

const getGameState = () => {
  console.log('sending get game state')
  const message = {type: 'gamestate'}
  ws.send(JSON.stringify(message))
}

ws.addEventListener('open', event => {
  console.log('Websocket connection opened')
})

let playerId = ref(0)

ws.onmessage = function (event) {
  console.log('Incoming websocket message:', event.data)
  const message = JSON.parse(event.data)
  switch (message.type) {
    case "playerid":
      playerId.value = message.id
      events.emit('playerid', message.id)
      break

    case "gamestate":
      const gameState: GameState = message.state
      gameState.units = gameState.units.map(state => ({
        id: state.id,
        player: state.player,
        remainingHealth: state.remainingHealth,
        coord: new DoubledCoord(state.coord.col, state.coord.row),
      }))
      // if (gameState.turn.summoned) {}
      state.value = gameState
      events.emit('gamestate', gameState)
      break
  }
}

// ws.addEventListener('message', event => {
// })

// ws.addEventListener('close', event => {
//   console.log('Websocket connection closed')
//   setTimeout(reconnect, 3000)
// })

events.on('-summon', ({unitId, coord}) => {
  ws.send(JSON.stringify({type: 'summon', unitId, coord}))
})

events.on('-move', ({from, to}) => {
  ws.send(JSON.stringify({type: 'move', from, to}))
})

events.on('-attack', ({from, to}) => {
  ws.send(JSON.stringify({type: 'attack', from, to}))
})

const skip = (state: GameState) => {
  console.log('sending skip', state?.turn?.stage)
  switch (state?.turn?.stage) {
    case TurnStage.Summon:
      ws.send(JSON.stringify({type: 'skipsummon'}))
      break
    case TurnStage.Move:
      ws.send(JSON.stringify({type: 'skipmove'}))
      break
    case TurnStage.Attack:
      ws.send(JSON.stringify({type: 'skipattack'}))
      break
  }
}

const getStageName = (state: GameState) => {
  switch (state?.turn?.stage) {
    case TurnStage.Summon:
      return 'summon'
    case TurnStage.Move:
      return 'move'
    case TurnStage.Attack:
      return 'attack'
    default:
      return ''
  }
}


</script>

<template>
  <div class="game">
    <div class="info">
      col: {{ highlight?.coord?.col }} row: {{ highlight?.coord?.row }}
      &nbsp;
      <template v-if="playerId > 0">
        <span>You are player {{ playerId }}.</span>
        &nbsp;
        <span>You have {{ state?.players?.[playerId - 1].energy }} energy (+{{state?.players?.[playerId - 1].energyGain}} per turn)</span>
        &nbsp;
        <span>It's player {{state?.turn?.player}}s go to {{getStageName(state)}}.</span>
      </template>
    </div>

    <div class="draw1">
      <Draw :events="events" :player="1"/>
    </div>

    <div class="draw2">
      <Draw :events="events" :player="2"/>
    </div>

    <div class="board">
      <Board :events="events" :on-tile-mouseover="onTileMouseover"></Board>
    </div>

    <div class="hands">
      <div class="p1">
        <Hand :events="events" :player="1"/>
      </div>
      <div class="controls">
        <button @mousedown="skip(state)">Skip</button>
      </div>
      <div class="p2">
        <Hand :events="events" :player="2"/>
      </div>
    </div>

    <div class="draw"></div>

  </div>
</template>

<style scoped>

.game {
  /*background: orange;*/
  display: grid;
  width: 100%;
  height: 100%;
  grid-template-columns: 15vw 70vw 15vw;
  grid-template-rows: 5vh 75vh 20vh;
  grid-template-areas:
    "info info info"
    "draw1 board draw2"
    "draw1 hands draw2";
}

.info {
  grid-area: info;
}

.board {
  grid-area: board;
  /*background: slategray;*/
  display: flex;
  align-items: center;
  height: 100%;
}

.hands {
  grid-area: hands;
  /*background: blue;*/
  display: flex;
  flex: 1 1 auto;
  overflow: hidden;
  justify-content: space-between;
  padding: 10px
}

.draw1 {
  grid-area: draw1;
  /*background: aqua;*/
}

.draw2 {
  grid-area: draw2;
  /*background: cadetblue;*/
}

svg.board {
  border: 1px solid rgba(255, 255, 255, 0.1);
  /*max-height: 100vh;*/
  /*min-height: 15vh;*/
}

svg {
  fill: white;
}


</style>

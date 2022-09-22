<script setup lang="ts">
import {
  DoubledCoord,
  GameState,
  TurnStage
} from 'engine';
import Hand from './Hand.vue'
import Board from './Board.vue'
import Draw from './Draw.vue'

import {Ref, ref} from "vue";
import {Tile} from "../util"
import {EventEmitter} from "events"

const events = new EventEmitter()

const state = ref({}) as Ref<GameState>
let playerId = ref(0)
let highlight = ref({}) as Ref<Tile>
let ws: WebSocket|undefined

const onTileMouseover = (tile: Tile) => {
  highlight.value = tile
}

if (!window.game) {
  window.game = {}
}

const fakeConnection = () => {
  const fakeState = {
    "units": [
      // {
      //   "id": 0,
      //   "player": 1,
      //   "coord": new DoubledCoord(0, 0)
      // },
      {
        "id": 0,
        "player": 1,
        "coord": new DoubledCoord(0, 2)
      },
      {
        "id": 0,
        "player": 2,
        "coord": new DoubledCoord(12, 2)
      }
    ],
    "players": [{
      "id": 1,
      "energy": 6,
      "energyGain": 1,
      "draw": [21, 11, 1, 23, 19, 5, 16, 20, 3, 4, 18, 9, 12, 6, 22, 8, 2, 24, 15],
      "hand": [17, 7, 14, 13, 10],
      "discard": []
    }, {
      "id": 2,
      "energy": 0,
      "energyGain": 1,
      "draw": [3, 7, 23, 19, 5, 22, 17, 4, 12, 15, 20, 13, 21, 24, 8, 1, 16, 6, 2],
      "hand": [18, 9, 11, 10, 14],
      "discard": []
    }],
    "turn": {"player": 1, "stage": 0, "unitsMoved": [], "unitsAttacked": []}
  }

  if (import.meta.hot) {
    import.meta.hot.on('vite:beforeUpdate', () => {
      setTimeout(() => {
        console.log('re-emitting')
        events.emit('playerid', 1)
        events.emit('gamestate', fakeState)
      }, 500)
    });
  }

  setTimeout(() => {
    events.emit('playerid', 1)
    events.emit('gamestate', fakeState)
  }, 100)
}

const realConnection = () => {
  if (window?.game?.ws) {
    return
  }

  ws = new WebSocket(`ws://${window.location.hostname}:3030`)
  window.game.ws = ws

  ws.addEventListener('open', event => {
    console.log('Websocket connection opened')
  })

  ws.onmessage = function (event) {
    console.log('Incoming websocket message:', event.data)
    const message = JSON.parse(event.data)
    switch (message.type) {
      case "playerid":
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
        events.emit('gamestate', gameState)
        break
    }
  }

  return window.game.ws
}

// realConnection()
fakeConnection()

events.on('playerid', (id) => {
  playerId.value = id
})

events.on('gamestate', (gameState) => {
  state.value = gameState
})

events.on('-summon', ({unitId, coord}) => {
  ws?.send(JSON.stringify({type: 'summon', unitId, coord}))
})

events.on('-move', ({from, to}) => {
  ws?.send(JSON.stringify({type: 'move', from, to}))
})

events.on('-attack', ({from, to}) => {
  ws?.send(JSON.stringify({type: 'attack', from, to}))
})

function skip(state: GameState) {
  console.log('sending skip', state?.turn?.stage)
  switch (state?.turn?.stage) {
    case TurnStage.Summon:
      ws?.send(JSON.stringify({type: 'skipsummon'}))
      break
    case TurnStage.Move:
      ws?.send(JSON.stringify({type: 'skipmove'}))
      break
    case TurnStage.Attack:
      ws?.send(JSON.stringify({type: 'skipattack'}))
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
<!--    <div class="info">-->
<!--      col: {{ highlight?.coord?.col }} row: {{ highlight?.coord?.row }}-->
<!--      &nbsp;-->
<!--      <template v-if="playerId > 0">-->
<!--        <span>You are player {{ playerId }}.</span>-->
<!--        &nbsp;-->
<!--        <span>You have {{-->
<!--            state?.players?.[playerId - 1].energy-->
<!--          }} energy (+{{ state?.players?.[playerId - 1].energyGain }} per turn)</span>-->
<!--        &nbsp;-->
<!--        <span>It's player {{ state?.turn?.player }}s go to {{ getStageName(state) }}.</span>-->
<!--      </template>-->
<!--    </div>-->

<!--    <div class="draw1">-->
<!--      <Draw :events="events" :player="1"/>-->
<!--    </div>-->

<!--    <div class="draw2">-->
<!--      <Draw :events="events" :player="2"/>-->
<!--    </div>-->

<!--    <div class="board">-->
<!--      <Board :events="events" :on-tile-mouseover="onTileMouseover"></Board>-->
<!--    </div>-->

<!--    <div class="hands">-->
<!--      <div class="p1">-->
<!--        <Hand :events="events" :player="1"/>-->
<!--      </div>-->
<!--      <div class="controls">-->
<!--        <button @mousedown="skip(state)">Skip</button>-->
<!--        <button @mousedown="skip(state)">Undo</button>-->
<!--        <button @mousedown="skip(state)">Resign</button>-->
<!--      </div>-->
<!--      <div class="p2">-->
<!--        <Hand :events="events" :player="2"/>-->
<!--      </div>-->
<!--    </div>-->

    <CanvasAttempt/>

    <div class="draw"></div>

  </div>
</template>

<style scoped>

.game {
  display: grid;
  width: 100%;
  height: 100%;
  grid-template-columns: 15vw 70vw 15vw;
  grid-template-rows: 5vh 70vh 25vh;
  grid-template-areas:
    "info info info"
    "draw1 board draw2"
    "draw1 hands draw2";
}

.info {
  grid-area: info;
}

div.board {
  grid-area: board;
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
  /*padding: 10px*/
}

.hands .p1 {
  display: flex;
  flex: 1 1 auto;
}

.hands .p2 {
  display: flex;
  flex: 1 1 auto;
}

.draw1 {
  grid-area: draw1;
  /*background: aqua;*/
}

.draw2 {
  grid-area: draw2;
  /*background: cadetblue;*/
}

.controls {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  padding: 20px 0 20px 0
}



</style>

<script setup lang="ts">

import {GameState, hexSetBounds, Layout, Orientation, TurnStage, Unit, UnitId} from "engine"
import {Colors, makeGrid, Tile} from "../util"
import {Ref, ref} from "vue"
import HexTile from './HexTile.vue'
import UnitCost from './UnitCost.vue'
import {EventEmitter} from "events"

interface Props {
  events: EventEmitter,
  player: number
}

const props = defineProps<Props>()

let units = ref([]) as Ref<Unit[]>
let summoning = ref(false)
let currentPlayer = ref(-1)

props.events.on('playerid', (id: number) => {
  currentPlayer.value = id
})

props.events.on('gamestate', (state: GameState) => {
  highlight.value = undefined
  let handUnitIds = state.players[props.player - 1].hand
  units.value = handUnitIds.map(id => UnitId.unit(id)).reverse()
  summoning.value = state.turn.player == props.player &&
      props.player == currentPlayer.value &&
      state.turn.stage == TurnStage.Summon
  redraw()
})

const layout = new Layout(Orientation.pointy, {width: 40, height:36}, {x: 0, y: 0})

let emptyGrid = makeGrid(layout, 1, 6, 0, 2)

let highlight = ref({}) as Ref<Tile | undefined>

const padding = 5

const viewBox = (() => {
  let {left, top, right, bottom} = hexSetBounds(layout, emptyGrid.map(t => t.hex))
  left = left - padding - 1
  top = top - padding - 1
  let width = right - left + 2 * padding
  let height = bottom - top + 2 * padding
  return [left, top, width, height].join(' ');
})()

const getUnitImage = (unit: Unit) => {
  return new URL(`../assets/images/units/${unit.fileName}.webp`, import.meta.url).href
}

const handleMouseoverTile = (tile: Tile) => {
  if (!summoning.value) {
    return
  }
  highlight.value = tile
  redraw()
}

const handleClickTile = (tile: Tile) => {
  if (!summoning.value) {
    return
  }
  props.events.emit('-setsummon', tile.unit)
  // props.ws.send(JSON.stringify({type: 'summon'}))
}

const withStyles = (tiles: Tile[]): Tile[] => {
  const styledTop: Tile[] = []
  const styledBottom: Tile[] = []
  for (const tile of tiles) {
    let style: {
      fill: string
      stroke: string
      darkStroke: string
    } = {
      stroke: 'white',
      darkStroke: 'black',
      fill: Colors.white,
    }

    if (tile.unit) {
      style.fill = `url(#${tile?.unit?.fileName})`
    }

    switch (props?.player) {
      case 1:
        style.stroke = "red"
        style.darkStroke = '#490000'
        break
      case 2:
        style.stroke = "blue"
        style.darkStroke = "#000099"
        break
    }

    if (highlight.value?.coord?.equals(tile.coord)) {
      styledTop.push({
        ...tile, style: {
          stroke: Colors.selectedStroke,
          darkStroke: Colors.selectedDarkStroke,
          cursor: 'pointer',
          fill: style.fill,
        }
      })
      continue
    }

    styledBottom.push({...tile, style})
  }
  return [...styledBottom, ...styledTop]
}

const withPlayerUnits = (tiles: Tile[]): Tile[] => {
  return tiles.map((tile, index) => {
    const unit = units.value?.[index]
    return {
      ...tile,
      unit,
      image: unit ? getUnitImage(unit) : undefined,
    }
  })
}

let grid = ref(emptyGrid)

const redraw = () => {
  let tiles = emptyGrid
  tiles = withPlayerUnits(tiles)
  tiles = withStyles(tiles)
  grid.value = tiles
}
redraw()

</script>

<template>
  <svg :class="{hand: true}" :viewBox="viewBox">
    <g v-for="(tile, k) in grid">
      <HexTile :k="k" :tile="tile" :mouseover="handleMouseoverTile" :mousedown="handleClickTile"/>
    </g>
    <g v-for="(tile, k) in grid">
      <UnitCost :k="k" :tile="tile"/>
    </g>
  </svg>
</template>

<style scoped>

svg.hand {
  width: auto;
  height: 100%;
  margin: 0 auto;
}

text {
  text-anchor: middle;
  font-size: 0.6em;
  fill: rgba(0, 0, 0, 0.9);
  pointer-events: none;
  font-family: "Permanent Marker", sans-serif;
}

.remainingHealth text {
  transform: translate(0px, 3px);
  fill: white;
}

</style>
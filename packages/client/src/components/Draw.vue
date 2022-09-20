<script setup lang="ts">

import {
  DoubledCoord,
  GameState,
  Hex,
  hexSetBounds,
  Layout,
  Orientation,
  Point,
  Unit,
  UnitId,
  UnitMap,
  Units
} from "engine"
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

interface DrawableUnit {
  unit: Unit
  drawn: boolean
}

const drawables = ref([]) as Ref<DrawableUnit[]>

const sortUnitsByCost = (a: Unit, b: Unit) => {
  if (a.cost < b.cost) {
    return -1
  }
  if (a.cost > b.cost) {
    return 1
  }
  return a.name.localeCompare(b.name)
}

props.events.on('gamestate', (state: GameState) => {
  let availableUnitIds = state.players[props.player - 1].draw
  let allUnits = Units.filter(unit => unit.id != UnitId.Summoner)
  let draw: DrawableUnit[] = []
  for (const unit of allUnits) {
    draw.push({unit, drawn: !availableUnitIds.includes(unit.id)})
  }
  draw.sort((a, b) => sortUnitsByCost(a.unit, b.unit))
  drawables.value = draw
  redraw()
})

const layout = new Layout(Orientation.pointy, {width: 40, height:36}, {x: 0, y: 0})

let emptyGrid = makeGrid(layout, 0, 4, 0, 12)
let highlight = ref({}) as Ref<Tile>

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
  if (!unit) {
    return '#'
  }
  return new URL(`../assets/images/units/${unit?.fileName}.webp`, import.meta.url).href
}

const withStyles = (tiles: Tile[]): Tile[] => {
  const styled: Tile[] = []
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

    styled.push({...tile, style})
  }
  return styled
}

const withPlayerUnits = (tiles: Tile[]): Tile[] => {
  return tiles.map((tile, index) => {
    const unit = drawables?.value?.[index]?.unit
    return {
      ...tile,
      unit,
      image: getUnitImage(unit),
      hasBeenDrawn: drawables?.value?.[index]?.drawn
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
  <svg class="draw" :viewBox="viewBox">
    <g v-for="(tile, k) in grid" :class="{hasBeenDrawn: tile.hasBeenDrawn}">
      <HexTile :k="k" :tile="tile"/>
    </g>
    <g v-for="(tile, k) in grid">
      <UnitCost :k="k" :tile="tile"/>
    </g>
  </svg>
</template>

<style scoped>
svg.draw {
  /*height: 100%;*/
}

text {
  text-anchor: middle;
  font-size: 0.6em;
  fill: rgba(0, 0, 0, 0.9);
  pointer-events: none;
  font-family: "Permanent Marker", sans-serif;
}

.hasBeenDrawn {
  opacity: 0.2;
}

</style>
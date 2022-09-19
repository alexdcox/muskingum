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
import {Colors, Tile} from "../util"
import {PropType, Ref, ref} from "vue"
import SwordSvg from './sword.vue'
import HeartSvg from './heart.vue'
import ShoeSvg from './shoe.vue'
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

const layout = new Layout(Orientation.flat, {width: 50, height: 50}, {x: 0, y: 0})

function makeGrid(layout: Layout, minCol: number, maxCol: number, minRow: number, maxRow: number): Tile[] {
  let results = [];
  let q = 0
  let r = 0
  let f = 0 // counter to protect against infinite loops
  while (true) {
    f++
    if (props.player == 1) {
      // player 1
      if (q > maxCol - 1) {
        q = 0
        r++
      }
      if (r > maxRow - 1) {
        break
      }
    } else {
      // player 2
      if (q < -(maxCol - 1)) {
        q = 0
        r -= 2
        if (r > maxRow - 1) {
          break
        }
      }
    }
    if (f > 200) {
      break
    }
    const hex = Hex.axial(q, r)
    const pos = layout.hexToPixel(hex)
    const coord = DoubledCoord.qdoubledFromCube(hex)
    const tile: Tile = {
      hex,
      coord,
      pos,
      points: layout.polygonCorners(hex).map((p: Point) => [p.x, p.y].join(',')).join(' '),
      translate: `translate(${pos.x},${pos.y})`,
    }
    results.push(tile);
    if (props.player == 1) {
      // player 1
      q++
    } else {
      // player 2
      q--
      r++
    }
  }
  return results;
}

let emptyGrid = makeGrid(layout, 0, 3, 0, 8)
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
    <g v-for="(tile, index) in grid" :class="{hasBeenDrawn: tile.hasBeenDrawn}">
      <defs>
        <template v-if="tile?.image">
          <pattern :id="tile?.unit?.fileName" height="100%" width="100%" patternContentUnits="objectBoundingBox">
            <image
                height="1"
                width="1"
                preserveAspectRatio="none"
                :href="tile?.image"/>
          </pattern>
        </template>
      </defs>
      <polygon
          :style="tile.style"
          :points="tile.points"
          :class="{hex: true, highlight: tile.hex.equals(highlight.hex)}">
      </polygon>
      <text :transform="tile.translate">{{ index }} ({{ tile.coord.col }},{{ tile.coord.row }})</text>
      <template v-if="tile.unit">
        <g class="unit-info">
          <g :transform="tile.translate">
            <g class="damage">
              <SwordSvg x="0" y="0" width="14px"/>
            </g>
            <g class="health">
              <HeartSvg x="0" y="0" width="14px"/>
            </g>
            <g class="movement">
              <ShoeSvg x="0" y="0" width="14px"/>
            </g>
            <text class="name">{{ tile.unit.name }}</text>
            <text class="damage">{{ tile.unit.damage }}</text>
            <text class="health">{{ tile.unit.health }}</text>
            <text class="movement">{{ tile.unit.movement }}</text>
          </g>
        </g>
      </template>
    </g>
    <g v-for="(tile) in grid">
      <template v-if="tile.unit">
        <g :transform="tile.translate">
          <g v-if="tile.unit.cost" transform="translate(24,-41)" class="cost" :class="{cost: true, hasBeenDrawn: tile.hasBeenDrawn}">
            <circle :style="{stroke: tile?.style?.stroke, fill: tile?.style?.darkStroke}" r="9px"></circle>
            <text>{{ tile.unit.cost }}</text>
          </g>
        </g>
      </template>
    </g>
  </svg>
</template>

<style scoped>
svg.draw {
  border: 1px solid rgba(255, 255, 255, 0.1);
}

text {
  text-anchor: middle;
  font-size: 0.6em;
  fill: rgba(0, 0, 0, 0.9);
  pointer-events: none;
  font-family: "Permanent Marker", sans-serif;
}

text.mouseover {
  fill: white;
  font-family: "consolas", sans-serif;
  font-size: 8px;
}

text.q {
  transform: translate(0, -30px);
}

text.r {
  transform: translate(25px, 25px);
}

text.s {
  transform: translate(-25px, 25px);
}

polygon.highlight {
  opacity: 0.9;
}

polygon.hex {
  stroke-width: 3px
}

.unit-info {
  opacity: 0.7;
}

.unit-info text {
  stroke-width: 9px;
  paint-order: stroke fill;
}

.unit-info text.name {
  /*transform: translate(0, -15px);*/
  fill: white;
  stroke: black;
}

.unit-info text.damage {
  transform: translate(-18px, 33px);
  fill: black;
  stroke: white;
}

.unit-info text.health {
  transform: translate(1px, 33px);
  fill: black;
  stroke: white;
}

.unit-info text.movement {
  transform: translate(20px, 33px);
  fill: black;
  stroke: white;
}

.unit-info g.damage {
  transform: translate(-24px, -384px);
  fill: white;
}

.unit-info g.health {
  transform: translate(-5px, -384px);
  fill: white;
}

.unit-info g.movement {
  transform: translate(13px, -384px);
  fill: white;
}

.cost circle {
  stroke-width: 2px;
}

.cost text {
  fill: white;
  transform: translate(0px, 4px);
}

.remainingHealth text {
  transform: translate(0px, 3px);
  fill: white;
}

.hasBeenDrawn {
  opacity: 0.2;
}

</style>
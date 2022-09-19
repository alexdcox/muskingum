<script setup lang="ts">

import {GameState, hexSetBounds, Layout, Orientation, TurnStage, Unit, UnitId} from "engine"
import {Colors, makeGrid, Tile} from "../util"
import {Ref, ref} from "vue"
import SwordSvg from './sword.vue'
import HeartSvg from './heart.vue'
import ShoeSvg from './shoe.vue'
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
  const isSummoning = state.turn.player == props.player &&
      props.player == currentPlayer.value &&
      state.turn.stage == TurnStage.Summon
  console.log(props.player, isSummoning)
  summoning.value = isSummoning
  redraw()
})

const layout = new Layout(Orientation.flat, {width: 50, height: 50}, {x: 0, y: 0})
let emptyGrid = makeGrid(layout, 0, 5, 0, 2)
let highlight = ref({}) as Ref<Tile|undefined>

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
      styledTop.push({...tile, style: {
        stroke: Colors.selectedStroke,
        darkStroke: Colors.selectedDarkStroke,
        cursor: 'pointer',
        fill: style.fill,
      }})
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
  <svg :class="{board: true}" :viewBox="viewBox">
    <g v-for="(tile) in grid">
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
          @mouseover="handleMouseoverTile(tile)"
          @mousedown="handleClickTile(tile)"
          :class="{hex: true, highlight: highlight?.hex?.equals(tile.hex)}">
      </polygon>
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
          <g :transform="tile.translate">
          </g>
        </g>
      </template>
    </g>
    <g v-for="(tile) in grid">
      <template v-if="tile.unit">
        <g :transform="tile.translate">
          <g v-if="tile.unit.cost" transform="translate(24,-41)" class="cost">
            <circle :style="{stroke: tile?.style?.stroke, fill: tile?.style?.darkStroke}" r="9px"></circle>
            <text>{{ tile.unit.cost }}</text>
          </g>
        </g>
      </template>
    </g>
  </svg>
</template>

<style scoped>

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

.unit-info g.damage {
  transform: translate(-24px, -60px);
  fill: white;
}

.unit-info text.health {
  transform: translate(1px, 33px);
  fill: black;
  stroke: white;
}

.unit-info g.health {
  transform: translate(-5px, -60px);
  fill: white;
}

.unit-info text.movement {
  transform: translate(20px, 33px);
  fill: black;
  stroke: white;
}

.unit-info g.movement {
  transform: translate(13px, -60px);
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

</style>
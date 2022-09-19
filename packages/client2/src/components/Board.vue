<script setup lang="ts">
import {DoubledCoord, GameState, hexSetBounds, Layout, Orientation, TurnStage, Unit, UnitId, UnitMap} from "engine"
import {Colors, makeGrid, Tile} from "../util"
import {ref, Ref} from "vue"
import SwordSvg from './sword.vue'
import HeartSvg from './heart.vue'
import ShoeSvg from './shoe.vue'
import {EventEmitter} from "events"

interface Props {
  events: EventEmitter,
  onTileMouseover?: (tile: Tile) => void
  onTileMousedown?: (tile: Tile) => void
}

const props = defineProps<Props>()

const options = {
  showQRSCoords: false,
  showDoubleCoords: false,
}

const padding = 15
const layout = new Layout(Orientation.flat, {width: 50, height: 50}, {x: 0, y: 0})

let state = ref({}) as Ref<GameState>
let currentPlayer = ref(0)
let highlight = ref({}) as Ref<Tile>
let selected = ref({}) as Ref<Tile|undefined>
let emptyGrid = makeGrid(layout, 0, 9, 0, 9)
const summonUnit = ref(undefined) as Ref<Unit|undefined>
const moveFrom = ref(undefined) as Ref<DoubledCoord|undefined>
const attackFrom = ref(undefined) as Ref<DoubledCoord|undefined>

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

const onTileMouseover = (tile: Tile) => {
  highlight.value = tile
  props.onTileMouseover?.(tile)
  redraw()
}

const onTileMousedown = (tile: Tile) => {
  if (summonUnit.value) {
    props.events.emit('-summon', {unitId: summonUnit.value.id, coord: tile.coord})
    return
  }

  if (state.value.turn.stage == TurnStage.Attack) {
    if (attackFrom.value) {
      props.events.emit('-attack', {from: attackFrom.value, to: tile.coord})
      attackFrom.value = undefined
      return
    }
    if (tile.unitState?.player != currentPlayer.value) {
      return
    }
    attackFrom.value = tile.coord
    redraw()
    return
  }

  if (state.value.turn.stage == TurnStage.Move) {
    if (moveFrom.value) {
      props.events.emit('-move', {from: moveFrom.value, to: tile.coord})
      moveFrom.value = undefined
      return
    }
    if (tile.unitState?.player != currentPlayer.value) {
      return
    }
    moveFrom.value = tile.coord
    redraw()
    return
  }

  // if (selected.value && selected.value?.coord?.equals(tile.coord)) {
  //   selected.value = undefined
  // } else {
  //   selected.value = tile
  // }

  // state.units.push({
  //   id: UnitId.Emberstrike,
  //   coord: tile.coord,
  //   player: 1,
  // })

  redraw()
  props.onTileMousedown?.(tile)
}

const withPlayerUnits = (tiles: Tile[]): Tile[] => {
  const withUnits: Tile[] = []
  for (const tile of tiles) {
    const p = {...tile}
    const unitState = state.value.units?.find(u => u.coord.equals(tile.coord))
    if (unitState) {
      p.unitState = unitState
      p.unit = UnitMap.get(unitState.id)
      p.image = getUnitImage(p.unit!)
    }
    withUnits.push(p)
  }
  return withUnits
}

const withStyles = (tiles: Tile[]): Tile[] => {
  const styled: Tile[] = []
  for (const tile of tiles) {
    let style: any = {
      stroke: 'white',
      darkStroke: 'black',
      fill: Colors.white,
    }

    if (tile.unit) {
      style.fill = `url(#${tile?.unit?.fileName})`
    }

    switch (tile?.unitState?.player) {
      case 1:
        style.stroke = "red"
        style.darkStroke = '#490000'
        break
      case 2:
        style.stroke = "blue"
        style.darkStroke = "#000099"
        break
    }

    if (selected?.value?.coord?.equals(tile.coord)) {
      style.stroke = "green"
      style.fill = "darkgreen"
      style.darkStroke = "darkgreen"
    }

    styled.push({...tile, style})
  }
  return styled
}

const withPlayerTilesSortedLast = (tiles: Tile[]): Tile[] => {
  const player = []
  const nonPlayer = []
  for (const tile of tiles) {
    if (tile?.unitState?.player) player.push(tile)
    else nonPlayer.push(tile)
  }
  return [...nonPlayer, ...player]
}

let grid = ref(emptyGrid)

const withSummonHighlight  = (tiles: Tile[]): Tile[] => {
  if (!summonUnit.value) {
    return tiles
  }

  const summonerTile = tiles.find(tile => tile.unitState?.player == currentPlayer.value && tile.unit?.id == UnitId.Summoner)!
  const summonerCoords = summonerTile.coord
  const summonerHex = summonerCoords.qdoubledToCube()

  const processedTop: Tile[] = []
  const processedBottom: Tile[] = []

  for (const tile of tiles) {
    if (tile.hex.distance(summonerHex) == 0) {
      processedTop.push({...tile})
    } else if (tile.hex.distance(summonerHex) == 1) {
      processedTop.push({...tile, style: {
          stroke: Colors.summonHighlightStroke,
          fill: Colors.summonHighlightFill,
          cursor: 'pointer',
        }})
    } else {
      processedBottom.push({...tile})
    }
  }

  return [...processedBottom, ...processedTop]
}

const withMoveHighlight  = (tiles: Tile[]): Tile[] => {
  if (state.value?.turn?.stage != TurnStage.Move) {
    return tiles
  }

  const processedTop: Tile[] = []
  const processedBottom: Tile[] = []

  if (moveFrom.value) {
    const moveFromTile = tiles.find(tile => tile.coord.equals(moveFrom.value!))!
    const unitMovement = moveFromTile.unit!.movement

    for (const tile of tiles) {
      const tileDistance = tile.hex.distance(moveFromTile.hex)
      if (tileDistance == 0) {
        processedTop.push({
          ...tile, style: {
            ...tile.style,
            stroke: Colors.selectedStroke,
            cursor: 'pointer',
          }
        })
      } else if (tile.unit) {
        processedTop.push({...tile})
      } else if (tileDistance > 0 && tileDistance <= unitMovement) {
        processedTop.push({...tile, style: {
            stroke: Colors.moveHighlightStroke,
            fill: Colors.moveHighlightFill,
            cursor: 'pointer',
          }})
      } else {
        processedBottom.push({...tile})
      }
    }
  } else {
    for (const tile of tiles) {
      const hasBeenMoved = state.value?.turn?.unitsMoved?.find(coord => tile?.coord?.equals(coord)) != undefined
      if (highlight.value?.coord?.equals(tile.coord) &&
          tile?.unitState?.player == currentPlayer.value &&
          !highlight.value?.coord?.equals(state.value?.turn?.summoned?.coord) &&
          !hasBeenMoved
      ) {
        processedTop.push({
          ...tile, style: {
            ...tile.style,
            stroke: Colors.selectedStroke,
            cursor: 'pointer',
          }
        })
      } else {
        processedBottom.push(tile)
      }
    }
  }

  return [...processedBottom, ...processedTop]
}

const withAttackHighlight  = (tiles: Tile[]): Tile[] => {
  if (state.value?.turn?.stage != TurnStage.Attack) {
    return tiles
  }

  const processedTop: Tile[] = []
  const processedBottom: Tile[] = []

  if (attackFrom.value) {
    const attackFromTile = tiles.find(tile => tile.coord.equals(attackFrom.value!))!

    for (const tile of tiles) {
      const tileDistance = tile.hex.distance(attackFromTile.hex)
      if (tileDistance == 0) {
        processedTop.push({
          ...tile, style: {
            ...tile.style,
            stroke: Colors.selectedStroke,
            cursor: 'pointer',
          }
        })
      } else if (tileDistance == 1 && tile.unit) {
        processedTop.push({...tile, style: {
            ...tile.style,
            stroke: Colors.attackHighlightStroke,
            cursor: 'pointer',
          }})
      } else {
        processedBottom.push({...tile})
      }
    }
  } else {
    for (const tile of tiles) {
      const hasBeenMoved = state.value.turn.unitsMoved.find(coord => tile.coord.equals(coord)) != undefined
      if (highlight.value?.coord?.equals(tile.coord) &&
          tile?.unitState?.player == currentPlayer.value &&
          !highlight.value.coord.equals(state.value?.turn?.summoned?.coord) &&
          !hasBeenMoved
      ) {
        processedTop.push({
          ...tile, style: {
            ...tile.style,
            stroke: Colors.selectedStroke,
            cursor: 'pointer',
          }
        })
      } else {
        processedBottom.push(tile)
      }
    }
  }

  return [...processedBottom, ...processedTop]
}


const redraw = () => {
  let tiles = emptyGrid
  tiles = withPlayerUnits(tiles)
  tiles = withStyles(tiles)
  tiles = withPlayerTilesSortedLast(tiles)
  tiles = withSummonHighlight(tiles)
  tiles = withMoveHighlight(tiles)
  tiles = withAttackHighlight(tiles)
  grid.value = tiles
}

redraw()

props.events.on('playerid', (id) => {
  currentPlayer.value = id
})

props.events.on('gamestate', (gameState: GameState) => {
  summonUnit.value = undefined
  moveFrom.value = undefined
  attackFrom.value = undefined
  selected.value = undefined
  state.value = gameState
  redraw()
})

props.events.on('-setsummon', (unit: Unit) => {
  summonUnit.value = unit
  redraw()
})

</script>

<template>
  <svg class="board" :viewBox="viewBox">
    <g v-for="(tile) in grid">
      <defs>
        <template v-if="tile.image">
          <pattern :id="tile.unit?.fileName" height="100%" width="100%" patternContentUnits="objectBoundingBox">
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
          @mouseover="onTileMouseover(tile)"
          @mousedown="onTileMousedown(tile)"
          :class="{hex: true, highlight: tile.hex.equals(highlight.hex)}">
      </polygon>
      <g v-if="!tile.unit">
        <circle
            v-if="tile.coord.row == 4 && tile.coord.col == 4"
            r="10px"
            fill="purple"
            :transform="tile.translate">
        </circle>
        <circle
            v-if="tile.coord.row == 2 && tile.coord.col == 2"
            r="4px"
            fill="green"
            :transform="tile.translate">
        </circle>
        <circle
            v-if="tile.coord.row == 2 && tile.coord.col == 6"
            r="4px"
            fill="green"
            :transform="tile.translate">
        </circle>
        <circle
            v-if="tile.coord.row == 6 && tile.coord.col == 6"
            r="4px"
            fill="green"
            :transform="tile.translate">
        </circle>
        <circle
            v-if="tile.coord.row == 6 && tile.coord.col == 2"
            r="4px"
            fill="green"
            :transform="tile.translate">
        </circle>
      </g>
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
            <template v-if="options.showQRSCoords">
              <text class="q">{{ tile.hex.q }}</text>
              <text class="r">{{ tile.hex.r }}</text>
              <text class="s">{{ tile.hex.s }}</text>
            </template>
            <template v-if="options.showDoubleCoords">
              <text :transform="tile.translate">{{ tile.coord.col }},
                {{ tile.coord.row }}
              </text>
            </template>
          </g>
        </g>
      </template>
    </g>
    <g v-for="(tile) in grid">
      <template v-if="tile.unit">
        <g :transform="tile.translate">
          <g v-if="tile.unit.cost" transform="translate(24,-41)" class="cost">
            <circle :style="{stroke: tile.style?.stroke, fill: tile.style?.darkStroke}" r="9px"></circle>
            <text>{{tile.unit.cost}}</text>
          </g>
          <g v-if="tile.unitState?.remainingHealth" transform="translate(0,30)" class="remainingHealth">
            <circle :style="{stroke: tile.style?.stroke, fill: tile.style?.darkStroke}" r="9px"></circle>
            <text>{{tile.unitState.remainingHealth}}</text>
          </g>
        </g>
      </template>
    </g>
  </svg>

</template>


<style scoped>
svg.board{
  height: 100%;
  margin: 0 auto;
}

text {
  text-anchor: middle;
  font-size: 0.6em;
  fill: rgba(0, 0, 0, 0.9);
  pointer-events: none;
  font-family: "Permanent Marker",sans-serif;
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

.selectable {

}

.unit-info {
  opacity: 0.7;
}

.unit-info text {
  stroke-width: 9px;
  paint-order: stroke fill;
}

.unit-info text.name {
  fill: white;
  stroke: black;
}

.unit-info text.damage {
  transform: translate(-18px, 33px);
  fill: black;
  stroke: white;
}

.unit-info g.damage {
  transform: translate(-24px, -226px);
  fill: white;
}

.unit-info text.health {
  transform: translate(1px, 33px);
  fill: black;
  stroke: white;
}

.unit-info g.health {
  transform: translate(-5px, -226px);
  fill: white;
}

.unit-info text.movement {
  transform: translate(20px, 33px);
  fill: black;
  stroke: white;
}

.unit-info g.movement {
  transform: translate(13px, -226px);
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
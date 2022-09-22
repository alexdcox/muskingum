<script setup lang="ts">
import {
  EnergyCoords,
  DoubledCoord,
  GameState,
  hexSetBounds,
  Layout,
  Orientation,
  TurnStage,
  Unit,
  UnitId,
  UnitMap,
  Hex
} from "engine"
import {Colors, makeGrid, Tile} from "../util"
import {ref, Ref} from "vue"
import HexTile from './HexTile.vue'
import PlainHexTile from './PlainHexTile.vue'
import UnitCost from './UnitCost.vue'
import {EventEmitter} from "events"

const props = defineProps<{
  events: EventEmitter,
  onTileMouseover?: (tile: Tile) => void
  onTileMousedown?: (tile: Tile) => void
}>()

const options = {
  showQRSCoords: false,
  showDoubleCoords: false,
}

const padding = 5
const layout = new Layout(Orientation.pointy, {width: 40, height: 36}, {x: 0, y: 0})

let state = ref({}) as Ref<GameState>
let currentPlayer = ref(0)
let highlight = ref({}) as Ref<Tile>
let selected = ref({}) as Ref<Tile | undefined>
let emptyGrid = makeGrid(layout, 0, 13, 0, 5)
    .filter(tile => {
      const omit = [[0, 0], [12, 0], [0, 4], [12, 4]]
      for (const [col, row] of omit) {
        if (tile.coord.col == col && tile.coord.row == row) {
          return false
        }
      }
      return true
    })
const summonUnit = ref(undefined) as Ref<Unit | undefined>
const moveFrom = ref(undefined) as Ref<DoubledCoord | undefined>
const attackFrom = ref(undefined) as Ref<DoubledCoord | undefined>

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
  props.onTileMouseover?.(tile)
  highlight.value = tile
  // redraw()
}

const onTileMousedown = (tile: Tile) => {
  console.log('ON TILE MOUSEDPOEN BOARD')

  props.onTileMousedown?.(tile)

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

  if (selected.value?.classes?.selected === true) {
    selected.value.classes.selected = false
  }
  tile.classes.selected = true
  selected.value = tile

  // state.units.push({
  //   id: UnitId.Emberstrike,
  //   coord: tile.coord,
  //   player: 1,
  // })

  redraw()
}

const withPlayerUnits = (tiles: Tile[]): Tile[] => {
  for (const tile of tiles) {
    tile.unitState = undefined
    tile.unit = undefined
    tile.image = undefined
    const unitState = state.value.units?.find(u => u.coord.equals(tile.coord))
    if (unitState) {
      tile.unitState = unitState
      tile.unit = UnitMap.get(unitState.id)
      tile.image = getUnitImage(tile.unit!)
    }
  }
  return tiles
}

const withStyles = (tiles: Tile[]): Tile[] => {
  // const ret: Tile[] = []
  for (const tile of tiles) {
    // tile.classes.selected = true

    // let style: any = {
    //   image: undefined,
    // }

    let classes: any = {
      p1: false,
      p2: false,
      selected: false,
      highlighted: false,
      summoning: false,
    }

    if (tile.unit) {
      tile.style.fill = `url(#${tile?.unit?.fileName})`
    }

    if (tile?.unitState?.player) {
      tile.classes[`p${tile.unitState.player}`] = true
    }

    if (selected?.value?.coord?.equals(tile.coord)) {
      // style.stroke = "green"
      // style.fill = "darkgreen"
      // style.darkStroke = "darkgreen"
      classes.selected = true
      // tile.classes = {selected: true}
    }

    tile.classes = classes

    // Object.assign(tile.style, style)
    // Object.assign(tile.classes, classes)

    // if (Object.keys(style).length > 0 || Object.keys(classes).length > 0) {
    //   styled.push({...tile, style, classes})
    // }

    // ret.push({...tile})
  }

  return tiles
}

let grid = ref(emptyGrid)

const withSummonHighlight = (tiles: Tile[]): Tile[] => {
  if (!summonUnit.value) {
    return tiles
  }

  const summonerTile = tiles.find(tile => tile.unitState?.player == currentPlayer.value && tile.unit?.id == UnitId.Summoner)!
  const summonerCoords = summonerTile.coord
  const summonerHex = summonerCoords.rdoubledToCube()

  const processedTop: Tile[] = []
  const processedBottom: Tile[] = []

  for (const tile of tiles) {
    if (tile.hex.distance(summonerHex) == 0) {
      processedTop.push({...tile})
    } else if (tile.hex.distance(summonerHex) == 1) {
      processedTop.push({
        ...tile, classes: {
          summoning: true,
        }
      })
      // TODO: Transition to class based animation
      // processedTop.push({...tile, class: {summoning: true}})
    } else {
      processedBottom.push({...tile})
    }
  }

  return [...processedBottom, ...processedTop]
}

const withMoveHighlight = (tiles: Tile[]): Tile[] => {
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
        processedTop.push({
          ...tile, style: {
            stroke: Colors.moveHighlightStroke,
            fill: Colors.moveHighlightFill,
            cursor: 'pointer',
          }
        })
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

const withAttackHighlight = (tiles: Tile[]): Tile[] => {
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
        processedTop.push({
          ...tile, style: {
            ...tile.style,
            stroke: Colors.attackHighlightStroke,
            cursor: 'pointer',
          }
        })
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
  console.log('redraw')
  let tiles = emptyGrid
  tiles = withPlayerUnits(tiles)
  tiles = withStyles(tiles)
  // tiles = withSummonHighlight(tiles)
  // tiles = withMoveHighlight(tiles)
  // tiles = withAttackHighlight(tiles)

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

// TODO: clean
const t: [DoubledCoord, number][] = EnergyCoords

const layerTwoClasses = ['selected']
const checkLayer = (tile: Tile, test: number): boolean => {
  if (test === 1) {
    return true
  }
  let tileLayer = 1
  for (const l2 of layerTwoClasses) {
    if (tile.classes[l2] === true) {
      tileLayer = 2
    }
  }
  return tileLayer === test
}

</script>



<template>
  <svg class="board" :viewBox="viewBox">
    <TransitionGroup name="list" :key="tile">
      <g v-for="(tile, k) in grid" :key="tile">
        <PlainHexTile layer="1" :k="k" :tile="tile" :mouseover="onTileMouseover" :mousedown="onTileMousedown"/>/>
        <text :transform="tile.translate">l1</text>
      </g>
    </TransitionGroup>
    <g v-for="(tile, k) in grid">
      <g v-if="checkLayer(tile, 2)">
        <PlainHexTile layer="2" :k="k" :tile="tile" :mouseover="onTileMouseover" :mousedown="onTileMousedown"/>/>
        <text :transform="tile.translate">l2</text>
      </g>
    </g>
<!--    <g v-for="(tile, k) in grid">-->
<!--      <g v-for="exy in EnergyCoords">-->
<!--        <circle-->
<!--            v-if="tile.coord.col === exy[0].col && tile.coord.row === exy[0].row"-->
<!--            :r="exy[1] === 2 ? '8px' : '5px'"-->
<!--            :fill="exy[1] === 2 ? 'purple' : 'green'"-->
<!--            :transform="tile.translate">-->
<!--        </circle>-->
<!--      </g>-->
<!--    </g>-->
<!--    <g v-for="(tile, k) in grid">-->
<!--      <HexTile :k="k" :tile="tile" :mouseover="onTileMouseover" :mousedown="onTileMousedown"/>-->
<!--    </g>-->
    <g v-for="(tile, k) in grid">
      <UnitCost :k="k" :tile="tile"/>
    </g>
  </svg>

</template>


<style scoped>

svg.board {
  transform: perspective(1100px) rotateX(15deg);
  margin: 0 auto;
  max-height: 100%;
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

.list-enter-active,
.list-leave-active {
  transition: all 0.5s ease;
}
.list-enter-from,
.list-leave-to {
  opacity: 0;
}

</style>
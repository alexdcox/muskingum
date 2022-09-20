<script setup lang="ts">
import {Tile} from "../util"
import SwordSvg from './sword.vue'
import HeartSvg from './heart.vue'
import ShoeSvg from './shoe.vue'
import {ref, Ref} from "vue"
import {Colors} from '../util.js'

const props = defineProps<{
  k: number
  tile: Tile
  mouseover?: (tile: Tile) => void
  mousedown?: (tile: Tile) => void
  summoning: boolean
}>()

const highlight = ref({}) as Ref<Tile>
const coords = `${props.k} (${props.tile.coord.col},${props.tile.coord.row})`

</script>

<template>
  <svg style="overflow: visible" v-if="tile.unit || tile.style">
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
        @mouseover="mouseover?.(tile)"
        @mousedown="mousedown?.(tile)"
        :class="{
          hex: true,
          highlight: tile.hex.equals(highlight.hex),
          ...tile.class,
        }">
    </polygon>
    <text v-if="!tile.unit" class="coords" :transform="tile.translate">{{ coords }}</text>
    <g v-if="!tile.unit">
      <g v-for="(exy) in t">
        <circle
            v-if="tile.coord.col === exy[0].col && tile.coord.row === exy[0].row"
            :r="exy[1] === 2 ? '8px' : '5px'"
            :fill="exy[1] === 2 ? 'purple' : 'green'"
            :transform="tile.translate">
        </circle>
      </g>
    </g>
    <template v-if="tile.unit">
      <g class="unit-info">
        <g :transform="tile.translate">
          <text class="name">{{ tile.unit.name }}</text>
          <g class="icons">
            <g class="damage">
              <SwordSvg width="14"/>
            </g>
            <g class="health">
              <HeartSvg width="14"/>
            </g>
            <g class="movement">
              <ShoeSvg width="14"/>
            </g>
          </g>
          <g class="numbers">
            <text class="damage">{{ tile.unit.damage }}</text>
            <text class="health">{{ tile.unit.health }}</text>
            <text class="movement">{{ tile.unit.movement }}</text>
          </g>
        </g>
      </g>
    </template>
  </svg>
</template>

<style scoped>
.coords {
  font-family: 'consolas', sans-serif;
  font-size: 8px;
  text-anchor: middle;
  fill: #e6e6e6;
  pointer-events: none;
}

.unit-info text {
  text-anchor: middle;
  font-size: 0.45em;
  fill: rgba(0, 0, 0, 0.9);
  pointer-events: none;
  font-family: "Permanent Marker", sans-serif;
}

polygon.highlight {
  opacity: 0.9;
}

polygon.hex {
  stroke: v-bind(Colors.tileStroke);
  fill: v-bind(Colors.tileFill);
  stroke-width: 2px;
  transition: background-color 0.5s ease-in-out;
}

polygon.hex.summoning {
  background-color: chartreuse;
}

.unit-info {
  opacity: 0.7;
}

.unit-info text {
  stroke-width: 9px;
  paint-order: stroke fill;
}

.unit-info .name {
  fill: white;
  stroke: black;
  transform: translate(0px, -8px);
}

.unit-info .icons {
  transform: translate(-6px, -2px);
}

.unit-info .numbers {
  transform: translate(0px, 22px);
}

.unit-info text.damage {
  transform: translate(-15px, 0);
  fill: black;
  stroke: white;
}

.unit-info g.damage {
  transform: translate(-15px, 0);
  fill: white;
}

.unit-info text.health {
  fill: black;
  stroke: white;
}

.unit-info g.health {
  fill: white;
}

.unit-info text.movement {
  transform: translate(15px, 0);
  fill: black;
  stroke: white;
}

.unit-info g.movement {
  transform: translate(15px, 0);
  fill: white;
}

.cost circle {
  stroke-width: 1px;
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
<script setup lang="ts">
import {Tile} from "../util"

defineProps<{
  k: number
  tile: Tile
}>()

</script>

<template>
  <g :transform="tile.translate">
    <g v-if="tile.unit?.cost" transform="translate(18,-28)" class="cost">
      <circle :style="{stroke: tile.style?.stroke, fill: tile.style?.darkStroke}" r="9px"></circle>
      <text>{{tile.unit.cost}}</text>
    </g>
    <g v-if="tile.unitState?.remainingHealth" transform="translate(0,30)" class="remainingHealth">
      <circle :style="{stroke: tile.style?.stroke, fill: tile.style?.darkStroke}" r="9px"></circle>
      <text>{{tile.unitState.remainingHealth}}</text>
    </g>
  </g></template>

<style scoped>
.cost circle {
  stroke-width: 1px;
}

.cost text {
  text-anchor: middle;
  font-size: 0.6em;
  pointer-events: none;
  font-family: "Permanent Marker", sans-serif;
  fill: white;
  transform: translate(0px, 4px);
}

.remainingHealth text {
  transform: translate(0px, 3px);
  fill: white;
}

</style>
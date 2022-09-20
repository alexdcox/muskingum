<script setup lang="ts">
import {Tile} from "../util"
import {Colors} from "../util"
import {ref, Ref} from "vue"

const props = defineProps<{
  k: number
  tile: Tile
}>()
const highlight = ref({}) as Ref<Tile>
const coords = `${props.k} (${props.tile.coord.col},${props.tile.coord.row})`
</script>

<template>
  <polygon :points="tile.points" class="hex"></polygon>
  <text v-if="!tile.unit" class="coords" :transform="tile.translate">{{ coords }}</text>
</template>

<style scoped>
.coords {
  font-family: 'consolas', sans-serif;
  font-size: 8px;
  text-anchor: middle;
  fill: #e6e6e6;
}

polygon.hex {
  stroke-width: 2px;
  fill: v-bind(Colors.tileFill);
  stroke: v-bind(Colors.tileStroke);
}
</style>
<script setup lang="ts">
import {Tile} from "../util"
import {Colors} from "../util"
import {computed, onBeforeUnmount, onDeactivated, onMounted, ref, Ref, Transition, watch} from "vue"

const props = defineProps<{
  k: number
  tile: Tile
  mouseover?: (tile: Tile) => void
  mousedown?: (tile: Tile) => void
  layer: number
}>()

const classes = ref({[`l${props.layer}`]: true}) as any

const visible = ref(false)
// onMounted(() => {
//   visible.value = true
// })
setTimeout(() => {
  visible.value = true
}, 0)

const tileClasses = computed(() => {
  console.log("computed prop changed")
  return props?.tile?.classes
})

watch(props.tile, () => {
  console.log('tile changed')
})

</script>

<template>
<!--  <Transition type="css">-->
    <g v-if="visible">
      <polygon
          :points="tile.points"
          :class="{hex: true, ...tileClasses, ...tile.classes}"
          @mouseover="mouseover?.(tile)"
          @mousedown="mousedown?.(tile)"
      ></polygon>
      <text class="coords" :transform="tile.translate">
        {{ `${k} (${tile.coord.col},${tile.coord.row})` }}
        {{ tile?.classes.join?.(",")}}
      </text>
    </g>
<!--  </Transition>-->
</template>

<style scoped>
.coords {
  font-family: 'consolas', sans-serif;
  font-size: 6px;
  text-anchor: middle;
  fill: black;
  pointer-events: none;
}

polygon.hex {
  transition: all 0.2s ease-in-out;
  opacity: 1;
  stroke-width: 2px;
  fill: v-bind(Colors.tileFill);
  stroke: v-bind(Colors.tileStroke);

}

.v-enter-active, .v-leave-active {
  stroke-width: 2px;
  transition: all 0.2s ease-in-out;
  fill: v-bind(Colors.tileFill);
  stroke: v-bind(Colors.tileStroke);
}

.v-enter-from, .v-leave-to {
  opacity: 0;
  /*stroke-width: 2px;*/
  /*fill: v-bind(Colors.tileFill);*/
  /*stroke: v-bind(Colors.tileStroke);*/
}

.hex.l2 {
  fill: white;
}

.hex.l2.enterStart.selected {
  opacity: 1;
  stroke-width: 2px;
  fill: v-bind(Colors.tileFill);
  stroke: v-bind(Colors.tileStroke);
}

.hex.l2.enterEnd.selected {
  opacity: 1;
  stroke-width: 2px;
  stroke: gold;
  fill: v-bind(Colors.tileFill);
}

.hex.selectable {
  cursor: pointer;
}

.hex.summoning {
  fill: v-bind(Colors.summonHighlightFill);
  /*stroke: v-bind(Colors.summonHighlightStroke);*/
}

</style>
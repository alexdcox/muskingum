<script setup lang="ts">
// This starter template is using Vue 3 <script setup> SFCs
// Check out https://vuejs.org/api/sfc-script-setup.html#script-setup
import HelloWorld from './components/HelloWorld.vue'

import Grid from './components/Grid.vue'
import Hand from './components/Hand.vue'
import Draw from './components/Draw.vue'
import {Unit, Units, UnitId, UnitMap} from "engine"

const fakeHand: Unit[] = [
  UnitMap.get(UnitId.Enforcer)!,
  UnitMap.get(UnitId.Scavenger)!,
  UnitMap.get(UnitId.Batariel)!,
  UnitMap.get(UnitId.FireStalker)!,
  UnitMap.get(UnitId.MagmaSpore)!,
]

</script>

<template>
  <Grid/>
<!--  <Hand :units="fakeHand" :player="1"/>-->
<!--  <Draw :units="Units" :player="1"/>-->

</template>

<style scoped>

</style>

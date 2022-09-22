<script setup lang="ts">

import {onMounted} from "vue"
import {Layout, Orientation, UnitId} from "engine"
import {Colors, makeGrid, Tile} from "../util"
import batarielPath from '../assets/images/units/batariel.webp'

const loadFonts = async () => new Promise(resolve => {
  const myFont = new FontFace('Permanent Marker', 'url(/fonts/PermanentMarker-Regular.ttf)');
  myFont.load().then((font) => {
    document.fonts.add(font);
    resolve(undefined)
  });
})

onMounted( async () => {
  await loadFonts()

  const canvas = document.getElementById('canvas') as HTMLCanvasElement
  const ctx = canvas.getContext('2d')!

  const scrollbarWidth = 10

  const width = canvas.width = window.innerWidth - scrollbarWidth
  const height = canvas.height = window.innerHeight - scrollbarWidth

  const layout = new Layout(Orientation.pointy, {width: 50, height: 50}, {x: 25, y: 25})

  let tiles = makeGrid(layout, 0, 13, 0, 5)
      .filter(tile => {
        const omit = [[0, 0], [12, 0], [0, 4], [12, 4]]
        for (const [col, row] of omit) {
          if (tile.coord.col == col && tile.coord.row == row) {
            return false
          }
        }
        return true
      })

  tiles[0].unit = UnitId.unit(UnitId.Summoner)

  const batarialImg: HTMLImageElement = await new Promise((resolve) => {
    const img = new Image()
    img.src = batarielPath
    img.onload = () => resolve(img)
  })

  console.log(batarialImg)

  const drawTile = (tile: Tile) => {
    ctx.lineWidth = 2
    ctx.lineCap = 'butt'
    ctx.lineJoin = 'round'
    ctx.strokeStyle = Colors.tileStroke
    ctx.fillStyle = Colors.tileFill

    ctx.beginPath()
    ctx.moveTo(tile.points[0].x, tile.points[0].y)
    for (let i = 0; i < 6; i++) {
      ctx.lineTo(tile.points[i].x, tile.points[i].y)
    }

    if (tile.unit) {
      ctx.save()
      ctx.clip()
      const w = 100
      const h = 100
      const x = tile.pos.x - (w / 2)
      const y = tile.pos.y - (h / 2)
      ctx.fillStyle = 'red'
      ctx.drawImage(batarialImg,  x, y, w, h)
      console.log('draw unit', tile.coord)
      ctx.restore()
    } else {
      ctx.fill()
      console.log('fill tile')
    }
    ctx.stroke()
    ctx.closePath()

    ctx.textAlign = 'center'
    ctx.font = "10px Permanent Marker";
    ctx.fillText("Hello World", 10, 50);
    // ctx.strokeText(`${tile.coord.col} ${tile.coord.row}`, tile.pos.x, tile.pos.y)
  }

  // ctx.scale(2,2);
  // ctx.globalAlpha = 1;
  ctx.save()
  ctx.translate(50, 50)

  for (const tile of tiles) {
    drawTile(tile)
  }
  console.log('drawn', 1)


  // ctx.restore()
  // ctx.clearRect(0, 0, canvas.width, canvas.height)


  // drawTile(tiles[1])
  // ctx.scale(1.5, 1.5)


  // ctx.restore()

  // const animate = () => {
  //   requestAnimationFrame(animate)
  // }
  //
  // requestAnimationFrame(animate)
})


</script>

<template>
  <canvas id="canvas"/>
</template>

<style scoped>
canvas {
  border: 1px solid white;
  /*background: orange;*/
  background: v-bind(Colors.tileStroke);
  font-family: 'Permanent Marker', sans-serif;
}
</style>
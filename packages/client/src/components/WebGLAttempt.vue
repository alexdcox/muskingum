<script setup lang="ts">

import {onMounted} from "vue"
import twgl from 'twgl.js'
import {Layout, Orientation, UnitId} from "engine"
import {Colors, makeGrid, Tile} from "../util"
import batarielPath from '../assets/images/units/batariel.webp'

const m3 = {
  projection: function (width: number, height: number) {
    // Note: This matrix flips the Y axis so that 0 is at the top.
    return [
      2 / width, 0, 0,
      0, -2 / height, 0,
      -1, 1, 1
    ];
  },

  identity: function () {
    return [
      1, 0, 0,
      0, 1, 0,
      0, 0, 1,
    ];
  },

  translation: function (tx: number, ty: number) {
    return [
      1, 0, 0,
      0, 1, 0,
      tx, ty, 1,
    ];
  },

  rotation: function (angleInRadians: number) {
    const c = Math.cos(angleInRadians);
    const s = Math.sin(angleInRadians);
    return [
      c, -s, 0,
      s, c, 0,
      0, 0, 1,
    ];
  },

  scaling: function (sx: number, sy: number) {
    return [
      sx, 0, 0,
      0, sy, 0,
      0, 0, 1,
    ];
  },

  multiply: function (a: any, b: any) {
    const a00 = a[0 * 3 + 0];
    const a01 = a[0 * 3 + 1];
    const a02 = a[0 * 3 + 2];
    const a10 = a[1 * 3 + 0];
    const a11 = a[1 * 3 + 1];
    const a12 = a[1 * 3 + 2];
    const a20 = a[2 * 3 + 0];
    const a21 = a[2 * 3 + 1];
    const a22 = a[2 * 3 + 2];
    const b00 = b[0 * 3 + 0];
    const b01 = b[0 * 3 + 1];
    const b02 = b[0 * 3 + 2];
    const b10 = b[1 * 3 + 0];
    const b11 = b[1 * 3 + 1];
    const b12 = b[1 * 3 + 2];
    const b20 = b[2 * 3 + 0];
    const b21 = b[2 * 3 + 1];
    const b22 = b[2 * 3 + 2];
    return [
      b00 * a00 + b01 * a10 + b02 * a20,
      b00 * a01 + b01 * a11 + b02 * a21,
      b00 * a02 + b01 * a12 + b02 * a22,
      b10 * a00 + b11 * a10 + b12 * a20,
      b10 * a01 + b11 * a11 + b12 * a21,
      b10 * a02 + b11 * a12 + b12 * a22,
      b20 * a00 + b21 * a10 + b22 * a20,
      b20 * a01 + b21 * a11 + b22 * a21,
      b20 * a02 + b21 * a12 + b22 * a22,
    ];
  },

  translate: function (m: number, tx: number, ty: number) {
    return m3.multiply(m, m3.translation(tx, ty));
  },

  rotate: function (m: number, angleInRadians: number) {
    return m3.multiply(m, m3.rotation(angleInRadians));
  },

  scale: function (m: number, sx: number, sy: number) {
    return m3.multiply(m, m3.scaling(sx, sy));
  },
};

const loadFonts = async () => new Promise(resolve => {
  const myFont = new FontFace('Permanent Marker', 'url(/fonts/PermanentMarker-Regular.ttf)');
  myFont.load().then((font) => {
    document.fonts.add(font);
    resolve(undefined)
  });
})

onMounted(async () => {
  await loadFonts()

  function createShader(gl: any, type: any, source: any) {
    let shader = gl.createShader(type);
    gl.shaderSource(shader, source);
    gl.compileShader(shader);
    let success = gl.getShaderParameter(shader, gl.COMPILE_STATUS);
    if (success) {
      return shader;
    }

    console.log(gl.getShaderInfoLog(shader));
    gl.deleteShader(shader);
  }

  // let vertexShaderElem: HTMLScriptElement = document.querySelector("#vertex-shader-2d")!
  // let vertexShaderSource = vertexShaderElem.text
  // let fragmentShaderElem: HTMLScriptElement = document.querySelector("#fragment-shader-2d")!
  // let fragmentShaderSource = fragmentShaderElem.text
  // let vertexShader = createShader(gl, gl.VERTEX_SHADER, vertexShaderSource);
  // let fragmentShader = createShader(gl, gl.FRAGMENT_SHADER, fragmentShaderSource);

  function createProgram(gl: any, vertexShader: any, fragmentShader: any) {
    let program = gl.createProgram();
    gl.attachShader(program, vertexShader);
    gl.attachShader(program, fragmentShader);
    gl.linkProgram(program);
    let success = gl.getProgramParameter(program, gl.LINK_STATUS);
    if (success) {
      return program;
    }

    console.log(gl.getProgramInfoLog(program));
    gl.deleteProgram(program);
  }

  // let program = createProgram(gl, vertexShader, fragmentShader);
  //
  // let positionAttributeLocation = gl.getAttribLocation(program, "a_position");
  // let positionBuffer = gl.createBuffer();
  // gl.bindBuffer(gl.ARRAY_BUFFER, positionBuffer);
  // // three 2d points
  // let positions = [
  //   0, 0,
  //   0, 0.5,
  //   0.7, 0,
  // ];
  // gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(positions), gl.STATIC_DRAW);


  function main() {
    const canvas = document.getElementById('canvas') as HTMLCanvasElement
    const gl = canvas.getContext('webgl')!
    if (!gl) {
      return;
    }

    // setup GLSL program
    const program = twgl.createProgramFromScripts(gl, ["vertex-shader-2d", "fragment-shader-2d"]);
    gl.useProgram(program);

    // look up where the vertex data needs to go.
    const positionAttributeLocation = gl.getAttribLocation(program, "a_position");

    // lookup uniforms
    const colorLocation = gl.getUniformLocation(program, "u_color");
    const matrixLocation = gl.getUniformLocation(program, "u_matrix");

    // Create a buffer to put three 2d clip space points in
    const positionBuffer = gl.createBuffer();

    // Bind it to ARRAY_BUFFER (think of it as ARRAY_BUFFER = positionBuffer)
    gl.bindBuffer(gl.ARRAY_BUFFER, positionBuffer);

    requestAnimationFrame(drawScene);

    // Draw the scene.
    function drawScene(now: number) {
      now *= 0.001; // convert to seconds

      twgl.resizeCanvasToDisplaySize(gl.canvas);

      gl.viewport(0, 0, gl.canvas.width, gl.canvas.height);

      // Clear the canvas.
      gl.clear(gl.COLOR_BUFFER_BIT);

      // Tell it to use our program (pair of shaders)
      gl.useProgram(program);

      // Turn on the attribute
      gl.enableVertexAttribArray(positionAttributeLocation);

      // Bind the position buffer.
      gl.bindBuffer(gl.ARRAY_BUFFER, positionBuffer);

      // Tell the attribute how to get data out of positionBuffer (ARRAY_BUFFER)
      const size = 2;          // 2 components per iteration
      const type = gl.FLOAT;   // the data is 32bit floats
      const normalize = false; // don't normalize the data
      const stride = 0;        // 0 = move forward size * sizeof(type) each iteration to get the next position
      let offset = 0;        // start at the beginning of the buffer
      gl.vertexAttribPointer(
          positionAttributeLocation, size, type, normalize, stride, offset);

      // Set Geometry.
      const radius = Math.sqrt(gl.canvas.width * gl.canvas.width + gl.canvas.height * gl.canvas.height) * 0.5;
      const angle = now;
      const x = Math.cos(angle) * radius;
      const y = Math.sin(angle) * radius;
      const centerX = gl.canvas.width / 2;
      const centerY = gl.canvas.height / 2;
      setGeometry(gl, centerX + x, centerY + y, centerX - x, centerY - y);

      // Compute the matrices
      const projectionMatrix = m3.projection(gl.canvas.width, gl.canvas.height);

      // Set the matrix.
      gl.uniformMatrix3fv(matrixLocation, false, projectionMatrix);

      // Draw in red
      gl.uniform4fv(colorLocation, [1, 0, 0, 1]);

      // Draw the geometry.
      const primitiveType = gl.LINES;
      offset = 0;
      const count = 2;
      gl.drawArrays(primitiveType, offset, count);

      requestAnimationFrame(drawScene);
    }

    const canvasToDisplaySizeMap = new Map([[canvas, [300, 150]]]);

    function onResize(entries: any) {
      for (const entry of entries) {
        let width;
        let height;
        let dpr = window.devicePixelRatio;
        if (entry.devicePixelContentBoxSize) {
          // NOTE: Only this path gives the correct answer
          // The other 2 paths are an imperfect fallback
          // for browsers that don't provide anyway to do this
          width = entry.devicePixelContentBoxSize[0].inlineSize;
          height = entry.devicePixelContentBoxSize[0].blockSize;
          dpr = 1; // it's already in width and height
        } else if (entry.contentBoxSize) {
          if (entry.contentBoxSize[0]) {
            width = entry.contentBoxSize[0].inlineSize;
            height = entry.contentBoxSize[0].blockSize;
          } else {
            // legacy
            width = entry.contentBoxSize.inlineSize;
            height = entry.contentBoxSize.blockSize;
          }
        } else {
          // legacy
          width = entry.contentRect.width;
          height = entry.contentRect.height;
        }
        const displayWidth = Math.round(width * dpr);
        const displayHeight = Math.round(height * dpr);
        canvasToDisplaySizeMap.set(entry.target, [displayWidth, displayHeight]);
      }
    }

    const resizeObserver = new ResizeObserver(onResize);
    resizeObserver.observe(canvas, {box: 'content-box'});

  }

// Fill the buffer with a line
  function setGeometry(gl, x1, y1, x2, y2) {
    gl.bufferData(
        gl.ARRAY_BUFFER,
        new Float32Array([
          x1, y1,
          x2, y2]),
        gl.STATIC_DRAW);
  }
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
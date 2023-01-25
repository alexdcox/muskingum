/*
 * I'm having to test here because jest doesn't work with ES6 modules.
 * e.g. npx ts-node --esm ./src/test.ts
 */

// import {Game} from "./game.js"
//
// const game = new Game()
//
// console.log('What units can P1 summon?')
// game.playerState[0].hand.getUnits().forEach(unit => {
//   console.log(unit.name)
// })
// game.nextTurn()

// --------------------------------------------------

// import {EventEmitter} from "events"
// const e = new EventEmitter()
// e.on('test', () => {
//   console.log('yep')
// })
// e.emit('test')

// --------------------------------------------------

const t: any = {}
t[1] = 44
// t.set('asd', 12321)
console.log(JSON.stringify(t))
// console.log({1: 1})

// --------------------------------------------------

// import {Game} from "./game.js"
// import {Player} from "./player.js"
//
// const p1 = new Player("p1");
// const p2 = new Player("p2");
// const g = new Game(p1, p2)
// console.log(g.getState())
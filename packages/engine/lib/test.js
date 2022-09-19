/*
 * I'm having to test here because jest doesn't work with ES6 modules.
 */
// import {Game} from "./game.js"
//
// const game = new Game()
//
// console.log('What units can P1 summon?')
// game.playerState[0].hand.getUnits().forEach(unit => {
//   console.log(unit.name)
// })
//
// game.nextTurn()
import { EventEmitter } from "events";
const e = new EventEmitter();
e.on('test', () => {
    console.log('yep');
});
e.emit('test');
//# sourceMappingURL=test.js.map
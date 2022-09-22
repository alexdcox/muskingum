import {DoubledCoord, Hex, Layout, Orientation, Point, Unit, UnitState} from "engine"

export interface Tile {
  hex: Hex,
  points: Point[]
  pos: Point
  translate: string
  coord: DoubledCoord
  unit?: Unit
  unitState?: UnitState
  image?: string
  style: any
  // style: {
  //   fill?: string
  // }
  classes: any
/*
  classes: {
    selected: boolean
    highlighted: boolean
    summoning: boolean
    p1: boolean
    p2: boolean
  }
*/
}

export const Colors = {
  tileFill: 'hsl(56,42%,84%)',
  tileStroke: '#242424',
  purple: 'hsl(300,72%,67%)',
  darkPurple: 'hsl(300,35%,30%)',
  green: 'hsl(88,69%,64%)',
  darkGreen: 'hsl(88,38%,28%)',
  blue: 'hsl(197,73%,67%)',
  darkBlue: 'hsl(197,36%,30%)',
  white: 'hsl(0,0%,80%)',
  black: '#242424',
  selectedStroke: 'rgb(255,245,51)',
  selectedDarkStroke: 'rgb(153,147,31)',
  moveHighlightStroke: 'rgb(45,229,70)',
  moveHighlightFill: 'rgb(25,128,46)',
  summonHighlightStroke: 'rgb(45,229,229)',
  summonHighlightFill: 'hsl(181, 44%, 52%)',
  attackHighlightStroke: 'rgb(229,155,45)'
}

export function makeGrid(layout: Layout, minCol: number, maxCol: number, minRow: number, maxRow: number): Tile[] {
  let results = [];
  for (let row = minRow; row <= maxRow - 1; row++) {
    for (let col = minCol; col <= maxCol - 1; col++) {
      if (row % 2 == 0 && col % 2 != 0) continue
      if (col % 2 == 0 && row % 2 != 0) continue
      const coord = new DoubledCoord(col, row)
      const hex = layout.orientation == Orientation.pointy ? coord.rdoubledToCube() : coord.qdoubledToCube()
      const pos = layout.hexToPixel(hex)
      const tile: Tile = {
        hex,
        coord,
        pos,
        points: layout.polygonCorners(hex),
        translate: `translate(${pos.x},${pos.y})`,
        style: {},
        classes: {
          selected: false,
        },
      }
      results.push(tile);
    }
  }
  return results;
}

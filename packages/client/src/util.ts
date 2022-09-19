import {DoubledCoord, Hex, Layout, Point, Unit, UnitState} from "engine"

export interface Tile {
  hex: Hex,
  points: string
  pos: Point
  translate: string
  coord: DoubledCoord
  unit?: Unit
  unitState?: UnitState
  image?: string
  style?: any
}

export const Colors = {
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
  summonHighlightFill: 'rgb(25,128,128)',
  attackHighlightStroke: 'rgb(229,155,45)'
}

export function makeGrid(layout: Layout, minCol: number, maxCol: number, minRow: number, maxRow: number): Tile[] {
  let results = [];
  for (let col = minCol; col <= maxCol - 1; col++) {
    for (let row = minRow; row <= maxRow - 1; row++) {
      if (row % 2 == 0 && col % 2 != 0) continue
      if (col % 2 == 0 && row % 2 != 0) continue
      const coord = new DoubledCoord(col, row)
      const hex = coord.qdoubledToCube()
      const pos = layout.hexToPixel(hex)
      const tile: Tile = {
        hex,
        coord,
        pos,
        points: layout.polygonCorners(hex).map((p: Point) => [p.x, p.y].join(',')).join(' '),
        translate: `translate(${pos.x},${pos.y})`,
      }
      results.push(tile);
    }
  }
  return results;
}
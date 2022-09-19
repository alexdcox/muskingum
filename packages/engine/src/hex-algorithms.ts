import {Hex, OffsetCoord, DoubledCoord, Point, Layout} from './hex.js'

/* When t = 0, it's all a; when t = 1, it's all b */
export function mix(a: number, b: number, t: number) {
  return a * (1 - t) + b * t;
}


/* return min, max of one field of an array of objects */
export function bounds(array: any[], field: string) {
  let min = Infinity, max = -Infinity;
  for (let object of array) {
    let value = object[field];
    if (value < min) {
      min = value;
    }
    if (value > max) {
      max = value;
    }
  }
  return {min, max};
}


/* return integer sequence min <= x <= max (half-open)  */
export function closedInterval({min, max}: { min: number, max: number }) {
  let results = [];
  for (let i = min; i <= max; i++) {
    results.push(i);
  }
  return results;
}

/* Specifically for offset grid diagrams */
export function makeRectangularShape(
  minCol: number,
  maxCol: number,
  minRow: number,
  maxRow: number,
  convert: (o: OffsetCoord) => Hex,
): Hex[] {
  let results = [];
  for (let col = minCol; col <= maxCol; col++) {
    for (let row = minRow; row <= maxRow; row++) {
      results.push(convert(new OffsetCoord(col, row)));
    }
  }
  return results;
}

/* Specifically for doubled grid diagrams */
export function makeRDoubledRectangularShape(minCol: number, maxCol: number, minRow: number, maxRow: number) {
  let results = [];
  for (let row = minRow; row <= maxRow; row++) {
    for (let col = minCol + (row & 1); col <= maxCol; col += 2) {
      let hex = new DoubledCoord(col, row).rdoubledToCube();
      results.push({...hex, col, row});
    }
  }
  return results;
}

/* Specifically for doubled grid diagrams */
export function makeQDoubledRectangularShape(minCol: number, maxCol: number, minRow: number, maxRow: number) {
  let results = [];
  for (let col = minCol; col <= maxCol; col++) {
    for (let row = minRow + (col & 1); row <= maxRow; row += 2) {
      let hex = new DoubledCoord(col, row).qdoubledToCube();
      results.push({...hex, col, row});
    }
  }
  return results;
}


export function makeHexagonalShape(n: number) {
  let results = [];
  for (let q = -n; q <= n; q++) {
    for (let r = -n; r <= n; r++) {
      let hex = new Hex(q, r, -q - r);
      if (hex.len() <= n) {
        results.push(hex);
      }
    }
  }
  return results;
}


export function makeDownTriangularShape(n: number) {
  let results = [];
  for (let r = 0; r < n; r++) {
    for (let q = 0; q < n - r; q++) {
      results.push(new Hex(q, r, -q - r));
    }
  }
  return results;
}


export function makeUpTriangularShape(n: number) {
  let results = [];
  for (let r = 0; r < n; r++) {
    for (let q = n - r - 1; q < n; q++) {
      results.push(new Hex(q, r, -q - r));
    }
  }
  return results;
}


export function makeRhombusShape(w: number, h: number) {
  if (!h) {
    h = w;
  }
  let results = [];
  for (let r = 0; r < h; r++) {
    for (let q = 0; q < w; q++) {
      results.push(new Hex(q, r, -q - r));
    }
  }
  return results;
}


/* Given a set of points, return the maximum extent
   {left, right, top, bottom} */
export function pointSetBounds(points: Point[]) {
  let left = Infinity, top = Infinity,
    right = -Infinity, bottom = -Infinity;
  for (let p of points) {
    if (p.x < left) {
      left = p.x;
    }
    if (p.x > right) {
      right = p.x;
    }
    if (p.y < top) {
      top = p.y;
    }
    if (p.y > bottom) {
      bottom = p.y;
    }
  }
  return {left, top, right, bottom};
}


/* Given a set of hexes, return the maximum extent
   {left, right, top, bottom} */
export function hexSetBounds(layout: Layout, hexes: Hex[]) {
  let corners = [];
  for (let corner = 0; corner < 6; corner++) {
    corners.push(layout.hexCornerOffset(corner));
  }
  let cornerBounds = pointSetBounds(corners);

  let centerBounds = pointSetBounds(hexes.map(h => layout.hexToPixel(h)));

  return {
    left: cornerBounds.left + centerBounds.left,
    top: cornerBounds.top + centerBounds.top,
    right: cornerBounds.right + centerBounds.right,
    bottom: cornerBounds.bottom + centerBounds.bottom,
  };
}

export function breadthFirstSearch(start: Hex, blocked: (h: Hex) => boolean) {
  /* see https://www.redblobgames.com/pathfinding/a-star/introduction.html */
  let cost_so_far = new Map<string, number>()
  cost_so_far.set(start.toString(), 0)

  let came_from = new Map<string, Hex|null>()
  came_from.set(start.toString(), null)

  let fringes = [[start]];
  for (let k = 0; fringes[k].length > 0; k++) {
    fringes[k + 1] = [];
    for (let hex of fringes[k]) {
      for (let dir = 0; dir < 6; dir++) {
        let neighbor = hex.neighbor(dir);
        if (cost_so_far.get(neighbor.toString()) === undefined
          && !blocked(neighbor)) {
          cost_so_far.set(neighbor.toString(), k + 1)
          came_from.set(neighbor.toString(), hex)
          fringes[k + 1].push(neighbor);
        }
      }
    }
  }
  return {cost_so_far, came_from};
}


/* NOTE: this returns the *fractional* hexes between A and B; you need
   to call round() on them to get the hex tiles */
export function hexLineFractional(A: Hex, B: Hex) {
  /* see https://www.redblobgames.com/grids/line-drawing.html */

  /* HACK: add a tiny offset to the start point to break ties,
   * because there are a lot of ties on a grid, and I want it to
   * always round the same direction for consistency. To demonstrate
   * the need for this hack, draw a line from Hex(-5, 0, +5) to
   * Hex(+5, -5, 0). Without the hack, there are points on the edge
   * that will sometimes be rounded one way and sometimes the other.
   * The hack will force them to be rounded consistently. */
  const offset = new Hex(1e-6, 2e-6, -3e-6);

  let N = A.subtract(B).len();
  let results = [];
  for (let i = 0; i <= N; i++) {
    results.push(A.lerp(B, i / Math.max(1, N)).add(offset));
  }
  return results;
}


export function hexRing(radius: number) {
  const results = [];
  let H = Hex.direction(4).scale(radius);
  for (let side = 0; side < 6; side++) {
    for (let step = 0; step < radius; step++) {
      results.push(H);
      H = H.neighbor(side);
    }
  }
  return results;
}
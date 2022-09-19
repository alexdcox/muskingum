export class Point {
  public x: number
  public y: number

  constructor(x: number, y: number) {
    this.x = x
    this.y = y
  }
}

export class Hex {
  public q: number
  public r: number
  public s: number

  static directions = [
    new Hex(1, 0, -1),
    new Hex(1, -1, 0),
    new Hex(0, -1, 1),
    new Hex(-1, 0, 1),
    new Hex(-1, 1, 0),
    new Hex(0, 1, -1),
  ];

  static diagonals = [
    new Hex(2, -1, -1),
    new Hex(1, -2, 1),
    new Hex(-1, -1, 2),
    new Hex(-2, 1, 1),
    new Hex(-1, 2, -1),
    new Hex(1, 1, -2),
  ];

  constructor(q: number, r: number, s: number) {
    this.q = q
    this.r = r
    this.s = s
    if (Math.round(q + r + s) !== 0)
      throw "q + r + s must be 0"
  }

  static cube(q: number, r: number, s: number) {
    return new Hex(q, r, s)
  }

  static axial(q: number, r: number) {
    const s = -q - r
    return new Hex(q, r, s)
  }

  toString(): string {
    return `${this.q},${this.r},${this.s}`
  }

  equals(other: Hex) {
    if (other === undefined) {
      return false
    }
    return this.q == other.q && this.r === other.r && this.s === other.s
  }

  add(b: Hex) {
    return new Hex(this.q + b.q, this.r + b.r, this.s + b.s);
  }

  subtract(b: Hex) {
    return new Hex(this.q - b.q, this.r - b.r, this.s - b.s);
  }

  scale(k: number) {
    return new Hex(this.q * k, this.r * k, this.s * k);
  }

  rotateLeft() {
    return new Hex(-this.s, -this.q, -this.r);
  }

  rotateRight() {
    return new Hex(-this.r, -this.s, -this.q);
  }

  static direction(direction: number) {
    return Hex.directions[direction];
  }

  neighbor(direction: number) {
    return this.add(Hex.direction(direction));
  }

  diagonalNeighbor(direction: number) {
    return this.add(Hex.diagonals[direction]);
  }

  len() {
    return (Math.abs(this.q) + Math.abs(this.r) + Math.abs(this.s)) / 2;
  }

  distance(b: Hex) {
    return this.subtract(b).len();
  }

  round() {
    let qi = Math.round(this.q);
    let ri = Math.round(this.r);
    let si = Math.round(this.s);
    let q_diff = Math.abs(qi - this.q);
    let r_diff = Math.abs(ri - this.r);
    let s_diff = Math.abs(si - this.s);
    if (q_diff > r_diff && q_diff > s_diff) {
      qi = -ri - si;
    } else if (r_diff > s_diff) {
      ri = -qi - si;
    } else {
      si = -qi - ri;
    }
    return new Hex(qi, ri, si);
  }

  lerp(b: Hex, t: number) {
    return new Hex(this.q * (1.0 - t) + b.q * t, this.r * (1.0 - t) + b.r * t, this.s * (1.0 - t) + b.s * t);
  }

  linedraw(b: Hex) {
    let N = this.distance(b);
    let a_nudge = new Hex(this.q + 1e-06, this.r + 1e-06, this.s - 2e-06);
    let b_nudge = new Hex(b.q + 1e-06, b.r + 1e-06, b.s - 2e-06);
    let results = [];
    let step = 1.0 / Math.max(N, 1);
    for (let i = 0; i <= N; i++) {
      results.push(a_nudge.lerp(b_nudge, step * i).round());
    }
    return results;
  }
}

export class OffsetCoord {
  public col: number
  public row: number

  static EVEN = 1;
  static ODD = -1;

  constructor(col: number, row: number) {
    this.col = col;
    this.row = row;
  }

  static qoffsetFromCube(offset: number, h: Hex) {
    let col = h.q;
    let row = h.r + (h.q + offset * (h.q & 1)) / 2;
    if (offset !== OffsetCoord.EVEN && offset !== OffsetCoord.ODD) {
      throw "offset must be EVEN (+1) or ODD (-1)";
    }
    return new OffsetCoord(col, row);
  }

  static qoffsetToCube(offset: number, h: OffsetCoord) {
    let q = h.col;
    let r = h.row - (h.col + offset * (h.col & 1)) / 2;
    let s = -q - r;
    if (offset !== OffsetCoord.EVEN && offset !== OffsetCoord.ODD) {
      throw "offset must be EVEN (+1) or ODD (-1)";
    }
    return new Hex(q, r, s);
  }

  static roffsetFromCube(offset: number, h: Hex) {
    let col = h.q + (h.r + offset * (h.r & 1)) / 2;
    let row = h.r;
    if (offset !== OffsetCoord.EVEN && offset !== OffsetCoord.ODD) {
      throw "offset must be EVEN (+1) or ODD (-1)";
    }
    return new OffsetCoord(col, row);
  }

  static roffsetToCube(offset: number, h: OffsetCoord) {
    let q = h.col - (h.row + offset * (h.row & 1)) / 2;
    let r = h.row;
    let s = -q - r;
    if (offset !== OffsetCoord.EVEN && offset !== OffsetCoord.ODD) {
      throw "offset must be EVEN (+1) or ODD (-1)";
    }
    return new Hex(q, r, s);
  }
}

export class DoubledCoord {
  public col: number
  public row: number

  constructor(col: number, row: number) {
    this.col = col;
    this.row = row;
  }

  equals(o: DoubledCoord) {
    if (o === undefined) {
      return false
    }
    return this.col == o.col && this.row == o.row
  }

  static qdoubledFromCube(h: Hex) {
    let col = h.q;
    let row = 2 * h.r + h.q;
    return new DoubledCoord(col, row);
  }

  qdoubledToCube() {
    let q = this.col;
    let r = (this.row - this.col) / 2;
    let s = -q - r;
    return new Hex(q, r, s);
  }

  static rdoubledFromCube(h: Hex) {
    let col = 2 * h.q + h.r;
    let row = h.r;
    return new DoubledCoord(col, row);
  }

  rdoubledToCube() {
    let q = (this.col - this.row) / 2;
    let r = this.row;
    let s = -q - r;
    return new Hex(q, r, s);
  }
}

export class Orientation {
  static pointy = new Orientation(
    Math.sqrt(3.0),
    Math.sqrt(3.0) / 2.0,
    0.0,
    3.0 / 2.0,
    Math.sqrt(3.0) / 3.0,
    -1.0 / 3.0,
    0.0,
    2.0 / 3.0,
    0.5
  )

  static flat = new Orientation(
    3.0 / 2.0,
    0.0,
    Math.sqrt(3.0) / 2.0,
    Math.sqrt(3.0),
    2.0 / 3.0,
    0.0,
    -1.0 / 3.0,
    Math.sqrt(3.0) / 3.0,
    0.0
  )

  public f0: number
  public f1: number
  public f2: number
  public f3: number
  public b0: number
  public b1: number
  public b2: number
  public b3: number
  public start_angle: number

  private constructor(
    f0: number,
    f1: number,
    f2: number,
    f3: number,
    b0: number,
    b1: number,
    b2: number,
    b3: number,
    start_angle: number,
  ) {
    this.f0 = f0;
    this.f1 = f1;
    this.f2 = f2;
    this.f3 = f3;
    this.b0 = b0;
    this.b1 = b1;
    this.b2 = b2;
    this.b3 = b3;
    this.start_angle = start_angle;
  }
}

export class Layout {
  public orientation: Orientation
  public size: { width: number, height: number }
  public origin: Point

  constructor(orientation: Orientation, size: { width: number, height: number }, origin: Point) {
    this.orientation = orientation;
    this.size = size;
    this.origin = origin;
  }

  hexToPixel(h: Hex) {
    let M = this.orientation;
    let size = this.size;
    let origin = this.origin;
    let x = (M.f0 * h.q + M.f1 * h.r) * size.width;
    let y = (M.f2 * h.q + M.f3 * h.r) * size.height;
    return new Point(x + origin.x, y + origin.y);
  }

  pixelToHex(p: Point) {
    let M = this.orientation;
    let size = this.size;
    let origin = this.origin;
    let pt = new Point((p.x - origin.x) / size.width, (p.y - origin.y) / size.height);
    let q = M.b0 * pt.x + M.b1 * pt.y;
    let r = M.b2 * pt.x + M.b3 * pt.y;
    return new Hex(q, r, -q - r);
  }

  hexCornerOffset(corner: number) {
    let M = this.orientation;
    let size = this.size;
    let angle = 2.0 * Math.PI * (M.start_angle - corner) / 6.0;
    return new Point(size.width * Math.cos(angle), size.height * Math.sin(angle));
  }

  polygonCorners(h: Hex) {
    let corners = [];
    let center = this.hexToPixel(h);
    for (let i = 0; i < 6; i++) {
      let offset = this.hexCornerOffset(i);
      corners.push(new Point(center.x + offset.x, center.y + offset.y));
    }
    return corners;
  }
}

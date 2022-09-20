export class Point {
    x;
    y;
    constructor(x, y) {
        this.x = x;
        this.y = y;
    }
}
export class Hex {
    q;
    r;
    s;
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
    constructor(q, r, s) {
        this.q = q;
        this.r = r;
        this.s = s;
        if (Math.round(q + r + s) !== 0)
            throw "q + r + s must be 0";
    }
    static cube(q, r, s) {
        return new Hex(q, r, s);
    }
    static axial(q, r) {
        const s = -q - r;
        return new Hex(q, r, s);
    }
    toString() {
        return `${this.q},${this.r},${this.s}`;
    }
    equals(other) {
        if (other === undefined) {
            return false;
        }
        return this.q == other.q && this.r === other.r && this.s === other.s;
    }
    add(b) {
        return new Hex(this.q + b.q, this.r + b.r, this.s + b.s);
    }
    subtract(b) {
        return new Hex(this.q - b.q, this.r - b.r, this.s - b.s);
    }
    scale(k) {
        return new Hex(this.q * k, this.r * k, this.s * k);
    }
    rotateLeft() {
        return new Hex(-this.s, -this.q, -this.r);
    }
    rotateRight() {
        return new Hex(-this.r, -this.s, -this.q);
    }
    static direction(direction) {
        return Hex.directions[direction];
    }
    neighbor(direction) {
        return this.add(Hex.direction(direction));
    }
    diagonalNeighbor(direction) {
        return this.add(Hex.diagonals[direction]);
    }
    len() {
        return (Math.abs(this.q) + Math.abs(this.r) + Math.abs(this.s)) / 2;
    }
    distance(b) {
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
        }
        else if (r_diff > s_diff) {
            ri = -qi - si;
        }
        else {
            si = -qi - ri;
        }
        return new Hex(qi, ri, si);
    }
    lerp(b, t) {
        return new Hex(this.q * (1.0 - t) + b.q * t, this.r * (1.0 - t) + b.r * t, this.s * (1.0 - t) + b.s * t);
    }
    linedraw(b) {
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
    col;
    row;
    static EVEN = 1;
    static ODD = -1;
    constructor(col, row) {
        this.col = col;
        this.row = row;
    }
    static qoffsetFromCube(offset, h) {
        let col = h.q;
        let row = h.r + (h.q + offset * (h.q & 1)) / 2;
        if (offset !== OffsetCoord.EVEN && offset !== OffsetCoord.ODD) {
            throw "offset must be EVEN (+1) or ODD (-1)";
        }
        return new OffsetCoord(col, row);
    }
    static qoffsetToCube(offset, h) {
        let q = h.col;
        let r = h.row - (h.col + offset * (h.col & 1)) / 2;
        let s = -q - r;
        if (offset !== OffsetCoord.EVEN && offset !== OffsetCoord.ODD) {
            throw "offset must be EVEN (+1) or ODD (-1)";
        }
        return new Hex(q, r, s);
    }
    static roffsetFromCube(offset, h) {
        let col = h.q + (h.r + offset * (h.r & 1)) / 2;
        let row = h.r;
        if (offset !== OffsetCoord.EVEN && offset !== OffsetCoord.ODD) {
            throw "offset must be EVEN (+1) or ODD (-1)";
        }
        return new OffsetCoord(col, row);
    }
    static roffsetToCube(offset, h) {
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
    col;
    row;
    constructor(col, row) {
        this.col = col;
        this.row = row;
    }
    toString() {
        return `${this.col},${this.row}`;
    }
    equals(o) {
        if (o === undefined) {
            return false;
        }
        return this.col == o.col && this.row == o.row;
    }
    static qdoubledFromCube(h) {
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
    static rdoubledFromCube(h) {
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
    static pointy = new Orientation(Math.sqrt(3.0), Math.sqrt(3.0) / 2.0, 0.0, 3.0 / 2.0, Math.sqrt(3.0) / 3.0, -1.0 / 3.0, 0.0, 2.0 / 3.0, 0.5);
    static flat = new Orientation(3.0 / 2.0, 0.0, Math.sqrt(3.0) / 2.0, Math.sqrt(3.0), 2.0 / 3.0, 0.0, -1.0 / 3.0, Math.sqrt(3.0) / 3.0, 0.0);
    f0;
    f1;
    f2;
    f3;
    b0;
    b1;
    b2;
    b3;
    start_angle;
    constructor(f0, f1, f2, f3, b0, b1, b2, b3, start_angle) {
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
    orientation;
    size;
    origin;
    constructor(orientation, size, origin) {
        this.orientation = orientation;
        this.size = size;
        this.origin = origin;
    }
    hexToPixel(h) {
        let M = this.orientation;
        let size = this.size;
        let origin = this.origin;
        let x = (M.f0 * h.q + M.f1 * h.r) * size.width;
        let y = (M.f2 * h.q + M.f3 * h.r) * size.height;
        return new Point(x + origin.x, y + origin.y);
    }
    pixelToHex(p) {
        let M = this.orientation;
        let size = this.size;
        let origin = this.origin;
        let pt = new Point((p.x - origin.x) / size.width, (p.y - origin.y) / size.height);
        let q = M.b0 * pt.x + M.b1 * pt.y;
        let r = M.b2 * pt.x + M.b3 * pt.y;
        return new Hex(q, r, -q - r);
    }
    hexCornerOffset(corner) {
        let M = this.orientation;
        let size = this.size;
        let angle = 2.0 * Math.PI * (M.start_angle - corner) / 6.0;
        return new Point(size.width * Math.cos(angle), size.height * Math.sin(angle));
    }
    polygonCorners(h) {
        let corners = [];
        let center = this.hexToPixel(h);
        for (let i = 0; i < 6; i++) {
            let offset = this.hexCornerOffset(i);
            corners.push(new Point(center.x + offset.x, center.y + offset.y));
        }
        return corners;
    }
}
//# sourceMappingURL=hex.js.map
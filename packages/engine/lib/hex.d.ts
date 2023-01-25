export declare class Point {
    x: number;
    y: number;
    constructor(x: number, y: number);
}
export declare class Hex {
    q: number;
    r: number;
    s: number;
    static directions: Hex[];
    static diagonals: Hex[];
    constructor(q: number, r: number, s: number);
    static cube(q: number, r: number, s: number): Hex;
    static axial(q: number, r: number): Hex;
    toString(): string;
    equals(other: Hex): boolean;
    add(b: Hex): Hex;
    subtract(b: Hex): Hex;
    scale(k: number): Hex;
    rotateLeft(): Hex;
    rotateRight(): Hex;
    static direction(direction: number): Hex;
    neighbor(direction: number): Hex;
    neighbors(): Hex[];
    diagonalNeighbor(direction: number): Hex;
    len(): number;
    distance(b: Hex): number;
    round(): Hex;
    lerp(b: Hex, t: number): Hex;
    linedraw(b: Hex): Hex[];
}
export declare class OffsetCoord {
    col: number;
    row: number;
    static EVEN: number;
    static ODD: number;
    constructor(col: number, row: number);
    static qoffsetFromCube(offset: number, h: Hex): OffsetCoord;
    static qoffsetToCube(offset: number, h: OffsetCoord): Hex;
    static roffsetFromCube(offset: number, h: Hex): OffsetCoord;
    static roffsetToCube(offset: number, h: OffsetCoord): Hex;
}
export declare class DoubledCoord {
    col: number;
    row: number;
    constructor(col: number, row: number);
    toString(): string;
    equals(o: DoubledCoord): boolean;
    static qdoubledFromCube(h: Hex): DoubledCoord;
    qdoubledToCube(): Hex;
    static rdoubledFromCube(h: Hex): DoubledCoord;
    rdoubledToCube(): Hex;
}
export declare class Orientation {
    static pointy: Orientation;
    static flat: Orientation;
    f0: number;
    f1: number;
    f2: number;
    f3: number;
    b0: number;
    b1: number;
    b2: number;
    b3: number;
    start_angle: number;
    private constructor();
}
export declare class Layout {
    orientation: Orientation;
    size: {
        width: number;
        height: number;
    };
    origin: Point;
    constructor(orientation: Orientation, size: {
        width: number;
        height: number;
    }, origin: Point);
    hexToPixel(h: Hex): Point;
    pixelToHex(p: Point): Hex;
    hexCornerOffset(corner: number): Point;
    polygonCorners(h: Hex): Point[];
}
//# sourceMappingURL=hex.d.ts.map
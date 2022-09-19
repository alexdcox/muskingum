import { Hex, OffsetCoord, Point, Layout } from './hex.js';
export declare function mix(a: number, b: number, t: number): number;
export declare function bounds(array: any[], field: string): {
    min: number;
    max: number;
};
export declare function closedInterval({ min, max }: {
    min: number;
    max: number;
}): number[];
export declare function makeRectangularShape(minCol: number, maxCol: number, minRow: number, maxRow: number, convert: (o: OffsetCoord) => Hex): Hex[];
export declare function makeRDoubledRectangularShape(minCol: number, maxCol: number, minRow: number, maxRow: number): {
    col: number;
    row: number;
    q: number;
    r: number;
    s: number;
}[];
export declare function makeQDoubledRectangularShape(minCol: number, maxCol: number, minRow: number, maxRow: number): {
    col: number;
    row: number;
    q: number;
    r: number;
    s: number;
}[];
export declare function makeHexagonalShape(n: number): Hex[];
export declare function makeDownTriangularShape(n: number): Hex[];
export declare function makeUpTriangularShape(n: number): Hex[];
export declare function makeRhombusShape(w: number, h: number): Hex[];
export declare function pointSetBounds(points: Point[]): {
    left: number;
    top: number;
    right: number;
    bottom: number;
};
export declare function hexSetBounds(layout: Layout, hexes: Hex[]): {
    left: number;
    top: number;
    right: number;
    bottom: number;
};
export declare function breadthFirstSearch(start: Hex, blocked: (h: Hex) => boolean): {
    cost_so_far: Map<string, number>;
    came_from: Map<string, Hex | null>;
};
export declare function hexLineFractional(A: Hex, B: Hex): Hex[];
export declare function hexRing(radius: number): Hex[];
//# sourceMappingURL=hex-algorithms.d.ts.map
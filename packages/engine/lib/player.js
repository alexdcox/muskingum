import { id } from './util.js';
export class Player {
    id;
    index = -1;
    name;
    password = '';
    email = '';
    gameId = -1;
    constructor(name) {
        this.id = id();
        this.name = name;
    }
}
//# sourceMappingURL=player.js.map
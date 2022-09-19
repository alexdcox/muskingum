import { id } from './util.js';
export class Player {
    id;
    name;
    password = '';
    email = '';
    constructor(name) {
        this.id = id();
        this.name = name;
    }
}
//# sourceMappingURL=player.js.map
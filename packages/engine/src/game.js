import { id } from './util';
export class GameServer {
    constructor() {
        console.log('New game server 😊!');
    }
    handleMessage(message) {
        switch (message.type) {
            case "login":
                console.log("Logging in user:", message.name);
                break;
            default:
                console.log("Received message with invalid type:", message.type);
                break;
        }
    }
}
class Player {
    id;
    name;
    constructor(name) {
        this.id = id();
        this.name = name;
    }
}
var ZoneName;
(function (ZoneName) {
    ZoneName[ZoneName["Draw"] = 0] = "Draw";
    ZoneName[ZoneName["Hand"] = 1] = "Hand";
    ZoneName[ZoneName["Grid"] = 2] = "Grid";
    ZoneName[ZoneName["Discard"] = 3] = "Discard";
})(ZoneName || (ZoneName = {}));
class Zone {
    units = [];
}
class Game {
    constructor() {
        console.log('New game started');
    }
}
class GamePlayerState {
    _zones;
    constructor() {
        this._zones = new Map([
            [ZoneName.Draw, new Zone()],
            [ZoneName.Hand, new Zone()],
            [ZoneName.Grid, new Zone()],
            [ZoneName.Discard, new Zone()],
        ]);
    }
}

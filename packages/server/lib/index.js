import express from 'express';
import { WebSocketServer } from 'ws';
import { GameServer } from 'engine';
const app = express();
const port = 3030;
const gameServer = new GameServer();
const wsServer = new WebSocketServer({ noServer: true });
wsServer.on('connection', (socket) => {
    gameServer.handleConnectionOpened(socket);
    socket.on('close', () => {
        console.log('[server] Websocket closed');
        gameServer.handleConnectionClosed(socket);
    });
    socket.on('message', (message) => {
        let json = '';
        try {
            json = JSON.parse(message.toString('utf-8'));
        }
        catch (e) {
            console.log('Dropping invalid json message: ' + message.toString('utf-8'));
        }
        if (json !== '') {
            gameServer.handleMessage(json, socket);
        }
    });
});
const server = app.listen(port, () => {
    console.log(`[server] Example app listening on port ${port}`);
});
server.on('upgrade', (request, socket, head) => {
    const ip = request.socket.remoteAddress;
    console.log('[server] New websocket connection', ip);
    // gameServer.newConnection()
    wsServer.handleUpgrade(request, socket, head, (socket2) => {
        wsServer.emit('connection', socket2, request);
    });
});
(async () => {
    const keypress = async () => {
        process.stdin.setRawMode(true);
        return new Promise(resolve => process.stdin.once('data', data => {
            const byteArray = [...data];
            if (byteArray.length > 0 && byteArray[0] === 3) {
                console.log('^C');
                process.exit(1);
            }
            process.stdin.setRawMode(false);
            resolve(undefined);
        }));
    };
    // console.log("press any key to send gamestate")
    // while (1) {
    //   await keypress()
    //   console.log('sending next one...')
    //
    //   wsServer.clients.forEach(websocket => {
    //     websocket.send({
    //       type: 'gamestate',
    //       state,
    //     })
    //   })
    // }
})();
//# sourceMappingURL=index.js.map
import { GameState } from '../../Game/GameState';

const serializePlayerUpdateEvent = (playerPosition: { x: number; y: number; z: number }) => {
    if (!GameState.playerID) return;

    const buffer = new ArrayBuffer(1 + 4 + 4 + 4 + 4);
    const view = new DataView(buffer);

    let offset = 0;

    //header
    view.setUint8(offset, 0x01);
    offset++;

    //player id
    view.setInt32(offset, GameState.playerID, true);
    offset += 4;

    //player x
    view.setFloat32(offset, playerPosition.x, true);
    offset += 4;

    //player y
    view.setFloat32(offset, playerPosition.y, true);
    offset += 4;

    //player z
    view.setFloat32(offset, playerPosition.z, true);
    offset += 4;

    return buffer;
};

export { serializePlayerUpdateEvent };

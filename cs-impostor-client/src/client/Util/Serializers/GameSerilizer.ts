const serializePlayerUpdateEvent = (playerPosition: { x: number; y: number; z: number }) => {
    const buffer = new ArrayBuffer(1 + 4 + 4 + 4 + 4);
    const view = new DataView(buffer);

    let offset = 0;

    //header
    view.setUint8(offset, 0x01);
    offset++;

    //player id
    view.setUint32(offset, 12345, true);
    offset += 4;

    //player x
    view.setUint32(offset, playerPosition.x, true);
    offset += 4;

    //player y
    view.setUint32(offset, playerPosition.y, true);
    offset += 4;

    //player z
    view.setUint32(offset, playerPosition.z, true);
    offset += 4;

    return buffer;
};

export { serializePlayerUpdateEvent };

const serializePlayerMessageEvent = (message: string) => {
    const encoder = new TextEncoder();
    const messageBytes = encoder.encode(message);
    const messageLength = messageBytes.length;
    const buffer = new ArrayBuffer(1 + 4 + messageLength);
    const view = new DataView(buffer);

    let offset = 0;

    // set the header
    view.setUint8(offset, 0x02);
    offset++;

    //player id
    view.setUint32(offset, 12345, true);
    offset += 4;

    // string message
    const uint8Array = new Uint8Array(buffer);
    uint8Array.set(messageBytes, offset);

    return buffer;
};

export { serializePlayerMessageEvent };

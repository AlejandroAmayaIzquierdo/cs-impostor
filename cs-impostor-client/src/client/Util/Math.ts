function clamp(x: number, a: number, b: number) {
    return Math.min(Math.max(x, a), b);
}

export { clamp };

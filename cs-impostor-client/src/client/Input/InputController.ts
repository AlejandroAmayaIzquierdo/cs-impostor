export class InputController {
    public target?: Document;

    public previous?: Controller.State;
    public current: Controller.State;

    private keys: any = {};

    public static readonly VALID_KEYS = ['W', 'S', 'A', 'D'];

    constructor(target?: Document) {
        this.target = target || document;
        this.current = {
            leftButton: false,
            rightButton: false,
            mouseXDelta: 0,
            mouseYDelta: 0,
            mouseX: 0,
            mouseY: 0,
        };

        this.target?.addEventListener('mousemove', (e) => this.onMouseMove(e), false);
        this.target?.addEventListener('keydown', (e) => this.onKeyDown(e), false);
        this.target?.addEventListener('keyup', (e) => this.onKeyUp(e), false);
    }

    public onMouseMove(e: MouseEvent) {
        this.current.mouseX = e.pageX - window.innerWidth / 2;
        this.current.mouseY = e.pageY - window.innerHeight / 2;

        if (!this.previous) this.previous = { ...this.current };

        this.current.mouseXDelta = this.current.mouseX - this.previous.mouseX;
        this.current.mouseYDelta = this.current.mouseY - this.previous.mouseY;
    }

    public onKeyDown(e: KeyboardEvent) {
        const eventKey = e.key.toUpperCase();

        if (InputController.VALID_KEYS.includes(eventKey)) this.keys[e.key.toUpperCase()] = true;
    }
    public onKeyUp(e: KeyboardEvent) {
        this.keys[e.key.toUpperCase()] = false;
    }

    public key(keyCode: string) {
        return !!this.keys[keyCode];
    }

    public isReady() {
        return this.previous !== null;
    }

    public isMoving(): boolean {
        return Object.values(this.keys).filter((e) => e === true).length > 0;
    }

    public update() {
        if (this.previous) {
            this.current.mouseXDelta = this.current.mouseX - this.previous.mouseX;
            this.current.mouseYDelta = this.current.mouseY - this.previous.mouseY;

            this.previous = { ...this.current };
        }
    }
}

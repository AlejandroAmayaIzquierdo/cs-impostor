import * as THREE from 'three';
import { PointerLockControls } from 'three/addons/controls/PointerLockControls.js';
import { InputController } from './Input/InputController';
import { serializePlayerUpdateEvent } from './Util/Serializers/GameSerilizer';
import { GameState } from './Game/GameState';

class Client {
    public static readonly PLAYER_SPEED = 10;

    private scene: THREE.Scene;
    private renderer: THREE.WebGLRenderer;
    private input = new InputController();
    private previousRAF: number = 0;
    private camera = new THREE.PerspectiveCamera(
        75,
        window.innerWidth / window.innerHeight,
        0.1,
        1000
    );

    private ws?: WebSocket;
    private controls: PointerLockControls;

    constructor() {
        this.scene = new THREE.Scene();
        this.renderer = new THREE.WebGLRenderer({ antialias: false });
        this.controls = new PointerLockControls(this.camera, this.renderer.domElement);

        this.camera.position.y = 2;

        const canvas = this.renderer.domElement;

        canvas.addEventListener(
            'click',
            () => {
                this.controls.lock();
            },
            false
        );

        this.initializeRenderer();
        this.initializeScene();

        this.raf();
        this.onWindowResize();

        this.ws = new WebSocket('ws://localhost:8081');

        this.ws.binaryType = 'arraybuffer';

        this.ws.onopen = () => {
            console.log('Connection stablish');
            const position = this.camera.position;
            const pack = serializePlayerUpdateEvent({
                x: position.x,
                y: position.y,
                z: position.z,
            });
            if (pack) this.ws?.send(pack);
        };
        this.ws.onmessage = (event) => {
            if (event.data instanceof ArrayBuffer) {
                if (this.ws?.readyState === WebSocket.OPEN && !GameState.playerID) {
                    const dataView = new DataView(event.data);
                    const playerID = dataView.getInt32(0, true);
                    console.log(playerID);
                    GameState.playerID = playerID;
                    return;
                }
                const dataView = new DataView(event.data);
                let offset = 0;
                const headerEvent = dataView.getInt8(offset);
                offset++;

                const playerID = dataView.getInt32(offset, true);
                offset += 4;

                const positionX = dataView.getFloat32(offset, true);
                offset += 4;
                const positionY = dataView.getFloat32(offset, true);
                offset += 4;
                const positionZ = dataView.getFloat32(offset, true);

                if (playerID === GameState.playerID) return;

                if (!GameState.players[playerID]) {
                    const cylinder = new THREE.CylinderGeometry(1, 1, 2.5, 10, 5);
                    const material = new THREE.MeshBasicMaterial({
                        color: 0x00bfff,
                        wireframe: true,
                    });

                    const cylinderMesh = new THREE.Mesh(cylinder, material);

                    cylinderMesh.position.x = positionX;
                    cylinderMesh.position.y = positionY - 1;
                    cylinderMesh.position.z = positionZ;

                    this.scene.add(cylinderMesh);

                    GameState.players[playerID] = cylinderMesh;
                } else {
                    const inGamePlayer: THREE.Mesh<
                        THREE.CylinderGeometry,
                        THREE.MeshBasicMaterial,
                        THREE.Object3DEventMap
                    > = GameState.players[playerID];

                    inGamePlayer.position.x = positionX;
                    inGamePlayer.position.y = positionY - 1;
                    inGamePlayer.position.z = positionZ;
                }

                console.log({ headerEvent, playerID, positionX, positionY, positionZ });
            } else {
                console.log('Text data received:', event.data);
            }
        };
    }

    private initializeRenderer() {
        this.renderer.shadowMap.enabled = true;
        this.renderer.shadowMap.type = THREE.PCFShadowMap;
        this.renderer.setPixelRatio(window.devicePixelRatio);
        this.renderer.setSize(window.innerWidth, window.innerHeight);

        document.body.appendChild(this.renderer.domElement);

        window.addEventListener(
            'resize',
            () => {
                this.onWindowResize();
            },
            false
        );
    }
    private initializeScene() {
        const geometry = new THREE.PlaneGeometry(100, 100, 10, 10);
        const material = new THREE.MeshBasicMaterial({
            color: 0x00ff00,
            wireframe: true,
        });

        const plane = new THREE.Mesh(geometry, material);
        plane.rotation.x = Math.PI / 2;
        this.scene.add(plane);
    }

    private onWindowResize() {
        this.camera.aspect = window.innerWidth / window.innerHeight;
        this.camera.updateProjectionMatrix();

        this.renderer.setSize(window.innerWidth, window.innerHeight);
    }

    public raf() {
        requestAnimationFrame((t) => {
            if (this.previousRAF === null) {
                this.previousRAF = t;
            }
            this.step(t - this.previousRAF);
            const position = this.camera.position;
            const pack = serializePlayerUpdateEvent({
                x: position.x,
                y: position.y,
                z: position.z,
            });
            if (this.ws?.readyState === WebSocket.OPEN && pack && this.input.isMoving())
                this.ws?.send(pack);
            this.renderer.autoClear = true;
            this.renderer.render(this.scene, this.camera);
            this.renderer.autoClear = false;
            this.previousRAF = t;
            this.raf();
        });
    }

    public step(timeElapsed: number) {
        const timeElapsedS = timeElapsed * 0.001;

        const forwardDirection = (this.input.key('W') ? 1 : 0) + (this.input.key('S') ? -1 : 0);
        const strafeDirection = (this.input.key('A') ? -1 : 0) + (this.input.key('D') ? 1 : 0);

        if (forwardDirection !== 0)
            this.controls.moveForward(timeElapsedS * Client.PLAYER_SPEED * forwardDirection);

        if (strafeDirection !== 0)
            this.controls.moveRight(timeElapsedS * Client.PLAYER_SPEED * strafeDirection);

        this.input.update();
    }
}

new Client();

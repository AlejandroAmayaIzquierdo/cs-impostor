import * as THREE from 'three';
import { clamp } from '../Util/Math';
import { InputController } from './InputController';

export class FirstPersonCamera {
    private camera: THREE.PerspectiveCamera;

    private input = new InputController();

    public rotation = new THREE.Quaternion();
    public translation = new THREE.Vector3(0, 2, 0);
    private phi = 0;
    private phiSpeed = 8;
    private theta = 0;
    private thetaSpeed = 5;

    constructor(camera: THREE.PerspectiveCamera) {
        this.camera = camera;
    }

    public update(timeElapsedS: number) {
        this.updateRotation(timeElapsedS);
        this.updateCamera(timeElapsedS);
        this.updateTranslation(timeElapsedS);
        this.input.update();
    }
    private updateRotation(timeElapsedS: number) {
        const xh = this.input.current.mouseXDelta / window.innerWidth;
        const yh = this.input.current.mouseYDelta / window.innerHeight;

        this.phi += -xh * this.phiSpeed;
        this.theta = clamp(this.theta + -yh * this.thetaSpeed, -Math.PI / 3, Math.PI / 3);

        const qx = new THREE.Quaternion();
        qx.setFromAxisAngle(new THREE.Vector3(0, 1, 0), this.phi);
        const qz = new THREE.Quaternion();
        qz.setFromAxisAngle(new THREE.Vector3(1, 0, 0), this.theta);

        const q = new THREE.Quaternion();
        q.multiply(qx);
        q.multiply(qz);

        this.rotation.copy(q);
    }
    private updateCamera(timeElapsedS: number) {
        this.camera.quaternion.copy(this.rotation);
        this.camera.position.copy(this.translation);

        const forward = new THREE.Vector3(0, 0, -1);
        forward.applyQuaternion(this.rotation);
        forward.multiplyScalar(100);
        forward.add(this.translation);

        this.camera.lookAt(forward);
    }

    private updateTranslation(timeElapsedS: number) {
        const forwardVelocity = (this.input.key('W') ? 1 : 0) + (this.input.key('S') ? -1 : 0);
        const strafeVelocity = (this.input.key('A') ? 1 : 0) + (this.input.key('D') ? -1 : 0);

        const qx = new THREE.Quaternion();
        qx.setFromAxisAngle(new THREE.Vector3(0, 1, 0), this.phi);

        const forward = new THREE.Vector3(0, 0, -1);
        forward.applyQuaternion(qx);
        forward.multiplyScalar(forwardVelocity * timeElapsedS * 10);

        const left = new THREE.Vector3(-1, 0, 0);
        left.applyQuaternion(qx);
        left.multiplyScalar(strafeVelocity * timeElapsedS * 10);

        this.translation.add(forward);
        this.translation.add(left);
    }
}

import {
  AmbientLight,
  CanvasTexture,
  CylinderGeometry,
  DirectionalLight,
  Group,
  MathUtils,
  Mesh,
  MeshPhysicalMaterial,
  MeshStandardMaterial,
  PerspectiveCamera,
  PMREMGenerator,
  RepeatWrapping,
  Scene,
  SphereGeometry,
  SRGBColorSpace,
  WebGLRenderer,
} from 'three';
import {RoomEnvironment} from 'three/examples/jsm/environments/RoomEnvironment.js';

/** Medidas, luces y velocidades del poste. */
export interface PoleConfig {
  radius: number;
  glassRadius: number;
  height: number;
  capRadius: number;
  capHeight: number;
  ballRadius: number;
  segments: number;
  cameraFov: number;
  cameraDistance: number;
  near: number;
  far: number;
  lean: number;
  stripeSpeed: number;
  spinSpeed: number;
  tiltLimit: number;
  tiltEase: number;
  degreesToTilt: number;
  textureRepeat: number;
  glassOpacity: number;
  glassRoughness: number;
  glassReflection: number;
  stripeRoughness: number;
  metalRoughness: number;
  ambient: number;
  keyLight: number;
  rimLight: number;
  keyX: number;
  keyY: number;
  keyZ: number;
  rimX: number;
  rimY: number;
  rimZ: number;
  capOffset: number;
  ballOffset: number;
  maxPixelRatio: number;
  msPerSecond: number;
}

/** Escena ya armada, con lo necesario para repintar las franjas y liberarla. */
export interface PoleScene {
  refreshStripes: () => void;
  dispose: () => void;
}

const metalColor = 0xd9d4c8;
const lightColor = 0xffffff;
const half = 0.5;

/** Arma el poste con Three.js, lo anima y lo inclina con el teléfono o el puntero. */
export function createPoleScene(
  canvas: HTMLCanvasElement,
  stripes: HTMLCanvasElement,
  config: PoleConfig,
  reduceMotion: boolean,
): PoleScene {
  const renderer = new WebGLRenderer({canvas, antialias: true, alpha: true});
  renderer.setPixelRatio(Math.min(window.devicePixelRatio, config.maxPixelRatio));
  renderer.outputColorSpace = SRGBColorSpace;
  const scene = new Scene();
  const environment = new PMREMGenerator(renderer).fromScene(new RoomEnvironment());
  scene.environment = environment.texture;
  const camera = new PerspectiveCamera(config.cameraFov, 1, config.near, config.far);
  camera.position.z = config.cameraDistance;

  const texture = new CanvasTexture(stripes);
  texture.colorSpace = SRGBColorSpace;
  texture.wrapS = RepeatWrapping;
  texture.wrapT = RepeatWrapping;
  texture.repeat.set(1, config.textureRepeat);

  const metal = new MeshStandardMaterial({color: metalColor, metalness: 1, roughness: config.metalRoughness});
  const body = new Mesh(
    new CylinderGeometry(config.radius, config.radius, config.height, config.segments, 1, true),
    new MeshStandardMaterial({map: texture, roughness: config.stripeRoughness}),
  );
  const glass = new Mesh(
    new CylinderGeometry(config.glassRadius, config.glassRadius, config.height, config.segments, 1, true),
    new MeshPhysicalMaterial({
      transparent: true,
      opacity: config.glassOpacity,
      roughness: config.glassRoughness,
      envMapIntensity: config.glassReflection,
      depthWrite: false,
    }),
  );
  const capGeometry = new CylinderGeometry(config.capRadius, config.capRadius, config.capHeight, config.segments);
  const top = new Mesh(capGeometry, metal);
  top.position.y = config.height * half + config.capOffset;
  const bottom = new Mesh(capGeometry, metal);
  bottom.position.y = -config.height * half - config.capOffset;
  const ball = new Mesh(new SphereGeometry(config.ballRadius, config.segments, config.segments), metal);
  ball.position.y = top.position.y + config.ballOffset;

  const group = new Group();
  group.add(body, glass, top, bottom, ball);
  group.rotation.z = config.lean;
  scene.add(group);
  scene.add(new AmbientLight(lightColor, config.ambient));
  const key = new DirectionalLight(lightColor, config.keyLight);
  key.position.set(config.keyX, config.keyY, config.keyZ);
  const rim = new DirectionalLight(lightColor, config.rimLight);
  rim.position.set(config.rimX, config.rimY, config.rimZ);
  scene.add(key, rim);

  /** Ajusta el lienzo y la cámara al tamaño del contenedor. */
  const resize = () => {
    const width = canvas.clientWidth;
    const height = canvas.clientHeight;
    renderer.setSize(width, height, false);
    camera.aspect = width / height;
    camera.updateProjectionMatrix();
  };
  const sizeObserver = new ResizeObserver(() => {
    resize();
    renderer.render(scene, camera);
  });
  sizeObserver.observe(canvas);
  resize();

  let tiltX = 0;
  let tiltZ = 0;
  /** Inclina el poste según cómo se sostiene el teléfono. */
  const onOrientation = (event: DeviceOrientationEvent) => {
    tiltX = MathUtils.clamp((event.beta ?? 0) * config.degreesToTilt, -config.tiltLimit, config.tiltLimit);
    tiltZ = MathUtils.clamp(-(event.gamma ?? 0) * config.degreesToTilt, -config.tiltLimit, config.tiltLimit);
  };
  /** Inclina el poste hacia donde está el puntero. */
  const onPointer = (event: PointerEvent) => {
    tiltZ = ((event.clientX / window.innerWidth) - half) * -config.tiltLimit;
    tiltX = ((event.clientY / window.innerHeight) - half) * config.tiltLimit;
  };

  let frame = 0;
  let last = performance.now();
  /** Avanza la animación un cuadro. */
  const animate = (now: number) => {
    const seconds = (now - last) / config.msPerSecond;
    last = now;
    texture.offset.y -= config.stripeSpeed * seconds;
    group.rotation.y += config.spinSpeed * seconds;
    group.rotation.x += (tiltX - group.rotation.x) * config.tiltEase;
    group.rotation.z += (config.lean + tiltZ - group.rotation.z) * config.tiltEase;
    renderer.render(scene, camera);
    frame = requestAnimationFrame(animate);
  };

  if (reduceMotion) {
    renderer.render(scene, camera);
  }
  else {
    window.addEventListener('deviceorientation', onOrientation);
    window.addEventListener('pointermove', onPointer);
    frame = requestAnimationFrame(animate);
  }

  return {
    refreshStripes: () => {
      texture.needsUpdate = true;
      renderer.render(scene, camera);
    },
    dispose: () => {
      cancelAnimationFrame(frame);
      window.removeEventListener('deviceorientation', onOrientation);
      window.removeEventListener('pointermove', onPointer);
      sizeObserver.disconnect();
      environment.dispose();
      texture.dispose();
      renderer.dispose();
    },
  };
}

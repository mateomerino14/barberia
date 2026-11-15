const webp = 'image/webp';
const jpeg = 'image/jpeg';
const png = 'image/png';
const quality = 0.85;

/** Medidas finales de una imagen que no pasa del lado más largo indicado. */
export function fitWithin(width: number, height: number, maxSide: number): {width: number; height: number} {
  const scale = Math.min(1, maxSide / Math.max(width, height));
  return {width: Math.round(width * scale), height: Math.round(height * scale)};
}

/** Convierte el lienzo en un archivo del tipo pedido. */
function toBlob(canvas: HTMLCanvasElement, type: string): Promise<Blob | null> {
  return new Promise((done) => canvas.toBlob(done, type, quality));
}

/**
 * Achica la foto en el teléfono antes de subirla, para que pese poco y cargue rápido.
 * Usa WebP si el navegador lo sabe hacer; si no, PNG cuando hay que conservar transparencia o JPEG.
 */
export async function shrinkImage(file: File, maxSide: number, keepTransparency = false): Promise<Blob> {
  const bitmap = await createImageBitmap(file);
  const size = fitWithin(bitmap.width, bitmap.height, maxSide);
  const canvas = document.createElement('canvas');
  canvas.width = size.width;
  canvas.height = size.height;
  canvas.getContext('2d')?.drawImage(bitmap, 0, 0, size.width, size.height);
  bitmap.close();
  const encoded = await toBlob(canvas, webp);
  if (encoded?.type === webp) {
    return encoded;
  }
  let fallback = jpeg;
  if (keepTransparency) {
    fallback = png;
  }
  return (await toBlob(canvas, fallback)) ?? file;
}

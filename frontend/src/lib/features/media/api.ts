import {apiUrl, request, type ApiResult} from '#lib/core/http.ts';

/** Imagen recién subida. */
interface UploadedImage {
  id: string;
}

/** Sube una imagen ya achicada y devuelve su id. */
export function uploadImage(file: Blob, token: string | null): Promise<ApiResult<UploadedImage>> {
  return request<UploadedImage>('/api/images', {method: 'POST', file, token});
}

/** Dirección pública de una imagen. */
export function imageUrl(id: string): string {
  return `${apiUrl()}/api/images/${id}`;
}

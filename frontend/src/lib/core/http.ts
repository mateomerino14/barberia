const defaultApiUrl = 'http://localhost:5000';
const noContent = 204;
const networkStatus = 0;
const unauthorized = 401;

let sessionEnded: (() => void) | null = null;

/** Error de la API tal como lo muestra la interfaz. */
export interface ApiError {
  status: number;
  code: string;
  message: string;
}

/** Resultado de una llamada: los datos o el error, sin excepciones. */
export type ApiResult<T> = {ok: true; data: T} | {ok: false; error: ApiError};

/** Opciones de una llamada a la API. */
export interface RequestOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE';
  body?: unknown;
  token?: string | null;
  fetcher?: typeof fetch;
}

/** Registra qué hacer cuando la API rechaza la sesión: vencida, cerrada o dada de baja. */
export function onSessionEnded(handler: (() => void) | null): void {
  sessionEnded = handler;
}

/** Devuelve la dirección base de la API. */
export function apiUrl(): string {
  return import.meta.env.VITE_API_URL || defaultApiUrl;
}

/** Llama a la API y convierte la respuesta en datos o en un error con mensaje en español. */
export async function request<T>(path: string, options: RequestOptions = {}): Promise<ApiResult<T>> {
  const headers: Record<string, string> = {};
  let body: string | undefined;
  if (options.body !== undefined) {
    headers['Content-Type'] = 'application/json';
    body = JSON.stringify(options.body);
  }
  if (options.token) {
    headers.Authorization = `Bearer ${options.token}`;
  }
  const fetcher = options.fetcher ?? fetch;
  let response: Response;
  try {
    response = await fetcher(`${apiUrl()}${path}`, {
      method: options.method ?? 'GET',
      headers,
      body,
    });
  }
  catch {
    return {ok: false, error: {status: networkStatus, code: 'network', message: 'No pudimos conectarnos. Revisa tu internet.'}};
  }
  if (!response.ok) {
    if (response.status === unauthorized && options.token) {
      sessionEnded?.();
    }
    return {ok: false, error: await readProblem(response)};
  }
  if (response.status === noContent) {
    return {ok: true, data: undefined as T};
  }
  return {ok: true, data: (await response.json()) as T};
}

/** Lee el ProblemDetails de una respuesta fallida. */
async function readProblem(response: Response): Promise<ApiError> {
  const fallback = {status: response.status, code: 'unknown', message: 'Algo salió mal. Intenta de nuevo.'};
  try {
    const problem = (await response.json()) as {title?: string; code?: string};
    return {status: response.status, code: problem.code ?? fallback.code, message: problem.title ?? fallback.message};
  }
  catch {
    return fallback;
  }
}

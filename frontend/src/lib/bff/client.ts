export interface ProblemDetailsCliente {
  status: number;
  title?: string;
  detail?: string;
  code?: string;
}

export class ErrorApiAscenso extends Error {
  readonly status: number;
  readonly code: string;
  readonly title: string;

  constructor(problem: ProblemDetailsCliente) {
    super(problem.detail ?? problem.title ?? "No se pudo completar la operación.");
    this.name = "ErrorApiAscenso";
    this.status = problem.status;
    this.code = problem.code ?? "ERROR_API";
    this.title = problem.title ?? "Error";
  }
}

/**
 * Cliente HTTP del navegador para llamar al BFF de Next.js (/api/bff/api/v1/...).
 * Las cookies HttpOnly viajan automáticamente en el mismo origen; el navegador nunca ve JWTs.
 */
export async function llamarApiBff<TResponse>(
  rutaApiV1: string,
  init?: Omit<RequestInit, "body"> & { body?: unknown }
): Promise<TResponse> {
  const rutaLimpia = rutaApiV1.startsWith("/") ? rutaApiV1 : `/${rutaApiV1}`;
  const url = rutaLimpia.startsWith("/api/auth/")
    ? rutaLimpia
    : `/api/bff${rutaLimpia}`;

  const headers = new Headers(init?.headers);
  if (init?.body !== undefined && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json; charset=utf-8");
  }

  const response = await fetch(url, {
    ...init,
    headers,
    body:
      init?.body === undefined
        ? undefined
        : typeof init.body === "string"
          ? init.body
          : JSON.stringify(init.body),
    credentials: "same-origin",
  });

  if (response.status === 204) {
    return undefined as TResponse;
  }

  const text = await response.text();
  let parsed: unknown = null;
  if (text) {
    try {
      parsed = JSON.parse(text);
    } catch {
      parsed = null;
    }
  }

  if (!response.ok) {
    const problem = (parsed ?? {}) as Partial<ProblemDetailsCliente>;
    throw new ErrorApiAscenso({
      status: response.status,
      title: problem.title,
      detail: problem.detail,
      code: problem.code,
    });
  }

  return parsed as TResponse;
}

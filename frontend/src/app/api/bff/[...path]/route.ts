import { llamarBackendDesdeBff } from "@/lib/bff/proxy";

async function manejarProxy(
  request: Request,
  context: { params: Promise<{ path: string[] }> }
) {
  const { path } = await context.params;
  const ruta = `/${path.join("/")}`;
  const url = new URL(request.url);
  const method = request.method.toUpperCase();

  let body: string | undefined;
  if (method !== "GET" && method !== "HEAD") {
    const texto = await request.text();
    if (texto.length > 0) {
      body = texto;
    }
  }

  const resp = await llamarBackendDesdeBff({
    method,
    ruta,
    queryString: url.search,
    body,
    requiereAutenticacion: true,
  });

  if (resp.status === 204) {
    return new Response(null, { status: 204 });
  }

  return new Response(resp.bodyText, {
    status: resp.status,
    headers: {
      "Content-Type": resp.contentType,
    },
  });
}

export const GET = manejarProxy;
export const POST = manejarProxy;
export const PUT = manejarProxy;
export const PATCH = manejarProxy;
export const DELETE = manejarProxy;

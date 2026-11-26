import { NextResponse, type NextRequest } from "next/server";

const COOKIE_SESION = "ascenso_session";
const RUTAS_PUBLICAS = new Set(["/login", "/verificacion"]);

/**
 * Guardia ligera en edge para rutas protegidas: exige presencia de la cookie HttpOnly
 * de sesión; la verificación criptográfica completa (AES-256-GCM + expiración) ocurre
 * en los Server Components y Route Handlers de Node.js.
 */
export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  if (
    pathname.startsWith("/_next") ||
    pathname.startsWith("/api/") ||
    pathname === "/favicon.ico"
  ) {
    return NextResponse.next();
  }

  const tieneCookie = Boolean(request.cookies.get(COOKIE_SESION)?.value);

  if (pathname === "/") {
    const destino = tieneCookie ? "/inicio" : "/login";
    return NextResponse.redirect(new URL(destino, request.url));
  }

  if (!tieneCookie && !RUTAS_PUBLICAS.has(pathname)) {
    const urlLogin = new URL("/login", request.url);
    urlLogin.searchParams.set("siguiente", pathname);
    return NextResponse.redirect(urlLogin);
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico).*)"],
};

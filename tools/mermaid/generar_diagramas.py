"""Extrae los diagramas Mermaid del DDD y genera la página de enlaces a mermaid.live.

Uso (desde la raíz del repositorio):

    python tools/mermaid/generar_diagramas.py docs/dominio/DDD_v3.3.md

Escribe:
  - docs/dominio/diagramas/NN-titulo.mmd   (un archivo por diagrama)
  - docs/dominio/diagramas/README.md       (índice)
  - docs/dominio/enlaces-mermaid-live-<nombre del DDD>.html

El título de cada diagrama es el encabezado (#, ##, ### …) más cercano por encima del bloque.
Solo usa la biblioteca estándar de Python.
"""

import base64
import html
import json
import pathlib
import re
import sys
import unicodedata
import zlib


def slug(texto: str, largo: int = 55) -> str:
    t = unicodedata.normalize("NFD", texto.lower())
    t = "".join(c for c in t if unicodedata.category(c) != "Mn")
    t = re.sub(r"[^a-z0-9]+", "-", t).strip("-")
    return t if len(t) <= largo else t[:largo].rsplit("-", 1)[0]


def pako(codigo: str) -> str:
    estado = {
        "code": codigo,
        "mermaid": json.dumps({"theme": "dark"}),
        "autoSync": True,
        "rough": False,
        "updateDiagram": True,
        "panZoom": True,
    }
    datos = json.dumps(estado, ensure_ascii=False).encode("utf-8")
    comprimido = zlib.compress(datos, 9)
    return base64.urlsafe_b64encode(comprimido).decode("ascii").rstrip("=")


def extraer(md: str):
    diagramas = []
    titulo = "Sin título"
    en_bloque = False
    lineas = []
    for linea in md.splitlines():
        if not en_bloque:
            m = re.match(r"^#{1,6}\s+(.*)", linea)
            if m:
                titulo = re.sub(r"[`*]", "", m.group(1)).strip()
            elif linea.strip() == "```mermaid":
                en_bloque, lineas = True, []
        elif linea.strip() == "```":
            diagramas.append((titulo, "\n".join(lineas)))
            en_bloque = False
        else:
            lineas.append(linea)
    return diagramas


def main() -> None:
    if len(sys.argv) != 2:
        sys.exit("Uso: python tools/mermaid/generar_diagramas.py docs/dominio/DDD_vX.md")
    ddd = pathlib.Path(sys.argv[1])
    diagramas = extraer(ddd.read_text(encoding="utf-8"))
    salida = ddd.parent / "diagramas"
    salida.mkdir(exist_ok=True)
    for viejo in salida.glob("*.mmd"):
        viejo.unlink()

    filas_md, filas_html = [], []
    for i, (titulo, codigo) in enumerate(diagramas):
        num = f"{i:02d}"
        archivo = f"{num}-{slug(titulo)}.mmd"
        (salida / archivo).write_text(codigo.rstrip() + "\n", encoding="utf-8")
        tipo = codigo.strip().split()[0] if codigo.strip() else ""
        enlace = pako(codigo)
        filas_md.append(f"| {num} | {titulo} | [`{archivo}`]({archivo}) |")
        filas_html.append(
            f"<tr><td>{num}</td><td>{html.escape(titulo)} <span class='tipo'>{tipo}</span></td>"
            f"<td><a href='https://mermaid.live/view#pako:{enlace}'>Ver</a></td>"
            f"<td><a href='https://mermaid.live/edit#pako:{enlace}'>Editar</a></td></tr>"
        )

    (salida / "README.md").write_text(
        f"# Diagramas del modelo de dominio\n\nExtraídos de [`../{ddd.name}`](../{ddd.name}) "
        "con `tools/mermaid/generar_diagramas.py`. No los edites aquí: la fuente de verdad es el `.md` del DDD.\n\n"
        "| # | Diagrama | Archivo |\n|---|---|---|\n" + "\n".join(filas_md) + "\n",
        encoding="utf-8",
    )

    pagina = f"""<!doctype html><html lang="es"><head><meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Enlaces mermaid.live · {html.escape(ddd.stem)}</title>
<style>
:root{{--bg:#0d1117;--fg:#e6edf3;--muted:#8b949e;--line:#30363d;--link:#58a6ff}}
body{{background:var(--bg);color:var(--fg);font-family:system-ui,sans-serif;margin:0 auto;max-width:960px;padding:24px 16px}}
table{{border-collapse:collapse;width:100%}}td,th{{padding:6px 10px;border-bottom:1px solid var(--line);text-align:left}}
a{{color:var(--link)}}.tipo{{color:var(--muted);font-size:.8em;margin-left:6px}}
</style></head><body>
<h1>Modelo de dominio ASCENSO · mermaid.live</h1>
<p>{len(diagramas)} diagramas de <code>{html.escape(ddd.name)}</code>. Ver: solo el dibujo. Editar: código y dibujo.</p>
<table><tr><th>#</th><th>Diagrama</th><th></th><th></th></tr>
{chr(10).join(filas_html)}
</table></body></html>
"""
    destino = ddd.parent / f"enlaces-mermaid-live-{ddd.stem}.html"
    destino.write_text(pagina, encoding="utf-8")
    print(f"{len(diagramas)} diagramas -> {salida} y {destino}")


if __name__ == "__main__":
    main()

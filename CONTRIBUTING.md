# Cómo contribuir

La nota individual se evalúa por el historial de commits, así que **cada integrante sube su propio trabajo desde su propia cuenta**.

## Ramas

- `main` está protegida: solo recibe cambios por pull request aprobado.
- Una rama por tarea, creada desde `main` actualizada:

| Prefijo | Para qué | Ejemplo |
|---|---|---|
| `feat/` | Funcionalidad nueva | `feat/s4-emitir-oferta` |
| `fix/` | Corrección de un error | `fix/s2-reserva-duplicada` |
| `docs/` | Documentación (DDD, ADR, HU) | `docs/ddd-v3-3` |
| `test/` | Solo pruebas | `test/s4-concurrencia-pago-revocacion` |
| `chore/` | Configuración, dependencias, CI | `chore/ci-cobertura` |

```bash
git switch main
git pull
git switch -c feat/s4-emitir-oferta
```

## Commits

Mensaje corto en imperativo y en español, con la referencia a la HU, el ADR o el cambio de OpenSpec que trabaja:

```
feat(s4): emitir oferta con reserva síncrona en S2 (HU-15.1, ADR-03)
docs(ddd): aplicar DDD-07 versión explícita en la raíz
fix(s2): no liberar reservas con oferta existente (ADR-B02)
```

Commits pequeños y frecuentes: es más fácil revisarlos y dejan un historial claro de quién hizo qué.

## Pull requests

1. Sube la rama y abre el pull request contra `main`. La plantilla se llena sola: completa cada punto.
2. Pide revisión a quien figure en [`.github/CODEOWNERS`](.github/CODEOWNERS) para esas carpetas.
3. Se fusiona con **al menos una aprobación** y la integración continua en verde (cuando exista).
4. Después de fusionar, borra la rama.

## Documentos

- La fuente de verdad son los `.md`. Los PDF se guardan como respaldo de lo entregado.
- Un ADR aprobado no se edita en silencio: el cambio entra por un pull request que explica el motivo (y, cuando OpenSpec esté inicializado, por una propuesta en `openspec/changes/`).
- Si cambias un diagrama del DDD, cámbialo en `docs/dominio/DDD_vX.md` y vuelve a generar `docs/dominio/diagramas/` con `tools/mermaid/generar_diagramas.py`.

## Secretos

Nunca subas credenciales: contraseñas, tokens, claves de Firebase o de Keycloak. Van en archivos `.env` o `appsettings.*.local.json`, que ya están en `.gitignore`. Si necesitas documentar una variable, agrégala a un `.env.example` sin el valor real.

Si alguien sube un secreto por error, avisa al grupo de inmediato: hay que **cambiar la credencial**, porque borrarla del repositorio no basta.

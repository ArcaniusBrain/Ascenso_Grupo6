# ADR-06 · Archivos: Firebase Storage solo para binarios

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`../ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](../ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice de decisiones en [`README.md`](README.md).

**Estado:** Obligatoria

## Contexto

Restricción del cliente: la Fundación ya usa Firebase para sus archivos y el sistema debe apoyarse en esa misma cuenta, sin crear almacenamiento paralelo. La frase de la lámina «Base de datos en FireBase con PostgreSQL» se interpreta así: Firebase para archivos y PostgreSQL para datos.

## Decisión

- En Firebase Storage van **solo archivos**: documentos de postulación (RF-03), cartas de otorgamiento (RF-15), compromisos firmados (RF-17), comprobantes y archivos exportados (HU-23.1, HU-33.1 y HU-36.1).
- PostgreSQL guarda la **referencia interna** del archivo (el value object ReferenciaArchivo), nunca una URL pública. Las descargas usan enlaces firmados de corta vigencia que se generan después de verificar la autorización.
- Queda prohibido guardar en Firebase entidades, mensajes, estados o modelos de lectura (HU-32.2).
- El acceso se hace solo por un adaptador detrás de una interfaz del dominio (capa anticorrupción).
- En desarrollo se usa el emulador local de Firebase; la cuenta de la Fundación, solo para la demostración.

## Riesgo

Verificar que la cuenta de Firebase no esté afectada por el aviso de cierre de cuentas corporativas en la nube del 11-nov-2026 mencionado en la clase del 02-nov.

## Fuentes

PRD, Restricciones técnicas del cliente; DDD v3.2, secciones 3b y 3.1; Anexo A de HU-23.1, HU-32.1, HU-32.2 y HU-33.1.

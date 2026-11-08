# ADR-21 · Migración de datos históricos (ETL)

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`../ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](../ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice de decisiones en [`README.md`](README.md).

**Estado:** Obligatoria / Propuesta

## Contexto

La lámina obligatoria pide ingeniería de datos y migración. El PRD señala como riesgo que el historial de 800 becarios y 12 programas solo existe en hojas de cálculo. Al haber seis servicios, no puede haber una única transacción para todo.

## Decisión

- **1. Extraer:** leer las planillas Excel o CSV a tablas de paso (staging).
- **2. Transformar y validar:** normalizar cédulas, correos, montos y periodos; verificar presupuesto (regla 8), no doble beneficio (C2) y estados válidos.
- **3. Ensayo:** un informe de filas válidas y con error, sin escribir nada.
- **4. Cargar por servicio y en orden de dependencias** (S2 catálogo y programas, S1 usuarios, S3 postulantes, S4 becas y pagos, S5 reportes), mediante un comando de importación de cada servicio que usa sus fábricas para que se cumplan las reglas. Nunca se escribe directamente en el schema de otro servicio.
- La carga es repetible: usa claves naturales (cédula, código de programa, código de periodo) para no duplicar.
- Cada registro migrado deja su RegistroDecision con actor «migración».
- **Día D:** congelar las planillas, ensayar, cargar y conciliar los totales con la Fundación (presupuesto y becarios activos).

## Corrige el borrador anterior

Proponía insertar todo en lote dentro de una sola transacción, lo que no es posible entre servicios y además saltaría las reglas del dominio y la auditoría.

## Pendiente

Las plantillas reales de Excel de la Fundación: su formato todavía no se conoce.

## Fuentes

Lámina «Decisiones Arquitectónicas»; PRD, Riesgos; clase del 21-sep; DDD v3.2, sección 17.

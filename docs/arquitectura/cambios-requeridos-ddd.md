# Cambios que requiere el DDD (versión 3.3)

> Fuente: *Propuesta de Arquitectura de Software (ADR) · Borrador v0.1*, 4-nov-2026. Documento original en [`ASCENSO_Propuesta_Arquitectura_v0.1.pdf`](ASCENSO_Propuesta_Arquitectura_v0.1.pdf). Índice en [`README.md`](README.md).

| Sección del DDD v3.2 | Cambio | Motivo |
|---|---|---|
| D-14 y sección 18 | La base de consulta es PostgreSQL en una base aparte, no NoSQL. Hay dos niveles de lectura y el ranking se lee en S3. | ADR-07 |
| D-8 | Se elige Keycloak como proveedor de identidad. | ADR-09 |
| Secciones 15.1, 15.2 y 16.3 | La máquina de estados de Beca y Postulación se implementa con Stateless. | ADR-08 |
| Secciones 2.2, 3 y 3.1 | Se agrega el mapeo de contextos a servicios. Becas y Fondos ya no son «módulos de la misma aplicación». | ADR-02, ADR-03 |
| Sección 3b | Se agregan la API de entrada para universidades y el cliente propio de cada universidad. | ADR-19 |
| Anexo A de las HU (20.1, 22.1, 22.2, 26.2) | Revocación y liberación de presupuesto ya no ocurren en la misma transacción (ya señalado en la sección 20.2 del DDD). | ADR-03 |

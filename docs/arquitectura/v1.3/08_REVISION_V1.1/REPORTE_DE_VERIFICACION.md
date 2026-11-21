# ASCENSO — Verificación de artefactos v1.1

**Fecha:** 18-nov-2026 · **Alcance:** validación documental y de sintaxis de los artefactos; **sin código de producto ejecutado**.

Los 33 hallazgos de la revisión anterior fueron atendidos documentalmente, con límites indicados en el registro. El seguimiento actualizado está en `09_REVISION_V1.2/` y prevalece sobre este informe histórico. Todos los 33 hallazgos del informe están documentados con evidencia de archivo en `MATRIZ_33_CORRECCIONES.csv`. Dos tienen límites de fuente ajenos al paquete: **A2**, porque no se adjuntó la lámina original (se aplicaron las obligaciones citadas en la revisión), y **A9**, porque el DDD v3.2 definitivo no se alteró y la adenda al despliegue distribuido requiere adopción formal por sus autores. No se presentan como aprobaciones institucionales.

## Comprobaciones de integridad y sintaxis

- [x] MATRIZ_40_RF_ARQUITECTURA.csv — 40
- [x] MATRIZ_73_HU_ARQUITECTURA.csv — 73
- [x] PROPIEDAD_25_AGREGADOS.csv — 25
- [x] MAPEO_34_EVENTOS_DOMINIO.csv — 34
- [x] REGISTRO_24_ADR.csv — 24
- [x] 20 DOT/SVG
- [x] 12 Mermaid
- [x] 12 SVG secuencias
- [x] DOT 13_s4_becas.dot
- [x] DOT 09_operacion_eventos.dot
- [x] DOT 06_datos_cqrs.dot
- [x] DOT 04_componentes_uml.dot
- [x] DOT 03_particion_contextos.dot
- [x] DOT 02_referencias_clean.dot
- [x] DOT 19_referencia_dependencias.dot
- [x] DOT 05_puertos_adaptadores.dot
- [x] DOT 20_registro_outbox.dot
- [x] DOT 14_s6_lecturas.dot
- [x] DOT 16_concurrencia.dot
- [x] DOT 11_s2_fondos.dot
- [x] DOT 17_seguridad.dot
- [x] DOT 15_despliegue.dot
- [x] DOT 07_propiedad_datos.dot
- [x] DOT 10_prueba_dependencias.dot
- [x] DOT 01_tres_niveles.dot
- [x] DOT 18_migracion.dot
- [x] DOT 08_seguridades.dot
- [x] DOT 12_s3_postulacion.dot
- [x] XML DIAGRAMA_CAPAS.drawio
- [x] XML DIAGRAMA_COMPONENTES.drawio
- [x] No rutas privadas en OpenAPI publico
- [x] Contratos internos aislados
- [x] Public OfertaRequest sin monto
- [x] Sin postulanteId/ evaluadorId en body
- [x] Universidades usan codigoEstudiante
- [x] Hu25.2 OTP
- [x] OpenAPI schemas references valid 1.1.0 ASCENSO - AP — 257 refs
- [x] OpenAPI schemas references valid 1.1.0 ASCENSO - AP — 44 refs
- [x] POST públicos con cuerpo
- [x] Lote universidad responde 202
- [x] Errores ProblemDetails
- [x] Read store MongoDB decidido
- [x] DDD original intacto
- [x] 33 filas matriz
- [x] MMD 11_secuencia_reserva_oferta.mmd
- [x] MMD 12_secuencia_aceptacion.mmd
- [x] MMD 13_secuencia_auditoria_proyeccion.mmd
- [x] MMD 14_secuencia_pago_otp.mmd
- [x] MMD 21_secuencia_control_2fa.mmd
- [x] MMD 22_secuencia_proyeccion.mmd
- [x] MMD 23_secuencia_concurrencia.mmd
- [x] MMD 24_secuencia_universidad.mmd
- [x] MMD 25_secuencia_cierre_periodo.mmd
- [x] MMD 26_secuencia_caducidad_oferta.mmd
- [x] MMD 27_secuencia_etl.mmd
- [x] MMD 28_secuencia_reserva_reconciliacion.mmd

## Límite honesto de las pruebas

Se verificaron estructuras CSV, YAML/OpenAPI por análisis de referencias, DOT por Graphviz, XML diagrams.net, fuentes Mermaid por control estructural, y correspondencia de nombres. Esto **no** comprueba que los enlaces Mermaid Live abran en servidores externos ni que el sistema implemente seguridad, transacciones o eventos, porque aún no existe código de producción. Esos escenarios están especificados para pruebas posteriores.
